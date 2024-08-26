namespace URLShortenerAPI.Data.Entites
{
    internal class UserModel
    {
        public int ID { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public required string Username { get; set; }
        public string? ResetCode { get; set; }
        public UserType Role { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<URLModel>? URLs { get; set; }
        public List<URLCategory>? URLCategories { get; set; }
    }

    public enum UserType
    {
        Admin,
        ChannelAdmin
    }
}
