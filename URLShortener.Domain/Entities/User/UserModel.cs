using URLShortener.Domain.Entities.Finance;
using URLShortener.Domain.Entities.URL;
using URLShortener.Domain.Entities.URLCategory;
using URLShortener.Domain.Enums;

namespace URLShortener.Domain.Entities.User;

public sealed class UserModel
{
    public long ID { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public string? PasswordResetCode { get; set; }
    public string? EmailResetCode { get; set; }
    public UserType Role { get; set; } = UserType.ChannelAdmin;
    public DateTime CreatedAt { get; set; }

    public ICollection<URLModel>? URLs { get; set; } = [];
    public ICollection<URLCategoryModel>? URLCategories { get; set; } = [];
    public ICollection<RefreshToken>? RefreshTokens { get; set; }

    public long FinancialID { get; set; }
    public required FinancialRecordModel FinancialRecord { get; set; }
}