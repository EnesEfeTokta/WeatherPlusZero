using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using Notification.Wpf;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace WeatherPlusZero
{
    /// <summary>
    /// Manages user-related operations including authentication, registration, and profile management.
    /// </summary>
    public class UserManager
    {
        private readonly DataBase dataBase;
        private readonly NotificationManagement notificationManagement;
        private readonly AuthenticationValidator authenticationValidator;
        private readonly AuthService authService;
        private readonly UserSessionWindow userSessionWindow;

        public User user { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserManager"/> class.
        /// </summary>
        /// <param name="authService">The authentication service.</param>
        public UserManager(AuthService authService)
        {
            dataBase = new DataBase();
            notificationManagement = new NotificationManagement();
            authenticationValidator = new AuthenticationValidator();
            this.authService = authService;
            // Atama: AuthService içindeki UserManager property’sine bu örneği atıyoruz.
            this.authService.UserManager = this;

            userSessionWindow = Application.Current.Windows.OfType<UserSessionWindow>().FirstOrDefault();
        }

        /// <summary>
        /// Validates user input fields based on the specified validation type.
        /// </summary>
        /// <param name="validateType">The type of validation to perform.</param>
        /// <param name="value">The value to validate.</param>
        /// <returns>True if the value is valid; otherwise, false.</returns>
        public bool ValidateFieldsContinuously(ValidateType validateType, string value)
        {
            return validateType switch
            {
                ValidateType.NameSurname => authenticationValidator.ValidateNameSurname(value),
                ValidateType.Email => authenticationValidator.ValidateEmail(value),
                ValidateType.Password => authenticationValidator.ValidatePassword(value),
                _ => false,
            };
        }

        /// <summary>
        /// Handles the login process for a user.
        /// </summary>
        /// <param name="user">The user credentials.</param>
        public async Task LogIn(User user)
        {
            try
            {
                if (!authenticationValidator.ValidateEmail(user.email))
                {
                    NotificationManagement.ShowNotification(
                        "Format Error", 
                        "Please enter the e-mail in the correct format. There must be an '@' sign in your e-mail...", 
                        NotificationType.Error);
                    return;
                }

                if (!authenticationValidator.ValidatePassword(user.password))
                {
                    NotificationManagement.ShowNotification(
                        "Format Error", 
                        "Please make sure your password has at least two capital characters, at least two special characters, at least two numbers and at least eight characters in length.", 
                        NotificationType.Error);
                    return;
                }

                bool loginStatus = await dataBase.LoginUserOwnAuth(user.email, user.password);

                if (loginStatus)
                {
                    NotificationManagement.ShowNotification(
                        "Login Successful", 
                        "Entry is successful. You will soon be redirected to the main screen...", 
                        NotificationType.Success);
                    Application.Current.Dispatcher.Invoke(() => WindowTransition());
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
                NotificationManagement.ShowNotification("Error", $"An error occurred during login: {ex.Message}", NotificationType.Error);
            }
        }

        /// <summary>
        /// Transitions the UI to the main application window.
        /// </summary>
        private void WindowTransition()
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
        public async void Register(User user)
        {
            if (!authenticationValidator.ValidateNameSurname(user.namesurname))
            {
                NotificationManagement.ShowNotification("Format Error", "Please enter the name in the correct format. At least five characters please...", NotificationType.Error);
                return;
            }

            if (!authenticationValidator.ValidateEmail(user.email))
            {
                NotificationManagement.ShowNotification("Format Error", "Please enter the e-mail in the correct format. There must be an '@' sign in your e-mail...", NotificationType.Error);
                return;
            }

            if (!authenticationValidator.ValidatePassword(user.password))
            {
                NotificationManagement.ShowNotification("Format Error", "Please make sure your password has at least two capital characters, at least two special characters, at least two numbers and at least eight characters in length.", NotificationType.Error);
                return;
            }

            this.user = user;
            await authService.AccountVerify(user, EmailSendType.UserVerificationEmail);
        }

        /// <summary>
        /// Registers a new user in the database.
        /// </summary>
        public async Task RegisterUser()
        {
            await dataBase.RegisterUserOwnAuth(this.user);
        }

        /// <summary>
        /// Handles the forgot password process for a user.
        /// </summary>
        /// <param name="user">The user credentials for password recovery.</param>
        public async Task Forgot(User user)
        {
            if (!authenticationValidator.ValidateNameSurname(user.namesurname))
            {
                NotificationManagement.ShowNotification(
                    "Format Error", 
                    "Please enter the name in the correct format. At least five characters please...", 
                    NotificationType.Error);
                return;
            }

            if (!authenticationValidator.ValidateEmail(user.email))
            {
                NotificationManagement.ShowNotification(
                    "Format Error", 
                    "Please enter the e-mail in the correct format. There must be an '@' sign in your e-mail...", 
                    NotificationType.Error);
                return;
            }

            if (!await dataBase.ForgotUserOwnAuth(user))
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

            this.user = user;
            await authService.AccountVerify(user, EmailSendType.PasswordResetEmail);
        }

        /// <summary>
        /// Changes the password for the current user.
        /// </summary>
        /// <param name="newPassword">The new password to set.</param>
        public async Task ChangePassword(string newPassword)
        {
            if (!authenticationValidator.ValidatePassword(newPassword))
            {
                NotificationManagement.ShowNotification(
                    "Format Error", 
                    "Please make sure your password has at least two capital characters, at least two special characters, at least two numbers and at least eight characters in length.", 
                    NotificationType.Error);
                return;
            }

            await dataBase.ChangePasswordOwnAuth(user.email, newPassword);

            NotificationManagement.ShowNotification(
                "Change Successful", 
                "Your new password has been registered. Please log in...", 
                NotificationType.Success);
            userSessionWindow.PanelTransition(Panels.Login);
        }
    }

    /// <summary>
    /// Provides methods for validating user input, such as names, emails, and passwords.
    /// </summary>
    public class AuthenticationValidator
    {
        private readonly char[] specialCharacters = new char[] { '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '_', '+', '=', '{', '}', '[', ']', '|', '\\', ':', ';', '"', '\'', '<', '>', ',', '.', '?', '/' };
        private readonly char[] numberCharacters = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        /// <summary>
        /// Validates that the given name or surname meets the minimum length requirement.
        /// </summary>
        /// <param name="nameSurname">The name or surname to validate.</param>
        /// <returns>True if the name or surname is valid; otherwise, false.</returns>
        public bool ValidateNameSurname(string nameSurname)
        {
            return !string.IsNullOrEmpty(nameSurname) && nameSurname.Length >= 5;
        }

        /// <summary>
        /// Validates that the given email address is in a valid format.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns>True if the email address is valid; otherwise, false.</returns>
        public bool ValidateEmail(string email)
        {
            return !string.IsNullOrEmpty(email) && email.Contains("@") && email.Length >= 5;
        }

        /// <summary>
        /// Validates that the given password meets the complexity requirements.
        /// </summary>
        /// <param name="password">The password to validate.</param>
        /// <returns>True if the password is valid; otherwise, false.</returns>
        public bool ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            int specialCount = 0, numberCount = 0, upperCaseCount = 0;

            foreach (char c in password)
            {
                if (specialCharacters.Contains(c))
                    specialCount++;
                if (numberCharacters.Contains(c))
                    numberCount++;
                if (char.IsUpper(c))
                    upperCaseCount++;
            }

            return specialCount >= 2 && numberCount >= 2 && password.Length >= 8 && upperCaseCount >= 2;
        }
    }

    /// <summary>
    /// Provides authentication-related services, including generating verification codes, sending emails, and managing timers.
    /// </summary>
    public class AuthService
    {
        private readonly UserSessionWindow userSessionWindow;
        private readonly HTMLReadService readService;
        private readonly EmailService emailService;

        // Bu property, dışarıdan atanarak UserManager örneğini tutar.
        public UserManager UserManager { get; set; }

        public int verificationCode { get; set; }
        private Timer timer;
        private int counter = 300; // Örneğin 300 saniye = 5 dakika

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthService"/> class.
        /// </summary>
        public AuthService()
        {
            emailService = new EmailService();
            readService = new HTMLReadService();
            userSessionWindow = Application.Current.Windows.OfType<UserSessionWindow>().FirstOrDefault();
        }

        /// <summary>
        /// Generates a random six-digit verification code.
        /// </summary>
        /// <returns>A random six-digit verification code.</returns>
        public int GenerateVerificationCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999);
        }

        /// <summary>
        /// Handles the account verification process, sending a verification code to the user's email address.
        /// </summary>
        /// <param name="user">The user to verify.</param>
        /// <param name="emailSendType">The type of email to send (user verification or password reset).</param>
        public async Task AccountVerify(User user, EmailSendType emailSendType)
        {
            verificationCode = GenerateVerificationCode();

            string htmlCode = readService.ReadHTML(emailSendType);

            switch (emailSendType)
            {
                case EmailSendType.UserVerificationEmail:
                    htmlCode = htmlCode.Replace("[VERIFICATION_CODE]", verificationCode.ToString());
                    htmlCode = htmlCode.Replace("[TO_EMAIL]", user.email);
                    break;
                case EmailSendType.PasswordResetEmail:
                    htmlCode = htmlCode.Replace("[VERIFICATION_CODE]", verificationCode.ToString());
                    htmlCode = htmlCode.Replace("[USER_NAME]", user.namesurname);
                    break;
            }

            await emailService.SendMail_SendGrid(user, htmlCode);

            NotificationManagement.ShowNotification(
                "E-Mail Sent",
                $"The verification code has been sent to your {user.email} e-mail address. Please check your email and enter the code here.",
                NotificationType.Information);

            userSessionWindow.PanelTransition(Panels.EmailVerification);

            if (!ValidateTimer())
                return;
            else
                StartTimer();
        }

        /// <summary>
        /// Verifies the input code against the generated verification code.
        /// </summary>
        /// <param name="inputCode">The code entered by the user.</param>
        /// <returns>True if the codes match; otherwise, false.</returns>
        private bool CodeVerify(string inputCode)
        {
            return verificationCode.ToString() == inputCode;
        }

        /// <summary>
        /// Handles the verification process for new user registration.
        /// </summary>
        /// <param name="inputCode">The verification code entered by the user.</param>
        public async Task RegisterVerificationCode(string inputCode)
        {
            if (CodeVerify(inputCode))
            {
                NotificationManagement.ShowNotification(
                    "Success",
                    "Your account has been successfully created. You can now log in with your account.",
                    NotificationType.Success);

                await UserManager.RegisterUser();
                userSessionWindow.PanelTransition(Panels.Login);
                return;
            }

            NotificationManagement.ShowNotification("Error", "Invalid code", NotificationType.Error);
        }

        /// <summary>
        /// Handles the verification process for forgot password.
        /// </summary>
        /// <param name="inputCode">The verification code entered by the user.</param>
        public void ForgotVerificationCode(string inputCode)
        {
            if (CodeVerify(inputCode))
            {
                userSessionWindow.PanelTransition(Panels.ChangePassword);
                NotificationManagement.ShowNotification(
                    "Success",
                    "Email verification completed. Please set a password...",
                    NotificationType.Success);
                return;
            }

            NotificationManagement.ShowNotification("Error", "Invalid code", NotificationType.Error);
        }

        /// <summary>
        /// Validates that the timer has not expired.
        /// </summary>
        /// <returns>True if the timer has not expired; otherwise, false.</returns>
        private bool ValidateTimer()
        {
            return counter >= 0;
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        private void StartTimer()
        {
            timer = new Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        private void StopTimer()
        {
            timer.Stop();
            timer.Dispose();
            timer = null;
        }

        /// <summary>
        /// Handles the elapsed event for the timer.
        /// </summary>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (counter > 0)
            {
                counter--;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    int minutes = counter / 60;
                    int seconds = counter % 60;
                    userSessionWindow.UpdateTimerText($"{minutes}:{seconds:D2}");
                });
            }
            else
            {
                NotificationManagement.ShowNotification(
                    "Error", 
                    "Time is up. Please try again.", 
                    NotificationType.Error);
                StopTimer();
            }
        }
    }

    /// <summary>
    /// Provides methods for sending emails using SendGrid.
    /// </summary>
    public class EmailService
    {
        private const string sendGridApiKey = "";
        private readonly HTMLReadService htmlReadService;

        public EmailService()
        {
            htmlReadService = new HTMLReadService();
        }

        /// <summary>
        /// Sends an email using SendGrid.
        /// </summary>
        /// <param name="user">The recipient user.</param>
        /// <param name="htmlCode">The HTML content of the email.</param>
        public async Task SendMail_SendGrid(User user, string htmlCode)
        {
            try
            {
                var client = new SendGridClient(sendGridApiKey);
                var from = new EmailAddress("enesefetokta009@gmail.com", "Weather Zero Plus");
                var to = new EmailAddress(user.email, user.namesurname);
                string plainTextContent = "This email was sent as plain text.";
                var msg = MailHelper.CreateSingleEmail(from, to, "Email from Weather Zero Plus...", plainTextContent, htmlCode);
                await client.SendEmailAsync(msg);
            }
            catch (Exception ex)
            {
                NotificationManagement.ShowNotification(
                    "Email Send Error", 
                    $"An error occurred while sending email: {ex.Message}", 
                    NotificationType.Error);
            }
        }
    }

    /// <summary>
    /// Provides methods for reading HTML files.
    /// </summary>
    public class HTMLReadService
    {
        /// <summary>
        /// Reads the content of an HTML file based on the specified email type.
        /// </summary>
        /// <param name="emailSendType">The type of email to read the HTML for.</param>
        /// <returns>The HTML content as a string.</returns>
        public string ReadHTML(EmailSendType emailSendType)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().FirstOrDefault(name => name.EndsWith(emailSendType + ".html"));

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }

    /// <summary>
    /// Defines the types of emails that can be sent.
    /// </summary>
    public enum EmailSendType { UserVerificationEmail, WeatherUpdateEmail, PasswordResetEmail, EmergencyWeatherAlertEmail }

    /// <summary>
    /// Defines the types of validation that can be performed on user input.
    /// </summary>
    public enum ValidateType { All, NameSurname, Email, Password }
}
