using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace WeatherPlusZero
{
    public static class UiUpdater
    {
        private static readonly string[] WindDirections = { "N", "NH", "E", "SH", "S", "SW", "W", "NW" }; // Array of wind directions.

        private static IConfiguration Configuration;

        /// <summary>
        /// Updates all UI components with weather data.
        /// Updates main weather parameters, icon, location, and future forecasts.
        /// </summary>
        /// <param name="weatherData">Weather data.</param>
        /// <param name="mainWindow">Main window object (optional).</param>
        public static void UpdateAllComponents(WeatherData weatherData, MainWindow mainWindow = null)
        {
            mainWindow ??= Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

            if (weatherData?.CurrentConditions == null) return;

            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

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
        private static void UpdateMainWeatherParameters(WeatherData data, MainWindow window)
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
        private static string ConvertWindDirection(double degrees)
        {
            degrees = (degrees % 360 + 360) % 360; // Normalizes degree to be between 0-360.
            return WindDirections[(int)Math.Round(degrees / 45.0) % 8]; // Converts degree to the nearest direction and gets direction from array.
        }

        private static void UpdateBackgroudImage(string path, MainWindow window) =>
            window.UpdateBackgroundImage(path);

        /// <summary>
        /// Updates location information in the UI.
        /// Displays city or location name in the UI.
        /// </summary>
        /// <param name="address">Location address.</param>
        /// <param name="window">Main window object.</param>
        private static void UpdateLocationDisplay(string address, MainWindow window) =>
            window.UpdateLocationText(address);

        /// <summary>
        /// Updates weather icon in the UI.
        /// Displays the icon in the UI using the icon file name.
        /// </summary>
        /// <param name="icon">Icon file name (e.g., "01d").</param>
        /// <param name="window">Main window object.</param>
        private static void UpdateWeatherIcon(string icon, MainWindow window) =>
            window.UpdateWeatherStatusIcon($"{ApplicationProgress.ImageBasePath}{icon}.png");

        /// <summary>
        /// Updates future days' weather forecasts in the UI.
        /// Converts daily forecast data to FutureDay objects and displays them in the UI.
        /// </summary>
        /// <param name="data">Weather data.</param>
        /// <param name="window">Main window object.</param>
        private static void UpdateFutureWeatherForecast(WeatherData data, MainWindow window)
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
}
