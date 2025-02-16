using Microsoft.Extensions.Configuration;
using Notification.Wpf;
using SendGrid.Helpers.Mail;
using SendGrid;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WeatherPlusZero
{
    /// <summary>
    /// Provides methods for sending emails using SendGrid.
    /// </summary>
    public static class EmailService
    {
        private static readonly string sendGridApiKey;
        private static readonly IConfiguration Configuration;

        static EmailService()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            sendGridApiKey = Configuration["Authentication:SendGrid_ApiKey"];
        }

        /// <summary>
        /// Sends an email using SendGrid.
        /// </summary>
        /// <param name="user">The recipient user.</param>
        /// <param name="htmlCode">The HTML content of the email.</param>
        public static async Task SendMail_SendGrid(User user, string htmlCode)
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
    /// Defines the types of emails that can be sent.
    /// </summary>
    public enum EmailSendType { UserVerificationEmail, WeatherUpdateEmail, PasswordResetEmail, EmergencyWeatherAlertEmail }
}
