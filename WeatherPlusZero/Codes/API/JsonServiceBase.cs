using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net.NetworkInformation;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace WeatherPlusZero
{
    public abstract class JsonServiceBase
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

        // Path to the json file for ApplicationActivityData (şifrelenmiş olarak saklanacak)
        protected static readonly string ApplicationActivityDataJsonFilePath = Path.Combine(AppDataPath, "ApplicationActivityData.json");

        static JsonServiceBase()
        {
            Directory.CreateDirectory(AppDataPath);

            // Ensure WeatherData.json exists (plain text)
            if (!File.Exists(WeatherDataJsonFilePath))
            {
                File.WriteAllText(WeatherDataJsonFilePath, "{}");
            }

            // Ensure ApplicationActivityData.json exists (encrypted empty JSON object)
            if (!File.Exists(ApplicationActivityDataJsonFilePath))
            {
                byte[] encryptedEmpty = Encryption.EncryptData(Encoding.UTF8.GetBytes("{}"));
                File.WriteAllBytes(ApplicationActivityDataJsonFilePath, encryptedEmpty);
            }
        }

        protected IConfiguration Configuration { get; }

        public JsonServiceBase()
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

    // Class representing weather forecasts for future days.
    // Used to display daily forecasts in the UI.
    public class FutureDay
    {
        public string DayName { get; set; } // Day of the week name.
        public string IconPath { get; set; } // Icon path.
        public string MinMaxTemperature { get; set; } // Min-Max temperature values.
    }
}