using System;
using System.Threading.Tasks;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Linq;
using Notification.Wpf;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace WeatherPlusZero
{
    public class ApplicationProgress
    {
        // Base path for weather status icon images.
        public const string ImageBasePath = "pack://application:,,,/Images/WeatherStatus/";

        private const string CultureName = "en-US"; // Culture name constant (English - US).
        private readonly CultureInfo _culture = new CultureInfo(CultureName); // Creates CultureInfo object.
        private DispatcherTimer _timer; // Timer for updating the UI at regular intervals.
        private WeatherData _weatherData; // Field to store weather data.
        
        private readonly MainWindow _mainWindow; // Main window object.

        /// <summary>
        /// Constructor for the ApplicationProgress class.
        /// Retrieves the main window instance.
        /// </summary>
        public ApplicationProgress()
        {
            _mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

            if (_mainWindow == null)
            {
                _mainWindow = new MainWindow();
                _mainWindow.Show();
            }
        }

        /// <summary>
        /// Main method called when the application starts.
        /// Initiates application startup processes.
        /// </summary>
        public void ApplicationStart()
        {
            InitializeApplication();
        }

        /// <summary>
        /// Initializes the application and loads necessary data.
        /// Asynchronously loads initial data and starts the timer.
        /// </summary>
        private async void InitializeApplication()
        {
            await LoadInitialData();
            InitializeTimer();
        }

        /// <summary>
        /// Loads the required weather data when the application first opens.
        /// Asynchronously fetches weather data and updates the UI.
        /// </summary>
        public async Task LoadInitialData()
        {
            _weatherData = null;
            _weatherData = await FetchWeatherData();
            UpdateUI();
        }

        /// <summary>
        /// Updates the UI with weather data.
        /// Executes UI updates on the main UI thread.
        /// </summary>
        private void UpdateUI()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                new UiUpdater().UpdateAllComponents(_weatherData, _mainWindow);
            });
        }

        #region UpdateDayBar
        /// <summary>
        /// Initializes the timer to update the UI regularly.
        /// Creates the timer, attaches the Tick event, and starts it.
        /// </summary>
        private void InitializeTimer()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += TimerTickHandler;
            _timer.Start();
        }

        /// <summary>
        /// Method to be executed each time the timer ticks.
        /// Updates the date and time.
        /// </summary>
        private void TimerTickHandler(object sender, EventArgs e)
        {
            UpdateDayNightBar();
            UpdateDateTime(); 
        }


        /// <summary>
        /// Updates the day-night bar in the UI.
        /// </summary>
        private void UpdateDayNightBar()
        {
            DateTime now = DateTime.Now;

            double hour = now.Hour + now.Minute / 60.0;

            _mainWindow.UpdateDayNightBar(hour);
        }
        #endregion

        /// <summary>
        /// Updates the date and time information in the UI.
        /// Gets the current date and time and displays it in the UI.
        /// </summary>
        private void UpdateDateTime()
        {
            DateTime currentTime = DateTime.Now;
            _mainWindow.Dispatcher.Invoke(() =>
            {
                _mainWindow.UpdateDateTimeText( // Updates the date and time texts.
                    currentTime.ToString("dd", _culture), // Day
                    currentTime.DayOfWeek.ToString(), // Day of the week
                    currentTime.ToString("MMMM", _culture), // Month
                    currentTime.ToString("yyyy", _culture), // Year
                    currentTime.ToString("HH", _culture), // Hour
                    currentTime.ToString("mm", _culture)  // Minute
                );
            });
        }

        /// <summary>
        /// Asynchronously fetches weather data.
        /// Retrieves data from JSON service, or API service if not available.
        /// </summary>
        /// <returns>WeatherData object or null.</returns>
        private async Task<WeatherData> FetchWeatherData()
        {
            JsonService jsonService = new JsonService(); // Creates a JSON service instance.
            WeatherData weatherDataJson = await jsonService.GetWeatherDataAsync(); // Gets weather data from JSON file.
            string city = weatherDataJson?.Address; // Gets city information.

            WeatherManager weatherService = new WeatherManager();
            return await weatherService.GetWeatherDataAsync(city, false);
        }
    }

    public class SearchCity
    {
        private const string SpecialCharactersPattern = @"[!@#$%^&*()_+=\[{\]};:<>|./?,\d-]"; // Regex pattern for special characters.
        private static readonly Regex SpecialCharRegex = new Regex(SpecialCharactersPattern, RegexOptions.Compiled); // Regex for special character check.
        private static readonly Regex EmojiRegex = new Regex(@"\p{Cs}", RegexOptions.Compiled); // Regex for emoji character check.

        private WeatherData _weatherData; // Field to store weather data.

        /// <summary>
        /// Property holding the searched city name.
        /// </summary>
        public string City { get; private set; }

        /// <summary>
        /// Searches for the entered city name and updates weather data.
        /// Validates the city name, normalizes it, and calls the weather service.
        /// </summary>
        /// <param name="city">City name to search.</param>
        /// <returns>True if search is successful, false otherwise.</returns>
        public async Task<bool> SearchCityName(string city)
        {
            if (IsInvalidCityName(city)) return false; // Returns false if city name is invalid.

            City = NormalizeCityName(city); // Normalizes the city name.

            WeatherManager weatherService = new WeatherManager();
            _weatherData = await weatherService.GetWeatherDataAsync(City, true);

            if (_weatherData == null) return false;

            Application.Current.Dispatcher.Invoke(() =>
            {
                new UiUpdater().UpdateAllComponents(_weatherData);
            });

            return true;
        }

        /// <summary>
        /// The user cancels the searched city and returns to the app.
        /// </summary>
        public async Task CanselCitySelect()
        {
            ApplicationProgress applicationProgress = new ApplicationProgress();
            await applicationProgress.LoadInitialData();
        }

        /// <summary>
        /// The user adds the selected city to the list of cities.
        /// </summary>
        public async Task AddSelectCity()
        {
            ApiService apiService = new ApiService();
            await apiService.SaveWeatherDataAsync(_weatherData);

            await CanselCitySelect();

            NotificationManagement.ShowNotification(
                "City added to the list.", 
                "The city has been successfully added to the list of cities.", 
                NotificationType.Success);
        }

        /// <summary>
        /// Checks if the city name is valid.
        /// Checks if the city name is empty, contains emojis, or special characters.
        /// </summary>
        /// <param name="city">City name to check.</param>
        /// <returns>True if city name is invalid, false otherwise.</returns>
        private bool IsInvalidCityName(string city)
        {
            return string.IsNullOrWhiteSpace(city) || ContainsEmoji(city) || ContainsSpecialCharacters(city);
        }

        /// <summary>
        /// Checks if the input text contains emoji characters.
        /// </summary>
        /// <param name="input">Text to check.</param>
        /// <returns>True if it contains emojis, false otherwise.</returns>
        private bool ContainsEmoji(string input) => EmojiRegex.IsMatch(input);

        /// <summary>
        /// Checks if the input text contains special characters.
        /// </summary>
        /// <param name="input">Text to check.</param>
        /// <returns>True if it contains special characters, false otherwise.</returns>
        private bool ContainsSpecialCharacters(string input) => SpecialCharRegex.IsMatch(input);

        /// <summary>
        /// Normalizes the city name.
        /// Makes the first letter uppercase, the rest lowercase, and replaces some Turkish characters with English equivalents.
        /// </summary>
        /// <param name="input">City name to normalize.</param>
        /// <returns>Normalized city name.</returns>
        private string NormalizeCityName(string input)
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

    public class UiUpdater
    {
        private static readonly string[] WindDirections = { "N", "NH", "E", "SH", "S", "SW", "W", "NW" }; // Array of wind directions.
        
        private IConfiguration Configuration;

        /// <summary>
        /// Updates all UI components with weather data.
        /// Updates main weather parameters, icon, location, and future forecasts.
        /// </summary>
        /// <param name="weatherData">Weather data.</param>
        /// <param name="mainWindow">Main window object (optional).</param>
        public void UpdateAllComponents(WeatherData weatherData, MainWindow mainWindow = null)
        {
            //mainWindow ??= (MainWindow)Application.Current.MainWindow;
            mainWindow ??= Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

            if (weatherData?.CurrentConditions == null) return;

            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            //NetworkChecker networkChecker = new NetworkChecker();

            //string imageUrl = networkChecker.IsConnected ? $"BackgroundImageURLs:{weatherData.CurrentConditions.Icon}-background" : "BackgroundImageURLs:default-background";

            string imageUrl = $"BackgroundImageURLs:{weatherData.CurrentConditions.Icon}-background";

            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateMainWeatherParameters(weatherData, mainWindow);
                UpdateWeatherIcon(weatherData.CurrentConditions.Icon, mainWindow);
                UpdateLocationDisplay(weatherData.ResolvedAddress, mainWindow);
                UpdateFutureWeatherForecast(weatherData, mainWindow);
                UpdateBackgroudImage(Configuration[imageUrl], mainWindow);
            });
        }

        /// <summary>
        /// Updates the main weather parameters in the UI.
        /// Displays temperature, wind, humidity, and pressure information in the UI.
        /// </summary>
        /// <param name="data">Weather data.</param>
        /// <param name="window">Main window object.</param>
        private void UpdateMainWeatherParameters(WeatherData data, MainWindow window)
        {
            CurrentConditions current = data.CurrentConditions;
            string windDirection = ConvertWindDirection(current.Winddir);

            window.UpdateMainWeatherParametersText( // Updates main weather parameter texts.
                Math.Round(Convert.ToDecimal(current.Temp)).ToString(), // Temperature
                current.Windspeed.ToString(), // Wind speed
                windDirection, // Wind direction (text)
                current.Winddir.ToString(), // Wind direction (degrees)
                Math.Round(Convert.ToDecimal(current.Humidity)).ToString(), // Humidity
                current.Pressure.ToString() // Pressure
            );
        }

        /// <summary>
        /// Converts wind direction degrees to textual direction.
        /// Converts a 360-degree angle to one of 8 cardinal directions.
        /// </summary>
        /// <param name="degrees">Wind direction in degrees.</param>
        /// <returns>Textual wind direction (e.g., "N", "E", "SW").</returns>
        private string ConvertWindDirection(double degrees)
        {
            degrees = (degrees % 360 + 360) % 360; // Normalizes degree to be between 0-360.
            return WindDirections[(int)Math.Round(degrees / 45.0) % 8]; // Converts degree to the nearest direction and gets direction from array.
        }

        private void UpdateBackgroudImage(string path, MainWindow window) =>
            window.UpdateBackgroundImage(path);

        /// <summary>
        /// Updates location information in the UI.
        /// Displays city or location name in the UI.
        /// </summary>
        /// <param name="address">Location address.</param>
        /// <param name="window">Main window object.</param>
        private void UpdateLocationDisplay(string address, MainWindow window) =>
            window.UpdateLocationText(address);

        /// <summary>
        /// Updates weather icon in the UI.
        /// Displays the icon in the UI using the icon file name.
        /// </summary>
        /// <param name="icon">Icon file name (e.g., "01d").</param>
        /// <param name="window">Main window object.</param>
        private void UpdateWeatherIcon(string icon, MainWindow window) =>
            window.UpdateWeatherStatusIcon($"{ApplicationProgress.ImageBasePath}{icon}.png");

        /// <summary>
        /// Updates future days' weather forecasts in the UI.
        /// Converts daily forecast data to FutureDay objects and displays them in the UI.
        /// </summary>
        /// <param name="data">Weather data.</param>
        /// <param name="window">Main window object.</param>
        private void UpdateFutureWeatherForecast(WeatherData data, MainWindow window)
        {
            if (data?.Days == null) return;

            ObservableCollection<FutureDay> forecastDays = new ObservableCollection<FutureDay>();
            foreach (var day in data.Days)
            {
                DateTime date = DateTime.TryParse(day.Datetime, out DateTime dateTime)
                    ? dateTime
                    : DateTime.Now;

                forecastDays.Add(new FutureDay
                {
                    DayName = date.ToString("dddd"),
                    IconPath = $"{ApplicationProgress.ImageBasePath}{day.Icon}.png",
                    MinMaxTemperature = $"{Math.Round(day.Tempmin)}℃ ~ {Math.Round(day.Tempmax)}℃"
                });
            }
            window.SetFutureDays(forecastDays);
        }
    }

    // Class representing weather forecasts for future days.
    // Used to display daily forecasts in the UI.
    public class FutureDay
    {
        public string DayName { get; set; } // Day of the week name.
        public string IconPath { get; set; } // Icon path.
        public string MinMaxTemperature { get; set; } // Min-Max temperature values.
    }

    // Burası uygulamanın yürütlme ayarlarını içeriyor...
    public class ApplicationActivityData
    {
        public User LoggedInUser { get; set; }

        public bool IsLoggedIn { get; set; }
        public string FirstOpeningDateTimeOfTheDay {  get; set; }
        public string LastOpeningDateTimeOfTheDay { get; set; }

        public bool IsInAppNotificationOn { get; set; }
        public bool IsDailyWeatherEmailsOpen { get; set; }
        public bool IsImportantWeatherEmailsOn { get; set; }

        public WeatherData WeatherData { get; set; }

        public short ApplicationVersion { get; set; }
    }
}