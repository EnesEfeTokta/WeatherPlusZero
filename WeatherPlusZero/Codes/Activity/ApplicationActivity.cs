using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Threading.Tasks;
using WeatherPlusZero.Codes.API;
using System.Windows.Controls;
using Notifications.Wpf.Annotations;

namespace WeatherPlusZero
{
    public static class ApplicationActivity
    {
        // Save the application activity data.
        public static async void SaveApplicationActivityData(ApplicationActivityData data)
        {
            JsonService jsonService = new JsonService();

            await jsonService.SaveApplicationActivityDataAsync(data);
        }

        // Save the city selected by the user.
        public static async Task SaveApplicationActivityDataByCity(string city)
        {
            JsonService jsonService = new JsonService();

            ApplicationActivityData applicationActivityData = await GetApplicationActivityData();
            applicationActivityData.SelectCity = city;

            await jsonService.SaveApplicationActivityDataAsync(applicationActivityData);
        }

        // Get the application activity data.
        public static async Task<ApplicationActivityData> GetApplicationActivityData()
        {
            JsonService jsonService = new JsonService();

            ApplicationActivityData applicationActivityData = await jsonService.GetApplicationActivityDataAsync();

            return applicationActivityData;
        }

        // The application is updating Activity Data.
        public static async Task UpdateApplicationActivityData(ApplicationActivityData applicationActivityData)
        {
            JsonService jsonService = new JsonService();

            await jsonService.SaveApplicationActivityDataAsync(applicationActivityData);
        }

        // The application is deleting Activity Data.
        public static async void ClearApplicationActivityData()
        {
            JsonService jsonService = new JsonService();

            // An empty ApplicationActivityData object is created and written to the file.
            ApplicationActivityData applicationActivityData = new ApplicationActivityData();

            await jsonService.SaveApplicationActivityDataAsync(applicationActivityData);
        }

        #region Change Methods
        // Save the city selected by the user.
        public static async Task ChangeApplicationActivityDataByCity(string city)
        {
            JsonService jsonService = new JsonService();

            ApplicationActivityData applicationActivityData = await GetApplicationActivityData();
            applicationActivityData.SelectCity = city;

            await jsonService.SaveApplicationActivityDataAsync(applicationActivityData);
        }

        // Save the notification status of the user.
        public static async Task ChnageApplicationActivityDataByNotifications(bool isInAppNotificationOn, bool isDailyWeatherEmailsOpen, bool isImportantWeatherEmailsOn)
        {
            JsonService jsonService = new JsonService();

            ApplicationActivityData applicationActivityData = await GetApplicationActivityData();

            applicationActivityData.IsInAppNotificationOn = isInAppNotificationOn;
            applicationActivityData.IsDailyWeatherEmailsOpen = isDailyWeatherEmailsOpen;
            applicationActivityData.IsImportantWeatherEmailsOn = isImportantWeatherEmailsOn;

            await jsonService.SaveApplicationActivityDataAsync(applicationActivityData);
        }

        // Save the user's name and email.
        public static async Task ChangeApplicationActivityDataByUser(string userName, string userEmail)
        {
            JsonService jsonService = new JsonService();

            ApplicationActivityData applicationActivityData = await GetApplicationActivityData();
            applicationActivityData.UserNameSurname = userName;
            applicationActivityData.UserEmail = userEmail;

            await jsonService.SaveApplicationActivityDataAsync(applicationActivityData);
        }

        // Save the first daily information date and time.
        public static async Task ChangeApplicationActivityDataByFirstDailyInformationDateTime(string dateTime)
        {
            JsonService jsonService = new JsonService();

            ApplicationActivityData applicationActivityData = await GetApplicationActivityData();
            applicationActivityData.FirstDailyInformationDateTime = dateTime;

            await jsonService.SaveApplicationActivityDataAsync(applicationActivityData);
        }

