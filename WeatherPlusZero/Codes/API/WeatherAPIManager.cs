using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Windows;

namespace WeatherPlusZero
{
    public class WeatherAPIManager
    {
        private const string API_KEY = "c01b9545b1fc4354b68193653242612";
        private const string API_URL = "http://api.weatherapi.com/v1/current.json";

        public string GetWeather(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return "Şehir adı boş olamaz.";
            }

            using (HttpClient client = new HttpClient())
            {
                string apiUrl = $"{API_URL}?key={API_KEY}&q={city}";
                HttpResponseMessage response = client.GetAsync(apiUrl).Result; // Senkron çağrı

                if (response.IsSuccessStatusCode)
                {
                    var data = response.Content.ReadAsStringAsync().Result; // JSON verisini al
                    WeatherResponse weatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(data);
                    return $"Sıcaklık: {weatherResponse.current.temp_c} °C"; // Sıcaklığı göster
                }
                else
                {
                    return "Hava durumu verisi alınamadı.";
                }
            }
        }
    }

    public class WeatherResponse
    {
        public Current current { get; set; }
    }

    public class Current
    {
        public float temp_c { get; set; } // Celsius sıcaklık
        public float temp_f { get; set; } // Fahrenheit sıcaklık
    }
}