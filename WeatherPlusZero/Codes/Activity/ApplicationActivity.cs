using Supabase.Gotrue.Mfa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WeatherPlusZero.Codes.API;

namespace WeatherPlusZero
{
    public static class ApplicationActivity
    {
        public static async void SaveApplicationActivityData(ApplicationActivityData data)
        {
            JsonService jsonService = new JsonService();

            await jsonService.SaveApplicationActivityDataAsync(data);
        }

        public static async void SaveApplicationActivityDataByCity(string city)
        {
            JsonService jsonService = new JsonService();

            ApplicationActivityData applicationActivityData = await GetApplicationActivityData();
            applicationActivityData.SelectCity = city;

            await jsonService.SaveApplicationActivityDataAsync(applicationActivityData);
        }

        public static async Task<ApplicationActivityData> GetApplicationActivityData()
        {
            JsonService jsonService = new JsonService();

            ApplicationActivityData applicationActivityData = await jsonService.GetApplicationActivityDataAsync();

            return applicationActivityData;
        }

        public static async Task UpdateApplicationActivityData(ApplicationActivityData applicationActivityData)
        {
            JsonService jsonService = new JsonService();

            await jsonService.SaveApplicationActivityDataAsync(applicationActivityData);
        }

        public static async Task ClearApplicationActivityData()
        {
            JsonService jsonService = new JsonService();

            // An empty ApplicationActivityData object is created and written to the file.
            ApplicationActivityData applicationActivityData = new ApplicationActivityData();

            await jsonService.SaveApplicationActivityDataAsync(applicationActivityData);
        }

        public static async Task ChangeApplicationActivityDataByCity(string city)
        {
            JsonService jsonService = new JsonService();

            ApplicationActivityData applicationActivityData = await GetApplicationActivityData();
            applicationActivityData.SelectCity = city;

            await jsonService.SaveApplicationActivityDataAsync(applicationActivityData);
        }

        public static async Task ChnageApplicationActivityDataByNotifications(bool isInAppNotificationOn, bool isDailyWeatherEmailsOpen, bool isImportantWeatherEmailsOn)
        {
            JsonService jsonService = new JsonService();

            ApplicationActivityData applicationActivityData = await GetApplicationActivityData();

            applicationActivityData.IsInAppNotificationOn = isInAppNotificationOn;
            applicationActivityData.IsDailyWeatherEmailsOpen = isDailyWeatherEmailsOpen;
            applicationActivityData.IsImportantWeatherEmailsOn = isImportantWeatherEmailsOn;

            await jsonService.SaveApplicationActivityDataAsync(applicationActivityData);
        }

        public static async Task ChangeApplicationActivityDataByUser(string userName, string userEmail)
        {
            JsonService jsonService = new JsonService();

            ApplicationActivityData applicationActivityData = await GetApplicationActivityData();
            applicationActivityData.UserNameSurname = userName;
            applicationActivityData.UserEmail = userEmail;

            await jsonService.SaveApplicationActivityDataAsync(applicationActivityData);
        }
    }

    public class ApplicationActivityData
    {
        public string UserNameSurname { get; set; }
        public string UserEmail { get; set; }

        public string SelectCity { get; set; } = "Oltu";

        public bool IsInAppNotificationOn { get; set; } = true;
        public bool IsDailyWeatherEmailsOpen { get; set; } = true;
        public bool IsImportantWeatherEmailsOn { get; set; } = true;
    }
}
