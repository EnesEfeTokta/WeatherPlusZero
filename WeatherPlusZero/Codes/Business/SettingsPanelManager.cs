using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using WeatherPlusZero.Codes.API;

namespace WeatherPlusZero
{
    public static class SettingsPanelManager
    {
        private static MainWindow mainWindow;

        static SettingsPanelManager()
        {
            mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
        }

        public static async Task UpdateSettingsPanelAsync()
        {
            ApplicationActivityData applicationActivityData = await ApplicationActivity.GetApplicationActivityData();
            UpdateUI(applicationActivityData);
        }

        public static async Task UpdateNotificationsApplicationActivityDataAsync(bool isInAppNotificationOn, bool isDailyWeatherEmailsOpen, bool isImportantWeatherEmailsOn)
        {
            ApplicationActivityData applicationActivityData = await ApplicationActivity.GetApplicationActivityData();

            applicationActivityData.IsInAppNotificationOn = isInAppNotificationOn;
            applicationActivityData.IsDailyWeatherEmailsOpen = isDailyWeatherEmailsOpen;
            applicationActivityData.IsImportantWeatherEmailsOn = isImportantWeatherEmailsOn;

            await ApplicationActivity.UpdateApplicationActivityData(applicationActivityData);

            await UpdateSettingsPanelAsync();
        }

        public static async void ClearCity()
        {
            JsonService jsonService = new JsonService();
            jsonService.RemoveCity();

            await UpdateSettingsPanelAsync();
        }

        public static void GoToGitHubPage()
        {
            Process.Start(new ProcessStartInfo("https://github.com/EnesEfeTokta/WeatherPlusZero")
            {
                UseShellExecute = true
            });
        }

        public static async Task LogOutAsync()
        {
            WindowTransition();
            await ApplicationActivity.ClearApplicationActivityData();
        }

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

        private static void UpdateUI(ApplicationActivityData data)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                mainWindow.UpdateSettingsPanel(data);
            });
        }
    }
}
