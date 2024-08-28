using URLShortenerAPI.Data.Entites.URL;
using URLShortenerAPI.Data.Entites.User;

namespace URLShortenerAPI.Data.Entites.URLCategory
{
    internal class URLCategoryModel
    {
        public int ID { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }

        public int UserID { get; set; }
        public required UserModel User { get; set; }
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
