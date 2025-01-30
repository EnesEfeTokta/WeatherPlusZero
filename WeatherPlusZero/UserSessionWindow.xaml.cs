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
        }

        private void LogInClickButton(object sender, RoutedEventArgs e)
        {
            User user = new User
            {
                email = EmailTextBox.Text,
                password = PasswordTextBox.Password
            };

            userManager.LogIn(user);
        }
    }
}
