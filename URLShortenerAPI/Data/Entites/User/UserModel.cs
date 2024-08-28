﻿using URLShortenerAPI.Data.Entites.URL;
using URLShortenerAPI.Data.Entites.URLCategory;

namespace URLShortenerAPI.Data.Entites.User
{
    internal class UserModel
    {
        public int ID { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public string? ResetCode { get; set; }
        public UserType Role { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<URLModel>? URLs { get; set; }
        public ICollection<URLCategoryModel>? URLCategories { get; set; }
    }
    public class UserDTO
    {
        public int ID { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Username { get; set; }
        public UserType Role { get; set; }

        public string? JWToken { get; set; }
        public string? RefreshToken { get; set; }

        public List<URLDTO>? URLs { get; set; }
        public List<CategoryDTO>? Categories { get; set; } // owned categories
    }
    public class UserCreateDTO
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string ConfirmPassword { get; set; }
    }
    public class UserUpdateDTO : UserCreateDTO { public int ID { get; set; } }

    public class UserLoginDTO
    {
        public required string Identifier { get; set; } // could be email or password or phone number.
        public required string Password { get; set; }
    }
    public enum UserType
    {
        Admin,
        ChannelAdmin
    }
}
