using System.Threading.Tasks;
using System.Windows;
using System.Text.RegularExpressions;
using Notification.Wpf;

namespace WeatherPlusZero
{
    public static class SearchCity
    {
        private const string SpecialCharactersPattern = @"[!@#$%^&*()_+=\[{\]};:<>|./?,\d-]"; // Regex pattern for special characters.
        private static readonly Regex SpecialCharRegex = new Regex(SpecialCharactersPattern, RegexOptions.Compiled); // Regex for special character check.
        private static readonly Regex EmojiRegex = new Regex(@"\p{Cs}", RegexOptions.Compiled); // Regex for emoji character check.

        private static WeatherData _weatherData; // Field to store weather data.

        /// <summary>
        /// Property holding the searched city name.
        /// </summary>
        public static string City { get; private set; }

        /// <summary>
        /// Searches for the entered city name and updates weather data.
        /// Validates the city name, normalizes it, and calls the weather service.
        /// </summary>
        /// <param name="city">City name to search.</param>
        /// <returns>True if search is successful, false otherwise.</returns>
        public static async Task<bool> SearchCityName(string city)
        {
            if (IsInvalidCityName(city)) return false; // Returns false if city name is invalid.

            City = NormalizeCityName(city); // Normalizes the city name.

            _weatherData = await WeatherManager.GetWeatherDataAsync(City, true);

            if (_weatherData == null) return false;

            Application.Current.Dispatcher.Invoke(() =>
            {
                UiUpdater.UpdateAllComponents(_weatherData, false);
            });

            return true;
        }

        /// <summary>
        /// The user cancels the searched city and returns to the app.
        /// </summary>
        public static async Task CanselCitySelect()  
            => await ApplicationProgress.LoadInitialData();

        /// <summary>
        /// The user adds the selected city to the list of cities.
        /// </summary>
        public static async Task AddSelectCity()
        {
            ApiService apiService = new ApiService();
            await apiService.SaveWeatherDataAsync(_weatherData);

            await CanselCitySelect();

            NotificationManagement.ShowNotification(
                "City added to the list.",
                "The city has been successfully added to the list of cities.",
                NotificationType.Success);

            ApplicationActivity.SaveApplicationActivityDataByCity(_weatherData.Address); // The saved city information is updated.
        }

        /// <summary>
        /// Checks if the city name is valid.
        /// Checks if the city name is empty, contains emojis, or special characters.
        /// </summary>
        /// <param name="city">City name to check.</param>
        /// <returns>True if city name is invalid, false otherwise.</returns>
        private static bool IsInvalidCityName(string city)
        {
            return string.IsNullOrWhiteSpace(city) || ContainsEmoji(city) || ContainsSpecialCharacters(city);
        }

        /// <summary>
        /// Checks if the input text contains emoji characters.
        /// </summary>
        /// <param name="input">Text to check.</param>
        /// <returns>True if it contains emojis, false otherwise.</returns>
        private static bool ContainsEmoji(string input) => EmojiRegex.IsMatch(input);

        /// <summary>
        /// Checks if the input text contains special characters.
        /// </summary>
        /// <param name="input">Text to check.</param>
        /// <returns>True if it contains special characters, false otherwise.</returns>
        private static bool ContainsSpecialCharacters(string input) => SpecialCharRegex.IsMatch(input);

        /// <summary>
        /// Normalizes the city name.
        /// Makes the first letter uppercase, the rest lowercase, and replaces some Turkish characters with English equivalents.
        /// </summary>
        /// <param name="input">City name to normalize.</param>
        /// <returns>Normalized city name.</returns>
        private static string NormalizeCityName(string input)
        {
            input = char.ToUpper(input[0]) + input.Substring(1).ToLower();

            input.Replace(" ", "%20").Replace("ç", "c")
                 .Replace("ğ", "g").Replace("ö", "o")
                 .Replace("ş", "s").Replace("ü", "u")
                 .Replace("ı", "i").Replace("Ç", "C")
                 .Replace("Ğ", "G").Replace("Ö", "O")
                 .Replace("Ş", "S").Replace("Ü", "U")
                 .Replace("İ", "I");

            return input;
        }
    }
}
