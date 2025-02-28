using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Notification.Wpf;

namespace WeatherPlusZero
{
    public class ApiService : JsonServiceBase, IWeatherProvider
    {
        // HttpClient instance for making requests.
        private readonly HttpClient _httpClient = new HttpClient();

        // Fetches weather data from the API.
        public async Task<WeatherData> GetWeatherDataAsync(string city)
        {
            try
            {
                var response = await _httpClient.GetStringAsync(BuildApiUrl(city));
                return DeserializeWeatherData(response);
            }
            catch (Exception ex)
            {
                NotificationManagement.ShowNotification(
                    "City Search Error", 
                    $"Data could not be retrieved from the city you wanted to search. Error: {ex}", 
                    NotificationType.Error);
                return null;
            }
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
}
