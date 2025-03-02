using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace WeatherPlusZero
{
    public static class LocationService
    {
        private static readonly HttpClient _httpClient;
        private static readonly string _locationApiUrl;

        public static IConfiguration Configuration { get; }

        static LocationService()
        {
            _httpClient = new HttpClient();

            // Set the location API URL.
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _locationApiUrl = Configuration["Authentication:Ip_Api_Url"];

        }

        // Get the location data.
        public static async Task<IpLocation> GetLocationDataByApiAsync()
        {
            var response = await _httpClient.GetAsync(_locationApiUrl);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                IpLocation location = Newtonsoft.Json.JsonConvert.DeserializeObject<IpLocation>(content);
                return location;
            }

            return null;
        }

        // Save the location data.
        public static async Task SaveLocationDataAsync()
        {
            var response = await _httpClient.GetAsync(_locationApiUrl);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                IpLocation location = Newtonsoft.Json.JsonConvert.DeserializeObject<IpLocation>(content);
                await ApplicationActivity.ChangeApplicationActivityDataByLocation(location);
            }
        }
    }

    public class IpLocation
    {
        public string status { get; set; }
        public string country { get; set; }
        public string countryCode { get; set; }
        public string region { get; set; }
        public string regionName { get; set; }
        public string city { get; set; }
        public string zip { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }
        public string timezone { get; set; }
        public string isp { get; set; }
        public string org { get; set; }
        public string @as { get; set; }
        public string query { get; set; }
    }
}
