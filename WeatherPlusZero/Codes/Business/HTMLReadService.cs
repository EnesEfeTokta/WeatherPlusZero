using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace WeatherPlusZero
{
    /// <summary>
    /// Provides methods for reading HTML files.
    /// </summary>
    public static class HTMLReadService
    {
        /// <summary>
        /// Reads the content of an HTML file based on the specified email type.
        /// </summary>
        /// <param name="emailSendType">The type of email to read the HTML for.</param>
        /// <returns>The HTML content as a string.</returns>
        public static string ReadHTML(EmailSendType emailSendType)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().FirstOrDefault(name => name.EndsWith(emailSendType + ".html"));

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
