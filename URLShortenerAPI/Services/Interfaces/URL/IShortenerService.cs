namespace URLShortenerAPI.Services.Interfaces.URLRelated
{
    public interface IShortenerService
    {
        public string HashURL(string longURL);
        public string CollisionHandler(ref string shortURL, int length = 1);
    }
}
