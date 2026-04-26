using URLShortener.Domain.Entities.Finance;
using URLShortener.Domain.Entities.URL;
using URLShortener.Domain.Entities.URLCategory;

namespace URLShortener.Domain.Entities.User;

public sealed class UserModel
{
    public long ID { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Username { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<URLModel>? URLs { get; set; } = [];
    public ICollection<URLCategoryModel>? URLCategories { get; set; } = [];

    public long FinancialID { get; set; }
    public required FinancialRecordModel FinancialRecord { get; set; }

    public void SoftDelete()
    {
        if (IsDeleted)
            return;
        Email = $"deleted_{DateTime.UtcNow:yyyyMMddHHmmssfff}@example.com";
        Username = $"deleted_{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
}