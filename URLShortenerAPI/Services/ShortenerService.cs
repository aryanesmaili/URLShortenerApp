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
        private const string Alphabets = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private const string Numbers = "0123456789";
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

            StringBuilder stringHash = new();

            // Convert each byte of the hash to a 2-character hexadecimal representation
            foreach (byte b in fullHash)
            {
                stringHash.Append(b.ToString("x2")); // Append the hex representation
            }
            string result = stringHash.ToString();

            result = EvaluateOutput(result); // check if the output is reliable.

            return result;
        }
        /// <summary>
        /// checks if the string is reliable, because if all the characters in the string are digits, example.com/12345 might be mistaken for an integer ID, thus creating issues.
        /// </summary>
        /// <param name="result">the string to be checked.</param>
        /// <returns>a reliable ShortCode string.</returns>
        private static string EvaluateOutput(string result)
        {
            // the starting index for slicing.
            int startingIndex = 0;

            // finding the first letter in the string using a for loop.
            for (int i = 0; i < result.Length - 6; i++) // (length -6) because we need at least 6 characters.
            {
                if (char.IsLetter(result[i]))
                {
                    startingIndex = i;
                    break;
                }
            }
            // the starting index is now 0 (if no letters are found or index 0 is a letter) or i (the index of the first found letter).
            result = result.Substring(startingIndex, 6);

            // check if all chars are digits
            if (result.All(char.IsDigit))
            {
                // if they are all digits, we replace the first character with a random letter.
                char[] temp = result.ToCharArray();
                temp[0] = Alphabets[random.Next(Alphabets.Length)];
                result = new string(temp);
            }
            return result;
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
