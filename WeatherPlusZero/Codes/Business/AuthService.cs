using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using Notification.Wpf;

namespace WeatherPlusZero
{
    /// <summary>
    /// Provides authentication-related services, including generating verification codes, sending emails, and managing timers.
    /// </summary>
    public static class AuthService
    {
        private static UserSessionWindow userSessionWindow;

        public static int verificationCode { get; set; }
        private static Timer timer;
        private static int counter = 300; // Örneğin 300 saniye = 5 dakika

        /// <summary>
        /// Initializes the <see cref="AuthService"/> class.
        /// </summary>
        static AuthService()
        {
            userSessionWindow = Application.Current.Windows.OfType<UserSessionWindow>().FirstOrDefault();
        }

        /// <summary>
        /// Generates a random six-digit verification code.
        /// </summary>
        /// <returns>A random six-digit verification code.</returns>
        public static int GenerateVerificationCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999);
        }

        /// <summary>
        /// Handles the account verification process, sending a verification code to the user's email address.
        /// </summary>
        /// <param name="user">The user to verify.</param>
        /// <param name="emailSendType">The type of email to send (user verification or password reset).</param>
        public static async Task AccountVerify(User user, EmailSendType emailSendType)
        {
            verificationCode = GenerateVerificationCode();

            string htmlCode = HTMLService.GetHTMLCode(emailSendType);

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

            await EmailService.SendMail_SendGrid(user.namesurname, user.email, htmlCode);

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
        private static bool CodeVerify(string inputCode)
        {
            return verificationCode.ToString() == inputCode;
        }

        /// <summary>
        /// Handles the verification process for new user registration.
        /// </summary>
        /// <param name="inputCode">The verification code entered by the user.</param>
        public static async Task RegisterVerificationCode(string inputCode)
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
        public static void ForgotVerificationCode(string inputCode)
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
        private static bool ValidateTimer()
        {
            return counter >= 0;
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        private static void StartTimer()
        {
            timer = new Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        private static void StopTimer()
        {
            timer.Stop();
            timer.Dispose();
            timer = null;
        }

        /// <summary>
        /// Handles the elapsed event for the timer.
        /// </summary>
        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
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
}
