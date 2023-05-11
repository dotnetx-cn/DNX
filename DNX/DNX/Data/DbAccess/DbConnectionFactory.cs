using System;
using System.Data.Common;

namespace DNX.Data.DbAccess
{
    /// <summary>
    /// Database connection factory class
    /// </summary>
    public class DbConnectionFactory
    {
        /// <summary>
        /// Create DbConnection Instance
        /// </summary>
        /// <returns></returns>
        public static DbConnection Create()
        {
            return Create(string.Empty);
        }

        /// <summary>
        /// Create DbConnection Instance
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static DbConnection Create(string name)
        {
            try
            {
                return DbConnectionManager.GetDbConnection(name);
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        /// <summary>
        /// Creates and returns the current connection, System.Data.Com. Mon DbCommand command object.
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static DbCommand GetDbCommand(DbConnection conn)
        {
            return GetDbCommand(conn, string.Empty);
        }

        /// <summary>
        /// Creates and returns the current connection, System.Data.Com. Mon DbCommand command object.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public static DbCommand GetDbCommand(DbConnection conn, string commandText)
        {
            DbCommand cmd = conn.CreateCommand();
            cmd.CommandTimeout = 1000 * 60 * 60;    

            if (!string.IsNullOrEmpty(commandText.Trim()))
                cmd.CommandText = commandText;

            return cmd;
        }

        /// <summary>
        /// Creates and returns the current connection, System.Data.Com. Mon DbCommand command object.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public static DbCommand GetDbCommand(DbConnection conn, DbTransaction trans)
        {
            return GetDbCommand(conn, string.Empty, trans);
        }

        /// <summary>
        /// Creates and returns the current connection, System.Data.Common DbCommand command object.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="commandText"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public static DbCommand GetDbCommand(DbConnection conn, string commandText, DbTransaction trans)
        {
            DbCommand cmd = conn.CreateCommand();
            cmd.Connection = conn;
            cmd.Transaction = trans;
            cmd.CommandTimeout = 1000 * 60 * 60;
            if (!string.IsNullOrEmpty(commandText.Trim()))
                cmd.CommandText = commandText;

            return cmd;
        }

        /// <summary>
        /// Creates and returns the current connection, System.Data.Command.DbDataAdapter command object.
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static DbDataAdapter GetDbDataAdapter(DbConnection conn)
        {
            return GetDbDataAdapter(conn, string.Empty,null);
        }

        /// <summary>
        /// Creates and returns the current connection, System.Data.Command.DbDataAdapter command object.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public static DbDataAdapter GetDbDataAdapter(DbConnection conn, string commandText)
        {
            return GetDbDataAdapter(conn, commandText, null);
        }

        /// <summary>
        /// Creates and returns the current connection, System.Data.Command.DbDataAdapter command object.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="commandText"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public static DbDataAdapter GetDbDataAdapter(DbConnection conn, string commandText, DbTransaction trans)
        {
            try
            {
                if (conn.State != System.Data.ConnectionState.Open)
                    conn.Open();

                DbProviderFactory factory = DbProviderFactories.GetFactory(conn.GetType().Namespace);

                DbDataAdapter adapater = factory.CreateDataAdapter();

                if (trans == null)
                {
                    adapater.SelectCommand = string.IsNullOrEmpty(commandText.Trim())
                        ? GetDbCommand(conn) : GetDbCommand(conn, commandText);
                }
                else
                {
                    adapater.SelectCommand = string.IsNullOrEmpty(commandText.Trim())
                        ? GetDbCommand(conn, trans) : GetDbCommand(conn, commandText, trans);
                    adapater.SelectCommand.Transaction = trans;
                }

                return adapater;
            }
            catch(Exception err)
            {
                throw err;
            }
        }
    }
}
