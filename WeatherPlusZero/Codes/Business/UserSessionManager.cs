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
        private AuthenticationValidator authenticationValidator;

        public UserManager() 
        {
            dataBase = new DataBase();
            notificationManagement = new NotificationManagement();
            authenticationValidator = new AuthenticationValidator();
        }

        public bool ValidateFieldsContinuously(ValidateType validateType, string value)
        {
            switch (validateType)
            {
                case ValidateType.NameSurname:
                    if (!authenticationValidator.ValidateNameSurname(value))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                case ValidateType.Email:
                    if (!authenticationValidator.ValidateEmail(value))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                case ValidateType.Password:
                    if (!authenticationValidator.ValidatePassword(value))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
            }

            return true;
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

    public class AuthenticationValidator
    {
        private readonly char[] specialCharacters = new char[] { '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '_', '+', '=', '{', '}', '[', ']', '|', '\\', ':', ';', '"', '\'', '<', '>', ',', '.', '?', '/' };
        private readonly char[] numberCharcters = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        public bool ValidateNameSurname(string nameSurname = null)
        {
            if (nameSurname.Length < 5)
                return false;

            return true;
        }

        public bool ValidateEmail(string email)
        {
            return email.Contains("@") && email.Length > 5;
        }

        public bool ValidatePassword(string password)
        {
            int specialCharactersCount = 0;
            int numberCharactersCount = 0;
            int upperCaseCount = 0;

            for (int i = 0; i < specialCharacters.Length; i++)
            {
                if (password.Contains(specialCharacters[i]))
                    specialCharactersCount++;
            }

            for (int i = 0; i < numberCharcters.Length; i++)
            {
                if (password.Contains(numberCharcters[i]))
                    numberCharactersCount++;
            }

            for (int i = 0; i < password.Length; i++)
            {
                if(char.IsUpper(password[i]))
                    upperCaseCount++;
            }

            if (specialCharactersCount < 1 || numberCharactersCount < 1 || password.Length < 8 || upperCaseCount < 2)
                return false;
            
            return true;
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

    public enum ValidateType { All, NameSurname, Email, Password }
}