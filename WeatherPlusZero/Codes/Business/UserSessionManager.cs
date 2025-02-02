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

namespace WeatherPlusZero
{
    public class UserManager
    {
        private readonly DataBase dataBase;
        private readonly NotificationManagement notificationManagement;
        private readonly AuthenticationValidator authenticationValidator;
        private readonly AuthService authService;
        private readonly UserSessionWindow userSessionWindow;

        public UserManager()
        {
            dataBase = new DataBase();
            notificationManagement = new NotificationManagement();
            authenticationValidator = new AuthenticationValidator();
            authService = new AuthService();

            userSessionWindow = Application.Current.Windows.OfType<UserSessionWindow>().FirstOrDefault();
        }

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

        public async Task Register(User user)
        {
            try
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

                // todo: E-Posta kodu doğrulandıktan sonra kullanıcı kayıt olacak.
                bool registerStatus = await dataBase.RegisterUserOwnAuth(user);

                if (registerStatus)
                {
                    // Register successful
                    NotificationManagement.ShowNotification("Verification Code", $"We have sent a six-digit verification code to {user.email}. Please enter the code...", NotificationType.Information);
                    await authService.SendVerificationEmail(user.email);
                    userSessionWindow.PanelTransition(Panels.EmailVerification);
                }
                else
                {
                    // Register failed
                    NotificationManagement.ShowNotification("Register Error", "Registration failed. Please try again to enter the information...", NotificationType.Error);
                }
            }
            catch (Exception ex)
            {
                NotificationManagement.ShowNotification("Error", $"An error occurred during registration: {ex.Message}", NotificationType.Error);
            }
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


        public bool ValidateNameSurname(string nameSurname)
        {
            return nameSurname.Length >= 5;
        }

        public bool ValidateEmail(string email)
        {
            return email.Contains("@") && email.Length >= 5;
        }

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

    public class AuthService
    {
        public EmailService emailService;
        public int verificationCode;

        public AuthService()
        {
            emailService = new EmailService();
        }

        public int GenerateVerificationCode()
        {
            Random random = new Random();
            verificationCode = random.Next(100000, 999999);
            return verificationCode;
        }

        public bool AccountVerify(string inputCode)
        {
            return inputCode == verificationCode.ToString();
        }

        public async Task SendVerificationEmail(string email)
        {
            await emailService.SendMail_SendGrid(new User { email = email }, EmailSendType.UserVerificationEmail, null, GenerateVerificationCode().ToString());
        }

        public async Task SendWeatherUpdateEmail(User user, WeatherData weatherData)
        {
            await emailService.SendMail_SendGrid(user, EmailSendType.WeatherUpdateEmail, weatherData);
        }

        public async Task SendPasswordResetEmail(User user)
        {
            await emailService.SendMail_SendGrid(user, EmailSendType.PasswordResetEmail);
        }

        public async Task SendEmergencyWeatherAlertEmail(User user, WeatherData weatherData)
        {
            await emailService.SendMail_SendGrid(user, EmailSendType.EmergencyWeatherAlertEmail, weatherData);
        }
    }
    public class EmailService
    {
        private static readonly string[] htmlFilePaths = new string[]
          {
                @"C:\Users\EnesEfeTokta\OneDrive\Belgeler\GitHub\WeatherPlusZeroRepo\WeatherPlusZero\WeatherPlusZero\WeatherPlusZero\Codes\EmailTemplates\UserVerificationEmailHTML.html",
                @"C:\Users\EnesEfeTokta\OneDrive\Belgeler\GitHub\WeatherPlusZeroRepo\WeatherPlusZero\WeatherPlusZero\WeatherPlusZero\Codes\EmailTemplates\WeatherUpdateEmailHTML.html",
                @"C:\Users\EnesEfeTokta\OneDrive\Belgeler\GitHub\WeatherPlusZeroRepo\WeatherPlusZero\WeatherPlusZero\WeatherPlusZero\Codes\EmailTemplates\PasswordResetEmailHTML.html",
                @"C:\Users\EnesEfeTokta\OneDrive\Belgeler\GitHub\WeatherPlusZeroRepo\WeatherPlusZero\WeatherPlusZero\WeatherPlusZero\Codes\EmailTemplates\EmergencyWeatherAlertEmailHTML.html"
          };
        private const string sendGridApiKey = "API_KEY";

        #region SendMail_v3 SendGrid
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
        #endregion

        private string GenerateEmailBody(User user, EmailSendType emailSendType, string code = null)
        {
            switch (emailSendType)
            {
                case EmailSendType.UserVerificationEmail:
                    return GenerateUserVerificationEmailBody(code, user.email);

                case EmailSendType.WeatherUpdateEmail:
                    return GenerateWeatherUpdateEmailBody(user);

                case EmailSendType.PasswordResetEmail:
                    return GeneratePasswordResetEmailBody(user, code);

                case EmailSendType.EmergencyWeatherAlertEmail:
                    return GenerateEmergencyWeatherAlertEmailBody(user);
                default:
                    throw new ArgumentOutOfRangeException(nameof(emailSendType), emailSendType, null);

            }
        }

        private string GenerateUserVerificationEmailBody(string code, string toEmail)
        {
            string htmlCode = ReadHTML(EmailSendType.UserVerificationEmail);
            htmlCode =
                htmlCode.Replace("[VERIFICATION_CODE]", code)
                .Replace("[TO_EMAIL]", toEmail);
            return htmlCode;
        }

        private string GenerateWeatherUpdateEmailBody(User user)
        {
            string htmlCode = ReadHTML(EmailSendType.WeatherUpdateEmail);
            htmlCode = htmlCode.Replace("[USERNAME]", user.namesurname);
            return htmlCode;
        }
        private string GeneratePasswordResetEmailBody(User user, string code)
        {
            string htmlCode = ReadHTML(EmailSendType.PasswordResetEmail);
            htmlCode = htmlCode.Replace("[USER_NAME]", user.namesurname)
                .Replace("[VERIFICATION_CODE]", code);
            return htmlCode;

        }
        private string GenerateEmergencyWeatherAlertEmailBody(User user)
        {
            string htmlCode = ReadHTML(EmailSendType.EmergencyWeatherAlertEmail);
            htmlCode = htmlCode.Replace("[USER_NAME]", user.namesurname);
            return htmlCode;
        }

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

    public enum EmailSendType { UserVerificationEmail, WeatherUpdateEmail, PasswordResetEmail, EmergencyWeatherAlertEmail }

    public enum ValidateType { All, NameSurname, Email, Password }
}