using System;
using System.Threading.Tasks;
using WeatherPlusZero.Codes.API;

namespace WeatherPlusZero
{
    public static class WeatherManager
    {
        private static readonly IWeatherProvider _apiProvider = new ApiService();
        private static readonly IWeatherProvider _jsonProvider = new JsonService();
        private static readonly NetworkChecker _networkChecker = new NetworkChecker();

        // Gets weather data for the specified city.
        public static async Task<WeatherData> GetWeatherDataAsync(string city, bool forceRefresh = false)
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

            // If the data is not old, it returns the data from json.
            return jsonWeatherData;
        }

        // Checks if the data is older than 5 hours.
        private static bool IsDataExpired(WeatherData data)
        {
            if (data?.CurrentConditions == null) return true;

            DateTime lastUpdated = DateTime.Parse(data.CurrentConditions.Datetime);
            return (DateTime.UtcNow - lastUpdated).TotalHours > 5;
        }

        // Saves the null weather data.
        public static async void ClearWeatherData()
        {
            await _jsonProvider.SaveWeatherDataAsync(null);
        }
    }
}
