using System;
using System.Linq;

namespace WeatherPlusZero
{
    /// <summary>
    /// Provides methods for validating user input, such as names, emails, and passwords.
    /// </summary>
    public static class AuthenticationValidator
    {
        private static readonly char[] specialCharacters = new char[] { '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '_', '+', '=', '{', '}', '[', ']', '|', '\\', ':', ';', '"', '\'', '<', '>', ',', '.', '?', '/' };
        private static readonly char[] numberCharacters = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        /// <summary>
        /// Validates that the given name or surname meets the minimum length requirement.
        /// </summary>
        /// <param name="nameSurname">The name or surname to validate.</param>
        /// <returns>True if the name or surname is valid; otherwise, false.</returns>
        public static bool ValidateNameSurname(string nameSurname)
        {
            return !string.IsNullOrEmpty(nameSurname) && nameSurname.Length >= 5;
        }

        /// <summary>
        /// Validates that the given email address is in a valid format.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns>True if the email address is valid; otherwise, false.</returns>
        public static bool ValidateEmail(string email)
        {
            return !string.IsNullOrEmpty(email) && email.Contains("@") && email.Length >= 5;
        }

        /// <summary>
        /// Validates that the given password meets the complexity requirements.
        /// </summary>
        /// <param name="password">The password to validate.</param>
        /// <returns>True if the password is valid; otherwise, false.</returns>
        public static bool ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            int specialCount = 0, numberCount = 0, upperCaseCount = 0;

            foreach (char c in password)
            {
                if (specialCharacters.Contains(c))
                    specialCount++;
                if (numberCharacters.Contains(c))
                    numberCount++;
                if (char.IsUpper(c))
                    upperCaseCount++;
            }

            return specialCount >= 2 && numberCount >= 2 && password.Length >= 8 && upperCaseCount >= 2;
        }
    }
}
