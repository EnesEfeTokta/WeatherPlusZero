﻿using Supabase.Gotrue;
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
        private readonly UserManager _userManager;
        private readonly Dictionary<Panels, Border> panelBorders;
        private readonly BitmapImage tickIcon;
        private readonly BitmapImage errorIcon;

        public UserSessionWindow()
        {
            _userManager = new UserManager();
            tickIcon = new BitmapImage(new Uri("Images/TickIcon.png", UriKind.Relative));
            errorIcon = new BitmapImage(new Uri("Images/ErrorIcon.png", UriKind.Relative));

            InitializeComponent();

            panelBorders = new Dictionary<Panels, Border>
            {
                { Panels.Login, LoginPanelBackgroundBorder },
                { Panels.Register, RegisterPanelBackgroundBorder },
                { Panels.Forgot, ForgotPanelBackgroundBorder },
                { Panels.EmailVerification, EmailVerificationPanelBackgroundBorder },
                { Panels.ChangePassword, ChangePasswordPanelBackgroundBorder }
            };

            ApplicationStart();
        }

        private void ApplicationStart() => PanelTransition(Panels.Login);

        private void LogInClickButton(object sender, RoutedEventArgs e)
        {
            
        }

        private void Login_EmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool iconStatus = !_userManager.ValidateFieldsContinuously(ValidateType.Email, Login_EmailTextBox.Text);
            TextBoxInputIcon(Panels.Login, TextBoxInputIconType.Email, iconStatus);
        }

        private void Login_PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool iconStatus = !_userManager.ValidateFieldsContinuously(ValidateType.Password, Login_PasswordTextBox.Text);
            TextBoxInputIcon(Panels.Login, TextBoxInputIconType.Password, iconStatus);
        }

        private void Register_NamesurnameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool iconStatus = !_userManager.ValidateFieldsContinuously(ValidateType.NameSurname, Register_NamesurnameTextBox.Text);
            TextBoxInputIcon(Panels.Register, TextBoxInputIconType.Namesurname, iconStatus);
        }

        private void Register_EmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool iconStatus = !_userManager.ValidateFieldsContinuously(ValidateType.Email, Register_EmailTextBox.Text);
            TextBoxInputIcon(Panels.Register, TextBoxInputIconType.Email, iconStatus);
        }

        private void Register_PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool iconStatus = !_userManager.ValidateFieldsContinuously(ValidateType.Password, Register_PasswordTextBox.Text);
            TextBoxInputIcon(Panels.Register, TextBoxInputIconType.Password, iconStatus);
        }

        private void Forgot_NamesurnameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool iconStatus = !_userManager.ValidateFieldsContinuously(ValidateType.NameSurname, Forgot_NamesurnameTextBox.Text);
            TextBoxInputIcon(Panels.Forgot, TextBoxInputIconType.Namesurname, iconStatus);
        }

        private void Forgot_EmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool iconStatus = !_userManager.ValidateFieldsContinuously(ValidateType.Email, Forgot_EmailTextBox.Text);
            TextBoxInputIcon(Panels.Forgot, TextBoxInputIconType.Email, iconStatus);
        }

        private void EmailVerification_EmailKeyCodeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            return;
        }

        private void ChangePassword_ChangePasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool iconStatus = !_userManager.ValidateFieldsContinuously(ValidateType.Password, ChangePassword_ChangePasswordTextBox.Text);
            TextBoxInputIcon(Panels.ChangePassword, TextBoxInputIconType.Password, iconStatus);
        }

        private void RegisterClickButton(object sender, RoutedEventArgs e) 
            => _userManager.Register(new User { email = Register_EmailTextBox.Text, password = Register_PasswordTextBox.Text });

        private void RegisterPanelClickButton(object sender, RoutedEventArgs e) 
            => PanelTransition(Panels.Register);

        private void LogInPanelClickButton(object sender, RoutedEventArgs e)
            => PanelTransition(Panels.Login);

        private void ForgotPasswordClickButton(object sender, RoutedEventArgs e) 
            => PanelTransition(Panels.Forgot);

        private void ForgotClickButton(object sender, RoutedEventArgs e) 
            => PanelTransition(Panels.EmailVerification);

        private void EmailVerificationClickButton(object sender, RoutedEventArgs e) 
            => PanelTransition(Panels.ChangePassword);

        private void ChangePasswordClickButton(object sender, RoutedEventArgs e) 
            => PanelTransition(Panels.Login);

        private void PanelTransition(Panels panelToShow)
        {
            foreach (var panel in panelBorders)
            {
                panel.Value.Visibility = panel.Key == panelToShow ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void TextBoxInputIcon(Panels panel, TextBoxInputIconType iconType, bool iconStatus)
        {
            Image statusIcon = GetStatusIcon(panel, iconType);

            if (iconStatus)
            {
                statusIcon.Source = tickIcon;
                return;
            }
            statusIcon.Source = errorIcon;
            return;
        }
        
        private Image GetStatusIcon(Panels panel, TextBoxInputIconType iconType)
        {
            switch (panel, iconType)
            {
                case (Panels.Login, TextBoxInputIconType.Email): return Login_EmailStatusIconImage;
                case (Panels.Login, TextBoxInputIconType.Password): return Login_PasswordStatusIconImage;
                case (Panels.Register, TextBoxInputIconType.Email): return Register_EmailStatusIconImage;
                case (Panels.Register, TextBoxInputIconType.Password): return Register_PasswordStatusIconImage;
                case (Panels.Register, TextBoxInputIconType.Namesurname): return Register_NamesurnameStatusIconImage;
                case (Panels.Forgot, TextBoxInputIconType.Email): return Forgot_EmailStatusIconImage;
                case (Panels.ChangePassword, TextBoxInputIconType.Password): return ChangePassword_StatusIconImage;
                default: return null;
            }
        }

        public enum Panels { Login, Register, Forgot, EmailVerification, ChangePassword }

        public enum TextBoxInputIconStatus { Error, Success }

        // todo: TextBoxInputIconStatus ile ValidateType aynı enumlardır. O yüzden birleştirilebilir.
        public enum TextBoxInputIconType { Email, Password, Namesurname }
    }
}