using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.NetworkInformation;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Windows;

namespace WeatherPlusZero
{
    public abstract class WeatherServiceBase
    {
        // API key and base URL.
        protected string API_KEY { get; set; }
        protected string API_BASE_URL { get; set; }

        // Path to the application data folder.
        protected static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WeatherPlusZero"
        );

        // Path to the json file.
        protected static readonly string WeatherDataJsonFilePath = Path.Combine(AppDataPath, "WeatherData.json");

        // Path to the json file.
        protected static readonly string ApplicationActivityDataJsonFilePath = Path.Combine(AppDataPath, "ApplicationActivityData.json");

        // Path to the application data folder.
        static WeatherServiceBase()
        {
            Directory.CreateDirectory(AppDataPath);
        }

        protected IConfiguration Configuration { get; }

        public WeatherServiceBase()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            API_KEY = Configuration["Authentication:Weather_ApiKey"];
            API_BASE_URL = Configuration["Authentication:Weather_BaseUrl"];
        }

        // Builds the API URL.
        protected string BuildApiUrl(string city) =>
            string.Format(API_BASE_URL, Uri.EscapeDataString(city), API_KEY);
    }

    public interface IWeatherProvider
    {
        public Task<WeatherData> GetWeatherDataAsync(string city); // Receives weather data.
        public Task SaveWeatherDataAsync(WeatherData data); // Records weather data.
    }

    public class ApiService : WeatherServiceBase, IWeatherProvider
    {
        // HttpClient instance for making requests.
        private readonly HttpClient _httpClient = new HttpClient();

        // Fetches weather data from the API.
        public async Task<WeatherData> GetWeatherDataAsync(string city)
        {
            var response = await _httpClient.GetStringAsync(BuildApiUrl(city));
            return DeserializeWeatherData(response);
        }

        // Saves the weather data to the json file.
        public async Task SaveWeatherDataAsync(WeatherData data)
        {
            data.CurrentConditions.Datetime = DateTime.Now.ToString();
            await File.WriteAllTextAsync(WeatherDataJsonFilePath, JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        public void AddCity(string city)
        {
            // Add city to the list of cities
        }

        // Deserializes the json string to the WeatherData object.
        private WeatherData DeserializeWeatherData(string json)
        {
            var settings = new JsonSerializerSettings
            {
                Error = HandleDeserializationError,
                Converters = { new SmartIntConverter() }
            };

            return JsonConvert.DeserializeObject<WeatherData>(json, settings);
        }

        // Handles deserialization errors.
        private void HandleDeserializationError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
        {
            args.ErrorContext.Handled = true;
        }
    }

    public class JsonService : WeatherServiceBase, IWeatherProvider
    {
        // Fetches weather information from the json file.
        public async Task<WeatherData> GetWeatherDataAsync(string city = null)
        {
            if (!File.Exists(WeatherDataJsonFilePath)) return null;

            var json = await File.ReadAllTextAsync(WeatherDataJsonFilePath);
            return JsonConvert.DeserializeObject<WeatherData>(json, new SmartIntConverter());
        }

        // Saves the weather data to the json file.
        public async Task SaveWeatherDataAsync(WeatherData data) => await File.WriteAllTextAsync(WeatherDataJsonFilePath, JsonConvert.SerializeObject(data, Formatting.Indented));

        // Deletes the registered city.
        public async void RemoveCity()
        {
            await SaveWeatherDataAsync(new WeatherData()); // An empty WeatherData object is created and written to the file.

            await ApplicationActivity.ChangeApplicationActivityDataByCity(null);
        }

        // Saves the application activity data to the json file.
        public async Task SaveApplicationActivityDataAsync(ApplicationActivityData data)
        {
            await File.WriteAllTextAsync(ApplicationActivityDataJsonFilePath, JsonConvert.SerializeObject(data, Formatting.Indented));
            await SettingsPanelManager.UpdateSettingsPanelAsync();
        }

        // Fetches application activity data from the json file.
        public async Task<ApplicationActivityData> GetApplicationActivityDataAsync()
        {
            if (!File.Exists(ApplicationActivityDataJsonFilePath)) return null;

            var json = await File.ReadAllTextAsync(ApplicationActivityDataJsonFilePath);
            return JsonConvert.DeserializeObject<ApplicationActivityData>(json, new SmartIntConverter());
        }
    }

    public class WeatherManager
    {
        private readonly IWeatherProvider _apiProvider;
        private readonly IWeatherProvider _jsonProvider;
        private readonly NetworkChecker _networkChecker;

        // Dependency injection for testing purposes.
        public WeatherManager()
        {
            _apiProvider = new ApiService();
            _jsonProvider = new JsonService();
            _networkChecker = new NetworkChecker();
        }
        
        // Gets weather data for the specified city.
        public async Task<WeatherData> GetWeatherDataAsync(string city, bool forceRefresh = false)
        {
            if (string.IsNullOrWhiteSpace(city)) return null;

            // If the 'forceRefresh' parameter is true, it pulls data from the API and returns it without saving.
            if (forceRefresh)
            {
                return await _apiProvider.GetWeatherDataAsync(city);
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
        // Checks if the device is connected to the internet.
        public bool IsConnected => NetworkInterface.GetIsNetworkAvailable();
    }

    public class SmartIntConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) =>
            objectType == typeof(short);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null) return (short)0;

            try
            {
                // Handle various input types
                switch (reader.Value)
                {
                    case string stringValue:
                        if (short.TryParse(stringValue, out short resultShort))
                            return resultShort;
                        if (int.TryParse(stringValue, out int resultInt))
                            return (short)resultInt;
                        if (double.TryParse(stringValue, out double resultDouble))
                            return (short)resultDouble;
                        return (short)0;

                    case int intValue:
                        return (short)intValue;

                    case double doubleValue:
                        return (short)doubleValue;

                    default:
                        return Convert.ToInt16(reader.Value);
                }
            }
            catch
            {
                return (short)0;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            writer.WriteValue(value.ToString());
    }

    public class WeatherData
    {
        public short QueryCost { get; set; }
        public float Latitude { get; set; }
        public float intitude { get; set; }
        public string ResolvedAddress { get; set; }
        public string Address { get; set; }
        public string Timezone { get; set; }
        [JsonConverter(typeof(SmartIntConverter))]
        public short Tzoffset { get; set; }
        public string Description { get; set; }
        public List<Day> Days { get; set; }
        public List<object> Alerts { get; set; }
        public Stations Stations { get; set; }
        public CurrentConditions CurrentConditions { get; set; }
    }

    public class Day
    {
        public string Datetime { get; set; }
        public int DatetimeEpoch { get; set; }
        public float Tempmax { get; set; }
        public float Tempmin { get; set; }
        public float Temp { get; set; }
        public float Feelslikemax { get; set; }
        public float Feelslikemin { get; set; }
        public float Feelslike { get; set; }
        public float Dew { get; set; }
        public float Humidity { get; set; }
        public float Precip { get; set; }
        [JsonConverter(typeof(SmartIntConverter))]
        public short Precipprob { get; set; }
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
        public short Uvindex { get; set; }
        public short Severerisk { get; set; }
        public string Sunrise { get; set; }
        public int SunriseEpoch { get; set; }
        public string Sunset { get; set; }
        public int SunsetEpoch { get; set; }
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
        public int DatetimeEpoch { get; set; }
        public float Temp { get; set; }
        public float Feelslike { get; set; }
        public float Humidity { get; set; }
        public float Dew { get; set; }
        public float Precip { get; set; }
        [JsonConverter(typeof(SmartIntConverter))]
        public short Precipprob { get; set; }
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
        public short Uvindex { get; set; }
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
        public float intitude { get; set; }
        public short UseCount { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public short Quality { get; set; }
        public short Contribution { get; set; }
    }

    public class CurrentConditions
    {
        public string Datetime { get; set; }
        public int DatetimeEpoch { get; set; }
        public float? Temp { get; set; }
        public float? Feelslike { get; set; }
        public float? Humidity { get; set; }
        public float? Dew { get; set; }
        public object Precip { get; set; }
        [JsonConverter(typeof(SmartIntConverter))]
        public short Precipprob { get; set; }
        public short Snow { get; set; }
        public float? Snowdepth { get; set; }
        public List<string> Preciptype { get; set; }
        public object Windgust { get; set; }
        public float Windspeed { get; set; }
        public float Winddir { get; set; }
        public float Pressure { get; set; }
        public float Visibility { get; set; }
        public float Cloudcover { get; set; }
        public float Solarradiation { get; set; }
        public float Solarenergy { get; set; }
        public short Uvindex { get; set; }
        public string Conditions { get; set; }
        public string Icon { get; set; }
        public List<string> Stations { get; set; }
        public string Source { get; set; }
        public string Sunrise { get; set; }
        public int SunriseEpoch { get; set; }
        public string Sunset { get; set; }
        public int SunsetEpoch { get; set; }
        public float Moonphase { get; set; }
    }
}