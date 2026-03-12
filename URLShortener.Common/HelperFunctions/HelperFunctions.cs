using System.Text.RegularExpressions;

namespace URLShortener.Common.HelperFunctions
{
    public static class HelperFunctions
    {
        /// <summary>
        /// validates if a given string is an Email or not.
        /// </summary>
        /// <param name="email"></param>
        /// <returns>True if it is an emailf false otherwise.</returns>
        public static bool IsEmail(this string email)
        {
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }
    }
}
