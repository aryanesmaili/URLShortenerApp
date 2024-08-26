namespace URLShortenerAPI.Data.Entites
{
    internal class URLCategory
    {
        public int ID { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }

        public int UserID { get; set; }
        public required UserModel User { get; set; }
        public List<URLModel>? URLs { get; set; }
    }
}
