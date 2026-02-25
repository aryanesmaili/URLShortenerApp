using URLShortener.Domain.Entities.Finance;
using URLShortener.Domain.Entities.URL;
using URLShortener.Domain.Entities.URLCategory;
using URLShortener.Domain.Enums;

namespace URLShortener.Domain.Entities.User;

public class UserModel
{
    public int ID { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public string? PasswordResetCode { get; set; }
    public string? EmailResetCode { get; set; }
    public UserType Role { get; set; } = UserType.ChannelAdmin;
    public DateTime CreatedAt { get; set; }

    public ICollection<URLModel>? URLs { get; set; }
    public ICollection<URLCategoryModel>? URLCategories { get; set; }
    public ICollection<RefreshToken>? RefreshTokens { get; set; }

    public int FinancialID { get; set; }
    public required FinancialRecord FinancialRecord { get; set; }
}