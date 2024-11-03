using SharedDataModels.DTOs;
using URLShortenerAPI.Data.Entities.URL;
using URLShortenerAPI.Data.Entities.URLCategory;

namespace URLShortenerAPI.Data.Entities.User
{
    internal class UserModel
    {
        public int ID { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public string? PasswordResetCode { get; set; }
        public string? EmailResetCode { get; set; }
        public UserType Role { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<URLModel>? URLs { get; set; }
        public ICollection<URLCategoryModel>? URLCategories { get; set; }
        public ICollection<RefreshToken>? RefreshTokens { get; set; }
    }

    internal class RefreshToken
    {
        public int Id { get; set; }
        public required string Token { get; set; }
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Revoked { get; set; }
        public bool IsActive => Revoked == null && !IsExpired;
        public bool IsExpired => DateTime.UtcNow >= Expires;

        public required int UserId { get; set; }
        public required UserModel User { get; set; }
    }

}
