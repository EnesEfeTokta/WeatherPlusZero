﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Reflection;
using System.Linq;
using static Supabase.Postgrest.Constants;
using Supabase.Postgrest.Exceptions;
using Notification.Wpf;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Windows.Media.Animation;

namespace WeatherPlusZero
{
    public static class DataBase
    {
        private static readonly Supabase.Client supabase;
        private static readonly Dictionary<Type, PropertyInfo> primaryKeyCache;

        public static IConfiguration Configuration { get; }

        public static User User { get; private set; }
        public static List<UserCity> UserCities { get; private set; }
        public static List<City> Cities { get; private set; }
        public static List<Weather> Weathers { get; private set; }
        public static List<Notification> Notifications { get; private set; }

        static DataBase()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string url = Configuration["Authentication:Supabase_Url"];
            string key = Configuration["Authentication:Supabase_Key"];
            supabase = new Supabase.Client(url, key);
            primaryKeyCache = new Dictionary<Type, PropertyInfo>();
        }

        private static PropertyInfo GetPrimaryKeyProperty<T>() where T : BaseModel, new()
        {
            var type = typeof(T);
            if (!primaryKeyCache.ContainsKey(type))
            {
                var property = type.GetProperties()
                    .FirstOrDefault(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);

                primaryKeyCache[type] = property;
            }

            return primaryKeyCache[type];
        }

        #region Supabase Auth Methods
        /// <summary>
        /// Registers a new user using Supabase authentication.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <param name="password">The password of the user.</param>
        public static async Task RegisterUserSupabaseAuth(string email, string password)
        {
            await supabase.Auth.SignUp(email, password);
        }

        /// <summary>
        /// Logs in a user using Supabase authentication.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <param name="password">The password of the user.</param>
        public static async Task LoginUserSupabaseAuth(string email, string password)
        {
            await supabase.Auth.SignIn(email, password);
        }

        /// <summary>
        /// Logs out the current user using Supabase authentication.
        /// </summary>
        public static async Task LogoutUserSupabaseAuth()
        {
            await supabase.Auth.SignOut();
        }

        /// <summary>
        /// Checks the current user session using Supabase authentication.
        /// </summary>
        public static void CheckUserSessionSupabaseAuth()
        {
            var user = supabase.Auth.CurrentUser;
        }
        #endregion

        #region Own Auth Methods
        /// <summary>
        /// Registers a new user using custom authentication.
        /// </summary>
        /// <param name="user">The user object containing user details.</param>
        /// <returns>True if registration is successful, otherwise false.</returns>
        public static async Task<bool> RegisterUserOwnAuth(User user)
        {
            try
            {
                await SetDatabaseData(user);
                return true;
            }
            catch (PostgrestException ex)
            {
                if (ex.Message.Contains("duplicate key value violates unique constraint \"users_email_key\""))
                    NotificationManagement.ShowNotification("Register Error", "This email is already in use. Please use another valid email.", NotificationType.Error);
                return false;
            }
        }

        /// <summary>
        /// Logs in a user using custom authentication.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>True if login is successful, otherwise false.</returns>
        public static async Task<bool> LoginUserOwnAuth(string email, string password)
        {
            User response = await supabase.From<User>()
                .Filter("email", Operator.Equals, email)
                .Filter("password", Operator.Equals, password)
                .Single();

            if (response == null)
                return false;

            User = response;

            await GetDatabaseData();

            NewSaveApplicationActivity();

            return true;
        }

        /// <summary>
        /// Sets the database data for a new user.
        /// </summary>
        /// <param name="user">The user object containing user details.</param>
        /// <returns>True if data is set successfully, otherwise false.</returns>
        private static async Task<bool> SetDatabaseData(User user)
        {
            User insertedUser = await TAsyncAddRow<User>(user);
            if (insertedUser == null) return false;

            // Ensure your static collections are instantiated.
            if (Cities == null) Cities = new List<City>();
            if (Weathers == null) Weathers = new List<Weather>();
            if (UserCities == null) UserCities = new List<UserCity>();
            if (Notifications == null) Notifications = new List<Notification>();

            City newCity = new City { cityname = "Ankara", countryname = "Turkey" };
            City insertedCity = await TAsyncAddRow<City>(newCity);
            if (insertedCity == null) return false;

            Weather newWeather = new Weather { cityid = insertedCity.cityid, weatherdata = await WeatherManager.GetWeatherDataAsync("Ankara", true) };
            Weather insertedWeather = await TAsyncAddRow<Weather>(newWeather);
            if (insertedWeather == null) return false;

            UserCity newUserCity = new UserCity { userid = insertedUser.userid, cityid = insertedCity.cityid, notificationpreference = true };
            UserCity insertedUserCity = await TAsyncAddRow<UserCity>(newUserCity);
            if (insertedUserCity == null) return false;

            Notification newNotification = new Notification { userid = insertedUser.userid, notificationtype = "Null", notificationmessage = "Null", notificationdatetime = DateTime.MinValue };
            Notification insertedNotification = await TAsyncAddRow<Notification>(newNotification);
            if (insertedNotification == null) return false;

            return true;
        }

