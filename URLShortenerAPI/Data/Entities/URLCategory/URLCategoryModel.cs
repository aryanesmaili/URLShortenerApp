using URLShortenerAPI.Data.Entities.URL;
using URLShortenerAPI.Data.Entities.User;

namespace URLShortenerAPI.Data.Entities.URLCategory
{
    internal class URLCategoryModel
    {
        public int ID { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }

        public int UserID { get; set; }
        public UserModel? User { get; set; }
        public ICollection<URLModel>? URLs { get; set; }
    }
    public class CategoryDTO
    {
        public int ID { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }

        public int UserID { get; set; }
        public List<URLDTO>? URLs { get; set; }
    }
}
