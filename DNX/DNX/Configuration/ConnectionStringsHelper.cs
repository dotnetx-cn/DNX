using System.Configuration;

namespace DNX.Configuration
{
    public static class ConnectionStringsHelper
    {
        /// <summary>
        /// Gets the value of the database connection string in the Web.config or App.config connectionStrings node
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetConnectionString(string name)
        {
            return string.Empty;
        }

        /// <summary>
        /// Create and access to the System. The Configuration. ConnectionStringSettings object
        /// </summary>
        /// <param name="name">connectionStrings Specifies the name of the node</param>
        /// <returns></returns>
        public static ConnectionStringSettings GetConnectionStringSettings(string name)
        {
            if (string.IsNullOrEmpty(name.Trim()))
                return new ConnectionStringSettings();

            try
            {
                return ConfigurationManager.ConnectionStrings[name];
            }
            catch
            {
                return new ConnectionStringSettings();
            }
        }
    }
}
