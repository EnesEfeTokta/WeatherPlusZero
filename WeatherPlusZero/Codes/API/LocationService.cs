using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WeatherPlusZero.Codes.API
{
    public static class LocationService
    {
        private static readonly HttpClient _httpClient;

        static LocationService()
        {
            _httpClient = new HttpClient();
        }

        // Get the location data.
        public static async Task<IpLocation> GetLocationDataByApiAsync()
        {
            var response = await _httpClient.GetAsync("http://ip-api.com/json");
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
            var response = await _httpClient.GetAsync("http://ip-api.com/json");
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
