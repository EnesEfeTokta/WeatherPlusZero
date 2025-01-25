using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Text.RegularExpressions;

namespace WeatherPlusZero
{
    public class ApplicationProgress()
    {
        public async void ApplicationStart()
        {
            GetWeather getWeather = new GetWeather();
            WeatherData weatherData = await getWeather.GetWeatherData("Agri");

            ApplyDataUIs applyDataUIs = new ApplyDataUIs();
            applyDataUIs.DataApply(weatherData);
        }
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