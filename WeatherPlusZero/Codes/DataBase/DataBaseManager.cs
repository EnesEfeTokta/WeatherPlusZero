﻿using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Windows;

namespace WeatherPlusZero
{
    public class DataBase
    {
        protected const string supabaseUrl = "https://szqsnyrrzydtgzqxwfwt.supabase.co";
        protected const string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InN6cXNueXJyenlkdGd6cXh3Znd0Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzM2OTc3NzMsImV4cCI6MjA0OTI3Mzc3M30.AJDtWaNxLLjGPRsecSqG7Cmf7KRiQaA6QgRxWwoNatk";
    }

    public class SetData : DataBase
    {
        public async Task<bool> AddRow(object newRow, string tableName)
        {
            using (var client = new HttpClient())
            {
                var endpoint = $"{supabaseUrl}/rest/v1/{tableName}";
                var json = JsonSerializer.Serialize(newRow);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("apikey", supabaseKey);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseKey}");
                client.DefaultRequestHeaders.Add("Prefer", "return=minimal");

                HttpResponseMessage response = await client.PostAsync(endpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Ekleme Hatası: {response.StatusCode} - {errorMessage}", "Hata");
                    return false;
                }

                return response.IsSuccessStatusCode;
            }
        }

        public async Task<bool> DeleteRow(int primaryKey, string primaryColumnName, string tableName)
        {
            using (var client = new HttpClient())
            {
                var endpoint = $"{supabaseUrl}/rest/v1/{tableName}?{primaryColumnName}=eq.{primaryKey}";

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("apikey", supabaseKey);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseKey}");

                HttpResponseMessage response = await client.DeleteAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Silme Hatası: {response.StatusCode} - {errorMessage}", "Hata");
                    return false;
                }

                return response.IsSuccessStatusCode;
            }
        }

        public async Task<bool> EditRow(object updatedRow, string tableName, string primaryColumnName, int primaryKey)
        {
            using (var client = new HttpClient())
            {
                var endpoint = $"{supabaseUrl}/rest/v1/{tableName}?{primaryColumnName}=eq.{primaryKey}";
                var json = JsonSerializer.Serialize(updatedRow);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("apikey", supabaseKey);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseKey}");
                client.DefaultRequestHeaders.Add("Prefer", "return=minimal");

                HttpResponseMessage response = await client.PatchAsync(endpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Güncelleme Hatası: {response.StatusCode} - {errorMessage}", "Hata");
                    return false;
                }

                return response.IsSuccessStatusCode;
            }
        }
    }

    public class GetData : DataBase
    {
        public async Task<string> ReadSpecificRow(string tableName, string primaryColumnName, int primaryKey)
        {
            using (var client = new HttpClient())
            {
                var endpoint = $"{supabaseUrl}/rest/v1/{tableName}?{primaryColumnName}=eq.{primaryKey}";

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("apikey", supabaseKey);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseKey}");

                HttpResponseMessage response = await client.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Okuma Hatası: {response.StatusCode} - {errorMessage}", "Hata");
                    return null;
                }

                return await response.Content.ReadAsStringAsync();
            }
        }
    }

    public class User
    {
        public string namesurname { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public DateTime registrationdate { get; set; }
    }

    public class City
    {
        public string cityname { get; set; }
        public string countryname { get; set; }
    }
}