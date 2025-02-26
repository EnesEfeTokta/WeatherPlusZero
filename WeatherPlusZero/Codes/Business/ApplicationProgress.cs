using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Globalization;
using System.Linq;
using WeatherPlusZero.Codes.API;

namespace WeatherPlusZero
{
    public static class ApplicationProgress
    {
        // Base path for weather status icon images.
        public const string ImageBasePath = "pack://application:,,,/Images/WeatherStatus/";

        private const string CultureName = "en-US"; // Culture name constant (English - US).
        private static readonly CultureInfo _culture = new CultureInfo(CultureName); // Creates CultureInfo object.
        private static DispatcherTimer _timer; // Timer for updating the UI at regular intervals.

        private static MainWindow _mainWindow; // Main window object.

        /// <summary>
        /// Static constructor for the ApplicationProgress class.
        /// Retrieves the main window instance.
        /// </summary>
        static ApplicationProgress()
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
        public static async void ApplicationStart()
        {
            InitializeApplication();

            await LocationService.SaveLocationDataAsync(); // Saves the location data.
            ApplicationActivityData data = await ApplicationActivity.GetApplicationActivityData(); // Gets the application activity data.
            string city = data.SelectCity ?? data.IpLocation.city; // Gets the city name.
            await SearchCity.SearchCityName(city); // Searches for the city name.

            //EmergencyWeatherAlertReportGenerator.EmergencySituationCheck((await FetchWeatherData()).CurrentConditions);

            DailyWeatherWeatherInformation();
        }

        private static async void DailyWeatherWeatherInformation()
        {
            DailyWeatherReportGenerator.SendWeatherReportEmail(await ApplicationActivity.GetUserEmailFromApplicationActivityData());
        }

        /// <summary>
        /// Initializes the application and loads necessary data.
        /// Asynchronously loads initial data and starts the timer.
        /// </summary>
        private static async void InitializeApplication()
        {
            await LoadInitialData();
            InitializeTimer();
        }

        /// <summary>
        /// Loads the required weather data when the application first opens.
        /// Asynchronously fetches weather data and updates the UI.
        /// </summary>
        public static async Task LoadInitialData()
            => UpdateUI(await FetchWeatherData());

        /// <summary>
        /// Updates the UI with weather data.
        /// Executes UI updates on the main UI thread.
        /// </summary>
        private static void UpdateUI(WeatherData data)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                UiUpdater.UpdateAllComponents(data, true);
            });
        }

        /// <summary>
        /// Initializes the timer to update the UI regularly.
        /// Creates the timer, attaches the Tick event, and starts it.
        /// </summary>
        private static void InitializeTimer()
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
        private static void TimerTickHandler(object sender, EventArgs e)
        {
            UpdateDayNightBar();
            UpdateDateTime();
        }


        /// <summary>
        /// Updates the day-night bar in the UI.
        /// </summary>
        private static void UpdateDayNightBar()
        {
            DateTime now = DateTime.Now;

            double hour = now.Hour + now.Minute / 60.0;

            _mainWindow.UpdateDayNightBar(hour);
        }

        /// <summary>
        /// Updates the date and time information in the UI.
        /// Gets the current date and time and displays it in the UI.
        /// </summary>
        private static void UpdateDateTime()
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
        /// Asynchronously fetches weather data..
        /// </summary>
        /// <returns>WeatherData object or null.</returns>
        private static async Task<WeatherData> FetchWeatherData()
        {
            JsonService jsonService = new JsonService(); // Creates a JSON service instance.
            string city = (await jsonService.GetApplicationActivityDataAsync()).SelectCity; // Gets city information.

            return await WeatherManager.GetWeatherDataAsync(city, false);
        }

        public static void LogOut()
        {
            UserManager.LogOut();
        }
    }
}