        /// <summary>
        /// Retrieves the database data for the current user.
        /// </summary>
        /// <returns>True if data is retrieved successfully, otherwise false.</returns>
        private static async Task<bool> GetDatabaseData()
        {
            var userCityResponse = await supabase.From<UserCity>()
                .Filter("userid", Operator.Equals, User.userid)
                .Get();
            UserCities = userCityResponse.Models;

            Cities = new List<City>();
            Weathers = new List<Weather>();

            foreach (UserCity userCity in UserCities)
            {
                City cityResponse = await supabase.From<City>()
                    .Filter("cityid", Operator.Equals, userCity.cityid)
                    .Single();
                Cities.Add(cityResponse);

                Weather weatherResponse = await supabase.From<Weather>()
                    .Filter("cityid", Operator.Equals, userCity.cityid)
                    .Single();
                Weathers.Add(weatherResponse);
            }

            var notificationResponse = await supabase.From<Notification>()
                .Filter("userid", Operator.Equals, User.userid)
                .Get();
            Notifications = notificationResponse.Models;

            return true;
        }

        /// <summary>
        /// Saves the application activity data for the current user.
        /// </summary>
        private static void NewSaveApplicationActivity()
        {
            ApplicationActivityData data = new ApplicationActivityData
            {
                UserId = User.userid,
                CityId = Cities[0].cityid,
                UserCityId = UserCities[0].recordid,
                WeatherId = Weathers[0].weatherid,
                NotificationId = Notifications[0].notificationid,

                UserNameSurname = User.namesurname,
                UserEmail = User.email,

                SelectCity = Cities[0].cityname,
                IsInAppNotificationOn = UserCities[0].notificationpreference,
                IsDailyWeatherEmailsOpen = true,
                IsImportantWeatherEmailsOn = true,
            };

            ApplicationActivity.SaveApplicationActivityData(data);
        }

        /// <summary>
        /// Logs out the current user using custom authentication.
        /// </summary>
        /// <returns>True if logout is successful.</returns>
        public static bool LogoutUserOwnAuth()
        {
            User = null;
            Cities.Clear();
            Weathers.Clear();
            UserCities.Clear();
            Notifications.Clear();
            return true;
        }

        /// <summary>
        /// Checks if a user exists using custom authentication.
        /// </summary>
        /// <param name="user">The user object containing user details.</param>
        /// <returns>True if user exists, otherwise false.</returns>
        public static async Task<bool> ForgotUserOwnAuth(User user)
        {
            User response = await supabase.From<User>()
                .Filter("email", Operator.Equals, user.email)
                .Single();

            if (response == null)
                return false;

            return true;
        }

        /// <summary>
        /// Changes the password of a user using custom authentication.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <param name="newPassword">The new password of the user.</param>
        public static async Task ChangePasswordOwnAuth(string email, string newPassword)
        {
            User response = await TAsyncGetUserByEmail(email);

            response.password = newPassword;

            await supabase.From<User>().Update(response);
        }
        #endregion

        public static User GetUser() => User;
        public static int GetUserId() => User.userid;
        public static string GetUsername() => User.namesurname;
        public static string GetUserEmail() => User.email;
        public static string GetHashPassword() => User.password;
        public static DateTime GetRegistrationDate() => User.registrationdate;

        public static City GetCity() => Cities[0];
        public static int GetCityId() => Cities[0].cityid;
        public static string GetCityName() => Cities[0].cityname;
        public static string GetCountryName() => Cities[0].countryname;

        public static Weather GetWeather() => Weathers[0];
        public static int GetWeatherId() => Weathers[0].weatherid;
        public static int GetCityIdWeather() => Weathers[0].cityid;
        public static WeatherData GetWeatherData() => Weathers[0].weatherdata;

        public static UserCity GetUserCity() => UserCities[0];
        public static int GetRecordId() => UserCities[0].recordid;
        public static int GetUserIdUserCity() => UserCities[0].userid;
        public static int GetCityIdUserCity() => UserCities[0].cityid;
        public static bool GetNotificationPreference() => UserCities[0].notificationpreference;

        public static Notification GetNotification() => Notifications[0];
        public static int GetNotificationId() => Notifications[0].notificationid;
        public static int GetUserIdNotification() => Notifications[0].userid;
        public static string GetNotificationType() => Notifications[0].notificationtype;
        public static string GetNotificationMessage() => Notifications[0].notificationmessage;
        public static DateTime GetNotificationDateTime() => Notifications[0].notificationdatetime;

