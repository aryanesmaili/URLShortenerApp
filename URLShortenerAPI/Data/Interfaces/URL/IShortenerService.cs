namespace URLShortenerAPI.Data.Interfaces.URL
{
    public interface IShortenerService
    {
        public string HashURL(string longURL);
        public string CollisionHandler(ref string shortURL, int lengthToAdd = 1);
    }
}
