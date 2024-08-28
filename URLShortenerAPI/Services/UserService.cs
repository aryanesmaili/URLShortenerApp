using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Pexita.Utility.Exceptions;
using URLShortenerAPI.Data;
using URLShortenerAPI.Data.Entites.User;
using URLShortenerAPI.Services.Interfaces;

namespace URLShortenerAPI.Services
{
    internal class UserService(AppDbContext context, IAuthorizationService authorizationService, IMapper mapper) : IUserService
    {
        private readonly AppDbContext _context = context;
        private readonly IAuthorizationService _authorizationService = authorizationService;
        private readonly IMapper _mapper = mapper;

        public async Task<UserDTO> GetUserByIDAsync(int id)
        {
            UserModel? user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.ID == id) ?? throw new NotFoundException($"User {id} Does not Exist");

            return UserModelToDTO(user);
        }

        public async Task<UserDTO> GetUserByUsernameAsync(string Username)
        {
            UserModel? user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Username == Username) ?? throw new NotFoundException($"User {Username} Does not Exist");

            return UserModelToDTO(user);
        }

        public async Task<UserDTO> LoginUserAsync(UserLoginDTO user)
        {
            throw new NotImplementedException();
        }

        public async Task<UserDTO> RegsiterUserAsync(UserCreateDTO newUserInfo)
        {
            UserModel newUser = _mapper.Map<UserModel>(newUserInfo);
            newUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newUserInfo.Password); // Hashing user's password to ensure security

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return UserModelToDTO(newUser);
        }

        public async Task<UserDTO> ResetPasswordAsync(string Identifier)
        {
            throw new NotImplementedException();
        }

        public async Task<UserDTO> ChangePasswordAsync(UserDTO user, string newPassword, string ConfirmPassword, string requestingUsername)
        {
            throw new NotImplementedException();
        }

        public async Task RevokeToken(string token)
        {
            throw new NotImplementedException();
        }

        public async Task<UserDTO> TokenRefresher(string refreshToken)
        {
            throw new NotImplementedException();
        }

        public async Task<UserDTO> UpdateUserInfoAsync(UserUpdateDTO newUserInfo, string requestingUsername)
        {
            UserModel user = await _authorizationService.AuthorizeUserAccessAsync(newUserInfo.ID, requestingUsername);

            user = _mapper.Map(newUserInfo, user);

            _context.Update(user);
            await _context.SaveChangesAsync();

            return UserModelToDTO(user);
        }

        public async Task DeleteUser(int id)
        {
            UserModel user = await _context.Users.FindAsync(id) ?? throw new NotFoundException($"User {id} Does not Exist");
            _context.Remove(user);
            await _context.SaveChangesAsync();
        }

        public UserDTO UserModelToDTO(UserModel user)
        {
            return _mapper.Map<UserDTO>(user);
        }

        public bool IsAdmin(int id)
        {
            return (_context.Users.AsNoTracking().FirstOrDefault(x => x.ID == id)
                ?? throw new NotFoundException($"User {id} Does not Exist")).Role == UserType.Admin;
        }

        public bool IsAdmin(string username)
        {
            return (_context.Users.AsNoTracking().FirstOrDefault(x => x.Username == username)
                ?? throw new NotFoundException($"User {username} Does not Exist")).Role == UserType.Admin;
        }


        public bool IsUser(int id)
        {
            return _context.Users.AsNoTracking().Any(x => x.ID == id);
        }

        public bool IsUser(string username)
        {
            return _context.Users.AsNoTracking().Any(x => x.Username == username);
        }
        
        public bool IsEmailTaken(string email)
        {
            return _context.Users.AsNoTracking().Any(x => x.Email == email);
        }
    }
}
