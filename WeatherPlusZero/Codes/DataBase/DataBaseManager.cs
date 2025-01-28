using System;
using System.Net.Http;
using System.Text;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace WeatherPlusZero
{
    public class DataBase
    {
        private readonly Supabase.Client supabase;

        public DataBase()
        {
            var url = "https://szqsnyrrzydtgzqxwfwt.supabase.co";
            var key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InN6cXNueXJyenlkdGd6cXh3Znd0Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzM2OTc3NzMsImV4cCI6MjA0OTI3Mzc3M30.AJDtWaNxLLjGPRsecSqG7Cmf7KRiQaA6QgRxWwoNatk";
            supabase = new Supabase.Client(url, key);
        }



        // İlgili tabloya yeni satır ekler.
        public async Task<T> TAsyncAddRow<T>(T newRow) where T : BaseModel, new()
        {
            var response = await supabase.From<T>().Insert(newRow);

            return response.Models[0];
        }

        // İlgili tablodaki satırı günceller.
        public async Task<T> TAsyncUpdateRow<T>(T updatedRow) where T : BaseModel, new()
        {
            var response = await supabase.From<T>().Update(updatedRow);

            return response.Models[0];
        }

        // İlgili tablodaki satırı siler.
        public async Task<bool> TAsyncDeleteRow<T>(int rowId) where T : BaseModel, IEntity, new()
        {
            var temp = new T();

            await supabase
              .From<T>()
              .Where(x => ((IEntity)x).GetPrimaryKey() == rowId)
              .Delete();

            return true;
        }

        // İlgili tablodaki belirli bir satırı getirir.
        public async Task<T> TAsyncGetRowById<T>(int rowId) where T : BaseModel, IEntity, new()
        {
            var response = await supabase.From<T>()
                                          .Where(x => ((IEntity)x).GetPrimaryKey() == rowId)
                                          .Single();

            return response;
        }

        // İlgili tablodaki tüm satırları getirir.
        public async Task<List<T>> TAsyncGetAllRows<T>() where T : BaseModel, new()
        {
            var response = await supabase.From<T>().Get();

            return response.Models;
        }
    }

    public interface IEntity
    {
        int GetPrimaryKey();
    }

    [Table("users")]
    public class User : BaseModel, IEntity
    {
        [PrimaryKey("userid")]
        public int userid { get; set; }

        [Column("namesurname")]
        public string namesurname { get; set; }

        [Column("email")]
        public string email { get; set; }

        [Column("password")]
        public string password { get; set; }

        [Column("registrationdate")]
        public DateTime registrationdate { get; set; }

        public int GetPrimaryKey()
        {
            return userid;
        }
    }

    [Table("cities")]
    public class City : BaseModel, IEntity
    {
        [PrimaryKey("cityid")]
        public int cityid { get; set; }

        [Column("cityname")]
        public string cityname { get; set; }

        [Column("countryname")]
        public string countryname { get; set; }

        public int GetPrimaryKey()
        {
            return cityid;
        }
    }

    [Table("weather")]
    public class Weather : BaseModel, IEntity
    {
        [PrimaryKey("weatherid")]
        public int weatherid { get; set; }

        [Column("cityid")]
        public int cityid { get; set; }

        [Column("weatherdata")]
        public WeatherData weatherdata { get; set; }

        public int GetPrimaryKey()
        {
            return weatherid;
        }
    }

    [Table("usercities")]
    public class UserCity : BaseModel, IEntity
    {
        [PrimaryKey("recordid")]
        public int recordid { get; set; }

        [Column("userid")]
        public int userid { get; set; }

        [Column("notificationpreference")]
        public bool notificationpreference { get; set; }

        public int GetPrimaryKey()
        {
            return recordid;
        }
    }

    [Table("notifications")]
    public class Notification : BaseModel, IEntity
    {
        [PrimaryKey("notificationid")]
        public int notificationid { get; set; }

        [Column("userid")]
        public int userid { get; set; }

        [Column("notificationtype")]
        public string notificationtype { get; set; }

        [Column("notificationmessage")]
        public string notificationmessage { get; set; }

        [Column("notificationdatetime")]
        public DateTime notificationdatetime { get; set; }

        public int GetPrimaryKey()
        {
            return notificationid;
        }
    }
}