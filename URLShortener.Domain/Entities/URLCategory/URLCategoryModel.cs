using URLShortener.Domain.Entities.URL;
using URLShortener.Domain.Entities.User;

namespace URLShortener.Domain.Entities.URLCategory;

public class URLCategoryModel
{
    public int ID { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }

    public int UserID { get; set; }
    public UserModel? User { get; set; }
    public ICollection<URLModel>? URLs { get; set; }
}
