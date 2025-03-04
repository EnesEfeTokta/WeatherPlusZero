using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WeatherPlusZero
{
    public partial class UserSessionWindow : Window
    {
        private readonly Dictionary<Panels, Border> panelBorders;
        private readonly BitmapImage tickIcon;
        private readonly BitmapImage errorIcon;

        private readonly TextBox[] textBoxes;

        private Panels isEmailVerificationPanelFrom;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserSessionWindow"/> class.
        /// Sets up the UI components, authentication services, and loads resources.
        /// </summary>
        public UserSessionWindow()
        {
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

            textBoxes = new TextBox[]
            {
                Login_EmailTextBox,
                Login_PasswordTextBox,
                Register_NamesurnameTextBox,
                Register_EmailTextBox,
                Register_PasswordTextBox,
                Forgot_NamesurnameTextBox,
                Forgot_EmailTextBox,
                EmailVerification_KeyCodeTextBox,
                ChangePassword_ChangePasswordTextBox
            };

            ApplicationStart();
        }

        /// <summary>
        /// Initializes the application by transitioning to the login panel.
        /// </summary>
        private void ApplicationStart()
        {
            UserManager.StartApplication();
            PanelTransition(Panels.Login);
        }

        /// <summary>
        /// Handles the login process when the login button is clicked.
        /// </summary>
        private async void LogInClickButton(object sender, RoutedEventArgs e)
            => await UserManager.LogIn(new User { email = Login_EmailTextBox.Text, password = Login_PasswordTextBox.Text });

        /// <summary>
        /// Handles the registration process when the register button is clicked.
        /// </summary>
        private void RegisterClickButton(object sender, RoutedEventArgs e)
            => UserManager.Register(new User { namesurname = Register_NamesurnameTextBox.Text, email = Register_EmailTextBox.Text, password = Register_PasswordTextBox.Text });

        /// <summary>
        /// Handles the forgot password process when the forgot button is clicked.
        /// </summary>
        private async void ForgotClickButton(object sender, RoutedEventArgs e)
            => await UserManager.Forgot(new User { namesurname = Forgot_NamesurnameTextBox.Text, email = Forgot_EmailTextBox.Text });

        /// <summary>
        /// Handles the email verification process when the email verification button is clicked.
        /// </summary>
        private async void EmailVerificationClickButton(object sender, RoutedEventArgs e)
        {
            switch (isEmailVerificationPanelFrom)
            {
                case Panels.Register:
                    await AuthService.RegisterVerificationCode(EmailVerification_KeyCodeTextBox.Text);
                    break;
                case Panels.Forgot:
                    AuthService.ForgotVerificationCode(EmailVerification_KeyCodeTextBox.Text);
                    break;
            }
        }

        /// <summary>
        /// Transitions the UI to the login panel.
        /// </summary>
        private async void ChangePasswordClickButton(object sender, RoutedEventArgs e)
            => await UserManager.ChangePassword(ChangePassword_ChangePasswordTextBox.Text);

        /// <summary>
        /// Updates the input icon based on the validation status of the email text box in the login panel.
        /// </summary>
        private void Login_EmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool iconStatus = UserManager.ValidateFieldsContinuously(ValidateType.Email, Login_EmailTextBox.Text);
            TextBoxInputIcon(Panels.Login, TextBoxInputIconType.Email, iconStatus);
        }

        /// <summary>
        /// Updates the input icon based on the validation status of the password text box in the login panel.
        /// </summary>
        private void Login_PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool iconStatus = UserManager.ValidateFieldsContinuously(ValidateType.Password, Login_PasswordTextBox.Text);
            TextBoxInputIcon(Panels.Login, TextBoxInputIconType.Password, iconStatus);
        }

        /// <summary>
        /// Updates the input icon based on the validation status of the namesurname text box in the register panel.
        /// </summary>
        private void Register_NamesurnameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool iconStatus = UserManager.ValidateFieldsContinuously(ValidateType.NameSurname, Register_NamesurnameTextBox.Text);
            TextBoxInputIcon(Panels.Register, TextBoxInputIconType.Namesurname, iconStatus);
        }

        /// <summary>
        /// Updates the input icon based on the validation status of the email text box in the register panel.
        /// </summary>
        private void Register_EmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool iconStatus = UserManager.ValidateFieldsContinuously(ValidateType.Email, Register_EmailTextBox.Text);
            TextBoxInputIcon(Panels.Register, TextBoxInputIconType.Email, iconStatus);
        }

        /// <summary>
        /// Updates the input icon based on the validation status of the password text box in the register panel.
        /// </summary>
        private void Register_PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool iconStatus = UserManager.ValidateFieldsContinuously(ValidateType.Password, Register_PasswordTextBox.Text);
            TextBoxInputIcon(Panels.Register, TextBoxInputIconType.Password, iconStatus);
        }

        /// <summary>
        /// Updates the input icon based on the validation status of the namesurname text box in the forgot password panel.
        /// </summary>
        private void Forgot_NamesurnameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool iconStatus = UserManager.ValidateFieldsContinuously(ValidateType.NameSurname, Forgot_NamesurnameTextBox.Text);
            TextBoxInputIcon(Panels.Forgot, TextBoxInputIconType.Namesurname, iconStatus);
        }

        /// <summary>
        /// Updates the input icon based on the validation status of the email text box in the forgot password panel.
        /// </summary>
        private void Forgot_EmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool iconStatus = UserManager.ValidateFieldsContinuously(ValidateType.Email, Forgot_EmailTextBox.Text);
            TextBoxInputIcon(Panels.Forgot, TextBoxInputIconType.Email, iconStatus);
        }

        /// <summary>
        /// This is intentionally left empty.
        /// </summary>
        private void EmailVerification_EmailKeyCodeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            return;
        }

        /// <summary>
        /// Updates the input icon based on the validation status of the password text box in the change password panel.
        /// </summary>
        private void ChangePassword_ChangePasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool iconStatus = UserManager.ValidateFieldsContinuously(ValidateType.Password, ChangePassword_ChangePasswordTextBox.Text);
            TextBoxInputIcon(Panels.ChangePassword, TextBoxInputIconType.Password, iconStatus);
        }

        /// <summary>
        /// Transitions the UI to the register panel.
        /// </summary>
        private void RegisterPanelClickButton(object sender, RoutedEventArgs e)
            => PanelTransition(Panels.Register);

        /// <summary>
        /// Transitions the UI to the login panel.
        /// </summary>
        private void LogInPanelClickButton(object sender, RoutedEventArgs e)
            => PanelTransition(Panels.Login);

        /// <summary>
        /// Transitions the UI to the forgot password panel.
        /// </summary>
        private void ForgotPasswordClickButton(object sender, RoutedEventArgs e)
            => PanelTransition(Panels.Forgot);

        /// <summary>
        /// Transitions the UI to the specified panel and resets the text boxes.
        /// </summary>
        /// <param name="panelToShow">The panel to transition to.</param>
        public void PanelTransition(Panels panelToShow)
        {
            foreach (var panel in panelBorders)
            {
                panel.Value.Visibility = panel.Key == panelToShow ? Visibility.Visible : Visibility.Collapsed;
            }

            switch (panelToShow)
            {
                case Panels.Register:
                    isEmailVerificationPanelFrom = Panels.Register;
                    Register_NamesurnameTextBox.Focus();
                    break;
                case Panels.Forgot:
                    isEmailVerificationPanelFrom = Panels.Forgot;
                    Forgot_NamesurnameTextBox.Focus();
                    break;
                case Panels.EmailVerification:
                    EmailVerification_KeyCodeTextBox.Focus();
                    break;
                case Panels.ChangePassword:
                    ChangePassword_ChangePasswordTextBox.Focus();
                    break;
            }

            ResetTextBoxes();
        }

        /// <summary>
        /// Sets the appropriate status icon (tick or error) for a given text box based on its validation status.
        /// </summary>
        /// <param name="panel">The panel the text box belongs to.</param>
        /// <param name="iconType">The type of text box input.</param>
        /// <param name="iconStatus">The validation status (true for valid, false for invalid).</param>
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

        /// <summary>
        /// Gets the status icon image associated with a given panel and text box input type.
        /// </summary>
        /// <param name="panel">The panel the text box belongs to.</param>
        /// <param name="iconType">The type of text box input.</param>
        /// <returns>The <see cref="Image"/> control representing the status icon.</returns>
        private Image GetStatusIcon(Panels panel, TextBoxInputIconType iconType)
        {
            switch (panel, iconType)
            {
                case (Panels.Login, TextBoxInputIconType.Email): return Login_EmailStatusIconImage;
                case (Panels.Login, TextBoxInputIconType.Password): return Login_PasswordStatusIconImage;
                case (Panels.Register, TextBoxInputIconType.Email): return Register_EmailStatusIconImage;
                case (Panels.Register, TextBoxInputIconType.Password): return Register_PasswordStatusIconImage;
                case (Panels.Register, TextBoxInputIconType.Namesurname): return Register_NamesurnameStatusIconImage;
                case (Panels.Forgot, TextBoxInputIconType.Namesurname): return Forgot_NamesurnameStatusIconImage;
                case (Panels.Forgot, TextBoxInputIconType.Email): return Forgot_EmailStatusIconImage;
                case (Panels.ChangePassword, TextBoxInputIconType.Password): return ChangePassword_StatusIconImage;
                default: return null;
            }
        }

        /// <summary>
        /// Updates the timer text displayed in the email verification panel.
        /// </summary>
        /// <param name="timer">The new timer text to display.</param>
        public void UpdateTimerText(string timer) => EmailVerification_TimerTextBlock.Text = timer;

        /// <summary>
        /// Resets all text boxes to their default state (empty text).
        /// </summary>
        private void ResetTextBoxes()
        {
            foreach (var textBox in textBoxes)
            {
                textBox.Text = "";
            }
        }

        /// <summary>
        /// Shows or hides the wait GIF based on the specified value.
        /// </summary>
        /// <param name="show">True to show the GIF, false to hide it.</param>
        public void ShowWaitGift(bool show)
        {
            if (show)
            {
                WaitGifBorder.Visibility = Visibility.Visible;
                return;
            }
            WaitGifBorder.Visibility = Visibility.Hidden;
        }
    }

    /// <summary>
    /// Defines the types of text box inputs for displaying status icons.
    /// </summary>
    public enum TextBoxInputIconType { Email, Password, Namesurname }

    /// <summary>
    /// Defines the different panels available in the user session window.
    /// </summary>
    public enum Panels { Login, Register, Forgot, EmailVerification, ChangePassword }
}