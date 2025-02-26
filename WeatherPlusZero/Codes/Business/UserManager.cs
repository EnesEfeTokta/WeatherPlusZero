using System;
using System.Text;
using System.Security.Cryptography;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Notification.Wpf;
using WeatherPlusZero.Codes.API;

namespace WeatherPlusZero
{
    /// <summary>
    /// Manages user-related operations including authentication, registration, and profile management.
    /// </summary>
    public static class UserManager
    {
        private static UserSessionWindow userSessionWindow = Application.Current.Windows.OfType<UserSessionWindow>().FirstOrDefault();

        public static User user { get; set; }

        /// <summary>
        /// Initializes the <see cref="UserManager"/> class.
        /// </summary>
        /// <param name="AuthService">The authentication service.</param>
        public static void Initialize()
        {
            //UserManager.AuthService = AuthService;
            //UserManager.AuthService.UserManager = UserManager;
        }

        /// <summary>
        /// Validates user input fields based on the specified validation type.
        /// </summary>
        /// <param name="validateType">The type of validation to perform.</param>
        /// <param name="value">The value to validate.</param>
        /// <returns>True if the value is valid; otherwise, false.</returns>
        public static bool ValidateFieldsContinuously(ValidateType validateType, string value)
        {
            return validateType switch
            {
                ValidateType.NameSurname => AuthenticationValidator.ValidateNameSurname(value),
                ValidateType.Email => AuthenticationValidator.ValidateEmail(value),
                ValidateType.Password => AuthenticationValidator.ValidatePassword(value),
                _ => false,
            };
        }

        /// <summary>
        /// Handles the login process for a user.
        /// </summary>
        /// <param name="user">The user credentials.</param>
        public static async Task LogIn(User user)
        {
            try
            {
                if (!AuthenticationValidator.ValidateEmail(user.email))
                {
                    NotificationManagement.ShowNotification(
                        "Format Error",
                        "Please enter the e-mail in the correct format. There must be an '@' sign in your e-mail...",
                        NotificationType.Error);
                    return;
                }

                if (!AuthenticationValidator.ValidatePassword(user.password))
                {
                    NotificationManagement.ShowNotification(
                        "Format Error",
                        "Please make sure your password has at least two capital characters, at least two special characters, at least two numbers and at least eight characters in length.",
                        NotificationType.Error);
                    return;
                }

                string hashedPassword = HashPassword(user.password);
                bool loginStatus = await DataBase.LoginUserOwnAuth(user.email, hashedPassword);

                if (loginStatus)
                {
                    LogInSuccess();
                }
                else
                {
                    NotificationManagement.ShowNotification(
                        "Login Error",
                        "Entry failed. Please try again to enter the information...",
                        NotificationType.Error);
                }
            }
            catch (Exception ex)
            {
                NotificationManagement.ShowNotification(
                    "Error", $"An error occurred during login: {ex.Message}",
                    NotificationType.Error);
            }
        }

        public static async void LogInSuccess()
        {
            NotificationManagement.ShowNotification(
                "Login Successful",
                "Entry is successful. You will soon be redirected to the main screen...",
                NotificationType.Success);

            await ApplicationActivity.ChangeApplicationActivityDataByLogIn(true);

            Application.Current.Dispatcher.Invoke(() => WindowTransition());
        }

        /// <summary>
        /// Transitions the UI to the main application window.
        /// </summary>
        private static void WindowTransition()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();

                Application.Current.MainWindow = mainWindow;

                foreach (Window window in Application.Current.Windows)
                {
                    if (window is UserSessionWindow)
                        window.Close();
                }
            });
        }

        /// <summary>
        /// Handles the registration process for a new user.
        /// </summary>
        /// <param name="user">The user's registration details.</param>
        public static async void Register(User user)
        {
            if (!AuthenticationValidator.ValidateNameSurname(user.namesurname))
            {
                NotificationManagement.ShowNotification(
                    "Format Error",
                    "Please enter the name in the correct format. At least five characters please...",
                    NotificationType.Error);
                return;
            }

            if (!AuthenticationValidator.ValidateEmail(user.email))
            {
                NotificationManagement.ShowNotification(
                    "Format Error",
                    "Please enter the e-mail in the correct format. There must be an '@' sign in your e-mail...",
                    NotificationType.Error);
                return;
            }

            if (!AuthenticationValidator.ValidatePassword(user.password))
            {
                NotificationManagement.ShowNotification(
                    "Format Error",
                    "Please make sure your password has at least two capital characters, at least two special characters, at least two numbers and at least eight characters in length.",
                    NotificationType.Error);
                return;
            }

            user.password = HashPassword(user.password);
            user.registrationdate = DateTime.UtcNow;

            UserManager.user = user;

            await AuthService.AccountVerify(user, EmailSendType.UserVerificationEmail);
        }

        /// <summary>
        /// Registers a new user in the DataBase.
        /// </summary>
        public static async Task RegisterUser()
        {
            await DataBase.RegisterUserOwnAuth(UserManager.user);
        }

        /// <summary>
        /// Handles the forgot password process for a user.
        /// </summary>
        /// <param name="user">The user credentials for password recovery.</param>
        public static async Task Forgot(User user)
        {
            if (!AuthenticationValidator.ValidateNameSurname(user.namesurname))
            {
                NotificationManagement.ShowNotification(
                    "Format Error",
                    "Please enter the name in the correct format. At least five characters please...",
                    NotificationType.Error);
                return;
            }

            if (!AuthenticationValidator.ValidateEmail(user.email))
            {
                NotificationManagement.ShowNotification(
                    "Format Error",
                    "Please enter the e-mail in the correct format. There must be an '@' sign in your e-mail...",
                    NotificationType.Error);
                return;
            }

            if (!await DataBase.ForgotUserOwnAuth(user))
            {
                NotificationManagement.ShowNotification(
                    "Error",
                    "The user with the entered e-mail address does not exist. Please check your e-mail address and try again...",
                    NotificationType.Error);
                return;
            }

            NotificationManagement.ShowNotification(
                "Information",
                "Verification code sent to your email address. Please check your email and enter the code here.",
                NotificationType.Information);

            UserManager.user = user;
            await AuthService.AccountVerify(user, EmailSendType.PasswordResetEmail);
        }

        /// <summary>
        /// Changes the password for the current user.
        /// </summary>
        /// <param name="newPassword">The new password.</param>
        public static async Task ChangePassword(string newPassword)
        {
            if (!AuthenticationValidator.ValidatePassword(newPassword))
            {
                NotificationManagement.ShowNotification(
                    "Format Error",
                    "Please make sure your password has at least two capital characters, at least two special characters, at least two numbers and at least eight characters in length.",
                    NotificationType.Error);
                return;
            }

            string hashedPassword = HashPassword(newPassword);
            await DataBase.ChangePasswordOwnAuth(UserManager.user.email, hashedPassword);

            NotificationManagement.ShowNotification(
                "Password Changed",
                "Your password has been successfully changed.",
                NotificationType.Success);
        }


        /// <summary>
        /// Logs out the current user.
        /// Application activities and weather data are cleared.
        /// </summary>
        /// <returns></returns>
        public static void LogOut()
        {
            ApplicationActivity.ClearApplicationActivityData();
            WeatherManager.ClearWeatherData();
        }

        /// <summary>
        /// Hashes the input password using SHA256.
        /// </summary>
        /// <param name="input">The input password.</param>
        /// <returns>The hashed password.</returns>
        private static string HashPassword(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }

    /// <summary>
    /// Defines the types of validation that can be performed on user input.
    /// </summary>
    public enum ValidateType { All, NameSurname, Email, Password }
}
