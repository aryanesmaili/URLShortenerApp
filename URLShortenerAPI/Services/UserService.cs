using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pexita.Utility.Exceptions;
using URLShortenerAPI.Data;
using URLShortenerAPI.Data.Entites.User;
using URLShortenerAPI.Services.Interfaces;

namespace URLShortenerAPI.Services
{
    internal class UserService(AppDbContext context, IAuthService authorizationService, IMapper mapper, IEmailService emailService) : IUserService
    {
        private readonly AppDbContext _context = context;
        private readonly IAuthService _authService = authorizationService;
        private readonly IMapper _mapper = mapper;
        private readonly IEmailService _emailService = emailService;

        /// <summary>
        /// gets a user's info by their ID asynchronously.
        /// </summary>
        /// <param name="id"></param>
        /// <returns> an object containing showable user info</returns>
        /// <exception cref="NotFoundException"></exception>
        public async Task<UserDTO> GetUserByIDAsync(int id)
        {
            UserModel? user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.ID == id) ?? throw new NotFoundException($"User {id} Does not Exist");

            return UserModelToDTO(user);
        }

        /// <summary>
        /// gets a user's info alongside the nested objects.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        public async Task<UserDTO> GetFullUserInfo(int id)
        {
            UserModel? user = await _context.Users
                .Include(u => u.URLs)!
                .ThenInclude(u => u.Category)
                .Include(u=>u.URLs)!
                .ThenInclude(u => u.URLAnalytics)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ID == id) 
                ?? throw new NotFoundException($"User {id} Does not Exist");
            return UserModelToDTO(user);
        }

        /// <summary>
        /// gets a user's info by their Username asynchronously.
        /// </summary>
        /// <param name="Username"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        public async Task<UserDTO> GetUserByUsernameAsync(string Username)
        {
            UserModel? user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Username == Username) ?? throw new NotFoundException($"User {Username} Does not Exist");

            return UserModelToDTO(user);
        }

        /// <summary>
        /// logs a user in and gives them respective tokens to surf across webpages.
        /// </summary>
        /// <param name="info">user login info</param>
        /// <returns>a <see cref="UserInfoDTO"/> object containing information.</returns>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="NotAuthorizedException"></exception>
        public async Task<UserDTO> LoginUserAsync(UserLoginDTO info)
        {
            UserModel? user = null;
            if (info.Identifier.IsEmail())

                user = await _context.Users.FirstOrDefaultAsync(x => x.Email == info.Identifier) ?? throw new NotFoundException($"No user with email {info.Identifier} exists.");
            else
                user = await _context.Users.FirstOrDefaultAsync(u => u.Username == info.Identifier) ?? throw new NotFoundException($"No user with username {info.Identifier} exists.");

            if (user == null || !BCrypt.Net.BCrypt.Verify(info.Password, user?.PasswordHash))
                throw new ArgumentException("Username or Password is not correct");

            UserDTO result = UserModelToDTO(user!);
            result.JWToken = _authService.GenerateJWToken(user!.Username, user.Role.ToString(), user.Email);
            string rawRefreshToken = _authService.GenerateRefreshToken();
            RefreshToken refreshToken = new()
            {
                Token = rawRefreshToken,
                User = user,
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                UserId = user.ID
            };
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
            result.RefreshToken = _mapper.Map<RefreshTokenDTO>(refreshToken);
            return result;
        }

        /// <summary>
        /// registers a new user.
        /// </summary>
        /// <param name="userCreateDTO">object containing information about the new user.</param>
        /// <returns>a <see cref="UserInfoDTO"/> object containing information about the new user.</returns>
        public async Task<UserDTO> RegsiterUserAsync(UserCreateDTO newUserInfo)
        {
            UserModel newUser = _mapper.Map<UserModel>(newUserInfo);
            newUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newUserInfo.Password); // Hashing user's password to ensure security

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return UserModelToDTO(newUser);
        }

        /// <summary>
        /// begins a Change password procedure for the user.
        /// </summary>
        /// <param name="identifier">user's input that can be either email or username.</param>
        /// <returns>a <see cref="UserDTO"/> object containing Info. </returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NotFoundException"></exception>
        public async Task<UserDTO> ResetPasswordAsync(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                throw new ArgumentNullException(identifier);

            UserModel? user;

            if (identifier.IsEmail()) // if the user has entered an email:
                user = await _context.Users.FirstOrDefaultAsync(u => u.Email == identifier); // we search by email
            else // if it's not an email then the user has entered their username
                user = await _context.Users.FirstOrDefaultAsync(user => user.Username == identifier); // we search by username

            if (user == null) // if no user exists with that email/username:
                throw new NotFoundException($"User {identifier} does not exist.");

            user.ResetCode = _authService.GenerateRandomPassword(8); // we generate a reset password code for them,
            string Subject = "Pexita Authentication code";
            string Body = $"Your Authentication Code Is {user.ResetCode}";
            _emailService.SendEmail(user.Email, Subject, Body); // we send the code to the user.

            _context.Update(user);
            await _context.SaveChangesAsync();

            return UserModelToDTO(user);
        }

        /// <summary>
        /// changes a user's password after making sure they're valid.
        /// </summary>
        /// <param name="userInfo">the user whose password is going to change.</param>
        /// <param name="NewPassword"></param>
        /// <param name="ConfirmPassword"></param>
        /// <param name="requestingUsername">the user requesting the change.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public async Task<UserDTO> ChangePasswordAsync(UserDTO userInfo, string NewPassword, string ConfirmPassword, string requestingUsername)
        {
            if (NewPassword.IsNullOrEmpty() || ConfirmPassword.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(NewPassword));
            else if (NewPassword != ConfirmPassword)
                throw new ArgumentException($"Entered values {NewPassword} and {ConfirmPassword} Do not match.");

            // checking if the user has the authorization to access this.
            UserModel user = await _authService.AuthorizeUserAccessAsync(userInfo.ID, requestingUsername);
            string hashedpassword = BCrypt.Net.BCrypt.HashPassword(NewPassword);
            user.PasswordHash = hashedpassword;
            user.ResetCode = null;
            _context.Update(user);
            await _context.SaveChangesAsync();
            return UserModelToDTO(user, userInfo.RefreshToken!, userInfo.JWToken!);
        }

        /// <summary>
        /// checks if the given code matches the one in Database.
        /// </summary>
        /// <param name="userID">user whom we want to edit.</param>
        /// <param name="Code">the ResetCode. entered by user.</param>
        /// <returns>a <see cref="UserInfoDTO"/> object containing tokens. the user is verified after this.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NotFoundException"></exception>
        public async Task<UserDTO> CheckResetCode(UserDTO user, string Code)
        {
            if (Code.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(Code));

            UserModel userRec = await _context.Users.FirstOrDefaultAsync(u => u.ID == user.ID)
                ?? throw new NotFoundException($"User {user.ID} does not exist.");

            string ResetCode = userRec.ResetCode ?? throw new ArgumentNullException("ResetCode");

            if (ResetCode != Code)
                throw new ArgumentException("Code is Wrong.");

            var result = UserModelToDTO(userRec);
            string token = _authService.GenerateJWToken(userRec.Username, userRec.Role.ToString(), userRec.Email);
            string refToken = _authService.GenerateRefreshToken();
            RefreshToken refreshToken = new()
            {
                Token = refToken,
                User = userRec,
                UserId = userRec.ID,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            result.RefreshToken = _mapper.Map<RefreshTokenDTO>(refreshToken);
            result.JWToken = token;
            return result;
        }

        /// <summary>
        /// revokes a user's refresh token on their logout
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NotFoundException"></exception>
        public async Task RevokeToken(string token)
        {
            if (token == null)
                throw new ArgumentNullException(token);

            var tokenRecord = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token) ?? throw new NotFoundException();
            if (tokenRecord != null && tokenRecord.IsActive)
            {
                tokenRecord.Revoked = DateTime.UtcNow;
                _context.RefreshTokens.Update(tokenRecord);
                await _context.SaveChangesAsync();
                return;
            }
            throw new Exception("token either invalid or already inactive.");
        }

        /// <summary>
        /// Generates a fresh JWToken for the user given the refreshToken.
        /// </summary>
        /// <param name="refreshToken">the string containing user's given refreshToken.</param>
        /// <returns>an object containing fresh JWToken.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NotFoundException"></exception>
        public async Task<UserDTO> TokenRefresher(string refreshToken)
        {
            if (refreshToken.IsNullOrEmpty())
                throw new ArgumentNullException(refreshToken);

            RefreshToken? currentRefreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
            if (currentRefreshToken == null || !currentRefreshToken.IsActive)
                throw new NotFoundException($"token {refreshToken} is not valid.");
            UserModel user = await _context.Users.FindAsync(currentRefreshToken.UserId) ?? throw new NotFoundException($"User {currentRefreshToken.UserId} Does not exist");

            var result = UserModelToDTO(user);
            // Generating both new JWToken and RefreshToken
            var newRefreshTokenStr = _authService.GenerateRefreshToken();
            result.JWToken = _authService.GenerateJWToken(user.Username, user.Role.ToString(), user.Email);

            // Revoking the current token that the user had.
            currentRefreshToken.Revoked = DateTime.UtcNow;
            _context.RefreshTokens.Update(currentRefreshToken);
            // Creating the new Refresh token object for the user.
            RefreshToken newToken = new RefreshToken()
            {
                Token = newRefreshTokenStr,
                User = user,
                UserId = user.ID,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            _context.RefreshTokens.Add(newToken);
            result.RefreshToken = _mapper.Map<RefreshTokenDTO>(newToken);

            await _context.SaveChangesAsync();
            return result;
        }

        /// <summary>
        /// updates a user's cred in database.
        /// </summary>
        /// <param name="newUserInfo">new information and changes.</param>
        /// <param name="requestingUsername">the username requesting the change.</param>
        /// <returns>a <see cref="UserDTO"/> object containing new record's info.</returns>
        /// <exception cref="NotFoundException"></exception>
        public async Task<UserDTO> UpdateUserInfoAsync(UserUpdateDTO newUserInfo, string requestingUsername)
        {
            UserModel user = await _authService.AuthorizeUserAccessAsync(newUserInfo.ID, requestingUsername);

            user = _mapper.Map(newUserInfo, user);

            _context.Update(user);
            await _context.SaveChangesAsync();

            return UserModelToDTO(user);
        }

        /// <summary>
        /// Deletes a user account from database.
        /// </summary>
        /// <param name="id">ID of the user to be deleted.</param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        public async Task DeleteUser(int id)
        {
            UserModel user = await _context.Users.FindAsync(id) ?? throw new NotFoundException($"User {id} Does not Exist");
            _context.Remove(user);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Maps a UserModel database record to a representable object.
        /// </summary>
        /// <param name="user">the database record.</param>
        /// <returns>a <see cref="UserInfoDTO"/> object containing information.</returns>
        public UserDTO UserModelToDTO(UserModel user)
        {
            return _mapper.Map<UserDTO>(user);
        }

        /// <summary>
        /// Maps a UserModel database record to a representable object.
        /// </summary>
        /// <param name="user">the database record.</param>
        /// <param name="refreshToken">RefreshToken of the user.</param>
        /// <param name="AccessToken">JWToken given to user to authenticate their requests.</param>
        /// <returns>a <see cref="UserDTO"/> object containing information.</returns>
        public UserDTO UserModelToDTO(UserModel user, RefreshTokenDTO refreshToken, string AccessToken)
        {
            var result = _mapper.Map<UserDTO>(user);
            result.RefreshToken = refreshToken;
            result.JWToken = AccessToken;
            return result;
        }

        /// <summary>
        /// checks whether a given user is an admin.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>True if is admin, false otherwise.</returns>
        /// <exception cref="NotFoundException"></exception>
        public bool IsAdmin(int id)
        {
            return (_context.Users.AsNoTracking().FirstOrDefault(x => x.ID == id)
                ?? throw new NotFoundException($"User {id} Does not Exist")).Role == UserType.Admin;
        }

        /// <summary>
        /// checks whether a given user is an admin.
        /// </summary>
        /// <param name="username"></param>
        /// <returns>True if is admin, false otherwise.</returns>
        /// <exception cref="NotFoundException"></exception>
        public bool IsAdmin(string username)
        {
            return (_context.Users.AsNoTracking().FirstOrDefault(x => x.Username == username)
                ?? throw new NotFoundException($"User {username} Does not Exist")).Role == UserType.Admin;
        }

        /// <summary>
        /// checks if a given id is a valid user.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsUser(int id)
        {
            return _context.Users.AsNoTracking().Any(x => x.ID == id);
        }

        /// <summary>
        /// checks if a given username is a valid user.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public bool IsUser(string username)
        {
            return _context.Users.AsNoTracking().Any(x => x.Username == username);
        }

        /// <summary>
        /// checks if a given email is already used.
        /// </summary>
        /// <param name="email"></param>
        /// <returns>True if in use, false otherwise.</returns>
        public bool IsEmailTaken(string email)
        {
            return _context.Users.AsNoTracking().Any(x => x.Email == email);
        }
    }
}
