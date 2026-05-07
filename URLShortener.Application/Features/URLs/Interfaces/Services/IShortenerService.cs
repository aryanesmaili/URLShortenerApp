namespace URLShortener.Application.Features.URLs.Interfaces.Services;

public interface IShortenerService
{
    string HashURL(string longURL);
    string CollisionHandler(string shortURL, int lengthToAdd = 1);
}
