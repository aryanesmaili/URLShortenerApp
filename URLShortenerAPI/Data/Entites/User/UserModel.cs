using URLShortenerAPI.Data.Entites.URL;
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
        public List<CategoryDTO>? Categories { get; set; }
    }
    public enum UserType
    {
        Admin,
        ChannelAdmin
    }
}
