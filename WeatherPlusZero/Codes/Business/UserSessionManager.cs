using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Notification.Wpf;
using System.Windows;
using System.IO;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Timers;

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
            this.authService.userManager = this;

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
            switch (validateType)
            {
                case ValidateType.NameSurname:
                    return authenticationValidator.ValidateNameSurname(value);
                case ValidateType.Email:
                    return authenticationValidator.ValidateEmail(value);
                case ValidateType.Password:
                    return authenticationValidator.ValidatePassword(value);
                default:
                    return false;
            }
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
                    NotificationManagement.ShowNotification("Format Error", "Please enter the e-mail in the correct format. There must be an '@' sign in your e-mail...", NotificationType.Error);
                    return;
                }

                if (!authenticationValidator.ValidatePassword(user.password))
                {
                    NotificationManagement.ShowNotification("Format Error", "Please make sure your password has at least two capital characters, at least two special characters, at least two numbers and at least eight characters in length.", NotificationType.Error);
                    return;
                }

                bool loginStatus = await dataBase.LoginUserOwnAuth(user.email, user.password);

                if (loginStatus)
                {
                    // Login successful
                    NotificationManagement.ShowNotification("Login Successful", "Entry is successful. You will soon be redirected to the main screen...", NotificationType.Success);

                    Application.Current.Dispatcher.Invoke(() => WindowTransition());
                }
                else
                {
                    // Login failed
                    NotificationManagement.ShowNotification("Login Error", "Entry failed. Please try again to enter the information...", NotificationType.Error);
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

        //public void LogOut(User user)
        //{
        //   // LogOut Method for the future implementations
        //}

        /// <summary>
        /// Handles the registration process for a new user.
        /// </summary>
        /// <param name="user">The user's registration details.</param>
        public async Task Register(User user)
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

            if (!authenticationValidator.ValidatePassword(user.password))
            {
                NotificationManagement.ShowNotification(
                    "Format Error", 
                    "Please make sure your password has at least two capital characters, at least two special characters, at least two numbers and at least eight characters in length.", 
                    NotificationType.Error);
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
                "Error",
                "Verification code sent to your email address.Please check your email and enter the code here.",
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

            MessageBox.Show(user.email);
            await dataBase.ChangePasswordOwnAuth(user.email, newPassword);

            NotificationManagement.ShowNotification(
                "Change Successful",
                "Your new password has been registered. Please log in...",
                NotificationType.Success);

            userSessionWindow.PanelTransition(Panels.Login);
        }

        /// <summary>
        /// Changes the email for the current user.
        /// </summary>
        /// <param name="user">The user object to change email.</param>
        /// <param name="newEmail">The new email to set.</param>
        public void ChangeEmail(User user, string newEmail)
        {
            user.email = newEmail;
        }

        /// <summary>
        /// Changes the namesurname for the current user.
        /// </summary>
        /// <param name="user">The user object to change namesurname.</param>
        /// <param name="newNamesurname">The new namesurname to set.</param>
        public void ChangeNamesurname(User user, string newNamesurname)
        {
            user.namesurname = newNamesurname;
        }
    }

    /// <summary>
    /// Provides methods for validating user input, such as names, emails, and passwords.
    /// </summary>
    public class AuthenticationValidator
    {
        private readonly char[] specialCharacters = new char[] { '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '_', '+', '=', '{', '}', '[', ']', '|', '\\', ':', ';', '"', '\'', '<', '>', ',', '.', '?', '/' };
        private readonly char[] numberCharcters = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        /// <summary>
        /// Validates that the given name or surname meets the minimum length requirement.
        /// </summary>
        /// <param name="nameSurname">The name or surname to validate.</param>
        /// <returns>True if the name or surname is valid; otherwise, false.</returns>
        public bool ValidateNameSurname(string nameSurname)
        {
            return nameSurname.Length >= 5;
        }

        /// <summary>
        /// Validates that the given email address is in a valid format.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns>True if the email address is valid; otherwise, false.</returns>
        public bool ValidateEmail(string email)
        {
            return email.Contains("@") && email.Length >= 5;
        }

        /// <summary>
        /// Validates that the given password meets the complexity requirements.
        /// </summary>
        /// <param name="password">The password to validate.</param>
        /// <returns>True if the password is valid; otherwise, false.</returns>
        // todo: Kullanıcı şifresi 256 bit şifreleme ile şifrelenecek...
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
                if (char.IsUpper(password[i]))
                    upperCaseCount++;
            }

            return specialCharactersCount >= 2 && numberCharactersCount >= 2 && password.Length >= 8 && upperCaseCount >= 2;
        }
    }

    /// <summary>
    /// Provides authentication-related services, including generating verification codes and sending emails.
    /// </summary>
    public class AuthService
    {
        private readonly UserSessionWindow userSessionWindow;
        public UserManager userManager { get; set; }
        public EmailService emailService;
        public int verificationCode { get; set; }

        private Timer timer;

        private int counter = 300;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthService"/> class.
        /// </summary>
        public AuthService()
        {
            emailService = new EmailService();
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
        /// <param name="emailSendType">The type of email to send (e.g., user verification, password reset).</param>
        public async Task AccountVerify(User user, EmailSendType emailSendType)
        {
            verificationCode = GenerateVerificationCode();
            if(EmailSendType.UserVerificationEmail == emailSendType) 
                await SendVerificationEmail(user.email, verificationCode.ToString());
            else
                await SendPasswordResetEmail(user, verificationCode.ToString());
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
            if (verificationCode.ToString() == inputCode)
                return true;
            return false;
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
                
                await userManager.RegisterUser();
                userSessionWindow.PanelTransition(Panels.Login);
                return;
            }

            NotificationManagement.ShowNotification(
                "Error", 
                "Invalid code", 
                NotificationType.Error);
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

            NotificationManagement.ShowNotification(
                "Error", 
                "Invalid code", 
                NotificationType.Error);
        }

        /// <summary>
        /// Validates that the timer has not expired.
        /// </summary>
        /// <returns>True if the timer has not expired; otherwise, false.</returns>
        private bool ValidateTimer()
        {
            if (counter >= 0)
                return true;
            return false;
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
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (counter > 0)
            {
                counter--;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    int minutes = counter / 60;
                    int seconds = counter % 60;
                    userSessionWindow.UpdateTimerText($"{minutes}:{seconds}".ToString());
                });

            }
            else
            {
                NotificationManagement.ShowNotification("Error", "Time is up. Please try again.", NotificationType.Error);
                StopTimer();
            }
        }

        /// <summary>
        /// Sends a verification email to the specified email address.
        /// </summary>
        /// <param name="email">The recipient's email address.</param>
        /// <param name="code">The verification code to include in the email.</param>
        public async Task SendVerificationEmail(string email, string code)
            => await emailService.SendMail_SendGrid(new User { email = email }, EmailSendType.UserVerificationEmail, null, code);

        /// <summary>
        /// Sends a weather update email to the specified user.
        /// </summary>
        /// <param name="user">The recipient user.</param>
        /// <param name="weatherData">The weather data to include in the email.</param>
        public async Task SendWeatherUpdateEmail(User user, WeatherData weatherData)
        {
            await emailService.SendMail_SendGrid(user, EmailSendType.WeatherUpdateEmail, weatherData);
        }

        /// <summary>
        /// Sends a password reset email to the specified user.
        /// </summary>
        /// <param name="user">The recipient user.</param>
        /// <param name="code">The verification code to include in the email.</param>
        public async Task SendPasswordResetEmail(User user, string code)
            => await emailService.SendMail_SendGrid(user, EmailSendType.PasswordResetEmail, null, code);

        /// <summary>
        /// Sends an emergency weather alert email to the specified user.
        /// </summary>
        /// <param name="user">The recipient user.</param>
        /// <param name="weatherData">The weather data to include in the email.</param>
        public async Task SendEmergencyWeatherAlertEmail(User user, WeatherData weatherData)
            => await emailService.SendMail_SendGrid(user, EmailSendType.EmergencyWeatherAlertEmail, weatherData);
    }

    /// <summary>
    /// Provides methods for sending emails using SendGrid.
    /// </summary>
    public class EmailService
    {
        private static readonly string[] htmlFilePaths = new string[]
          {
                @"C:\Users\EnesEfeTokta\OneDrive\Belgeler\GitHub\WeatherPlusZeroRepo\WeatherPlusZero\WeatherPlusZero\WeatherPlusZero\Codes\EmailTemplates\UserVerificationEmailHTML.html",
                @"C:\Users\EnesEfeTokta\OneDrive\Belgeler\GitHub\WeatherPlusZeroRepo\WeatherPlusZero\WeatherPlusZero\WeatherPlusZero\Codes\EmailTemplates\WeatherUpdateEmailHTML.html",
                @"C:\Users\EnesEfeTokta\OneDrive\Belgeler\GitHub\WeatherPlusZeroRepo\WeatherPlusZero\WeatherPlusZero\WeatherPlusZero\Codes\EmailTemplates\PasswordResetEmailHTML.html",
                @"C:\Users\EnesEfeTokta\OneDrive\Belgeler\GitHub\WeatherPlusZeroRepo\WeatherPlusZero\WeatherPlusZero\WeatherPlusZero\Codes\EmailTemplates\EmergencyWeatherAlertEmailHTML.html"
          };
        private const string sendGridApiKey = "";

        /// <summary>
        /// Sends an email using SendGrid.
        /// </summary>
        /// <param name="user">The recipient user.</param>
        /// <param name="emailSendType">The type of email to send.</param>
        /// <param name="weatherData">Optional weather data for the email.</param>
        /// <param name="code">Optional verification code for the email.</param>
        public async Task SendMail_SendGrid(User user, EmailSendType emailSendType, WeatherData weatherData = null, string code = null)
        {
            try
            {
                SendGridClient client = new SendGridClient(sendGridApiKey);
                EmailAddress from = new EmailAddress("enesefetokta009@gmail.com", "Weather Zero Plus");
                EmailAddress to = new EmailAddress(user.email, user.namesurname);
                string plainTextContent = "Bu e-posta düz metin olarak gönderilmiştir.";
                string htmlContent = GenerateEmailBody(user, emailSendType, code);
                var msg = MailHelper.CreateSingleEmail(from, to, "Email from Weather Zero Plus...", plainTextContent, htmlContent);

                var response = await client.SendEmailAsync(msg);
            }
            catch (Exception ex)
            {
                NotificationManagement.ShowNotification("Email Send Error" ,$"An error occurred while sending email: {ex.Message}", NotificationType.Error);
            }
        }

        /// <summary>
        /// Generates the HTML email body based on the specified email type.
        /// </summary>
        /// <param name="user">The recipient user.</param>
        /// <param name="emailSendType">The type of email to generate.</param>
        /// <param name="code">Optional verification code.</param>
        /// <returns>The generated HTML email body.</returns>
        private string GenerateEmailBody(User user, EmailSendType emailSendType, string code = null)
        {
            return emailSendType switch
            {
                EmailSendType.UserVerificationEmail => GenerateUserVerificationEmailBody(code, user.email),
                EmailSendType.WeatherUpdateEmail => GenerateWeatherUpdateEmailBody(user),
                EmailSendType.PasswordResetEmail => GeneratePasswordResetEmailBody(user.namesurname, code),
                EmailSendType.EmergencyWeatherAlertEmail => GenerateEmergencyWeatherAlertEmailBody(user),
                _ => throw new ArgumentOutOfRangeException(nameof(emailSendType), emailSendType, null),
            };
        }

        /// <summary>
        /// Generates the HTML body for a user verification email.
        /// </summary>
        /// <param name="code">The verification code.</param>
        /// <param name="toEmail">The recipient's email address.</param>
        /// <returns>The generated HTML body for the user verification email.</returns>
        private string GenerateUserVerificationEmailBody(string code, string toEmail)
        {
            string htmlCode = ReadHTML(EmailSendType.UserVerificationEmail);
            htmlCode =
                htmlCode.Replace("[VERIFICATION_CODE]", code)
                .Replace("[TO_EMAIL]", toEmail);
            return htmlCode;
        }

        /// <summary>
        /// Generates the HTML body for a weather update email.
        /// </summary>
        /// <param name="user">The recipient user.</param>
        /// <returns>The generated HTML body for the weather update email.</returns>
        private string GenerateWeatherUpdateEmailBody(User user)
        {
            string htmlCode = ReadHTML(EmailSendType.WeatherUpdateEmail);
            htmlCode = htmlCode.Replace("[USERNAME]", user.namesurname);
            return htmlCode;
        }

        /// <summary>
        /// Generates the HTML body for a password reset email.
        /// </summary>
        /// <param name="namesurname">The recipient's name.</param>
        /// <param name="code">The verification code.</param>
        /// <returns>The generated HTML body for the password reset email.</returns>
        private string GeneratePasswordResetEmailBody(string namesurname, string code)
        {
            string htmlCode = ReadHTML(EmailSendType.PasswordResetEmail);
            htmlCode = htmlCode.Replace("[USER_NAME]", namesurname)
                .Replace("[VERIFICATION_CODE]", code);
            return htmlCode;
        }

        /// <summary>
        /// Generates the HTML body for an emergency weather alert email.
        /// </summary>
        /// <param name="user">The recipient user.</param>
        /// <returns>The generated HTML body for the emergency weather alert email.</returns>
        private string GenerateEmergencyWeatherAlertEmailBody(User user)
        {
            string htmlCode = ReadHTML(EmailSendType.EmergencyWeatherAlertEmail);
            htmlCode = htmlCode.Replace("[USER_NAME]", user.namesurname);
            return htmlCode;
        }

        /// <summary>
        /// Reads the content of an HTML file based on the specified email type.
        /// </summary>
        /// <param name="emailSendType">The type of email to read the HTML for.</param>
        /// <returns>The HTML content of the specified email type.</returns>
        private string ReadHTML(EmailSendType emailSendType)
        {
            try
            {
                switch (emailSendType)
                {
                    case EmailSendType.UserVerificationEmail:
                        return File.ReadAllText(htmlFilePaths[0]);

                    case EmailSendType.WeatherUpdateEmail:
                        return File.ReadAllText(htmlFilePaths[1]);

                    case EmailSendType.PasswordResetEmail:
                        return File.ReadAllText(htmlFilePaths[2]);

                    case EmailSendType.EmergencyWeatherAlertEmail:
                        return File.ReadAllText(htmlFilePaths[3]);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(emailSendType), emailSendType, "Invalid email send type");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while reading html file: {ex.Message}");
                throw;
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