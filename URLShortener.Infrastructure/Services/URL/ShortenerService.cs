using System.Security.Cryptography;
using System.Text;
using URLShortener.Application.Interfaces.Services.URL;

namespace URLShortener.Infrastructure.Services.URL;

public sealed class ShortenerService : IShortenerService
{
    private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    private const int ShortCodeLength = 6;

    /// <summary>
    /// Generates a 6-character short code from the given URL using SHA-256 hashing.
    /// The hash bytes are mapped directly to a Base62 character set, guaranteeing
    /// the first character is always a letter.
    /// </summary>
    /// <param name="longURL">The original long URL to shorten.</param>
    /// <returns>A 6-character Base62 short code derived from the URL's SHA-256 hash.</returns>
    public string HashURL(string longURL)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(longURL));
        return ToBase62(hash, ShortCodeLength);
    }

    /// <summary>
    /// Resolves a short code collision by appending a cryptographically random suffix.
    /// Each call produces a new candidate code that can be re-checked against the database.
    /// </summary>
    /// <param name="shortURL">The colliding short code to extend.</param>
    /// <param name="suffixLength">Number of random characters to append. Defaults to 1.</param>
    /// <returns>A new short code with a random suffix appended.</returns>
    public string CollisionHandler(string shortURL, int suffixLength = 1)
    {
        return shortURL + GenerateRandomSuffix(suffixLength);
    }

    /// <summary>
    /// Converts raw SHA-256 bytes to a Base62 string of the desired length.
    /// Uses modulo to map each byte (0-255) to an index in the 62-char set.
    /// Forces the first character to be a letter via % 52 (first 52 chars are letters).
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    private static string ToBase62(byte[] bytes, int length)
    {
        var result = new char[length];
        for (int i = 0; i < length; i++)
            result[i] = Chars[bytes[i] % Chars.Length];

        if (char.IsDigit(result[0]))
            result[0] = Chars[bytes[0] % 52];

        return new string(result);
    }

    /// <summary>
    /// Fills a byte array with cryptographically secure random bytes via
    /// RandomNumberGenerator, then maps each byte to a Base62 character. 
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    private static string GenerateRandomSuffix(int length)
    {
        var buffer = new byte[length];
        RandomNumberGenerator.Fill(buffer);
        var result = new char[length];
        for (int i = 0; i < length; i++)
            result[i] = Chars[buffer[i] % Chars.Length];
        return new string(result);
    }

}
