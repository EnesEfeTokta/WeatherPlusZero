using System.Linq;
using System.Windows;
using System.Diagnostics;

namespace WeatherPlusZero
{
    public static class SettingsPanelManager
    {
        private static MainWindow mainWindow;

        static SettingsPanelManager()
        {
            mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
        }

        /// <summary>
        /// Opens the settings panel.
        /// </summary>
        public static void OpenSettingsPanel()
        {
            UpdateSettingsPanelAsync();
        }

        /// <summary>
        /// Updates the settings panel asynchronously.
        /// </summary>
        public static async void UpdateSettingsPanelAsync() => UpdateUI(await ApplicationActivity.GetApplicationActivityData());

        /// <summary>
        /// Updates the application activity data for notifications asynchronously.
        /// </summary>
        /// <param name="isInAppNotificationOn">Indicates if in-app notifications are on.</param>
        /// <param name="isDailyWeatherEmailsOn">Indicates if daily weather emails are on.</param>
        /// <param name="isImportantWeatherEmailsOn">Indicates if important weather emails are on.</param>
        public static async void UpdateNotificationsApplicationActivityDataAsync(bool isInAppNotificationOn, bool isDailyWeatherEmailsOn, bool isImportantWeatherEmailsOn)
        {
            await ApplicationActivity.ChnageApplicationActivityDataByNotifications(isInAppNotificationOn, isDailyWeatherEmailsOn, isImportantWeatherEmailsOn);

            UserCity newUserCity = new UserCity()
            {
                recordid = DataBase.GetRecordId(),
                userid = DataBase.GetUserId(),
                cityid = DataBase.GetCityId(),

                inappnotificationon = isInAppNotificationOn,
                dailyweatheremailson = isDailyWeatherEmailsOn,
                importantweatheremailson = isImportantWeatherEmailsOn
            };

            await DataBase.TAsyncUpdateRow<UserCity>(newUserCity); ;
            UpdateSettingsPanelAsync();
        }

        /// <summary>
        /// Clears the city data.
        /// </summary>
        public static void ClearCity()
        {
            JsonService jsonService = new JsonService();
            jsonService.RemoveCity();

            UpdateSettingsPanelAsync();
        }

        /// <summary>
        /// Opens the GitHub page of the project.
        /// </summary>
        public static void GoToGitHubPage()
        {
            Process.Start(new ProcessStartInfo("https://github.com/EnesEfeTokta/WeatherPlusZero")
            {
                UseShellExecute = true
            });
        }

        /// <summary>
        /// Logs the user out of the application.
        /// </summary>
        public static void LogOut() 
        {
            UserManager.LogOut();
            WindowTransition(); 
        }

        /// <summary>
        /// Transitions the window to the user session window.
        /// </summary>
        private static void WindowTransition()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                UserSessionWindow userSessionWindow = new UserSessionWindow();
                userSessionWindow.Show();

                Application.Current.MainWindow = userSessionWindow;

                foreach (Window window in Application.Current.Windows.OfType<MainWindow>())
                {
                    window.Close();
                }
            });
        }

        /// <summary>
        /// Updates the UI with the provided application activity data.
        /// </summary>
        /// <param name="data">The application activity data.</param>
        private static void UpdateUI(ApplicationActivityData data)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                mainWindow.UpdateSettingsPanel(data);
            });
        }
    }
}
