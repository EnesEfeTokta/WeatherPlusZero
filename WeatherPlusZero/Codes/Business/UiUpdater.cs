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
        private static readonly string[] WindDirections = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" }; // Improved wind directions

        private static IConfiguration Configuration;

        /// <summary>
        /// Updates all UI components with weather data.
        /// Updates main weather parameters, icon, location, and future forecasts.
        /// </summary>
        /// <param name="weatherData">Weather data.</param>
        /// <param name="IsDateTimeBased">Flag indicating whether to use date/time based update (optional, default: true).</param>
        public static void UpdateAllComponents(WeatherData weatherData, bool IsDateTimeBased = true)
        {
            MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

            if (weatherData?.CurrentConditions == null) return;

            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string imageUrl = $"BackgroundImageURLs:{weatherData.CurrentConditions.Icon}-background";

            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateBackgroudImage(Configuration[imageUrl], mainWindow);

                if (IsDateTimeBased)
                {
                    UpdateMainWeatherParametersByHour(weatherData, mainWindow);
                    UpdateWeatherIcon(weatherData.CurrentConditions.Icon, mainWindow);
                }
                else
                {
                    UpdateMainWeatherParametersByCurrentCondition(weatherData.CurrentConditions, mainWindow);
                    UpdateWeatherIcon(weatherData.CurrentConditions.Icon, mainWindow);
                }

                UpdateLocationDisplay(weatherData.ResolvedAddress, mainWindow);
                UpdateFutureWeatherForecast(weatherData, mainWindow);
            });
        }

        private static void UpdateMainWeatherParametersByHour(WeatherData weatherData, MainWindow window)
        {
            DateTime now = DateTime.Now;
            string day = now.ToString("yyyy-MM-dd");
            string hour = now.ToString("HH:00:00"); // Check at O'clock for stable results

            Day today = weatherData.Days.FirstOrDefault(d => d.Datetime == day);
            if (today != null)
            {
                Hour currentHourData = today.Hours.FirstOrDefault(h => h.Datetime == hour);
                if (currentHourData != null)
                {
                    string windDirection = ConvertWindDirection(currentHourData.Winddir);

                    window.UpdateMainWeatherParametersText( // Updates main weather parameter texts.
                        Math.Round(Convert.ToDecimal(currentHourData.Temp)).ToString(), // Temperature
                        currentHourData.Windspeed.ToString(), // Wind speed
                        windDirection, // Wind direction (text)
                        currentHourData.Winddir.ToString(), // Wind direction (degrees)
                        Math.Round(Convert.ToDecimal(currentHourData.Humidity)).ToString(), // Humidity
                        currentHourData.Pressure.ToString() // Pressure
                    );

                    // We return currentHourData.Icon before UpdateWeatherIcon(weatherData.CurrentConditions.Icon, mainWindow);
                    return;
                }
            }

            // If hourly data not found, fallback to current conditions:
            UpdateMainWeatherParametersByCurrentCondition(weatherData.CurrentConditions, window);
        }

        /// <summary>
        /// Updates the main weather parameters in the UI based on CurrentConditions.
        /// </summary>
        private static void UpdateMainWeatherParametersByCurrentCondition(CurrentConditions data, MainWindow window)
        {
            string windDirection = ConvertWindDirection(data.Winddir);

            window.UpdateMainWeatherParametersText(
                Math.Round(Convert.ToDecimal(data.Temp)).ToString(),
                data.Windspeed.ToString(),
                windDirection,
                data.Winddir.ToString(),
                Math.Round(Convert.ToDecimal(data.Humidity)).ToString(),
                data.Pressure.ToString()
            );
        }

        /// <summary>
        /// Converts wind direction degrees to textual direction.
        /// Converts a 360-degree angle to one of 8 cardinal directions.
        /// </summary>
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
        private static void UpdateLocationDisplay(string address, MainWindow window) =>
            window.UpdateLocationText(address);

        /// <summary>
        /// Updates weather icon in the UI.
        /// Displays the icon in the UI using the icon file name.
        /// </summary>
        private static void UpdateWeatherIcon(string icon, MainWindow window) =>
            window.UpdateWeatherStatusIcon($"{ApplicationProgress.ImageBasePath}{icon}.png");

        /// <summary>
        /// Updates future days' weather forecasts in the UI.
        /// Converts daily forecast data to FutureDay objects and displays them in the UI.
        /// </summary>
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