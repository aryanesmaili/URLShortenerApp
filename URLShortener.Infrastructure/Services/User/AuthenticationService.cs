using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using SharedDataModels.Responses;
using System.Net;
using System.Text;
using System.Text.Json;
using URLShortener.Application.DTOs.EntityDTOs.User;
using URLShortener.Application.DTOs.Settings;
using URLShortener.Application.Interfaces.Infrastructure.External;
using URLShortener.Application.Interfaces.Services.User;
using URLShortener.Application.Models;
using URLShortener.Application.Repositories;
using URLShortener.Application.Utility.Exceptions;
using URLShortener.Domain.Entities.User;

namespace URLShortener.Infrastructure.Services.User;

public sealed class AuthenticationService(
    IMapper mapper,
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork uow,
    IEmailService emailService,
    HttpClient httpClient,
    UserManager<AppIdentityUser> userManager,
    SignInManager<AppIdentityUser> signInManager,
    ITokenService tokenService,
    IOptions<ApplicationInfoSettings> appInfo,
    IOptions<AuthorizationSettings> authorizationSettings) : IAuthenticationService
{
    private readonly IMapper _mapper = mapper;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly IUnitOfWork _uow = uow;
    private readonly IEmailService _emailService = emailService;
    private readonly HttpClient _httpClient = httpClient;
    private readonly UserManager<AppIdentityUser> _userManager = userManager;
    private readonly SignInManager<AppIdentityUser> _signInManager = signInManager;
    private readonly ITokenService _tokenService = tokenService;
    private readonly ApplicationInfoSettings _appInfo = appInfo.Value;
    private readonly AuthorizationSettings _authorizationSettings = authorizationSettings.Value;

    /// <summary>
    /// Authenticates a user by validating their credentials and generating authentication tokens.
    /// </summary>
    /// <param name="loginInfo">Contains the user identifier (email or username) and password.</param>
    /// <returns>An Object containing the authenticated user's DTO and generated JWT/refresh tokens.</returns>
    /// <remarks>
    /// This method orchestrates the entire login flow:
    /// 1. Finds the user by email or username
    /// 2. Validates the password (with lockout on failure)
    /// 3. Retrieves the domain user model
    /// 4. Generates JWT and refresh tokens
    /// </remarks>
    public async Task<UserLoginResponse> LoginAsync(UserLoginDTO loginInfo)
    {
        // Find user by email or username
        AppIdentityUser? identityUser;
        if (loginInfo.IsEmailIdentifier)
            identityUser = await _userManager.FindByEmailAsync(loginInfo.Identifier!);
        else
            identityUser = await _userManager.FindByNameAsync(loginInfo.Identifier!);

        if (identityUser == null)
            throw new ArgumentException("Identifier Or Password is incorrect.");

        // Validate password (lockout on failure)
        var signInResult = await _signInManager.CheckPasswordSignInAsync(identityUser, loginInfo.Password, lockoutOnFailure: true);
        if (!signInResult.Succeeded)
            throw new ArgumentException("Identifier Or Password is incorrect.");

        // Retrieve domain user
        var domainUser = await _userRepository.GetAsync(x => x.ID == identityUser.Id)
            ?? throw new ArgumentException("Identifier Or Password is incorrect."); // I throw ArgumentException instead of NotFoundException to avoid user enumeration

        // Generate tokens
        string jwt = await _tokenService.GenerateJWTokenAsync(identityUser);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(identityUser.Id);

        var userDto = _mapper.Map<UserDTO>(domainUser);

        return new UserLoginResponse
        {
            User = userDto,
            JWToken = jwt,
            RefreshToken = refreshToken
        };
    }

    /// <summary>
    /// Registers a new user by creating both an ASP.NET Core Identity user and a domain user in a transactional context.
    /// </summary>
    /// <param name="dto">The user creation data transfer object containing username, email, and password.</param>
    /// <returns>A <see cref="UserDTO"/> representing the newly created user.</returns>
    /// <remarks>
    /// This method ensures data consistency by using a database transaction. If any step fails, 
    /// the Identity user is cleaned up to prevent orphaned records. The method performs the following steps:
    /// 1. Creates an ASP.NET Core Identity user
    /// 2. Creates a domain user entity
    /// 3. Establishes bidirectional relationship between Identity and domain user
    /// 4. Assigns the "User" role
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when user creation fails or when updating the identity user fails.</exception>
    /// <exception cref="Exception">Thrown when role assignment fails or if any other operation fails during registration.</exception>
    public async Task<UserDTO> RegisterUserAsync(UserCreateDTO dto)
    {
        // Initialize transaction for atomic operations
        await using var transaction = await _uow.BeginTransactionAsync();

        // Track identity user for cleanup in case of failure
        AppIdentityUser? identityUser = null;

        try
        {
            // 1. Create Identity user with provided credentials
            identityUser = new AppIdentityUser
            {
                UserName = dto.Username,
                Email = dto.Email,
                CreatedAt = DateTime.UtcNow
            };

            // Attempt to create the identity user and validate result
            var result = await _userManager.CreateAsync(identityUser, dto.Password);

            if (!result.Succeeded)
                throw new ArgumentException(string.Join(", ", result.Errors.Select(e => e.Description)));

            // 2. Create domain user entity by mapping from DTO
            var domainUser = _mapper.Map<UserModel>(dto);
            // Use the generated Identity user ID for correlation
            domainUser.ID = identityUser.Id;

            // Persist domain user to repository and commit changes
            _userRepository.Add(domainUser);
            await _uow.SaveChangesAsync();

            // 3. Assign the default "User" role to the newly created user
            var roleResult = await _userManager.AddToRoleAsync(identityUser, _authorizationSettings.DefaultRegistrationRole);
            if (!roleResult.Succeeded)
                throw new Exception("Failed to assign role");

            // Commit the transaction to persist all changes atomically
            await transaction.CommitAsync();

            // Return mapped DTO representation of the created user
            return _mapper.Map<UserDTO>(domainUser);
        }
        catch (Exception ex)
        {
            // Rollback the transaction to maintain data consistency
            await transaction.RollbackAsync();

            // Cleanup: Attempt to delete the identity user if it was created before failure
            // This prevents orphaned Identity user records in case of domain user creation failure
            if (identityUser != null && identityUser.Id != 0)
            {
                try
                {
                    await _userManager.DeleteAsync(identityUser);
                }
                catch (Exception cleanupEx)
                {
                    // Log cleanup failure separately to distinguish from primary failure
                    //_logger.LogError(cleanupEx, "Failed to cleanup identity user after registration failure");
                }
            }

            // Log the registration error (commented out, should be enabled with actual logger)
            //_logger.LogError(ex, "Error occurred during user registration");

            // Re-throw the original exception so the API layer can return appropriate error response
            throw;
        }
    }

    // TODO: convert this to Strategy Pattern to allow for different captcha providers in the future.
    public async Task<CaptchaVerificationResponse> VerifyCaptcha(string token, string userIP)
    {
        const string cloudflareURL = "https://challenges.cloudflare.com/turnstile/v0/siteverify";

        FormUrlEncodedContent formData = new(
        [
            // TODO: solve this error:
            //new KeyValuePair<string, string>("secret", _secretKey),
            new KeyValuePair<string, string>("response", token),
            new KeyValuePair<string, string>("remoteip", userIP ?? string.Empty)
        ]);

        HttpResponseMessage response = await _httpClient.PostAsync(cloudflareURL, formData);

        if (!response.IsSuccessStatusCode)
            return new CaptchaVerificationResponse { Success = false, ErrorCodes = ["bad-request"] };

        CaptchaVerificationResponse? captchaResponse = await JsonSerializer.DeserializeAsync<CaptchaVerificationResponse>(await response.Content.ReadAsStreamAsync());

        return captchaResponse ?? new CaptchaVerificationResponse { Success = false, ErrorCodes = ["public-error"] };
    }

    /// <summary>
    /// Revokes a JWT token by adding it to the token blacklist, effectively invalidating it immediately.
    /// </summary>
    /// <param name="token">The JWT token to be revoked. Cannot be null or empty.</param>
    /// <returns>A completed task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method is used for immediate token invalidation during logout or security operations.
    /// The token is blacklisted regardless of its expiration time, preventing its further use.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when the token parameter is null or empty.</exception>
    public async Task RevokeTokenAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
            throw new ArgumentNullException(nameof(token));

        // Delegate to token service to add token to blacklist
        await _tokenService.RevokeTokenAsync(token);
    }

    /// <summary>
    /// Refreshes an expired or expiring JWT token using a valid refresh token.
    /// </summary>
    /// <param name="refreshToken">A valid refresh token issued to the user. Cannot be null or empty.</param>
    /// <returns>
    /// A tuple containing:
    /// - jwt: The new JWT access token
    /// - refreshToken: A new refresh token for future refreshes
    /// </returns>
    /// <remarks>
    /// This method invalidates the current refresh token and issues a new pair of tokens.
    /// The refresh token rotation strategy prevents token reuse attacks and maintains security.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when the refreshToken parameter is null or empty.</exception>
    public async Task<(string jwt, string refreshToken)> TokenRefresher(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            throw new ArgumentNullException(nameof(refreshToken));

        // Request new tokens from the token service
        var (jwt, newRefreshToken) = await _tokenService.RefreshAsync(refreshToken);
        return (jwt, newRefreshToken);
    }

    /// <summary>
    /// Initiates an email change request by sending a confirmation link to the new email address.
    /// </summary>
    /// <param name="userId">The unique identifier of the user requesting the email change.</param>
    /// <param name="newEmail">The new email address to be verified and assigned to the user.</param>
    /// <returns>A completed task representing the asynchronous operation.</returns>
    /// <remarks>
    /// The method performs the following steps:
    /// 1. Validates that the user exists in the identity system
    /// 2. Prevents duplicate email changes (new email must differ from current)
    /// 3. Generates a secure, time-limited confirmation token
    /// 4. Encodes the token for URL safety using Base64Url encoding
    /// 5. Constructs a confirmation link with encoded parameters
    /// 6. Sends the confirmation email to the new address
    /// 
    /// The user must click the confirmation link to complete the email change process.
    /// </remarks>
    /// <exception cref="NotFoundException">Thrown when the user with the specified userId is not found.</exception>
    /// <exception cref="ArgumentException">Thrown when the new email is identical to the current email.</exception>
    public async Task RequestEmailChangeAsync(long userId, string newEmail)
    {
        // Load the Identity user from the database
        var identityUser = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException(nameof(AppIdentityUser), nameof(AppIdentityUser.Id), userId);

        // Ensure new email is different from current email (case-insensitive comparison)
        if ((identityUser.Email ?? string.Empty).Equals(newEmail, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("New email is the same as the current email.");

        // Generate a secure token that will be used to verify the email change request
        var token = await _userManager.GenerateChangeEmailTokenAsync(identityUser, newEmail);
        // Encode the token for safe inclusion in URLs
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        // Encode the new email for safe URL parameter transmission
        var emailEncoded = WebUtility.UrlEncode(newEmail);

        // Construct the confirmation link with all necessary parameters
        var confirmationLink = BuildFrontendUrl(
            _appInfo.EmailChangeConfirmationPath,
            $"userId={userId}&email={emailEncoded}&token={encodedToken}");

        // Prepare email subject and body with clear instructions
        var subject = "Confirm your email change";
        var body = $"Click this link to confirm your email change:\n{confirmationLink}";

        // Send the confirmation email to the new email address
        await _emailService.SendEmail(newEmail, subject, body);
    }

    /// <summary>
    /// Completes the email change process by verifying the token and updating the user's email in both the identity system and domain model.
    /// </summary>
    /// <param name="userId">The unique identifier of the user confirming the email change.</param>
    /// <param name="newEmail">The new email address to be confirmed and assigned.</param>
    /// <param name="encodedToken">The Base64Url-encoded confirmation token sent to the user's new email.</param>
    /// <returns>A completed task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method is transactional and ensures consistency across the identity and domain models:
    /// 1. Retrieves the identity user by ID
    /// 2. Captures the old email address for notification purposes
    /// 3. Decodes the Base64Url-encoded token
    /// 4. Validates and applies the email change in the identity system
    /// 5. Synchronizes the domain model with the new email
    /// 6. Sends a security notification to the old email address asynchronously
    /// 7. Commits all changes atomically
    /// 
    /// If any step fails, the entire operation is rolled back to maintain data integrity.
    /// The notification email to the old address is sent on a separate thread to avoid blocking the response.
    /// </remarks>
    /// <exception cref="NotFoundException">Thrown when the user or domain user is not found.</exception>
    /// <exception cref="ArgumentException">Thrown when the token is invalid or email change fails in the identity system.</exception>
    public async Task ConfirmEmailChangeAsync(long userId, string newEmail, string encodedToken)
    {
        // Begin a database transaction to ensure atomicity of all operations
        await using var _ = await _uow.BeginTransactionAsync();
        try
        {
            // Retrieve the identity user by ID
            var identityUser = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new NotFoundException(nameof(AppIdentityUser), nameof(AppIdentityUser.Id), userId);

            // Store the old email for security notification purposes
            var oldEmail = identityUser.Email;
            // Decode the Base64Url-encoded token to its original string format
            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(encodedToken));
            // Apply the email change in the identity system using the decoded token for verification
            var result = await _userManager.ChangeEmailAsync(identityUser, newEmail, token);

            // Validate that the identity system email change succeeded
            if (!result.Succeeded)
                throw new ArgumentException(string.Join(", ", result.Errors.Select(e => e.Description)));

            // Send security notification to the old email asynchronously (fire and forget)
            // Exceptions in this task are handled internally to avoid blocking the main operation
            var __ = Task.Run(() => NotifyOldEmailChangeAsync(oldEmail, newEmail, userId));

            // Retrieve the domain user model to synchronize email across both systems
            var domainUser = await _userRepository.GetAsync(x => x.ID == userId)
                ?? throw new NotFoundException(nameof(UserModel), nameof(UserModel.ID), userId);

            // Update the domain user's email to match the verified new email
            domainUser.Email = newEmail;

            // Persist the domain user changes to the repository
            _userRepository.Update(domainUser);
            await _uow.SaveChangesAsync();

            // Commit the transaction to atomically apply all changes
            await _uow.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            // Attempt to rollback the transaction if an error occurs
            try
            {
                await _uow.RollbackTransactionAsync();
            }
            catch (Exception rbEx)
            {
                // Log rollback failures separately (logging temporarily commented out)
                //_logger.LogError(rbEx, "Rollback failed for ConfirmEmailChangeAsync for userId {UserId}", userId);
            }

            // Log the email change confirmation failure (logging temporarily commented out)
            //_logger.LogError(ex, "Failed to confirm email change for userId {UserId}", userId);

            // Re-throw the original exception for the API layer to handle
            throw;
        }
    }

    /// <summary>
    /// Sends a security notification to the user's old email address informing them of the email change.
    /// </summary>
    /// <param name="oldEmail">The previous email address of the user. Can be null or empty.</param>
    /// <param name="newEmail">The new email address that has been set on the account.</param>
    /// <param name="userId">The unique identifier of the user, used for reference in the notification.</param>
    /// <returns>A completed task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This is a private helper method that:
    /// 1. Validates that the old email exists and differs from the new email
    /// 2. Constructs a security notification message
    /// 3. Sends the email without throwing exceptions (swallows errors to avoid breaking the main flow)
    /// 
    /// Any exceptions during email sending are caught and logged but not propagated.
    /// This method is safe to call asynchronously without blocking the user's response.
    /// </remarks>
    private async Task NotifyOldEmailChangeAsync(string? oldEmail, string newEmail, long userId)
    {
        // Early exit if old email is null, empty, or identical to the new email
        if (string.IsNullOrEmpty(oldEmail) || string.Equals(oldEmail, newEmail, StringComparison.OrdinalIgnoreCase))
            return;

        try
        {
            // Compose the security notification
            var subject = "Your account email has changed";
            var body = $"The email for the account (ID: #{userId}) has been changed to {newEmail}. " +
                       "If you did not request this change, please contact support immediately.";

            // Send the notification to the old email address
            await _emailService.SendEmail(oldEmail, subject, body);
        }
        catch
        {
            // Silently catch exceptions to prevent disrupting the main email change process
            // Consider logging this in production using a proper logger implementation
        }
    }

    /// <summary>
    /// Initiates a password reset request by sending a reset link to the user's email address.
    /// </summary>
    /// <param name="request">The user's email address or username to identify the account requesting a password reset.</param>
    /// <returns>A completed task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method implements a security best practice by NOT revealing whether a user exists:
    /// 1. Accepts either email or username as identifier
    /// 2. Silently returns if user not found (prevents user enumeration attacks)
    /// 3. Generates a time-limited, cryptographically secure reset token
    /// 4. Encodes the token for URL safety
    /// 5. Constructs a password reset link with encoded parameters
    /// 6. Sends the reset email to the user's registered address
    /// 
    /// The reset link contains a time-limited token that can only be used once.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when the identifier is null or contains only whitespace.</exception>
    public async Task RequestPasswordResetAsync([FromBody] ChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Identifier))
            throw new ArgumentNullException(nameof(request.Identifier));

        // Determine if the identifier is an email or username and find the user accordingly
        AppIdentityUser? user;

        if (request.IdentifierIsEmail)
            // Search for user by email address
            user = await _userManager.FindByEmailAsync(request.Identifier);
        else
            // Search for user by username
            user = await _userManager.FindByNameAsync(request.Identifier);

        // Security: Do NOT reveal whether the user exists to prevent email enumeration attacks
        if (user == null)
            return;

        // Generate a secure, time-limited token for password reset
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        // Encode the token using Base64Url to ensure it's safe for URL transmission
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        // Encode the user's email for safe URL parameter transmission
        var emailEncoded = WebUtility.UrlEncode(user.Email);

        // Construct the password reset link with all necessary encoded parameters
        var resetLink = BuildFrontendUrl(
            _appInfo.PasswordResetPath,
            $"email={emailEncoded}&token={encodedToken}");

        // Prepare and send the password reset email
        var subject = "Reset your password";
        var body = $"Click the link to reset your password:\n{resetLink}";

        await _emailService.SendEmail(user.Email!, subject, body);
    }

    /// <summary>
    /// Completes the password reset process by validating the reset token and updating the user's password.
    /// </summary>
    /// <param name="email">The email address of the user whose password is being reset.</param>
    /// <param name="encodedToken">The Base64Url-encoded password reset token provided in the reset link.</param>
    /// <param name="newPassword">The new password to be set for the user's account.</param>
    /// <returns>A completed task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method performs the following operations:
    /// 1. Retrieves the user by their email address
    /// 2. Decodes the Base64Url-encoded reset token
    /// 3. Validates the token and applies the password change
    /// 4. Revokes all existing refresh tokens for the user (security measure to invalidate all sessions)
    /// 
    /// After a successful password reset, all previous sessions are terminated by revoking their refresh tokens,
    /// ensuring the user must re-authenticate with their new password. This is a critical security measure
    /// to prevent unauthorized access if the account was compromised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when the email parameter is null or contains only whitespace.</exception>
    /// <exception cref="NotFoundException">Thrown when no user with the specified email address is found.</exception>
    /// <exception cref="ArgumentException">Thrown when the reset token is invalid or the password reset operation fails.</exception>
    public async Task ResetPasswordAsync(string email, string encodedToken, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email));

        // Retrieve the user by their email address
        var identityUser = await _userManager.FindByEmailAsync(email)
            ?? throw new NotFoundException(nameof(AppIdentityUser), nameof(AppIdentityUser.Email), email);

        // Decode the Base64Url-encoded token to its original string format
        var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(encodedToken));
        // Apply the password reset using the decoded token for verification
        var result = await _userManager.ResetPasswordAsync(identityUser, token, newPassword);

        // Validate that the password reset succeeded
        if (!result.Succeeded)
            throw new ArgumentException(string.Join(", ", result.Errors.Select(e => e.Description)));

        // Revoke all existing refresh tokens to invalidate all user sessions (security measure)
        // This forces the user to re-authenticate with their new password
        await _tokenService.RevokeAllUserRefreshTokens(identityUser.Id);
    }

    /// <summary>
    /// Soft deletes the specified identity user by anonymizing their email and username, and marking them as deleted.
    /// </summary>
    /// <param name="user">The <see cref="AppIdentityUser"/> to be soft deleted.</param>
    /// <remarks>
    /// This method anonymizes the user's email and username by assigning unique values based on the current UTC timestamp,
    /// sets the <c>IsDeleted</c> flag to <c>true</c>, and records the deletion time in <c>DeletedAt</c>.
    /// This approach preserves referential integrity while ensuring the user's credentials are no longer usable.
    /// </remarks>
    public void SoftDeleteIdentityUser(AppIdentityUser user)
    {
        user.Email = $"deleted_{DateTime.UtcNow:yyyyMMddHHmmssfff}@example.com";
        user.UserName = $"deleted_{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
    }

    private string BuildFrontendUrl(string path, string query)
    {
        var absoluteUri = new Uri(new Uri($"{_appInfo.BaseUrl.TrimEnd('/')}/"), path.TrimStart('/'));
        var uriBuilder = new UriBuilder(absoluteUri)
        {
            Query = query
        };

        return uriBuilder.Uri.ToString();
    }
}
