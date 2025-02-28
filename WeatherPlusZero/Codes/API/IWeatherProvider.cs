using System.Threading.Tasks;

namespace WeatherPlusZero
{
    public interface IWeatherProvider
    {
        public Task<WeatherData> GetWeatherDataAsync(string city); // Receives weather data.
        public Task SaveWeatherDataAsync(WeatherData data); // Records weather data.
    }
}