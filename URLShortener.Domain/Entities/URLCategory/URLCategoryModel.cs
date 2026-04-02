using URLShortener.Domain.Entities.URL;
using URLShortener.Domain.Entities.User;

namespace URLShortener.Domain.Entities.URLCategory;

public sealed class URLCategoryModel
{
    public long ID { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }

    public long UserID { get; set; }
    public UserModel? User { get; set; }
    public ICollection<URLModel>? URLs { get; set; }
}
