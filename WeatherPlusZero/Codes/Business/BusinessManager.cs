using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace WeatherPlusZero
{
    public class ApplicationProgress
    {
        public const string ImageBasePath = "pack://application:,,,/Images/WeatherStatus/";
        private const string CultureName = "en-US";
        private readonly CultureInfo _culture = new CultureInfo(CultureName);
        private DispatcherTimer _timer;
        private WeatherData _weatherData;
        private readonly MainWindow _mainWindow;

        public ApplicationProgress()
        {
            _mainWindow = (MainWindow)Application.Current.MainWindow;
        }

        public void ApplicationStart()
        {
            InitializeApplication();
        }

        private async void InitializeApplication()
        {
            await LoadInitialData();
            InitializeTimer();
        }

        private async Task LoadInitialData()
        {
            _weatherData = await FetchWeatherData();
            if (_weatherData != null)
            {
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                new UiUpdater().UpdateAllComponents(_weatherData, _mainWindow);
            });
        }

        private void InitializeTimer()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += TimerTickHandler;
            _timer.Start();
        }

        private void TimerTickHandler(object sender, EventArgs e) => UpdateDateTime();

        private void UpdateDateTime()
        {
            var currentTime = DateTime.Now;
            _mainWindow.Dispatcher.Invoke(() =>
            {
                _mainWindow.UpdateDateTimeText(
                    currentTime.ToString("dd", _culture),
                    currentTime.DayOfWeek.ToString(),
                    currentTime.ToString("MMMM", _culture),
                    currentTime.ToString("yyyy", _culture),
                    currentTime.ToString("HH", _culture),
                    currentTime.ToString("mm", _culture)
                );
            });
        }

        private async Task<WeatherData> FetchWeatherData()
        {
            var jsonService = new JsonService();
            var weatherDataJson = await jsonService.GetWeather();
            var city = weatherDataJson?.Address;

            var weatherService = new GetWeather();
            return await weatherService.GetWeatherData(city);
        }
    }

    public class SearchCity
    {
        private const string SpecialCharactersPattern = @"[!@#$%^&*()_+=\[{\]};:<>|./?,\d-]";
        private static readonly Regex SpecialCharRegex = new Regex(SpecialCharactersPattern, RegexOptions.Compiled);
        private static readonly Regex EmojiRegex = new Regex(@"\p{Cs}", RegexOptions.Compiled);

        public string City { get; private set; }

        public async Task<bool> SearchCityName(string city)
        {
            if (IsInvalidCityName(city)) return false;

            City = NormalizeCityName(city);

            var weatherService = new GetWeather();
            var weatherData = await weatherService.GetWeatherData(City, RequestType.Instant);

            if (weatherData == null) return false;

            Application.Current.Dispatcher.Invoke(() =>
            {
                new UiUpdater().UpdateAllComponents(weatherData);
            });

            return true;
        }

        private bool IsInvalidCityName(string city)
        {
            return string.IsNullOrWhiteSpace(city) || ContainsEmoji(city) || ContainsSpecialCharacters(city);
        }

        private bool ContainsEmoji(string input) => EmojiRegex.IsMatch(input);

        private bool ContainsSpecialCharacters(string input) => SpecialCharRegex.IsMatch(input);

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
        private static readonly string[] WindDirections = { "N", "NH", "E", "SH", "S", "SW", "W", "NW" };

        public void UpdateAllComponents(WeatherData weatherData, MainWindow mainWindow = null)
        {
            mainWindow ??= (MainWindow)Application.Current.MainWindow;

            if (weatherData?.CurrentConditions == null) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateMainWeatherParameters(weatherData, mainWindow);
                UpdateWeatherIcon(weatherData.CurrentConditions.Icon, mainWindow);
                UpdateLocationDisplay(weatherData.ResolvedAddress, mainWindow);
                UpdateFutureWeatherForecast(weatherData, mainWindow);
            });
        }

        private void UpdateMainWeatherParameters(WeatherData data, MainWindow window)
        {
            var current = data.CurrentConditions;
            var windDirection = ConvertWindDirection(current.Winddir);

            window.UpdateMainWeatherParametersText(
                Math.Round(Convert.ToDecimal(current.Temp)).ToString(),
                current.Windspeed.ToString(),
                windDirection,
                current.Winddir.ToString(),
                Math.Round(Convert.ToDecimal(current.Humidity)).ToString(),
                current.Pressure.ToString()
            );
        }

        private string ConvertWindDirection(double degrees)
        {
            degrees = (degrees % 360 + 360) % 360;
            return WindDirections[(int)Math.Round(degrees / 45.0) % 8];
        }

        private void UpdateLocationDisplay(string address, MainWindow window) =>
            window.UpdateLocationText(address);

        private void UpdateWeatherIcon(string icon, MainWindow window) =>
            window.UpdateWeatherStatusIcon($"{ApplicationProgress.ImageBasePath}{icon}.png");

        private void UpdateFutureWeatherForecast(WeatherData data, MainWindow window)
        {
            if (data?.Days == null) return;

            var forecastDays = new ObservableCollection<FutureDay>();
            foreach (var day in data.Days)
            {
                var date = DateTime.TryParse(day.Datetime, out var dateTime)
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

    public class FutureDay
    {
        public string DayName { get; set; }
        public string IconPath { get; set; }
        public string MinMaxTemperature { get; set; }
    }
}