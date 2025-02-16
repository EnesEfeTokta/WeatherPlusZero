using System.Threading.Tasks;

namespace WeatherPlusZero.Codes.API
{
    public interface IWeatherProvider
    {
        public Task<WeatherData> GetWeatherDataAsync(string city); // Receives weather data.
        public Task SaveWeatherDataAsync(WeatherData data); // Records weather data.
    }
}
