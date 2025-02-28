using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace WeatherPlusZero
{
    public static class DailyWeatherReportGenerator
    {
        // Default values for trial purposes.
        private static readonly Dictionary<string, string> weatherUpdateEmailPlaceholders = new Dictionary<string, string>
            {
                { "USER_NAME", null },
                { "USER_LOCATION", null },
                { "CURRENT_TEMP", null },
                { "CITY_NAME", null },
                { "WEATHER_DESCRIPTION", null },
                { "FEELS_LIKE", null },
                { "HUMIDITY", null },
                { "WIND_SPEED", null },
                { "PRESSURE", null },
                { "SUNRISE", null },
                { "SUNSET", null },
                { "WIND_CHILL", null },
                { "AQI_VALUE", null },
                { "PRIMARY_POLLUTANT", null },
                { "AIR_QUALITY_DESCRIPTION", null },

                { "HOUR1_TIME", null },
                { "HOUR1_TEMP", null },

                { "HOUR2_TIME", null },
                { "HOUR2_TEMP", null },

                { "HOUR3_TIME", null },
                { "HOUR3_TEMP", null },

                { "HOUR4_TIME", null },
                { "HOUR4_TEMP", null },

                { "HOUR5_TIME", null },
                { "HOUR5_TEMP", null },

                { "HOUR6_TIME", null },
                { "HOUR6_TEMP", null },

                { "HOUR7_TIME", null },
                { "HOUR7_TEMP", null },

                { "HOUR8_TIME", null },
                { "HOUR8_TEMP", null },

                { "HOUR9_TIME", null },
                { "HOUR9_TEMP", null },

                { "HOUR10_TIME", null },
                { "HOUR10_TEMP", null },

                { "HOUR11_TIME", null },
                { "HOUR11_TEMP", null },

                { "HOUR12_TIME", null },
                { "HOUR12_TEMP", null },

                { "HOUR13_TIME", null },
                { "HOUR13_TEMP", null },

                { "HOUR14_TIME", null },
                { "HOUR14_TEMP", null },

                { "HOUR15_TIME", null },
                { "HOUR15_TEMP", null },

                { "HOUR16_TIME", null },
                { "HOUR16_TEMP", null },

                { "HOUR17_TIME", null },
                { "HOUR17_TEMP", null },

                { "HOUR18_TIME", null },
                { "HOUR18_TEMP", null },

                { "HOUR19_TIME", null },
                { "HOUR19_TEMP", null },

                { "HOUR20_TIME", null },
                { "HOUR20_TEMP", null },

                { "HOUR21_TIME", null },
                { "HOUR21_TEMP", null },

                { "HOUR22_TIME", null },
                { "HOUR22_TEMP", null },

                { "HOUR23_TIME", null },
                { "HOUR23_TEMP", null },

                { "HOUR24_TIME", null },
                { "HOUR24_TEMP", null },

                { "DAY1_NAME", null },
                { "DAY1_TEMP", null },

                { "DAY2_NAME", null },
                { "DAY2_TEMP", null },

                { "DAY3_NAME", null },
                { "DAY3_TEMP", null },

                { "DAY4_NAME", null },
                { "DAY4_TEMP", null },

                { "DAY5_NAME", null },
                { "DAY5_TEMP", null },

                { "DAY6_NAME", null },
                { "DAY6_TEMP", null },

                { "DAY7_NAME", null },
                { "DAY7_TEMP", null },

                { "UV_INDEX", null },
                { "WEATHER_MAP_LINK", null },
                { "PERSONALIZED_ADVICE", null },
                { "MORE_INFO_LINK", null },
                { "ALERT_MESSAGE", null }
            };

        /// <summary>
        /// Sends the weather report email to the specified email address.
        /// </summary>
        /// <param name="sendEmail">Email address</param>
        public static async void SendWeatherReportEmail(string sendEmail)
        {
            DateTime firstDailyInformationDateTime = DateTime.Parse(await ApplicationActivity.GetFirstDailyInformationDateTimeFromApplicationActivityData());

            if ((DateTime.UtcNow - firstDailyInformationDateTime).TotalHours > 24 && await ApplicationActivity.GetIsDailyWeatherEmailsOpenFromApplicationActivityData())
                CreateEmail(sendEmail);
        }

        /// <summary>
        /// Creates an email with the weather information and sends it to the specified email address.
        /// </summary>
        private static async void CreateEmail(string sendEmail)
        {
            await PopulateWeatherDataAsync();
            string emailHTML = HTMLService.GetHTMLCode(EmailSendType.WeatherUpdateEmail);

            foreach (var item in weatherUpdateEmailPlaceholders)
            {
                emailHTML = emailHTML.Replace($"[{item.Key.ToUpper()}]", item.Value);
            }

            User user = new User();
            user.email = sendEmail;

            await EmailService.SendMail_SendGrid(user, emailHTML);

            await ApplicationActivity.ChangeApplicationActivityDataByFirstDailyInformationDateTime(DateTime.Now.ToString());

            string emailBody = string.Empty;
            foreach (var item in weatherUpdateEmailPlaceholders)
            {
                emailBody += $" {item.Key.ToUpper()}:{item.Value}";
            }

            MessageBox.Show(emailBody);

            Notification newNotification = new Notification()
            {
                notificationid = DataBase.GetNotificationId(),
                userid = DataBase.GetUserId(),
                notificationtype = "Daily Weather Report",
                notificationmessage = emailBody,
                notificationdatetime = DateTime.UtcNow.ToString()
            };

            // [!] For now, instead of adding a new notification line, we update the existing notification line.
            await DataBase.TAsyncUpdateRow<Notification>(newNotification);
            //await DataBase.TAsyncAddRow<Notification>(newNotification);
        }

        /// <summary>
        /// Gets the appropriate weather icon based on the weather icon code.
        /// </summary>
        /// <param name="iconCode">The weather icon code.</param>
        /// <returns>The corresponding weather icon.</returns>
        private static string GetWeatherIcon(string iconCode)
        {
            switch (iconCode)
            {
                case "rain": return "🌧️";
                case "rain-showers-day": return "🌦️";
                case "clear-day": return "☀️";
                case "clear-night": return "🌙";
                case "partly-cloudy-day": return "🌤️";
                case "partly-cloudy-night": return "🌃";
                case "cloudy": return "☁️";
                case "wind": return "🌬️";
                case "fog": return "🌫️";
                case "snow": return "❄️";
                case "sleet": return "🌨️";
                case "thunder-storm-day": return "⛈️";
                case "thunder-storm-night": return "⛈️";
                case "tornado": return "🌪️";
                case "hail": return "🌨️";
                case "snow-showers-day": return "🌨️";
                case "snow-showers-night": return "🌨️";
                case "showers-day": return "🌧️";
                case "showers-night": return "🌧️";
                default: return "❓";
            }
        }

        /// <summary>
        /// Populates the weather data into the email placeholders.
        /// </summary>
        private static async Task PopulateWeatherDataAsync()
        {
            try
            {
                WeatherData weatherResponse = await WeatherManager.GetWeatherDataAsync("None");

                // Current weather information
                weatherUpdateEmailPlaceholders["USER_NAME"] = "Enes Efe Tokta";
                weatherUpdateEmailPlaceholders["USER_LOCATION"] = weatherResponse.ResolvedAddress;
                weatherUpdateEmailPlaceholders["CURRENT_TEMP"] = weatherResponse.CurrentConditions.Temp.ToString();
                weatherUpdateEmailPlaceholders["CITY_NAME"] = weatherResponse.Address;
                weatherUpdateEmailPlaceholders["WEATHER_DESCRIPTION"] = weatherResponse.CurrentConditions.Conditions;
                weatherUpdateEmailPlaceholders["FEELS_LIKE"] = weatherResponse.CurrentConditions.Feelslike.ToString();
                weatherUpdateEmailPlaceholders["HUMIDITY"] = weatherResponse.CurrentConditions.Humidity.ToString();
                weatherUpdateEmailPlaceholders["WIND_SPEED"] = weatherResponse.CurrentConditions.Windspeed.ToString() + " km/h, " + weatherResponse.CurrentConditions.Winddir.ToString();
                weatherUpdateEmailPlaceholders["PRESSURE"] = weatherResponse.CurrentConditions.Pressure.ToString() + " hPa";
                weatherUpdateEmailPlaceholders["SUNRISE"] = weatherResponse.CurrentConditions.Sunrise;
                weatherUpdateEmailPlaceholders["SUNSET"] = weatherResponse.CurrentConditions.Sunset;
                weatherUpdateEmailPlaceholders["UV_INDEX"] = weatherResponse.CurrentConditions.Uvindex.ToString();

                // Air quality (default values)
                weatherUpdateEmailPlaceholders["AQI_VALUE"] = "N/A";
                weatherUpdateEmailPlaceholders["PRIMARY_POLLUTANT"] = "N/A";
                weatherUpdateEmailPlaceholders["AIR_QUALITY_DESCRIPTION"] = "Air quality data not available.";

                // Hourly forecasts
                for (int i = 0; i < Math.Min(24, weatherResponse.Days[0].Hours.Count); i++)
                {
                    weatherUpdateEmailPlaceholders[$"HOUR{i + 1}_TIME"] = weatherResponse.Days[0].Hours[i].Datetime;
                    weatherUpdateEmailPlaceholders[$"HOUR{i + 1}_TEMP"] = weatherResponse.Days[0].Hours[i].Temp.ToString();
                    weatherUpdateEmailPlaceholders[$"HOUR{i + 1}_ICON"] = GetWeatherIcon(weatherResponse.Days[0].Hours[i].Icon);
                }

                // Daily forecasts
                for (int i = 0; i < 7; i++)
                {
                    weatherUpdateEmailPlaceholders[$"DAY{i + 1}_NAME"] = DateTime.Parse(weatherResponse.Days[i].Datetime).ToString("ddd");
                    weatherUpdateEmailPlaceholders[$"DAY{i + 1}_TEMP"] = weatherResponse.Days[i].Temp.ToString();
                    weatherUpdateEmailPlaceholders[$"DAY{i + 1}_ICON"] = GetWeatherIcon(weatherResponse.Days[i].Icon);
                }

                // Weather Alert
                weatherUpdateEmailPlaceholders["ALERT_MESSAGE"] = weatherResponse.Description;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}