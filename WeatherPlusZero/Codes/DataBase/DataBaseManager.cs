using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Reflection;
using System.Linq;
using static Supabase.Postgrest.Constants;

namespace WeatherPlusZero
{
    public class DataBase
    {
        private readonly Supabase.Client supabase;
        private readonly Dictionary<Type, PropertyInfo> primaryKeyCache;

        public User user { get; set; }

        public DataBase()
        {
            var url = "https://szqsnyrrzydtgzqxwfwt.supabase.co";
            var key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InN6cXNueXJyenlkdGd6cXh3Znd0Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzM2OTc3NzMsImV4cCI6MjA0OTI3Mzc3M30.AJDtWaNxLLjGPRsecSqG7Cmf7KRiQaA6QgRxWwoNatk";
            supabase = new Supabase.Client(url, key);

            primaryKeyCache = new Dictionary<Type, PropertyInfo>();
        }

        /// <summary>
        /// Retrieves the PropertyInfo of the primary key for a given BaseModel type.
        /// Caches the result for subsequent lookups to improve performance.
        /// </summary>
        /// <typeparam name="T">The BaseModel type to get the primary key property for.</typeparam>
        /// <returns>The PropertyInfo of the primary key, or null if not found.</returns>
        private PropertyInfo GetPrimaryKeyProperty<T>() where T : BaseModel, new()
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
        public async Task RegisterUserSupabaseAuth(string email, string password)
        {
            await supabase.Auth.SignUp(email, password);
        }

        public async Task LoginUserSupabaseAuth(string email, string password)
        {
            await supabase.Auth.SignIn(email, password);
        }

        public async Task LogoutUserSupabaseAuth()
        {
            await supabase.Auth.SignOut();
        }

        public void CheckUserSessionSupabaseAuth()
        {
            var user = supabase.Auth.CurrentUser;
        }
        #endregion

        #region Own Auth Methods
        public async Task<bool> RegisterUserOwnAuth(User user)
        {
            var response = await supabase.From<User>().Insert(user);
            return true;
        }

        public async Task<bool> LoginUserOwnAuth(string email, string password)
        {
            User response = await supabase.From<User>()
                .Filter("email", Operator.Equals, email)
                .Filter("password", Operator.Equals, password)
                .Single();

            if (response == null)
                return false;

            user = response;

            return true;
        }

        public bool LogoutUserOwnAuth()
        {
            user = null;
            return true;
        }
        #endregion


        /// <summary>
        /// Asynchronously adds a new row to the corresponding Supabase table. 
        /// PrimaryKeys and some columns do not need to be given a value as they are autofilled. 
        /// Supabase automatically gives a value.
        /// </summary>
        /// <typeparam name="T">The BaseModel type representing the table to add the row to.</typeparam>
        /// <param name="newRow">The BaseModel object containing the data for the new row.</param>
        /// <returns>A Task that represents the asynchronous add operation. Returns the added BaseModel object of type T if successful, otherwise null.</returns>
        public async Task<T> TAsyncAddRow<T>(T newRow) where T : BaseModel, new()
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
        /// Asynchronously updates an existing row in the corresponding Supabase table. 
        /// The primary key must be known to update. 
        /// </summary>
        /// <typeparam name="T">The BaseModel type representing the table to update the row in.</typeparam>
        /// <param name="updatedRow">The BaseModel object containing the updated data for the row.</param>
        /// <returns>A Task that represents the asynchronous update operation. Returns the updated BaseModel object of type T if successful, otherwise null.</returns>
        public async Task<T> TAsyncUpdateRow<T>(T updatedRow) where T : BaseModel, new()
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
        /// Asynchronously deletes a row from the corresponding Supabase table based on its primary key ID.
        /// </summary>
        /// <typeparam name="T">The BaseModel type representing the table to delete the row from.</typeparam>
        /// <param name="rowId">The integer ID of the row to delete (primary key value).</param>
        /// <returns>A Task that represents the asynchronous delete operation. Returns true if deletion is successful, otherwise false.</returns>
        public async Task<bool> TAsyncDeleteRow<T>(int rowId) where T : BaseModel, new()
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
        /// Asynchronously retrieves a specific row from the corresponding Supabase table based on its primary key ID.
        /// </summary>
        /// <typeparam name="T">The BaseModel type representing the table to get the row from.</typeparam>
        /// <param name="rowId">The integer ID of the row to retrieve (primary key value).</param>
        /// <returns>A Task that represents the asynchronous get operation. Returns the BaseModel object of type T if found, otherwise null.</returns>
        public async Task<T> TAsyncGetRowById<T>(int rowId) where T : BaseModel, new()
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
        /// Asynchronously retrieves a specific row from the corresponding Supabase table based on a specific E-Mail value.
        /// </summary>
        /// <param name="email">The e-mail address you want to call should be given.</param>
        /// <returns>If the row with the related e-mail is found, it returns User type, but if the row is not found, it returns null.</returns>
        public async Task<User> TAsyncGetUserByEmail(string email)
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
        /// Asynchronously retrieves all rows from the corresponding Supabase table.
        /// </summary>
        /// <typeparam name="T">The BaseModel type representing the table to get all rows from.</typeparam>
        /// <returns>A Task that represents the asynchronous get all operation. Returns a List of BaseModel objects of type T if successful, otherwise null.</returns>
        public async Task<List<T>> TAsyncGetAllRows<T>() where T : BaseModel, new()
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