using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Globalization;

namespace WeatherPlusZero
{
    public class ApplicationProgress()
    {
        private DispatcherTimer timer;
        private DateTime targetDateTime;
        private CultureInfo culture = new CultureInfo("en-US");

        public void ApplicationStart()
        {
            GetInitialData();

            // Timer başlatılıyor.
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private async void GetInitialData()
        {
            // Json dosyasından şehir bilgisi alınıyor.
            JsonService jsonService = new JsonService();
            WeatherData weatherDataJson = await jsonService.GetWeather();
            string city = weatherDataJson?.Address;

            // Alınan şehir bilgisi ile hava durumu bilgisi alınıyor.
            GetWeather getWeather = new GetWeather();
            WeatherData _weatherData = await getWeather.GetWeatherData(city);

            // Alınan hava durumu bilgisi uygulamaya aktarılıyor.
            ApplyDataUIs applyDataUIs = new ApplyDataUIs();
            applyDataUIs.DataApply(_weatherData);

            // Uygulama ana penceresindeki konum bilgisi güncelleniyor.
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.UpdateLocationText(_weatherData.ResolvedAddress);

            // Hava durumu ikonu güncelleniyor.
            string iconPath = $"pack://application:,,,/Images/WeatherStatus/{_weatherData.CurrentConditions.Icon}.png";
            mainWindow.UpdateWeatherStatusIcon(iconPath);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateDateTime();
        }

        private void UpdateDateTime()
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

            Application.Current.Dispatcher.Invoke(() =>
            {
                mainWindow.UpdateDateTimeText(
                    TimeDateCounter(GetDateTimeType.DayNumber),
                    TimeDateCounter(GetDateTimeType.DayName),
                    TimeDateCounter(GetDateTimeType.Month),
                    TimeDateCounter(GetDateTimeType.Year),
                    TimeDateCounter(GetDateTimeType.Hour),
                    TimeDateCounter(GetDateTimeType.Minute)
                    );
            });
        }

        private string TimeDateCounter(GetDateTimeType getDateTimeType)
        {
            switch (getDateTimeType)
            {
                case GetDateTimeType.Second:
                    return DateTime.Now.ToString("ss");

                case GetDateTimeType.Minute:
                    return DateTime.Now.ToString("mm");

                case GetDateTimeType.Hour:
                    return DateTime.Now.ToString("HH");

                case GetDateTimeType.DayNumber:
                    return DateTime.Now.ToString("dd");

                case GetDateTimeType.DayName:
                    DateTime dateTime = DateTime.Now;
                    DayOfWeek day = dateTime.DayOfWeek;
                    return day.ToString();

                case GetDateTimeType.Month:
                    return DateTime.Now.ToString("MMMMMMMM", culture);

                case GetDateTimeType.Year:
                    return DateTime.Now.ToString("yyyy");

                default:
                    return null;
            }
        }
    }

    // Tarih ve saat bilgilerinin alınacağı tip.
    public enum GetDateTimeType
    {
        Second,
        Minute,
        Hour,
        DayNumber,
        DayName,
        Month,
        Year
    }







    public class SearchCity
    {
        private string _city;
        public string City
        {
            get { return _city; }
        }

        public async Task<bool> SearchCityName(string city)
        {
            // Gerekli kontroller yapılıyor.
            if (string.IsNullOrEmpty(city) || ContainsEmoji(city) || ContainsSpecialChar(city))
                return false;

            // Şehir ismi büyük harfle başlayacak ve geri kalanı küçük harf olacak şekilde düzenleniyor.
            city = char.ToUpper(city[0]) + city.Substring(1).ToLower();

            // Türkçe karakterlerin düzeltilmesi ve boşlukların %20 ile değiştirilmesi işlemi yapılıyor.
            city = ReplaceTurkishChars(city);
            _city = city;

            GetWeather getWeather = new GetWeather();
            WeatherData weatherData = await getWeather.GetWeatherData(city, RequestType.Instant);

            ApplyDataUIs applyDataUIs = new ApplyDataUIs();
            applyDataUIs.DataApply(weatherData);

            return true;
        }

        // Emoji girilme durumu kontrol ediliyor.
        private bool ContainsEmoji(string city)
        {
            Regex emojiRegex = new Regex(@"\p{Cs}");
            return emojiRegex.IsMatch(city);
        }

        // Özel karakter girilme durumu kontrol ediliyor.
        private bool ContainsSpecialChar(string city)
        {
            Regex specialCharRegex = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-,\d]");
            return specialCharRegex.IsMatch(city);
        }

        // Türkçe karakterlerin düzeltilmesi ve boşlukların %20 ile değiştirilmesi işlemi yapılıyor.
        private string ReplaceTurkishChars(string city)
        {
            return city
                .Replace(" ", "%20")
                .Replace("ç", "c")
                .Replace("ğ", "g")
                .Replace("ö", "o")
                .Replace("ş", "s")
                .Replace("ü", "u")
                .Replace("ı", "i")
                .Replace("Ç", "C")
                .Replace("Ğ", "G")
                .Replace("Ö", "O")
                .Replace("Ş", "S")
                .Replace("Ü", "U")
                .Replace("İ", "I");
        }
    }












    public class ApplyDataUIs
    {
        public void DataApply(WeatherData weatherData)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

            string nowTemp = Math.Round(Convert.ToDecimal(weatherData.CurrentConditions.Temp)).ToString();

            string nowWind = weatherData.CurrentConditions.Windspeed.ToString();
            float degrees = weatherData.CurrentConditions.Winddir;
            string nowWindAngle = degrees.ToString();
            string nowWindDir = ConvertDegreeToDirection(degrees);

            string nowHumidity = Math.Round(Convert.ToDecimal(weatherData.CurrentConditions.Humidity)).ToString();
            string nowPressure = weatherData.CurrentConditions.Pressure.ToString();

            string nowIcon = weatherData.CurrentConditions.Icon;

            Application.Current.Dispatcher.Invoke(() =>
            {
                mainWindow.UpdateMainWeatherParametersText(nowTemp, nowWind, nowWindDir, nowWindAngle, nowHumidity, nowPressure);
            });
        }

        private string ConvertDegreeToDirection(double degrees)
        {
            degrees = (degrees % 360 + 360) % 360;
            string[] directions = { "N", "NH", "E", "SH", "S", "SW", "W", "NW" };
            int index = (int)Math.Round(degrees / 45.0) % 8;
            return directions[index];
        }
    }
}