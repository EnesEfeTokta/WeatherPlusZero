using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.NetworkInformation;
using System.IO;

namespace WeatherPlusZero
{
    public abstract class WeatherServiceBase
    {
        protected const string API_KEY = "AHK6CFQXSP74M46LDVVVESQ4V";
        protected const string API_BASE_URL = "https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/{0}?key={1}&unitGroup=metric";

        protected static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WeatherPlusZero"
        );

        protected static readonly string JsonFilePath = Path.Combine(AppDataPath, "WeatherData.json");

        static WeatherServiceBase()
        {
            Directory.CreateDirectory(AppDataPath);
        }

        protected string BuildApiUrl(string city) =>
            string.Format(API_BASE_URL, Uri.EscapeDataString(city), API_KEY);
    }





    public interface IWeatherProvider
    {
        public Task<WeatherData> GetWeatherDataAsync(string city);
        public Task SaveWeatherDataAsync(WeatherData data);
    }





    public class ApiService : WeatherServiceBase, IWeatherProvider
    {
        private readonly HttpClient httpClient = new HttpClient();

        // Fetches weather data from the API.
        public async Task<WeatherData> GetWeatherDataAsync(string city)
        {
            var response = await httpClient.GetStringAsync(BuildApiUrl(city));
            return DeserializeWeatherData(response);
        }

        // Saves the weather data to the json file.
        public async Task SaveWeatherDataAsync(WeatherData data)
        {
            data.CurrentConditions.Datetime = DateTime.Now.ToString();
            await File.WriteAllTextAsync(JsonFilePath, JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        // Deserializes the json string to the WeatherData object.
        private WeatherData DeserializeWeatherData(string json)
        {
            var settings = new JsonSerializerSettings
            {
                Error = HandleDeserializationError,
                Converters = { new IntConverter() }
            };

            return JsonConvert.DeserializeObject<WeatherData>(json, settings);
        }

        // Handles deserialization errors.
        private void HandleDeserializationError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
        {
            args.ErrorContext.Handled = true;
            Console.WriteLine($"Deserialization Error: {args.ErrorContext.Error.Message}");
        }
    }





    public class JsonService : WeatherServiceBase, IWeatherProvider
    {
        /// <summary>
        /// Fetches weather information from the json file.
        /// </summary>
        /// <param name="city">The city they want to bring.</param>
        /// <returns>Fetched weather information.</returns>
        public async Task<WeatherData> GetWeatherDataAsync(string city = null)
        {
            if (!File.Exists(JsonFilePath)) return null;

            var json = await File.ReadAllTextAsync(JsonFilePath);
            return JsonConvert.DeserializeObject<WeatherData>(json, new IntConverter());
        }

        /// <summary>
        /// Saves the weather information to the json file.
        /// </summary>
        /// <param name="data">Weather information to be recorded.</param>
        /// <returns>Registration status.</returns>
        public Task SaveWeatherDataAsync(WeatherData data) => Task.CompletedTask;
    }





    public class WeatherManager
    {
        private readonly IWeatherProvider _apiProvider; // = new ApiService();
        private readonly IWeatherProvider _jsonProvider; // = new JsonService();
        private readonly NetworkChecker _networkChecker; // = new NetworkChecker();

        // Dependency injection for testing purposes.
        public WeatherManager()
        {
            _apiProvider = new ApiService();
            _jsonProvider = new JsonService();
            _networkChecker = new NetworkChecker();
        }

        /// <summary>
        /// Gets weather data for the specified city.
        /// </summary>
        /// <param name="city">The name of the city to be searched.</param>
        /// <param name="forceRefresh">'True' for calling data directly from the API, 'False' for normal calls.</param>
        /// <returns></returns>
        public async Task<WeatherData> GetWeatherDataAsync(string city, bool forceRefresh = false)
        {
            if (string.IsNullOrWhiteSpace(city)) return null;

            // If the 'forceRefresh' parameter is true, it pulls data from the API and returns it without saving.
            if (forceRefresh)
            {
                WeatherData apiData = await _apiProvider.GetWeatherDataAsync(city);
                return apiData;
            }

            // If the 'forceRefresh' parameter is false, it first pulls data from json and returns if there is data.
            WeatherData jsonWeatherData = await _jsonProvider.GetWeatherDataAsync(city);

            // If data could not be retrieved from json or the data is old, it pulls data from the API and saves it.
            // If the last data extraction was more than 5 hours ago, extracts data from the API and saves it.
            // If data cannot be retrieved from the API, it returns the data from json.
            if (_networkChecker.IsConnected && (jsonWeatherData == null || IsDataExpired(jsonWeatherData)))
            {
                WeatherData freshData = await _apiProvider.GetWeatherDataAsync(city);
                if (freshData != null)
                {
                    await _apiProvider.SaveWeatherDataAsync(freshData);
                    return freshData;
                }
            }

            return jsonWeatherData;
        }

        // Checks if the data is older than 5 hours.
        private bool IsDataExpired(WeatherData data)
        {
            if (data?.CurrentConditions == null) return true;

            DateTime lastUpdated = DateTime.Parse(data.CurrentConditions.Datetime);
            return (DateTime.UtcNow - lastUpdated).TotalHours > 5;
        }
    }





    public class NetworkChecker
    {
        public bool IsConnected => NetworkInterface.GetIsNetworkAvailable();
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