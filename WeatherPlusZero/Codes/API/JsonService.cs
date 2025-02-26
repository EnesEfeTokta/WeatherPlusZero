using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace WeatherPlusZero.Codes.API
{
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
            => await File.WriteAllTextAsync(ApplicationActivityDataJsonFilePath, JsonConvert.SerializeObject(data, Formatting.Indented));

        // Fetches application activity data from the json file.
        public async Task<ApplicationActivityData> GetApplicationActivityDataAsync()
        {
            if (!File.Exists(ApplicationActivityDataJsonFilePath)) return null;

            var json = await File.ReadAllTextAsync(ApplicationActivityDataJsonFilePath);
            return JsonConvert.DeserializeObject<ApplicationActivityData>(json, new SmartIntConverter());
        }
    }
}
