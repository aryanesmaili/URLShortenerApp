using System.Text.RegularExpressions;

namespace URLShortenerAPI.Services.Utility
{
    internal static class UtilityFunctions
    {
        /// <summary>
        /// validates if a given string is an Email or not.
        /// </summary>
        /// <param name="email"></param>
        /// <returns>True if it is an emailf false otherwise.</returns>
        internal static bool IsEmail(this string email)
        {
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }
    }
}
