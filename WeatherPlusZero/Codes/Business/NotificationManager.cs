using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Toolkit.Uwp.Notifications;

using Notification.Wpf;

namespace WeatherPlusZero
{
    public class Notification_
    {
        public void ShowNotification(string title, string message, NotificationType notificationType = NotificationType.Information)
        {
            NotificationManager notificationManager = new NotificationManager();
            notificationManager.Show(title, message, notificationType);
        }
    }
}