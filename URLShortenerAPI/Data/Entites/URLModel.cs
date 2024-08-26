namespace URLShortenerAPI.Data.Entites
{
    internal class URLModel
    {
        public int ID { get; set; }
        public string? Description { get; set; }
        public required string ShortCode { get; set; }
        public required string LongURL { get; set; }
        public int ClickCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }


        public int UserID { get; set; }
        public required UserModel User { get; set; }
        public List<ClickInfoModel>? Clicks { get; set; }
        public int? CategoryID { get; set; }
        public URLCategory? Category { get; set; }
    }
}