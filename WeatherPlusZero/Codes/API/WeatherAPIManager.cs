using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows;
using System.Net.NetworkInformation;
using System.IO;
using System.Reflection;

namespace WeatherPlusZero
{
    public class WeatherService
    {
        protected internal const string API_KEY = "AHK6CFQXSP74M46LDVVVESQ4V";
        protected internal const string API_URL = "https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/CITY_NAME?key=YOUR_API_KEY&unitGroup=metric";

        protected internal static readonly string JsonFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WeatherPlusZero", "WeatherData.json");

        static WeatherService()
        {
            // Klasör yoksa oluştur.
            string directoryPath = Path.GetDirectoryName(JsonFilePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }
    }

    public class APIService : WeatherService
    {
        public async Task<WeatherData> GetWeather(string city)
        {
            string url = API_URL.Replace("CITY_NAME", city).Replace("YOUR_API_KEY", API_KEY);

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"API'dan hava durumu bilgisi alınamadı: {response.StatusCode} - {response.ReasonPhrase}", "Hata");
                    return null;
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                try
                {
                    var settings = new JsonSerializerSettings
                    {
                        Error = (sender, args) =>
                        {
                            Console.WriteLine($"Dönüştürme Hatası: {args.ErrorContext.Error.Message}, path: {args.ErrorContext.Path}");
                            args.ErrorContext.Handled = true;
                        }
                    };
                    return JsonConvert.DeserializeObject<WeatherData>(jsonResponse, settings);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"JSON dönüştürme hatası: {ex.Message}", "Hata");
                    return null;
                }
            }
        }
    }

    public class JsonService : WeatherService
    {
        // JSON dosyasını WeatherData sınıfına dönüştürüyor ve veriyor...
        public async Task<WeatherData> GetWeather()
        {
            if (!File.Exists(JsonFilePath))
                return null;

            string json = string.Empty;

            try
            {
                json = await File.ReadAllTextAsync(JsonFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Dosya okuma hatası: {ex.Message}", "Hata");
                return null;
            }

            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonConvert.DeserializeObject<WeatherData>(json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"JSON dönüştürme hatası: {ex.Message}", "Hata");
                return null;
            }
        }

        // JSON dosyasına dönüştürme işlemi ve kayıt etme...
        public async Task<bool> SetJsonDataAsync(WeatherData weatherData)
        {
            if (weatherData == null)
                return false;
            try
            {
                weatherData.CurrentConditions.Datetime = DateTime.Now.ToString();
                string json = JsonConvert.SerializeObject(weatherData, Formatting.Indented);
                await File.WriteAllTextAsync(JsonFilePath, json);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"JSON Kayıt Hatası: {ex.Message}", "Hata");
                return false;
            }
        }
    }


    public class GetWeather
    {
        private readonly APIService _apiService;
        private readonly JsonService _jsonService;

        public GetWeather()
        {
            _apiService = new APIService();
            _jsonService = new JsonService();
        }

        public async Task<WeatherData> GetWeatherData(string city, RequestType requestType = RequestType.InstantNot)
        {
            if (string.IsNullOrWhiteSpace(city))
                return null;

            if (requestType == RequestType.Instant)
            {
                return await ApiWeatherData(city);
                //return await JsonWeatherData();
            }

            bool isInternetConnected = IsConnectedInternet();

            if (await IsCityChangedAsync(city) || (await IsTimePassed() && isInternetConnected))
            {
                // API 'den veri çekme işlemi yapılacak...
                return await ApiWeatherData(city);
            }

            if (!await IsCityChangedAsync(city) && !await IsTimePassed() || !isInternetConnected)
            {
                // JSON dosyasından veri çekme işlemi yapılacak...
                return await JsonWeatherData();
            }

            return null;
        }


        // API 'den veri çekme işlemi yapılıyor...
        private async Task<WeatherData> ApiWeatherData(string city)
        {
            MessageBox.Show("API'den veri çekiliyor...");
            WeatherData weatherData = await _apiService.GetWeather(city);
            if (weatherData != null)
            {
                await _jsonService.SetJsonDataAsync(weatherData);
            }
            return weatherData;
        }


        // JSON dosyasından veri çekme işlemi yapılıyor...
        private async Task<WeatherData> JsonWeatherData()
        {
            MessageBox.Show("JSON dosyasından veri çekiliyor...");
            return await _jsonService.GetWeather();
        }


        // Şehir isminin değişikliği kontrol ediliyor...
        private async Task<bool> IsCityChangedAsync(string city)
        {
            WeatherData weatherData = await _jsonService.GetWeather();
            if (weatherData == null)
                return true;

            string oldCity = weatherData.Address;
            if (string.IsNullOrEmpty(oldCity))
                return true;

            return city != oldCity;
        }


        // JSON dosyasının zaman aşımına uğramışlığı kontrol ediliyor...
        private async Task<bool> IsTimePassed()
        {
            DateTime nowDateTime = DateTime.Now;
            WeatherData weatherData = await _jsonService.GetWeather();

            if (weatherData == null || string.IsNullOrEmpty(weatherData.CurrentConditions.Datetime))
                return true;

            DateTime oldDateTime = DateTime.Parse(weatherData.CurrentConditions.Datetime);

            TimeSpan timeSpan = nowDateTime - oldDateTime;

            double remainingTime = timeSpan.TotalHours;

            return remainingTime > 5;
        }


        // İnternet bağlantısı kontrol ediliyor...
        private bool IsConnectedInternet()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }
    }

    public enum RequestType
    {
        Instant,
        InstantNot
    }


    public class IntConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(int);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return 0;

            if (reader.Value is string stringValue)
            {
                if (int.TryParse(stringValue, out int result))
                    return result;
                if (double.TryParse(stringValue, out double resultDouble))
                {
                    return Convert.ToInt32(resultDouble);

                }
                return 0;
            }

            return Convert.ToInt32(reader.Value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }


    // Data sınıflarında mantık hatası bulunmuyor, sadece kod okunabilirliği için düzenlemeler yapıldı.
    public class WeatherData
    {
        public int QueryCost { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string ResolvedAddress { get; set; }
        public string Address { get; set; }
        public string Timezone { get; set; }
        [JsonConverter(typeof(IntConverter))]
        public int Tzoffset { get; set; }
        public string Description { get; set; }
        public List<Day> Days { get; set; }
        public List<object> Alerts { get; set; }
        public Stations Stations { get; set; }
        public CurrentConditions CurrentConditions { get; set; }
    }

    public class Day
    {
        public string Datetime { get; set; }
        public long DatetimeEpoch { get; set; }
        public double Tempmax { get; set; }
        public double Tempmin { get; set; }
        public double Temp { get; set; }
        public double Feelslikemax { get; set; }
        public double Feelslikemin { get; set; }
        public double Feelslike { get; set; }
        public double Dew { get; set; }
        public double Humidity { get; set; }
        public double Precip { get; set; }
        [JsonConverter(typeof(IntConverter))]
        public int Precipprob { get; set; }
        public double Precipcover { get; set; }
        public List<string> Preciptype { get; set; }
        public double Snow { get; set; }
        public double? Snowdepth { get; set; }
        public double Windgust { get; set; }
        public double Windspeed { get; set; }
        public double Winddir { get; set; }
        public double Pressure { get; set; }
        public double Cloudcover { get; set; }
        public double Visibility { get; set; }
        public double Solarradiation { get; set; }
        public double Solarenergy { get; set; }
        public int Uvindex { get; set; }
        public int Severerisk { get; set; }
        public string Sunrise { get; set; }
        public long SunriseEpoch { get; set; }
        public string Sunset { get; set; }
        public long SunsetEpoch { get; set; }
        public double Moonphase { get; set; }
        public string Conditions { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public List<string> Stations { get; set; }
        public string Source { get; set; }
        public List<Hour> Hours { get; set; }
    }

    public class Hour
    {
        public string Datetime { get; set; }
        public long DatetimeEpoch { get; set; }
        public double Temp { get; set; }
        public double Feelslike { get; set; }
        public double Humidity { get; set; }
        public double Dew { get; set; }
        public double Precip { get; set; }
        [JsonConverter(typeof(IntConverter))]
        public int Precipprob { get; set; }
        public double Snow { get; set; }
        public double? Snowdepth { get; set; }
        public List<string> Preciptype { get; set; }
        public double Windgust { get; set; }
        public double Windspeed { get; set; }
        public double Winddir { get; set; }
        public double Pressure { get; set; }
        public double Visibility { get; set; }
        public double Cloudcover { get; set; }
        public double Solarradiation { get; set; }
        public double Solarenergy { get; set; }
        public int Uvindex { get; set; }
        public double Severerisk { get; set; }
        public string Conditions { get; set; }
        public string Icon { get; set; }
        public List<string> Stations { get; set; }
        public string Source { get; set; }
    }
    public class Stations
    {
        [JsonProperty("LTCB")]
        public Station LTCB { get; set; }
        [JsonProperty("LTCO")]
        public Station LTCO { get; set; }
        [JsonProperty("LTCT")]
        public Station LTCT { get; set; }
    }
    public class Station
    {
        public double Distance { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int UseCount { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public int Quality { get; set; }
        public int Contribution { get; set; }
    }

    public class CurrentConditions
    {
        public string Datetime { get; set; }
        public long DatetimeEpoch { get; set; }
        public double? Temp { get; set; }
        public double? Feelslike { get; set; }
        public double? Humidity { get; set; }
        public double? Dew { get; set; }
        public object Precip { get; set; }
        [JsonConverter(typeof(IntConverter))]
        public int Precipprob { get; set; }
        public int Snow { get; set; }
        public double? Snowdepth { get; set; }
        public List<string> Preciptype { get; set; }
        public object Windgust { get; set; }
        public double Windspeed { get; set; }
        public float Winddir { get; set; }
        public double Pressure { get; set; }
        public double Visibility { get; set; }
        public double Cloudcover { get; set; }
        public double Solarradiation { get; set; }
        public double Solarenergy { get; set; }
        public int Uvindex { get; set; }
        public string Conditions { get; set; }
        public string Icon { get; set; }
        public List<string> Stations { get; set; }
        public string Source { get; set; }
        public string Sunrise { get; set; }
        public long SunriseEpoch { get; set; }
        public string Sunset { get; set; }
        public long SunsetEpoch { get; set; }
        public double Moonphase { get; set; }
    }
}