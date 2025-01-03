using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WeatherPlusZero
{
    public class BusinessManager
    {
        public void SearchCity(string city)
        {
            // Şehrin olup olmadığı kontrol edilecek...
        }
    }

    internal class CityApiSearch
    {
        public void SearchCity(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                MessageBox.Show("The city name cannot be empty.");
                return;
            }

            WeatherAPIManager weatherAPIManager = new WeatherAPIManager();
            string result = weatherAPIManager.GetWeather(city);
            MessageBox.Show(result);
        }
    }
}
