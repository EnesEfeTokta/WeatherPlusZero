﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
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

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(60);

            timer.Tick += Timer_Tick;
            timer.Start();

            // Veri kaynağını başlat
            WeatherList = new ObservableCollection<DayForecast>
            {
                new DayForecast
                {
                    DayName = "Monday",
                    IconPath = "C:\\Users\\EnesEfeTokta\\OneDrive\\Belgeler\\GitHub\\WeatherPlusZeroRepo\\WeatherPlusZero_Demo\\WpfApp1Demo\\Images\\Icons\\SunIcon.png", // İkon dosyasının doğru yolunu verin
                    MinMaxTemperature = "12°C / 20°C"
                },
                new DayForecast
                {
                    DayName = "Tuesday",
                    IconPath = "C:\\Users\\EnesEfeTokta\\OneDrive\\Belgeler\\GitHub\\WeatherPlusZeroRepo\\WeatherPlusZero_Demo\\WpfApp1Demo\\Images\\Icons\\SunIcon.png",
                    MinMaxTemperature = "10°C / 18°C"
                },
                new DayForecast
                {
                    DayName = "Wednesday",
                    IconPath = "C:\\Users\\EnesEfeTokta\\OneDrive\\Belgeler\\GitHub\\WeatherPlusZeroRepo\\WeatherPlusZero_Demo\\WpfApp1Demo\\Images\\Icons\\SunIcon.png",
                    MinMaxTemperature = "8°C / 15°C"
                },
                new DayForecast
                {
                    DayName = "Thursday",
                    IconPath = "C:\\Users\\EnesEfeTokta\\OneDrive\\Belgeler\\GitHub\\WeatherPlusZeroRepo\\WeatherPlusZero_Demo\\WpfApp1Demo\\Images\\Icons\\SunIcon.png",
                    MinMaxTemperature = "10°C / 18°C"
                },
                new DayForecast
                {
                    DayName = "Tuesday",
                    IconPath = "C:\\Users\\EnesEfeTokta\\OneDrive\\Belgeler\\GitHub\\WeatherPlusZeroRepo\\WeatherPlusZero_Demo\\WpfApp1Demo\\Images\\Icons\\SunIcon.png",
                    MinMaxTemperature = "10°C / 18°C"
                },
                new DayForecast
                {
                    DayName = "Wednesday",
                    IconPath = "C:\\Users\\EnesEfeTokta\\OneDrive\\Belgeler\\GitHub\\WeatherPlusZeroRepo\\WeatherPlusZero_Demo\\WpfApp1Demo\\Images\\Icons\\SunIcon.png",
                    MinMaxTemperature = "8°C / 15°C"
                },
                new DayForecast
                {
                    DayName = "Thursday",
                    IconPath = "C:\\Users\\EnesEfeTokta\\OneDrive\\Belgeler\\GitHub\\WeatherPlusZeroRepo\\WeatherPlusZero_Demo\\WpfApp1Demo\\Images\\Icons\\SunIcon.png",
                    MinMaxTemperature = "10°C / 18°C"
                },

            };

            SetFutureDays(WeatherItemsControl, WeatherList);
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
                SetCityName(CityNameSearchTextBox.Text);
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
            string cityName = CityNameSearchTextBox.Text;
            SetCityName(cityName);
        }

        /// <summary>
        /// Kullanıcının girmiş olduğu şehri alır.
        /// </summary>
        /// <param name="cityName"></param>
        /// <returns>Alım başarılı olmup olmadığı belirlenir.</returns>
        public bool SetCityName(string cityName)
        {
            this.cityName = cityName;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="textBlock"></param>
        /// <returns></returns>
        public bool SetUITexts(string text, TextBlock textBlock)
        {
            if (textBlock == null)
            {
                return false;
            }

            textBlock.Text = text;
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
            newImage.UriSource = new Uri(filePath);
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

            // ProgressBar değerini güncelle
            SetDayInformation(hour, TimeDayProgressBar, Colors.Yellow);

            SetImagePosition(TimeDayProgressBar, WeatherIconIcon);

            // Gün batımı kontrolü ve resmin değiştirilmesi
            if (hour >= 18 || hour < 6) // Gece veya gün batımı
            {
                SetIconImage("C:\\Users\\EnesEfeTokta\\OneDrive\\Belgeler\\GitHub\\WeatherPlusZeroRepo\\WeatherPlusZero_Demo\\WpfApp1Demo\\Images\\Icons\\SunIcon.png", WeatherStatusIconImage); // Ay resmi
            }
            else
            {
                SetIconImage("C:\\Users\\EnesEfeTokta\\OneDrive\\Belgeler\\GitHub\\WeatherPlusZeroRepo\\WeatherPlusZero_Demo\\WpfApp1Demo\\Images\\Icons\\MoonIcon.png", WeatherStatusIconImage); // Güneş resmi
            }
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
        /// <param name="progressBar"></param>
        /// <returns></returns>
        public bool SetImagePosition(ProgressBar progressBar, Image image)
        {
            if (progressBar == null && image == null)
            {
                return false;
            }

            double progressWidth = progressBar.ActualWidth;
            double imagePosition = (progressBar.Value / progressBar.Maximum) * progressWidth;
            Canvas.SetLeft(image, imagePosition - (image.Width / 2));
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
        public string DayName { get; set; }
        public string IconPath { get; set; }
        public string MinMaxTemperature { get; set; }
    }
}