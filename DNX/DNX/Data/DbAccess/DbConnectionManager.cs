
using System;
using System.Configuration;
using System.Data.Common;
using DNX.Configuration;

namespace DNX.Data.DbAccess
{
    /// <summary>
    /// DbConnection management class
    /// </summary>
    public class DbConnectionManager
    {
        /// <summary>
        /// Gets whether the specified database connection is defined
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool ConnectionNameIsConfiged(string name)
        {
            return ConnectionStringsHelper.GetConnectionStringSettings(name) != null;
        }

        /// <summary>
        /// Creates and returns the current connection with associated System.Data.Com mon. DbConnection object.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static DbConnection GetDbConnection(string name)
        {
            if (ConnectionNameIsConfiged(name))
            {
                DbConnection dbConnection = GetDbProviderFactory(name).CreateConnection();
                dbConnection.ConnectionString = ConnectionStringsHelper.GetConnectionStringSettings(name).ConnectionString;

                return dbConnection;
            }
            throw new ArgumentNullException(name);
        }

        internal static DbProviderFactory GetDbProviderFactory(ConnectionStringSettings settings)
        {
            string providerName = string.IsNullOrEmpty(settings.ProviderName)
                ? "System.Data.SqlClient" : settings.ProviderName;

            return DbProviderFactories.GetFactory(providerName);
        }

        /// <summary>
        /// Gets the DbProviderFactory object for the specified connection
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static DbProviderFactory GetDbProviderFactory(string name)
        {
            if (!string.IsNullOrEmpty(name.Trim()))
            {
                ConnectionStringSettings settings = ConnectionStringsHelper.GetConnectionStringSettings(name);

                string providerName = string.IsNullOrEmpty(settings.ProviderName)
                    ? "System.Data.SqlClient" : settings.ProviderName;

                return DbProviderFactories.GetFactory(providerName);
            }
            else
            {
                throw new ArgumentNullException(name.Trim());
            }
        }
    }
}