        /// <summary>
        /// Adds a new row to the database.
        /// </summary>
        /// <typeparam name="T">The type of the row to add.</typeparam>
        /// <param name="newRow">The new row to add.</param>
        /// <returns>The added row.</returns>
        public static async Task<T> TAsyncAddRow<T>(T newRow) where T : BaseModel, new()
        {
            try
            {
                var response = await supabase.From<T>().Insert(newRow);
                return response.Models[0];
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error: {e.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// Updates an existing row in the database.
        /// </summary>
        /// <typeparam name="T">The type of the row to update.</typeparam>
        /// <param name="updatedRow">The updated row.</param>
        /// <returns>The updated row.</returns>
        public static async Task<T> TAsyncUpdateRow<T>(T updatedRow) where T : BaseModel, new()
        {
            try
            {
                var response = await supabase.From<T>().Update(updatedRow);
                return response.Models[0];
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error: {e.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// Deletes a row from the database by its ID.
        /// </summary>
        /// <typeparam name="T">The type of the row to delete.</typeparam>
        /// <param name="rowId">The ID of the row to delete.</param>
        /// <returns>True if deletion is successful, otherwise false.</returns>
        public static async Task<bool> TAsyncDeleteRow<T>(int rowId) where T : BaseModel, new()
        {
            try
            {
                var property = GetPrimaryKeyProperty<T>();

                await supabase.From<T>()
                    .Filter(property.Name, Operator.Equals, rowId)
                    .Delete();

                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error: {e.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Gets a row from the database by its ID.
        /// </summary>
        /// <typeparam name="T">The type of the row to get.</typeparam>
        /// <param name="rowId">The ID of the row to get.</param>
        /// <returns>The row with the specified ID.</returns>
        public static async Task<T> TAsyncGetRowById<T>(int rowId) where T : BaseModel, new()
        {
            try
            {
                var property = GetPrimaryKeyProperty<T>();

                var response = await supabase.From<T>()
                    .Filter(property.Name, Operator.Equals, rowId)
                    .Single();

                return response;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error: {e.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// Gets a user from the database by their email.
        /// </summary>
        /// <param name="email">The email of the user to get.</param>
        /// <returns>The user with the specified email.</returns>
        public static async Task<User> TAsyncGetUserByEmail(string email)
        {
            try
            {
                User response = await supabase.From<User>()
                    .Filter("email", Operator.Equals, email)
                    .Single();

                return response;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error: {e.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// Gets all rows of a specified type from the database.
        /// </summary>
        /// <typeparam name="T">The type of the rows to get.</typeparam>
        /// <returns>A list of all rows of the specified type.</returns>
        public static async Task<List<T>> TAsyncGetAllRows<T>() where T : BaseModel, new()
        {
            try
            {
                var response = await supabase.From<T>().Get();
                return response.Models;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error: {e.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }

    [Table("users")]
    public class User : BaseModel
    {
        [PrimaryKey("userid")]
        public int userid { get; set; } // WARNING => User ID is given a random number by Supabase. So it cannot get value from here.

        [Column("namesurname")]
        public string namesurname { get; set; } // WARNING => NOT NULL

        [Column("email")]
        public string email { get; set; } // WARNING => NOT NULL & UNIQUE

        [Column("password")]
        public string password { get; set; } // WARNING => NOT NULL

        [Column("registrationdate")]
        public DateTime registrationdate { get; set; } // WARNING => Registration Date is given a value by Supabase. Therefore it cannot get value from here.
    }

    [Table("cities")]
    public class City : BaseModel
    {
        [PrimaryKey("cityid")]
        public int cityid { get; set; } // WARNING => City ID is given a random number by Supabase. So it cannot get value from here.

        [Column("cityname")]
        public string cityname { get; set; } // WARNING => NOT NULL

        [Column("countryname")]
        public string countryname { get; set; } // WARNING => NOT NULL
    }

    [Table("weather")]
    public class Weather : BaseModel
    {
        [PrimaryKey("weatherid")]
        public int weatherid { get; set; } // WARNING => Weather ID is given a random number by Supabase. So it cannot get value from here.

        [Column("cityid")]
        public int cityid { get; set; } // WARNING => FOREIGN KEY

        [Column("weatherdata")]
        public WeatherData weatherdata { get; set; } // WARNING => NOT NULL
    }

    [Table("usercities")]
    public class UserCity : BaseModel
    {
        [PrimaryKey("recordid")]
        public int recordid { get; set; } // WARNING => Record ID is given a random number by Supabase. So it cannot get value from here.

        [Column("userid")]
        public int userid { get; set; } // WARNING => FOREIGN KEY

        [Column("cityid")]
        public int cityid { get; set; } // WARNING => FOREIGN KEY

        [Column("notificationpreference")]
        public bool notificationpreference { get; set; } // WARNING => NOT NULL
    }

    [Table("notifications")]
    public class Notification : BaseModel
    {
        [PrimaryKey("notificationid")]
        public int notificationid { get; set; } // WARNING => Notification ID is given a random number by Supabase. So it cannot get value from here.

        [Column("userid")]
        public int userid { get; set; } // WARNING => FOREIGN KEY

        [Column("notificationtype")]
        public string notificationtype { get; set; } // WARNING => NOT NULL

        [Column("notificationmessage")]
        public string notificationmessage { get; set; } // WARNING => NOT NULL

        [Column("notificationdatetime")]
        public DateTime notificationdatetime { get; set; } // WARNING => Notification Date Time is given a value by Supabase. Therefore it cannot get value from here.
    }
}