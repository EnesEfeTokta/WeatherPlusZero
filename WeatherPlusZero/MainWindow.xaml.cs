using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Notification.Wpf;

namespace WeatherPlusZero
{
    public partial class MainWindow : Window
    {

        private DispatcherTimer timer { get; set; }

        private ObservableCollection<FutureDay> WeatherList { get; set; }

        private Point _startPoint { get; set; }
        private bool _isDragging { get; set; }

        private bool isSettingsPanelVisible { get; set; } = false;

        public MainWindow()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            ApplicationStart();
        }

        private void ApplicationStart()
        {
            ApplicationProgress.ApplicationStart();

            // Hide the add city and search clear buttons.
            CancelCitySelectButton.Visibility = Visibility.Hidden;
            AddCitySelectButton.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// It runs when the user enters the TextBox...
        /// </summary>
        private void CityNameSearchTextBox_GoFocus(object sender, RoutedEventArgs e)
        {
            if (CityNameSearchTextBox.Text == "Search for city...")
            {
                CityNameSearchTextBox.Text = "";
                CityNameSearchTextBox.Foreground = Brushes.LightGray;
            }
        }

        /// <summary>
        /// Runs when the user exits the TextBox...
        /// </summary>
        private void CityNameSearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CityNameSearchTextBox.Text))
            {
                CityNameSearchTextBox.Text = "Search for city...";
                CityNameSearchTextBox.Foreground = Brushes.White;
            }
        }

        /// <summary>
        /// The user confirms the city he typed in with the ENTER key.
        /// </summary>
        private void CityNameSearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                StartSearchCity();
                e.Handled = false;
            }
        }

        /// <summary>
        /// UI function that starts the user city name search.
        /// </summary>
        private void CityNameSearchClickButton(object sender, RoutedEventArgs e)
            => StartSearchCity();

        private async void StartSearchCity()
        {
            string cityName = CityNameSearchTextBox.Text;
            if (cityName == "Search for city...")
                return;

            bool successStatus = await SearchCity.SearchCityName(cityName);

            if (successStatus)
            {
                AddCitySelectButton.Visibility = Visibility.Visible;
                CancelCitySelectButton.Visibility = Visibility.Visible;
            }
            else
            {
                NotificationManagement.ShowNotification(
                    "Error", 
                    "Please enter a valid city name. No special characters, emoji and numeric characters.", 
                    NotificationType.Warning);
            }
        }

        private void AddCitySelectButton_Click(object sender, RoutedEventArgs e)
        {
            CityButtonOperations();
            SearchCity.AddSelectCity();
        }

        private void CanselCitySelectButton_Click(object sender, RoutedEventArgs e)
        {
            CityButtonOperations();
            SearchCity.CanselCitySelect();
        }

        private void CityButtonOperations()
        {
            AddCitySelectButton.Visibility = Visibility.Hidden;
            CancelCitySelectButton.Visibility = Visibility.Hidden;
            CityNameSearchTextBox.Text = "Search for city...";
            CityNameSearchTextBox.Foreground = Brushes.White;
        }

        /// <summary>
        /// Changes the background image of the home screen.
        /// </summary>
        /// <param name="path">Path to the image to be replaced.</param>
        public void UpdateBackgroundImage(string path)
        {
            BackgroundImageBrush.ImageSource = new BitmapImage(new Uri(path, UriKind.Absolute));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="temp"></param>
        /// <param name="windSpeed"></param>
        /// <param name="windDir"></param>
        /// <param name="degrees"></param>
        /// <param name="humid"></param>
        /// <param name="pressure"></param>
        /// <returns></returns>
        public void UpdateMainWeatherParametersText(string temp = null, string windSpeed = null, string windDir = null, string degrees = null, string humid = null, string pressure = null)
        {
            if (temp != null)
                TemperatureValueTextBlock.Text = $"{temp}℃";

            if (windSpeed != null)
                WindValueTextBlock.Text = $"{degrees} °{windDir} {windSpeed}km.h";

            if (humid != null)
                HumidValueTextBlock.Text = $"%{humid}";

            if (pressure != null)
                PressureValueTextBlock.Text = $"{pressure} hPa/mb";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dayNumber"></param>
        /// <param name="dayName"></param>
        /// <param name="month"></param>
        /// <param name="year"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        public void UpdateDateTimeText(string dayNumber = null, string dayName = null, string month = null, string year = null, string hour = null, string minute = null)
        {
            if (dayNumber != null && month != null)
                DateTextBlock.Text = $"{month} {dayNumber}";

            if (hour != null && minute != null && dayName != null)
                TimeAndDayTextBlock.Text = $"{hour}:{minute} {dayName}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        public void UpdateLocationText(string location = null)
        {
            if (location != null)
                LocationTextBlock.Text = location;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iconPath"></param>
        public void UpdateWeatherStatusIcon(string iconPath = null)
        {
            if (iconPath != null)
            {
                BitmapImage newImage = new BitmapImage();
                newImage.BeginInit();
                newImage.UriSource = new Uri(iconPath);
                newImage.CacheOption = BitmapCacheOption.OnLoad;
                newImage.EndInit();

                WeatherStatusIconImage.Source = newImage;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        public bool SetIconImage(string filePath, Image image)
        {
            if (filePath == null && image == null)
            {
                return false;
            }

            BitmapImage newImage = new BitmapImage();
            newImage.BeginInit();
            newImage.UriSource = new Uri(filePath, UriKind.Absolute);
            newImage.CacheOption = BitmapCacheOption.OnLoad;
            newImage.EndInit();

            image.Source = newImage;

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="progressBar"></param>
        /// <returns></returns>
        public bool SetDayInformation(double value, ProgressBar progressBar, Color color)
        {
            if (progressBar == null)
            {
                return false;
            }

            progressBar.Value = value;
            progressBar.Foreground = new SolidColorBrush(color);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public void UpdateDayNightBar(double hour)
        {
            if (hour >= 16 || hour < 6)
            {
                SetDayInformation(hour, TimeDayProgressBar, Colors.DarkBlue);
                SetIconImage("pack://application:,,,/Images/MoonIcon.png", WeatherTimeStatusIconImage);
            }
            else
            {
                SetDayInformation(hour, TimeDayProgressBar, Colors.Yellow);
                SetIconImage("pack://application:,,,/Images/SunIcon.png", WeatherTimeStatusIconImage);
            }

            double fillWidth = (TimeDayProgressBar.Value / TimeDayProgressBar.Maximum) * (TimeDayProgressBar.ActualWidth);

            WeatherStatusTransform.X = fillWidth - (WeatherTimeStatusIconImage.ActualWidth / 2);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="days"></param>
        public void SetFutureDays(ObservableCollection<FutureDay> days)
        {
            WeatherList = days;
            WeatherItemsControl.ItemsSource = WeatherList;
        }

        private void WeatherScrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(WeatherScrollViewer);
            _isDragging = true;
            WeatherScrollViewer.CaptureMouse();
        }

        private void WeatherScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point currentPoint = e.GetPosition(WeatherScrollViewer);
                double deltaX = _startPoint.X - currentPoint.X;
                _startPoint = currentPoint;

                WeatherScrollViewer.ScrollToHorizontalOffset(WeatherScrollViewer.HorizontalOffset + deltaX);
            }
        }

        private void WeatherScrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            WeatherScrollViewer.ReleaseMouseCapture();
        }

        /// <summary>
        /// Shows or hides the settings panel.
        /// </summary>
        private void SettingsShowButton_Click(object sender, RoutedEventArgs e)
        {
            if (isSettingsPanelVisible)
            {
                SettingsPanelBorder.Visibility = Visibility.Hidden;
            }
            else
            {
                SettingsPanelBorder.Visibility = Visibility.Visible;
                SettingsPanelManager.OpenSettingsPanel();
            }

            isSettingsPanelVisible = !isSettingsPanelVisible;
        }

        /// <summary>
        /// Logs the user out of the application.
        /// </summary>
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
            => SettingsPanelManager.LogOut();

        /// <summary>
        /// Updates the settings panel with the received data.
        /// </summary>
        /// <param name="data">...</param>
        public void UpdateSettingsPanel(ApplicationActivityData data)
        {
            UserNameSurnameTextBlock.Text = "Name Surname: " + data.UserNameSurname;
            UserEmailTextBlock.Text = "Email: " + data.UserEmail;

            InAppCheckBox.IsChecked = data.IsInAppNotificationOn;
            DailyWeatherCheckBox.IsChecked = data.IsDailyWeatherEmailsOn;
            EmergencyWeatherCheckBox.IsChecked = data.IsImportantWeatherEmailsOn;

            UserSelectCityTextBlock.Text = "Selected City: " + data.SelectCity;
        }

        private void NotificationsCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (!isSettingsPanelVisible) // If the settings panel is not visible, do not return.
                return;
            SettingsPanelManager.UpdateNotificationsApplicationActivityDataAsync(
                InAppCheckBox.IsChecked ?? false,
                DailyWeatherCheckBox.IsChecked ?? false,
                EmergencyWeatherCheckBox.IsChecked ?? false);
        }

        /// <summary>
        /// Removes the city from the user's list.
        /// </summary>
        public void RemoveCity_Click(object sender, RoutedEventArgs e)
            => SettingsPanelManager.ClearCity();

        /// <summary>
        /// Opens the GitHub page of the project.
        /// </summary>
        private void GoToGitHub_Click(object sender, RoutedEventArgs e)
            => SettingsPanelManager.GoToGitHubPage();
    }
}