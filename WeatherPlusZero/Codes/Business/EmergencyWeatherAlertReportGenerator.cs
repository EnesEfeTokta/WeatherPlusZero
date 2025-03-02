using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Windows;

namespace WeatherPlusZero
{
    public static class EmergencyWeatherAlertReportGenerator
    {
        private static readonly Dictionary<string, string> WeatherChangeTypes = new Dictionary<string, string>
            {
                { "HighTemperature", "“High Air Temperature 🥵”"},
                { "LowTemperature", "“Low Air Temperature 🥶”"},
                { "HighWindSpeed", "“High Wind Speed 🌬️”"},
                { "HighUVIndex", "“High UV Index ☀️”"}
            };

        private static readonly Dictionary<string, string> Precautions = new Dictionary<string, string>
            {
                { "HighTemperature1", "Avoid direct contact with sunlight. ⛱️" },
                { "HighTemperature2", "Use sunscreen. 🛡️" },
                { "HighTemperature3", "Drink plenty of water. 💧" },
                { "LowTemperature1", "Wear thick clothes. 🧥" },
                { "LowTemperature2", "Do not go outside unless necessary. ❌" },
                { "LowTemperature3", "Keep yourself warm. ♨" },
                { "HighWindSpeed1", "Wait in a closed area. 🏠" },
                { "HighWindSpeed2", "Do not go outside unless necessary. ❌" },
                { "HighWindSpeed3", "Stay away from tall buildings. 🏢" },
                { "HighUVIndex1", "Avoid direct contact with sunlight. ⛱️" },
                { "HighUVIndex2", "Use sunscreen. 🛡️" },
                { "HighUVIndex3", "Stay outside for a maximum of 10 minutes. ⏰" }
            };

        /// <summary>
        /// Checks the current weather conditions and generates alerts if any emergency situations are detected.
        /// </summary>
        /// <param name="data">The current weather conditions.</param>
        public static async void EmergencySituationCheck()
        {
            if (!await ApplicationActivity.GetIsImportantWeatherEmailsOnFromApplicationActivityData())
                return;

            CurrentConditions data = (await WeatherManager.GetWeatherDataAsync("None")).CurrentConditions;

            List<(string type, string status, string value)> alerts = new List<(string type, string status, string value)>();

            if (data.Temp.Value >= 30)
            {
                alerts.Add(("HighTemperature", $"{data.Temp}℃", "High"));
            }
            else if (data.Temp.Value <= -20)
            {
                alerts.Add(("LowTemperature", $"{data.Temp}℃", "Low"));
            }

            if (data.Windspeed >= 40)
            {
                alerts.Add(("HighWindSpeed", $"{data.Windspeed}km/s", "High"));
            }

            if (data.Uvindex >= 8)
            {
                alerts.Add(("HighUVIndex", $"{data.Uvindex} UVs", "High"));
            }

            foreach (var alert in alerts)
            {
                await GenerateAndSendReport(alert.type, alert.status, alert.value, data.Datetime);
            }
        }

        /// <summary>
        /// Generates and sends an emergency weather alert report.
        /// </summary>
        /// <param name="type">The type of weather change.</param>
        /// <param name="status">The status of the weather condition.</param>
        /// <param name="value">The value of the weather condition.</param>
        /// <param name="dateTime">The date and time of the weather condition.</param>
        private static async Task GenerateAndSendReport(string type, string status, string value, string dateTime)
        {
            string[] relatedArray = {
                    WeatherChangeTypes[type],
                    Precautions[$"{type}1"],
                    Precautions[$"{type}2"],
                    Precautions[$"{type}3"]
                };

            var (report, emailBody) = await GenerateReport(relatedArray, status, value, dateTime);

            SendReport(report);
            SaveReport(emailBody);
        }

        /// <summary>
        /// Generates an emergency weather alert report.
        /// </summary>
        /// <param name="relatedArray">An array of related weather change types and precautions.</param>
        /// <param name="status">The status of the weather condition.</param>
        /// <param name="severity">The severity of the weather condition.</param>
        /// <param name="startTime">The start time of the weather condition.</param>
        /// <returns>The generated report as a string.</returns>
        private static async Task<(string, string)> GenerateReport(string[] relatedArray, string status, string severity, string startTime)
        {
            string report = HTMLService.GetHTMLCode(EmailSendType.EmergencyWeatherAlertEmail);

            if (string.IsNullOrEmpty(report))
                return (null, null);

            string city = await ApplicationActivity.GetCityVariationFromApplicationActivityData();
            report = report.Replace("[LOCATION]", city);

            report = report.Replace("[WEATHER_CHANGE_TYPE]", relatedArray[0]);
            report = report.Replace("[WEATHER_STATUS]", status);
            report = report.Replace("[WEATHER_SEVERITY]", severity);
            report = report.Replace("[WEATHER_START_TIME]", startTime);

            report = report.Replace("[PRECAUTION_1]", relatedArray[1]);
            report = report.Replace("[PRECAUTION_2]", relatedArray[2]);
            report = report.Replace("[PRECAUTION_3]", relatedArray[3]);

            string emailBody = 
                $"[LOCATION:{city}], " +
                $"[WEATHER_CHANGE_TYPE:{relatedArray[0]}], " +
                $"[WEATHER_STATUS:{status}], " +
                $"[WEATHER_SEVERITY:{severity}], " +
                $"[WEATHER_START_TIME:{startTime}], " +
                $"[PRECAUTION_1:{relatedArray[1]}], " +
                $"[PRECAUTION_2:{relatedArray[2]}], " +
                $"[PRECAUTION_3:{relatedArray[3]}]";

            return (report, emailBody);
        }

        /// <summary>
        /// Sends the generated report via email.
        /// </summary>
        /// <param name="report">The generated report as a string.</param>
        private static async void SendReport(string report)
        {
            await EmailService.SendMail_SendGrid(
                await ApplicationActivity.GetUserNameSurnameFromApplicationActivityData(), 
                await ApplicationActivity.GetUserEmailFromApplicationActivityData(), 
                report);
        }

        /// <summary>
        /// Saves the generated report to the database.
        /// </summary>
        /// <param name="report">The generated report as a string.</param>
        private static async void SaveReport(string report)
        {
            Notification newNotification = new Notification
            {
                notificationid = DataBase.GetNotificationId(),
                userid = DataBase.GetUserId(),
                notificationtype = "Emergency Weather Alert",
                notificationmessage = report,
                notificationdatetime = DateTime.UtcNow.ToString()
            };

            // [!] For now, instead of adding a new notification line, we update the existing notification line.
            await DataBase.TAsyncUpdateRow<Notification>(newNotification);
            //await DataBase.TAsyncAddRow<Notification>(newNotification);
        }
    }
}