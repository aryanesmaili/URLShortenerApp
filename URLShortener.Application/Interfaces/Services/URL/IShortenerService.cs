namespace URLShortener.Application.Interfaces.Services.URL;

public interface IShortenerService
{
    string HashURL(string longURL);
    string CollisionHandler(string shortURL, int lengthToAdd = 1);
}