        // Save the location information.
        public static async Task ChangeApplicationActivityDataByLocation(IpLocation ipLocation)
        {
            JsonService jsonService = new JsonService();

            ApplicationActivityData applicationActivityData = await GetApplicationActivityData();
            applicationActivityData.IpLocation = ipLocation;

            await jsonService.SaveApplicationActivityDataAsync(applicationActivityData);
        }

        // Save the LogIn status.
        public static async Task ChangeApplicationActivityDataByLogIn(bool isLogIn)
        {
            JsonService jsonService = new JsonService();

            ApplicationActivityData applicationActivityData = await GetApplicationActivityData();
            applicationActivityData.IsLogIn = isLogIn;

            await jsonService.SaveApplicationActivityDataAsync(applicationActivityData);
        }
        #endregion

        #region Get Methods
        // Get the name of the logged in user.
        public static async Task<string> GetUserNameSurnameFromApplicationActivityData()
        {
            ApplicationActivityData applicationActivityData = await GetApplicationActivityData();

            return applicationActivityData.UserNameSurname;
        }

        // Get the email of the logged in user.
        public static async Task<string> GetUserEmailFromApplicationActivityData()
        {
            ApplicationActivityData applicationActivityData = await GetApplicationActivityData();

            return applicationActivityData.UserEmail;
        }

        // Get the city selected by the user.
        public static async Task<string> GetCityVariationFromApplicationActivityData()
        {
            ApplicationActivityData applicationActivityData = await GetApplicationActivityData();

            return applicationActivityData.SelectCity;
        }

        // Get the notification status of the user.
        public static async Task<bool> GetIsInAppNotificationOnFromApplicationActivityData()
        {
            ApplicationActivityData applicationActivityData = await GetApplicationActivityData();

            return applicationActivityData.IsInAppNotificationOn;
        }

        // Get the status of daily weather emails.
        public static async Task<bool> GetIsDailyWeatherEmailsOpenFromApplicationActivityData()
        {
            ApplicationActivityData applicationActivityData = await GetApplicationActivityData();

            return applicationActivityData.IsDailyWeatherEmailsOpen;
        }

        // Get the status of important weather emails.
        public static async Task<bool> GetIsImportantWeatherEmailsOnFromApplicationActivityData()
        {
            ApplicationActivityData applicationActivityData = await GetApplicationActivityData();

            return applicationActivityData.IsImportantWeatherEmailsOn;
        }

        // Get the first daily information date and time.
        public static async Task<string> GetFirstDailyInformationDateTimeFromApplicationActivityData()
        {
            ApplicationActivityData applicationActivityData = await GetApplicationActivityData();

            return applicationActivityData.FirstDailyInformationDateTime;
        }

        // Get the location information.
        public static async Task<IpLocation> GetIpLocationFromApplicationActivityData()
        {
            ApplicationActivityData applicationActivityData = await GetApplicationActivityData();

            return applicationActivityData.IpLocation;
        }

        // Get the LogIn status.
        public static async Task<bool> GetIsLogInFromApplicationActivityData()
        {
            ApplicationActivityData applicationActivityData = await GetApplicationActivityData();

            return applicationActivityData.IsLogIn;
        }
        #endregion
    }

    public class ApplicationActivityData
    {
        public int UserId { get; set; } // User ID information.
        public int CityId { get; set; } // City ID information.
        public int UserCityId { get; set; } // User city ID information.
        public int WeatherId { get; set; } // Weather ID information.
        public int NotificationId { get; set; } // Notification ID information.

        public string UserNameSurname { get; set; } // User name and surname information.
        public string UserEmail { get; set; } // User email information.

        public bool IsLogIn { get; set; } = false; // Login status.

        public string SelectCity { get; set; } = "Oltu"; // City information.

        public bool IsInAppNotificationOn { get; set; } = true; // In-app notification status.
        public bool IsDailyWeatherEmailsOpen { get; set; } = true; // Daily weather email status.
        public bool IsImportantWeatherEmailsOn { get; set; } = true; // Important weather email status.

        public string FirstDailyInformationDateTime { get; set; } = "01.01.0001 01:01:01"; // First daily information date and time.

        public IpLocation IpLocation { get; set; } // Location information.
    }
}
