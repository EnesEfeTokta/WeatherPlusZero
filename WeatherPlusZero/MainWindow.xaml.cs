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
        public string cityName { get; set; }

        private DispatcherTimer timer { get; set; }

        public ObservableCollection<DayForecast> WeatherList { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            InitializeAsync();
        }

        private void InitializeAsync()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(60);

            timer.Tick += Timer_Tick;
            timer.Start();

            // Veri kaynağını başlat
            WeatherList = new ObservableCollection<DayForecast>
            {
                new DayForecast
                {
                    dayName = "Monday",
                    iconPath = "C:\\Users\\EnesEfeTokta\\OneDrive\\Belgeler\\GitHub\\WeatherPlusZeroRepo\\WeatherPlusZero_Demo\\WpfApp1Demo\\Images\\Icons\\SunIcon.png", // İkon dosyasının doğru yolunu verin
                    minMaxTemperature = "12°C / 20°C"
                },
                new DayForecast
                {
                    dayName = "Tuesday",
                    iconPath = "C:\\Users\\EnesEfeTokta\\OneDrive\\Belgeler\\GitHub\\WeatherPlusZeroRepo\\WeatherPlusZero_Demo\\WpfApp1Demo\\Images\\Icons\\SunIcon.png",
                    minMaxTemperature = "10°C / 18°C"
                },
                new DayForecast
                {
                    dayName = "Wednesday",
                    iconPath = "C:\\Users\\EnesEfeTokta\\OneDrive\\Belgeler\\GitHub\\WeatherPlusZeroRepo\\WeatherPlusZero_Demo\\WpfApp1Demo\\Images\\Icons\\SunIcon.png",
                    minMaxTemperature = "8°C / 15°C"
                },
                new DayForecast
                {
                    dayName = "Thursday",
                    iconPath = "C:\\Users\\EnesEfeTokta\\OneDrive\\Belgeler\\GitHub\\WeatherPlusZeroRepo\\WeatherPlusZero_Demo\\WpfApp1Demo\\Images\\Icons\\SunIcon.png",
                    minMaxTemperature = "10°C / 18°C"
                },
                new DayForecast
                {
                    dayName = "Tuesday",
                    iconPath = "C:\\Users\\EnesEfeTokta\\OneDrive\\Belgeler\\GitHub\\WeatherPlusZeroRepo\\WeatherPlusZero_Demo\\WpfApp1Demo\\Images\\Icons\\SunIcon.png",
                    minMaxTemperature = "10°C / 18°C"
                },
                new DayForecast
                {
                    dayName = "Wednesday",
                    iconPath = "C:\\Users\\EnesEfeTokta\\OneDrive\\Belgeler\\GitHub\\WeatherPlusZeroRepo\\WeatherPlusZero_Demo\\WpfApp1Demo\\Images\\Icons\\SunIcon.png",
                    minMaxTemperature = "8°C / 15°C"
                },
                new DayForecast
                {
                    dayName = "Thursday",
                    iconPath = "C:\\Users\\EnesEfeTokta\\OneDrive\\Belgeler\\GitHub\\WeatherPlusZeroRepo\\WeatherPlusZero_Demo\\WpfApp1Demo\\Images\\Icons\\SunIcon.png",
                    minMaxTemperature = "10°C / 18°C"
                },

            };

            SetFutureDays(WeatherItemsControl, WeatherList);

            ApplicationStart();
        }

        private void ApplicationStart()
        {
            ApplicationProgress applicationProgress = new ApplicationProgress();
            applicationProgress.ApplicationStart();
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
        /// <param name="value"></param>
        /// <param name="textBlock"></param>
        /// <returns></returns>
        public bool UpdateMainWeatherParametersText(string temp = null, string windSpeed = null, string windDir = null, string degrees = null, string humid = null, string pressure = null)
        {
            if (temp != null)
                TemperatureValueTextBlock.Text = $"{temp}℃";

            if (windSpeed != null)
                WindValueTextBlock.Text = $"{degrees} °{windDir} {windSpeed}km.h";

            if (humid != null)
                HumidValueTextBlock.Text = $"%{humid}";

            if (pressure != null)
                PressureValueTextBlock.Text = $"{pressure} hPa/mb";

            return true;
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
            newImage.UriSource = new Uri($"pack://application:,,,/{filePath}", UriKind.Absolute);
            newImage.CacheOption = BitmapCacheOption.OnLoad;
            newImage.EndInit();

            image.Source = newImage;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            //==== BURASI İŞ KATMANI OLUŞTURULANA KADAR BÖYLE KACAKTIR ====

            // Cihazın mevcut saatini al
            DateTime now = DateTime.Now;

            // Günün saatini 0-24 aralığında al
            double hour = now.Hour + now.Minute / 60.0; // Dakikaları saate çevir

            // Gün batımı kontrolü ve resmin değiştirilmesi
            if (hour >= 16 || hour < 6) // Gece veya gün batımı
            {
                // ProgressBar değerini güncelle
                SetDayInformation(hour, TimeDayProgressBar, Colors.DarkBlue);
                SetIconImage("Images/MoonIcon.png", WeatherStatusIconImage); // Ay resmi
            }
            else
            {
                // ProgressBar değerini güncelle
                SetDayInformation(hour, TimeDayProgressBar, Colors.Yellow);
                SetIconImage("Images/SunIcon.png", WeatherStatusIconImage); // Güneş resmi
            }

            SetImagePosition(TimeDayProgressBar, WeatherStatusTransform);
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
        public bool SetImagePosition(ProgressBar progressBar, TranslateTransform transform)
        {
            if (progressBar == null || transform == null)
            {
                return false;
            }

            double progressWidth = progressBar.ActualWidth;

            double imagePosition = (progressBar.Value / progressBar.Maximum) * progressWidth;

            transform.X = imagePosition - (WeatherStatusIconImage.Width / 2);

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemsControl"></param>
        /// <param name="days"></param>
        /// <returns></returns>
        public bool SetFutureDays(ItemsControl itemsControl, ObservableCollection<DayForecast> days)
        {
            if (itemsControl == null && days.Count > 0)
            {
                return false;
            }

            itemsControl.ItemsSource = days;
            return true;
        }

    }

    public class DayForecast
    {
        public string dayName { get; set; }
        public string iconPath { get; set; }
        public string minMaxTemperature { get; set; }
    }
}