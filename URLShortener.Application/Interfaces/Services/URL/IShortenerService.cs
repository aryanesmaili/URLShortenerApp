namespace URLShortener.Application.Interfaces.Services.URL
{
    public interface IShortenerService
    {
        string HashURL(string longURL);
        string CollisionHandler(ref string shortURL, int lengthToAdd = 1);
    }
}
