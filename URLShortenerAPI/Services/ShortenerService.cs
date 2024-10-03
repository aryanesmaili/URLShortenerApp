using Pexita.Utility.Exceptions;
using System.Security.Cryptography;
using System.Text;
using URLShortenerAPI.Data;
using URLShortenerAPI.Data.Entities.URL;
using URLShortenerAPI.Services.Interfaces;

namespace URLShortenerAPI.Services
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// A service that provides URL shortening functionalities.
    /// </summary>
    internal class ShortenerService : IShortenerService
    {
        // fixed length for the length of shortened URLs.
        private const int ShortUrlLength = 6;
        // Allowed characters to exist in shortened URL.
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        // Random number generator for creating random suffixes
        private readonly static Random random = new();

        /// <summary>
        /// Generates a hashed version of the provided long URL and returns a shortened URL.
        /// The shortened URL is derived from the SHA-256 hash of the long URL.
        /// </summary>
        /// <param name="longURL">The long URL to be shortened.</param>
        /// <returns>A 6-character shortened URL derived from the hash of the long URL.</returns>
        public string HashURL(string longURL)
        {
            // Compute the SHA-256 hash of the long URL
            byte[] fullHash = SHA256.HashData(Encoding.UTF8.GetBytes(longURL));

            StringBuilder result = new();

            // Convert each byte of the hash to a 2-character hexadecimal representation
            foreach (byte b in fullHash)
            {
                result.Append(b.ToString("x2")); // Append the hex representation
            }

            // Return the first 6 characters of the hexadecimal string as the short URL
            return result.ToString().Substring(0, 6);
        }

        /// <summary>
        /// Handles collisions by generating a random suffix to ensure uniqueness of the short URL.
        /// </summary>
        /// <param name="shortURL">The base short URL to check for collisions.</param>
        /// <param name="length">The length of the random suffix to be generated (default is 6).</param>
        /// <returns>A unique short URL with a random suffix added to it.</returns>
        public string CollisionHandler(ref string shortURL, int length = ShortUrlLength)
        {
            // Create a buffer to hold the randomly generated characters
            char[] buffer = new char[length];

            // Fill the buffer with random characters from the allowed set
            for (int i = 0; i < length; i++)
            {
                buffer[i] = Chars[random.Next(Chars.Length)];
            }

            // Append the random characters to the base short URL and return the result
            return shortURL += new string(buffer);
        }
    }

}
