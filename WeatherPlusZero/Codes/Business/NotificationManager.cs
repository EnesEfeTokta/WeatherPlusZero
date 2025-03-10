﻿using Notification.Wpf;

namespace WeatherPlusZero
{
    public static class NotificationManagement
    {
        public static void ShowNotification(string title, string message, NotificationType notificationType = NotificationType.Information)
        {
            NotificationManager notificationManager = new NotificationManager();
            notificationManager.Show(title, message, notificationType);
        }
    }
}