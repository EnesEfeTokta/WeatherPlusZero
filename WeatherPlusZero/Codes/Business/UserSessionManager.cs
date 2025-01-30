using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Notification.Wpf;
using System.Windows;

namespace WeatherPlusZero
{
    public class UserManager
    {
        private DataBase dataBase;
        private NotificationManagement notificationManagement;
        public UserManager() 
        {
            dataBase = new DataBase();
            notificationManagement = new NotificationManagement();
        }

        public async void LogIn(User user)
        {
            bool loginStatus = await dataBase.LoginUserOwnAuth(user.email, user.password);

            if (loginStatus)
            {
                // Login successful
                notificationManagement.ShowNotification("Login", "Login successful", NotificationType.Success);
                Application.Current.Dispatcher.Invoke(() => WindowTransition());


            }
            else
            {
                // Login failed
                notificationManagement.ShowNotification("Login", "Login failed", NotificationType.Error);
            }
        }

        private void WindowTransition()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Yeni MainWindow oluştur ve göster
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();

                // Uygulamanın ana penceresini yeni MainWindow olarak ayarla
                Application.Current.MainWindow = mainWindow;

                // Tüm UserSessionWindow örneklerini kapat
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is UserSessionWindow)
                        window.Close();
                }
            });
        }

        public void LogOut(User user)
        {

        }

        public void Register(User user)
        {

        }
        
        public void ChangePassword(User user, string newPassword)
        {
            user.password = newPassword;
        }

        public void ChangeEmail(User user, string newEmail)
        {
            user.email = newEmail;
        }

        public void ChangeNamesurname(User user, string newNamesurname)
        {
            user.namesurname = newNamesurname;
        }
    }

    public class AuthService
    {
        private readonly UserManager _userManager;

        public AuthService(UserManager userManager)
        {
            _userManager = userManager;
        }
    }
}