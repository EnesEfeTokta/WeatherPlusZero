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

        public static void OpenSettingsPanel()
        {
            UpdateSettingsPanelAsync();
        }

        public static async void UpdateSettingsPanelAsync() => UpdateUI(await ApplicationActivity.GetApplicationActivityData());

        public static async void UpdateNotificationsApplicationActivityDataAsync(bool isInAppNotificationOn, bool isDailyWeatherEmailsOpen, bool isImportantWeatherEmailsOn)
        {
            ApplicationActivityData applicationActivityData = await ApplicationActivity.GetApplicationActivityData();

            applicationActivityData.IsInAppNotificationOn = isInAppNotificationOn;
            applicationActivityData.IsDailyWeatherEmailsOpen = isDailyWeatherEmailsOpen;
            applicationActivityData.IsImportantWeatherEmailsOn = isImportantWeatherEmailsOn;

            await ApplicationActivity.UpdateApplicationActivityData(applicationActivityData);

            UpdateSettingsPanelAsync();
        }

        public static void ClearCity()
        {
            JsonService jsonService = new JsonService();
            jsonService.RemoveCity();

            UpdateSettingsPanelAsync();
        }

        public static void GoToGitHubPage()
        {
            Process.Start(new ProcessStartInfo("https://github.com/EnesEfeTokta/WeatherPlusZero")
            {
                UseShellExecute = true
            });
        }

        public static void LogOut()
        {
            ApplicationActivity.ClearApplicationActivityData();
            WeatherManager.ClearWeatherData();
            WindowTransition();
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
