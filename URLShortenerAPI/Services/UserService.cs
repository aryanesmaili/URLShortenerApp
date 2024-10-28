using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SharedDataModels.CustomClasses;
using SharedDataModels.DTOs;
using URLShortenerAPI.Data;
using URLShortenerAPI.Data.Entities.ClickInfo;
using URLShortenerAPI.Data.Entities.User;
using URLShortenerAPI.Services.Interfaces;
using URLShortenerAPI.Utility.Exceptions;

namespace URLShortenerAPI.Services
{
    internal class UserService(AppDbContext context, IAuthService authorizationService, IMapper mapper, IEmailService emailService, ICacheService cacheService) : IUserService
    {
        private readonly AppDbContext _context = context;
        private readonly IAuthService _authService = authorizationService;
        private readonly IMapper _mapper = mapper;
        private readonly IEmailService _emailService = emailService;
        private readonly ICacheService _cacheService = cacheService;

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
        public async Task<UserDTO> GetFullUserInfoAsync(int id)
        {
            UserModel? user = await _context.Users
                .Include(u => u.URLs)!
                .ThenInclude(u => u.Categories)
                .Include(u => u.URLs)!
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
        /// used in pagination. gives the records required to be shown in a table.
        /// </summary>
        /// <param name="userID">ID of the user whose records we're loading.</param>
        /// <param name="pageNumber">number of the page to retrieve.</param>
        /// <param name="pageSize">Count of elements in each page.</param>
        /// <param name="reqUsername">username asking the operation.</param>
        /// <returns></returns>
        public async Task<PagedResult<URLDTO>> GetPagedResult(int userID, int pageNumber, int pageSize, string reqUsername)
        {
            await _authService.AuthorizeURLsAccessAsync(userID, reqUsername);

            // get the total of URLs a user has shortened.
            var totalCount = await _context.URLs.CountAsync(x => x.UserID == userID);
            // Fetch the paginated URLs
            var URLs = await _context.URLs
                .Where(u => u.UserID == userID)
                .OrderByDescending(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Include(x => x.Categories)
                .Take(pageSize)
                .ToListAsync();

            var result = URLs.Select(_mapper.Map<URLDTO>).ToList();
            return new PagedResult<URLDTO>
            {
                Items = result,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        /// <summary>
        /// Gets the info required for dashboard from database.
        /// </summary>
        /// <param name="userID">ID Of the user</param>
        /// <param name="reqUsername">the username requesting the data.</param>
        /// <returns>a <see cref="UserDashboardDTO"/> object containing dashboard info.</returns>
        public async Task<UserDashboardDTO> GetDashboardByIDAsync(int userID, string reqUsername)
        {
            await _authService.AuthorizeUserAccessAsync(userID, reqUsername);

            List<URLDTO> recentURLs = await GetRecentURLs(userID) ?? [];
            Dictionary<string, int> ClicksThisMonth = await GetClicksInMonth(userID) ?? [];
            Dictionary<string, int> ClicksThisDay = await GetClicksInDay(userID) ?? [];
            List<string>? topCountries = await GetTopCountries(userID) ?? [];
            List<string>? topDevices = await GetTopDeviceOS(userID) ?? [];

            UserDashboardDTO result = new()
            {
                DailyChartData = ClicksThisDay,
                MonthlyChartData = ClicksThisMonth,
                MostRecentURLs = recentURLs,
                TopCountries = topCountries,
                TopDevices = topDevices
            };

            return result;
        }
        /// <summary>
        /// Gets the most recent URLs of user.
        /// </summary>
        /// <param name="userID">ID of the user</param>
        /// <returns></returns>
        private async Task<List<URLDTO>?> GetRecentURLs(int userID)
        {
            // Asynchronously retrieve recent URLs
            List<URLDTO> recentURLs = await _context.URLs
                .Where(x => x.UserID == userID)
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .Select(url => _mapper.Map<URLDTO>(url))
                .ToListAsync();

            return recentURLs;
        }
        /// <summary>
        /// Gets the 5 top countries the user's urls are clicked from.
        /// </summary>
        /// <param name="userID">ID of the user</param>
        /// <returns></returns>
        private async Task<List<string>?> GetTopCountries(int userID)
        {
            // retrieve from cache
            List<string>? topLocations = await _cacheService.GetValueAsync<List<string>>("TopCountry_" + userID.ToString());
            if (topLocations != null)
                return topLocations;

            List<LocationInfo> countries = await _context.Clicks
                .Include(x => x.URL)
                .AsNoTracking()
                .Where(x => x.URL.UserID == userID)
                .Select(x => x.PossibleLocation)
                .ToListAsync();

            topLocations = countries
                .GroupBy(x => x.Country)
                .OrderByDescending(x => x.Count())
                .Select(x => x.Key)
                .Take(5)
                .ToList();

            if (topLocations != null)
                await _cacheService.SetAsync("TopCountry_" + userID.ToString(), topLocations);
            return topLocations;
        }
        /// <summary>
        /// Gets the 5most used OS by the users.
        /// </summary>
        /// <param name="userID">ID of the user</param>
        /// <returns></returns>
        private async Task<List<string>?> GetTopDeviceOS(int userID)
        {
            List<string>? topUsers = await _cacheService.GetValueAsync<List<string>>("TopUser_" + userID.ToString());
            if (topUsers != null)
                return topUsers;

            List<DeviceInfo> topDevices = await _context.Clicks
                .Include(x => x.URL)
                .AsNoTracking()
                .Where(c => c.URL.UserID == userID)
                .Select(x => x.DeviceInfo)
                .ToListAsync();

            topUsers = topDevices
                        .Where(x => !string.IsNullOrWhiteSpace(x?.OS))
                        .GroupBy(x => x?.OS ?? "")
                        .OrderByDescending(x => x.Count())
                        .Select(x => x.Key)
                        .Take(5)
                        .ToList();

            if (topUsers != null)
                await _cacheService.SetAsync("TopUser_" + userID.ToString(), topUsers);
            return topUsers;
        }

        /// <summary>
        /// Gets the statistics about the times that user's urls are more likely to be clicked.
        /// </summary>
        /// <param name="userID">ID of the user</param>
        /// <returns></returns>
        private async Task<Dictionary<string, int>?> GetClicksInDay(int userID)
        {
            // try to retrieve from cache first
            Dictionary<string, int>? ClicksInDay = await _cacheService.GetValueAsync<Dictionary<string, int>>("D_Clicks_" + userID.ToString());
            if (ClicksInDay != null)
                return ClicksInDay;

            // if not, we query database.
            ClicksInDay = await _context.Clicks.Include(x => x.URL).AsNoTracking().Where(x => x.URL.UserID == userID).GroupBy(x => x.ClickedAt.Hour).ToDictionaryAsync(g => g.Key.ToString(), g => g.Count());

            // cache in Redis
            if (ClicksInDay != null)
                await _cacheService.SetAsync("D_Clicks_" + userID.ToString(), ClicksInDay);

            return ClicksInDay;
        }

        /// <summary>
        /// Gets the statistics about the clicks on current month.
        /// </summary>
        /// <param name="userID">ID of the user</param>
        /// <returns></returns>
        private async Task<Dictionary<string, int>?> GetClicksInMonth(int userID)
        {
            // first we try to retrieve from cache
            Dictionary<string, int>? clicksinMonth = await _cacheService.GetValueAsync<Dictionary<string, int>>("M_Clicks_" + userID.ToString());
            if (clicksinMonth != null)
                return clicksinMonth;

            var currentDate = DateTime.Now;
            int daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);

            // Initialize dictionary with all days of the month set to 0
            clicksinMonth = Enumerable.Range(1, daysInMonth)
                .ToDictionary(day => day.ToString(), day => 0);

            // Fetch and count clicks by day
            var clickCounts = await _context.Clicks
                .Include(x => x.URL)
                .AsNoTracking()
                .Where(x => x.URL.UserID == userID && x.ClickedAt.Month == currentDate.Month && x.ClickedAt.Year == currentDate.Year)
                .GroupBy(x => x.ClickedAt.Day)
                .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count());

            // Update the initialized dictionary with actual counts
            foreach (var (day, count) in clickCounts)
            {
                clicksinMonth[day] = count;
            }

            if (clicksinMonth != null)
                await _cacheService.SetAsync("M_Clicks_" + userID.ToString(), clicksinMonth, DateTime.Now.AddDays(1) - DateTime.Now);

            return clicksinMonth;
        }

        /// <summary>
        /// logs a user in and gives them respective tokens to surf across webpages.
        /// </summary>
        /// <param name="info">user login info</param>
        /// <returns>a <see cref="UserInfoDTO"/> object containing information.</returns>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="NotAuthorizedException"></exception>
        public async Task<UserLoginResponse> LoginUserAsync(UserLoginDTO info)
        {
            UserModel? user = null;
            if (info.Identifier.IsEmail())

                user = await _context.Users.FirstOrDefaultAsync(x => x.Email == info.Identifier) ?? throw new NotFoundException($"No user with email {info.Identifier} exists.");
            else
                user = await _context.Users.FirstOrDefaultAsync(u => u.Username == info.Identifier) ?? throw new NotFoundException($"No user with username {info.Identifier} exists.");

            if (user == null || !BCrypt.Net.BCrypt.Verify(info.Password, user?.PasswordHash))
                throw new ArgumentException("Username or Password is not correct");

            UserDTO userDTO = UserModelToDTO(user!);
            userDTO.JWToken = _authService.GenerateJWToken(user!.Username, user.Role.ToString(), user.Email);
            string rawRefreshToken = _authService.GenerateRefreshToken();
            RefreshToken refreshToken = new()
            {
                Token = rawRefreshToken,
                User = user,
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                UserId = user.ID
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            RefreshTokenDTO refreshTokenDTO = _mapper.Map<RefreshTokenDTO>(refreshToken);
            UserLoginResponse response = new()
            { User = userDTO, RefreshToken = refreshTokenDTO };

            return response;
        }

        /// <summary>
        /// registers a new user.
        /// </summary>
        /// <param name="userCreateDTO">object containing information about the new user.</param>
        /// <returns>a <see cref="UserInfoDTO"/> object containing information about the new user.</returns>
        public async Task<UserDTO> RegisterUserAsync(UserCreateDTO newUserInfo)
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

            _context.Update(user);
            await _context.SaveChangesAsync();

            await _emailService.SendEmail(user.Email, Subject, Body); // we send the code to the user.

            return UserModelToDTO(user);
        }

        /// <summary>
        /// checks if the given code matches the one in Database.
        /// </summary>
        /// <param name="userID">user whom we want to edit.</param>
        /// <param name="Code">the ResetCode. entered by user.</param>
        /// <returns>a <see cref="UserInfoDTO"/> object containing tokens. the user is verified after this.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NotFoundException"></exception>
        public async Task<UserLoginResponse> CheckResetCodeAsync(string identifier, string Code)
        {
            if (Code.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(Code));

            UserModel? userRec;

            if (identifier.IsEmail()) // if the user has entered an email:
                userRec = await _context.Users.FirstOrDefaultAsync(u => u.Email == identifier); // we search by email
            else // if it's not an email then the user has entered their username
                userRec = await _context.Users.FirstOrDefaultAsync(user => user.Username == identifier); // we search by username

            string ResetCode = userRec!.ResetCode ?? throw new ArgumentNullException("ResetCode");

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

            result.JWToken = token;
            RefreshTokenDTO refreshTokenDTO = _mapper.Map<RefreshTokenDTO>(refreshToken);

            UserLoginResponse response = new()
            { User = result, RefreshToken = refreshTokenDTO };
            return response;
        }

        /// <summary>
        /// changes a user's password after making sure they're valid.
        /// </summary>
        /// <param name="reqInfo">required information for changing the password.</param>
        /// <param name="requestingUsername">the user requesting the change.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public async Task<UserDTO> ChangePasswordAsync(ChangePasswordRequest reqInfo, string requestingUsername)
        {
            if (reqInfo.NewPassword.IsNullOrEmpty() || reqInfo.ConfirmPassword.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(reqInfo.NewPassword));
            else if (reqInfo.NewPassword != reqInfo.ConfirmPassword)
                throw new ArgumentException($"Entered values {reqInfo.NewPassword} and {reqInfo.ConfirmPassword} Do not match.");

            // checking if the user has the authorization to access this.
            UserModel user = await _authService.AuthorizeUserAccessAsync(reqInfo.UserInfo.ID, requestingUsername);
            string hashedpassword = BCrypt.Net.BCrypt.HashPassword(reqInfo.NewPassword);
            if (hashedpassword == user.PasswordHash)
                throw new ArgumentException("input password is no different from the current password.");
            user.PasswordHash = hashedpassword;
            user.ResetCode = null;
            _context.Update(user);
            await _context.SaveChangesAsync();

            return UserModelToDTO(user, reqInfo.UserInfo.JWToken!);
        }

        /// <summary>
        /// revokes a user's refresh token on their logout
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NotFoundException"></exception>
        public async Task RevokeTokenAsync(string token)
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
        public async Task<string> TokenRefresher(string refreshToken)
        {
            if (refreshToken.IsNullOrEmpty())
                throw new ArgumentNullException(refreshToken);

            RefreshToken? currentRefreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);

            if (currentRefreshToken == null)
                throw new NotFoundException($"token {refreshToken} is not valid.");

            else if (!currentRefreshToken.IsActive)
                throw new RefreshTokenExpiredException("RefreshToken is Expired");

            UserModel user = await _context.Users.FindAsync(currentRefreshToken.UserId) ?? throw new NotFoundException($"User {currentRefreshToken.UserId} Does not exist");

            // Generating both new JWToken and RefreshToken
            string jwToken = _authService.GenerateJWToken(user.Username, user.Role.ToString(), user.Email);

            return jwToken;
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
            UserModel temp = user;

            user = _mapper.Map(newUserInfo, user);
            _context.Update(user);
            await _context.SaveChangesAsync();

            UserDTO userDTO = UserModelToDTO(user);
            if (temp.Username != newUserInfo.Username || temp.Email != newUserInfo.Email)
            {
                // username or email has changed, thus the user needs new tokens.
                string jwt = _authService.GenerateJWToken(user.Username, user.Role.ToString(), user.Email);
                userDTO.JWToken = jwt;
            }
            return userDTO;
        }

        /// <summary>
        /// Deletes a user account from database.
        /// </summary>
        /// <param name="id">ID of the user to be deleted.</param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        public async Task DeleteUserAsync(int id)
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
        private UserDTO UserModelToDTO(UserModel user)
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
        private UserDTO UserModelToDTO(UserModel user, string AccessToken)
        {
            var result = _mapper.Map<UserDTO>(user);
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
