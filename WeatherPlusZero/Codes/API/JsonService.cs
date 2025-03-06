using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows;

namespace WeatherPlusZero
{
    public class JsonService : JsonServiceBase, IWeatherProvider
    {
        private static readonly SemaphoreSlim fileSemaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Retrieves weather data asynchronously for a given city.
        /// </summary>
        /// <param name="city">The city to get weather data for.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the weather data.</returns>
        public async Task<WeatherData> GetWeatherDataAsync(string city = null)
        {
            if (!File.Exists(WeatherDataJsonFilePath)) return null;

            var json = await File.ReadAllTextAsync(WeatherDataJsonFilePath);
            return JsonConvert.DeserializeObject<WeatherData>(json, new SmartIntConverter());
        }

        /// <summary>
        /// Saves weather data asynchronously.
        /// </summary>
        /// <param name="data">The weather data to save.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task SaveWeatherDataAsync(WeatherData data) =>
            await File.WriteAllTextAsync(WeatherDataJsonFilePath, JsonConvert.SerializeObject(data, Formatting.Indented));

        /// <summary>
        /// Removes the city data asynchronously.
        /// </summary>
        public async void RemoveCity()
        {
            await SaveWeatherDataAsync(new WeatherData());
            await ApplicationActivity.ChangeApplicationActivityDataByCity(null);
        }

        /// <summary>
        /// Saves application activity data asynchronously.
        /// </summary>
        /// <param name="data">The application activity data to save.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task SaveApplicationActivityDataAsync(ApplicationActivityData data)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            byte[] encryptedData = Encryption.EncryptData(Encoding.UTF8.GetBytes(json));

            await fileSemaphore.WaitAsync();
            try
            {
                await File.WriteAllBytesAsync(ApplicationActivityDataJsonFilePath, encryptedData);
            }
            finally
            {
                fileSemaphore.Release();
            }
        }

        /// <summary>
        /// Retrieves application activity data asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the application activity data.</returns>
        public async Task<ApplicationActivityData> GetApplicationActivityDataAsync()
        {
            if (!File.Exists(ApplicationActivityDataJsonFilePath))
                return null;

            await fileSemaphore.WaitAsync();
            try
            {
                byte[] encryptedData = await File.ReadAllBytesAsync(ApplicationActivityDataJsonFilePath);
                byte[] decryptedData = Encryption.DecryptData(encryptedData);
                string json = Encoding.UTF8.GetString(decryptedData);

                return JsonConvert.DeserializeObject<ApplicationActivityData>(json, new SmartIntConverter());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Decryption error: {ex.Message}");
                return null;
            }
            finally
            {
                fileSemaphore.Release();
            }
        }
    }
}
