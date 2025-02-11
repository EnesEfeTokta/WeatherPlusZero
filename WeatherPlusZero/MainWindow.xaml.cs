using Supabase.Gotrue;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WeatherPlusZero
{
    public partial class MainWindow : Window
    {

        private DispatcherTimer timer { get; set; }

        private ObservableCollection<FutureDay> WeatherList { get; set; }

        private Point _startPoint;
        private bool _isDragging;

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
            ApplicationProgress applicationProgress = new ApplicationProgress();
            applicationProgress.ApplicationStart();

            //HelloCard helloCard = new HelloCard();
            //helloCard.Show();
        }

        /// <summary>
        /// Kullanıcı TextBox 'a girdiğinde çalışır...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CityNameSearchTextBox_GoFocus(object sender, RoutedEventArgs e)
        {
            if (CityNameSearchTextBox.Text == "Search for city...")
            {
                CityNameSearchTextBox.Text = "";
                CityNameSearchTextBox.Foreground = Brushes.LightGray;
            }
        }

        /// <summary>
        /// Kullanıcı TextBox 'tan çıktığında çalışır...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CityNameSearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CityNameSearchTextBox.Text))
            {
                CityNameSearchTextBox.Text = "Search for city...";
                CityNameSearchTextBox.Foreground = Brushes.White;
            }
        }

        /// <summary>
        /// Kullanıcı yazıdığı şehri ENTER tuşu ile onaylıyor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CityNameSearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchCity();
                e.Handled = false;
            }
        }

        /// <summary>
        /// Kullanıcı şehir adı aramsını başatan UI fonksiyon.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CityNameSearchClickButton(object sender, RoutedEventArgs e)
        {
            SearchCity();
        }

        private async void SearchCity()
        {
            string cityName = CityNameSearchTextBox.Text;
            if (cityName == "Search for city...")
                return;

            SearchCity citySearch = new SearchCity();
            bool successStatus = await citySearch.SearchCityName(cityName);

            if (!successStatus)
            {
                MessageBox.Show("Please enter a valid city name. No special characters, emoji and numeric characters.", "Error");
            }
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
                SetIconImage("pack://application:,,,/Images/MoonIcon.png", WeatherTimeStatusIconImage); // Ay resmi
            }
            else
            {
                SetDayInformation(hour, TimeDayProgressBar, Colors.Yellow);
                SetIconImage("pack://application:,,,/Images/SunIcon.png", WeatherTimeStatusIconImage); // Güneş resmi
            }

            double progressWidth = TimeDayProgressBar.ActualWidth;

            double imagePosition = (TimeDayProgressBar.Value / TimeDayProgressBar.Maximum) * progressWidth;

            WeatherStatusTransform.X = imagePosition - (WeatherStatusIconImage.Width / 2);
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

    }
}