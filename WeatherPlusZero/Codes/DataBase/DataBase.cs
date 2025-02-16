using System;
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

namespace WeatherPlusZero
{
    public static class DataBase
    {
        private static readonly Supabase.Client supabase;
        private static readonly Dictionary<Type, PropertyInfo> primaryKeyCache;

        public static IConfiguration Configuration { get; }

        public static User user { get; private set; }

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
                var response = await supabase.From<User>().Insert(user);
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

            user = response;

            ApplicationActivityData data = new ApplicationActivityData()
            {
                UserNameSurname = user.namesurname,
                UserEmail = user.email,
            };

            ApplicationActivity.SaveApplicationActivityData(data);

            return true;
        }

        /// <summary>
        /// Logs out the current user using custom authentication.
        /// </summary>
        /// <returns>True if logout is successful.</returns>
        public static bool LogoutUserOwnAuth()
        {
            user = null;
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
        public string registrationdate { get; set; } // WARNING => Registration Date is given a value by Supabase. Therefore it cannot get value from here.
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