using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using System.Xml.Serialization;


namespace WeatherPlusZero
{
    public class WeatherAPI
    {
        protected const string API_KEY = "AHK6CFQXSP74M46LDVVVESQ4V";
        protected const string API_URL = "https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/CITY_NAME?key=YOUR_API_KEY&unitGroup=metric";
    }

    public class GetAPI : WeatherAPI
    {
        public async Task<string> GetWeatherData(string city)
        {
            //SaveJson saveJson = new SaveJson();

            string url = API_URL.Replace("CITY_NAME", city).Replace("YOUR_API_KEY", API_KEY);

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Hava durumu bilgisi alınamadı.", "Hata");
                    return null;
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var settings = new JsonSerializerSettings
                {
                    // Bu kısımda JSON değerlerini ayarlayabiliyoruz.
                    Error = (sender, args) =>
                    {
                        Console.WriteLine($"Dönüştürme Hatası: {args.ErrorContext.Error.Message}, path: {args.ErrorContext.Path}");
                        args.ErrorContext.Handled = true; // Hatayı görmezden gel.
                    }
                };

                WeatherData weather = JsonConvert.DeserializeObject<WeatherData>(jsonResponse, settings);

                return weather.CurrentConditions.Temp.ToString();
            }
        }
    }

    public class ServiceJson
    {
        private string jsonFilePath = "Codes\\Resources\\WeatherData.json";

        public void SaveJsonData(WeatherData weatherData)
        {
            if (IsJsonCurrent(weatherData))
            {
                return;
            }

            // JSON dosyasına dönüştürme işlemi ve kayıt etme.
            string json = JsonConvert.SerializeObject(weatherData, Formatting.Indented);
            System.IO.File.WriteAllText(jsonFilePath, json);
        }

        public bool IsJsonCurrent(WeatherData weatherData)
        {
            return false;
        }

        private WeatherData GetWeatherData()
        {
            string json = System.IO.File.ReadAllText(jsonFilePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<WeatherData>(json);
        }
    }

    public class WeatherData
    {
        public int QueryCost { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public string ResolvedAddress { get; set; }
        public string Address { get; set; }
        public string Timezone { get; set; }
        public int Tzoffset { get; set; }
        public string Description { get; set; }
        public List<Day> Days { get; set; }
        public List<object> Alerts { get; set; } // Burayı daha spesifik bir tipe dönüştürebilirsiniz
        public Stations Stations { get; set; }
        public CurrentConditions CurrentConditions { get; set; }
    }

    public class Day
    {
        public string Datetime { get; set; }
        public long DatetimeEpoch { get; set; }
        public float Tempmax { get; set; }
        public float Tempmin { get; set; }
        public float Temp { get; set; }
        public float Feelslikemax { get; set; }
        public float Feelslikemin { get; set; }
        public float Feelslike { get; set; }
        public float Dew { get; set; }
        public float Humidity { get; set; }
        public float Precip { get; set; }
        public int Precipprob { get; set; }
        public float Precipcover { get; set; }
        public List<string> Preciptype { get; set; }
        public float Snow { get; set; }
        public float? Snowdepth { get; set; }
        public float Windgust { get; set; }
        public float Windspeed { get; set; }
        public float Winddir { get; set; }
        public float Pressure { get; set; }
        public float Cloudcover { get; set; }
        public float Visibility { get; set; }
        public float Solarradiation { get; set; }
        public float Solarenergy { get; set; }
        public int Uvindex { get; set; }
        public int Severerisk { get; set; }
        public string Sunrise { get; set; }
        public long SunriseEpoch { get; set; }
        public string Sunset { get; set; }
        public long SunsetEpoch { get; set; }
        public float Moonphase { get; set; }
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
        public float Temp { get; set; }
        public float Feelslike { get; set; }
        public float Humidity { get; set; }
        public float Dew { get; set; }
        public float Precip { get; set; }
        public int Precipprob { get; set; }
        public float Snow { get; set; }
        public float? Snowdepth { get; set; }
        public List<string> Preciptype { get; set; }
        public float Windgust { get; set; }
        public float Windspeed { get; set; }
        public float Winddir { get; set; }
        public float Pressure { get; set; }
        public float Visibility { get; set; }
        public float Cloudcover { get; set; }
        public float Solarradiation { get; set; }
        public float Solarenergy { get; set; }
        public int Uvindex { get; set; }
        public float Severerisk { get; set; }
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
        public float Distance { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
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
        public float? Temp { get; set; }
        public float? Feelslike { get; set; }
        public float? Humidity { get; set; }
        public float? Dew { get; set; }
        public object Precip { get; set; }
        public int Precipprob { get; set; }
        public int Snow { get; set; }
        public int Snowdepth { get; set; }
        public List<string> Preciptype { get; set; }
        public object Windgust { get; set; }
        public float Windspeed { get; set; }
        public int Winddir { get; set; }
        public int Pressure { get; set; }
        public float Visibility { get; set; }
        public float Cloudcover { get; set; }
        public float Solarradiation { get; set; }
        public float Solarenergy { get; set; }
        public int Uvindex { get; set; }
        public string Conditions { get; set; }
        public string Icon { get; set; }
        public List<string> Stations { get; set; }
        public string Source { get; set; }
        public string Sunrise { get; set; }
        public long SunriseEpoch { get; set; }
        public string Sunset { get; set; }
        public long SunsetEpoch { get; set; }
        public float Moonphase { get; set; }
    }
}