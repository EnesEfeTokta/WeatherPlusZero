using Supabase.Gotrue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WeatherPlusZero
{
    public partial class UserSessionWindow : Window
    {
        UserManager userManager;

        public UserSessionWindow()
        {
            userManager = new UserManager();
            InitializeComponent();

            ApplicationStart();
        }

        private void ApplicationStart()
        {
            LoginPanelBackgroundBorder.Visibility = Visibility.Visible;

            RegisterPanelBackgroundBorder.Visibility = Visibility.Collapsed;

            EmailVerificationPanelBackgroundBorder.Visibility = Visibility.Collapsed;

            ChangePasswordPanelBackgroundBorder.Visibility = Visibility.Collapsed;
        }

        private void LogInClickButton(object sender, RoutedEventArgs e)
        {
            User user = new User
            {
                email = Login_EmailTextBox.Text,
                password = Login_PasswordTextBox.Password
            };

            userManager.LogIn(user);
        }

        private void RegisterClickButton(object sender, RoutedEventArgs e)
        {
            User user = new User
            {
                email = Register_EmailTextBox.Text,
                password = Register_PasswordTextBox.Password
            };

            userManager.Register(user);
        }

        private void RegisterPanelClickButton(object sender, RoutedEventArgs e)
        {
            RegisterPanelBackgroundBorder.Visibility = Visibility.Visible;
            LoginPanelBackgroundBorder.Visibility = Visibility.Collapsed;
        }

        private void LogInPanelClickButton(object sender, RoutedEventArgs e)
        {
            LoginPanelBackgroundBorder.Visibility = Visibility.Visible;
            RegisterPanelBackgroundBorder.Visibility = Visibility.Collapsed;
        }
    }
}
