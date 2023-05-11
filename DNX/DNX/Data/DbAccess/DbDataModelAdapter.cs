
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using DNX.Core;
using DNX.Data.Attributes;
using DNX.Data.SQL;
using DNX.Runtime;

namespace DNX.Data.DbAccess
{
    /// <summary>
    /// Database operation core methods (If using distributed transactions, do not assign database transaction objects in any method)
    /// <remarks>Static and non-static instance supports distributed transactions System. Transaction. The TransactionScope</remarks>
    /// </summary>
    public class DbDataModelAdapter
    {
        #region constructor
        /// <summary>
        /// constructor
        /// </summary>
        public DbDataModelAdapter()
        {
        }
        #endregion

        #region Static instance
        /// <summary>
        /// Statically instantiated (method calls, because of concurrency issues, are still multi-instance types)
        /// </summary>
        public static DbDataModelAdapter Instance
        {
            get
            {
                return new DbDataModelAdapter();
            }
        }
        #endregion

        #region Data Insert
        /// <summary>
        /// Inserts data to the specified data table based on the object
        /// </summary>
        /// <param name="oData"></param>
        /// <returns></returns>
        public Result InsertModel(object oData)
        {
            return InsertModel(oData, false, null);
        }
        /// <summary>
        /// Inserts data to the specified data table based on the object
        /// </summary>
        /// <param name="oData"></param>
        /// <param name="returnValue"></param>
        /// <returns></returns>
        public Result InsertModel(object oData, bool returnValue)
        {
            return InsertModel(oData, returnValue);
        }

        /// <summary>
        /// Inserts data to the specified data table based on the object
        /// </summary>
        /// <param name="oData"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public Result InsertModel(object oData, DbTransaction trans)
        {
            return InsertModel(oData, false, trans);
        }

        /// <summary>
        /// Inserts data to the specified data table based on the object
        /// </summary>
        /// <param name="oData"></param>
        /// <param name="returnValue"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public Result InsertModel(object oData, bool returnValue, DbTransaction trans)
        {
            Type t = oData.GetType();

            try
            {
                DataSchemaAttribute attrData = AttributeHelper.GetCustomerAttribute<DataSchemaAttribute>(t);

                if (attrData == null)
                    return new Result(ErrorType.DataDesignError, "Design error", "Object is not configured with the [DataSchemaAttribute] feature");


                if (trans == null)
                {
                    using (DbConnection conn = DbConnectionFactory.Create(attrData.DataSource))
                    {
                        string sqlCommand = SQLFactory.GetInsertSqlStr(oData, attrData.Name);

                        if (returnValue)
                        {
                            DataTable dt = new DataTable();

                            using (DbDataAdapter adapter = DbConnectionFactory.GetDbDataAdapter(conn, sqlCommand))
                            {
                                adapter.Fill(dt);

                                if (dt.Rows.Count > 0)
                                    return new Result(dt.Rows[0][0]);
                                else
                                   return new Result(ErrorType.OperationFailure, "Logical error", "Data has no return value or data insertion failed");
                            }
                        }
                        else
                        {
                            using (DbCommand cmd = DbConnectionFactory.GetDbCommand(conn, sqlCommand))
                            {
                                if (conn.State != ConnectionState.Open)
                                    conn.Open();
                                cmd.Connection = conn;

                                int affectRows = cmd.ExecuteNonQuery();

                                if (affectRows > 0)
                                    return new Result(affectRows);
                                else
                                       return new Result(ErrorType.OperationFailure, "Logical error", "Data insertion failed. The cause of the error is unknown");
                            }
                        }
                    }
                }
                else
                {
                    DbConnection conn = trans.Connection;

                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    string sqlCommand = SQLFactory.GetInsertSqlStr(oData, attrData.Name);

                    if (returnValue)
                    {
                        DataTable dt = new DataTable();
                        DbDataAdapter adapter = DbConnectionFactory.GetDbDataAdapter(conn, sqlCommand);

                        adapter.Fill(dt);

                        if (dt.Rows.Count > 0)
                            return new Result(dt.Rows[0][0]);
                        else
                            return new Result(ErrorType.OperationFailure, "Logical error", "Data has no return value or data insertion failed");
                    }
                    else
                    {
                        DbCommand cmd = DbConnectionFactory.GetDbCommand(conn, sqlCommand, trans);

                        int affectRows = cmd.ExecuteNonQuery();

                        if (affectRows > 0)
                            return new Result(affectRows);
                        else
                            return new Result(ErrorType.OperationFailure, "Logical error", "Data insertion failed. The cause of the error is unknown");
                    }
                }
            }
             catch (TimeoutException)
            {
                return new Result(ErrorType.OperationTimeout, "Access", "Timeout");
            }
            catch (SqlException err)
            {
                return SQLExceptionHelper.SQLExceptionAnalyze(err);
            }
            catch (OutOfMemoryException)
            {
                return new Result(ErrorType.OperationFailure, "System", "A memory overflow occurred during a database query");
            }
            catch (DbException err)
            {
                return new Result(ErrorType.OperationFailure, "Data Engine", ExceptionHelper.GetRealException(err).Message);
            }
            catch (Exception err)
            {
                return new Result(ErrorType.SupportFailed, "Unknow", ExceptionHelper.GetRealException(err).Message);
            }
        }
        #endregion

        #region Batch Data Insert
        /// <summary>
        /// Bulk insert data into the database using the base InsertModel as the core
        /// </summary>
        /// <param name="oData"></param>
        /// <returns></returns>
        public Result InsertModels(object[] oData)
        {
            return InsertModels(oData, null);
        }

        /// <summary>
        /// Bulk insert data into the database using the base InsertModel as the core
        /// </summary>
        /// <param name="oData"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public Result InsertModels(object[] oData, DbTransaction trans)
        {
            int affectRows = 0;

            foreach (object item in oData)
            {
                Result result = InsertModel(oData, trans);

                if (!result.Succeed)
                    return result;

                affectRows++;
            }

            return new Result(affectRows);
        }
        #endregion

        #region Data Update
        /// <summary>
        /// Update data objects (update a batch of objects based on conditions, with updated properties specified in SQLUpdateCondition)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uc"></param>
        /// <param name="wc"></param>
        /// <returns></returns>
        public Result UpdateModels<T>(SQLUpdateCondition uc, SQLWhereCondition wc)
        {
            return UpdateModels<T>(uc, wc, null);
        }
        /// <summary>
        /// Update data objects (update a batch of objects based on conditions, with updated properties specified in SQLUpdateCondition)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uc"></param>
        /// <param name="wc"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public Result UpdateModels<T>(SQLUpdateCondition uc, SQLWhereCondition wc, DbTransaction trans)
        {
            if (uc == null || string.IsNullOrEmpty(uc.ToSqlString().Trim()))
                return new Result(0);

            try
            {
                DataSchemaAttribute attrData = AttributeHelper.GetCustomerAttribute<DataSchemaAttribute>(typeof(T));

                if (attrData == null)
                    return new Result(ErrorType.DataDesignError, "Design error", "Object is not configured with the [DataSchemaAttribute] feature");

                if (trans == null)
                {
                    using (DbConnection conn = DbConnectionFactory.Create(attrData.DataSource))
                    {
                        string sqlCommand = string.Format("UPDATE [{0}] SET {1} WHERE {2}", attrData.Name, uc.ToSqlString(), wc.ToSqlString().Length > 0 ? wc.ToSqlString() : "1=1");

                        using (DbCommand command = DbConnectionFactory.GetDbCommand(conn, sqlCommand))
                        {
                            if (conn.State != ConnectionState.Open)
                                conn.Open();

                            command.Connection = conn;

                            int affectedRows = command.ExecuteNonQuery();

                            return new Result(affectedRows);
                        }
                    }
                }
                else
                {
                    DbConnection conn = trans.Connection;

                    string sqlCommand = string.Format("UPDATE [{0}] SET {1} WHERE {2}", attrData.Name, uc.ToSqlString(), wc.ToSqlString().Length > 0 ? wc.ToSqlString() : "1=1");

                    DbCommand command = DbConnectionFactory.GetDbCommand(conn, sqlCommand, trans);

                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    int affectedRows = command.ExecuteNonQuery();

                    return new Result(affectedRows);
                }
            }
             catch (TimeoutException)
            {
                return new Result(ErrorType.OperationTimeout, "Access", "Timeout");
            }
            catch (SqlException err)
            {
                return SQLExceptionHelper.SQLExceptionAnalyze(err);
            }
            catch (OutOfMemoryException)
            {
                return new Result(ErrorType.OperationFailure, "System", "A memory overflow occurred during a database query");
            }
            catch (DbException err)
            {
                return new Result(ErrorType.OperationFailure, "Data Engine", ExceptionHelper.GetRealException(err).Message);
            }
            catch (Exception err)
            {
                return new Result(ErrorType.SupportFailed, "Unknow", ExceptionHelper.GetRealException(err).Message);
            }
        }
        #endregion

        #region Data Load

        /// <summary>
        ///  Load object (Loads an object that matches the specified criteria
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="wc"></param>
        /// <returns></returns>
        public Result LoadModel<T>(SQLWhereCondition wc)
        {
            return LoadModel<T>(wc, null);
        }

        /// <summary>
        ///  Load object (Loads an object that matches the specified criteria
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="wc"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public Result LoadModel<T>(SQLWhereCondition wc, DbTransaction trans)
        {
            return LoadModel<T>(wc, new SQLOrderCondition(), trans);
        }

        /// <summary>
        /// Load object (Loads an object that matches the specified criteria
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="wc"></param>
        /// <param name="oc"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public Result LoadModel<T>(SQLWhereCondition wc, SQLOrderCondition oc, DbTransaction trans)
        {
            Type t = typeof(T);

            DataTable dt = new DataTable();

            try
            {
                DataSchemaAttribute attrData = AttributeHelper.GetCustomerAttribute<DataSchemaAttribute>(typeof(T));

                if (attrData == null)
                    return new Result(ErrorType.DataDesignError, "Design error", "The specified Model does not contain the [DataSchemaAttribute] feature");

                if (trans == null)
                {
                    using (DbConnection conn = DbConnectionFactory.Create(attrData.DataSource))
                    {
                        string sqlCommand = string.Format("SELECT TOP 1 * FROM [{0}] WHERE {1} ORDER BY {2}",
                            attrData.Name, wc.ToSqlString().Length > 0 ? wc.ToSqlString() : "1=1", oc.ToSqlString().Length > 0 ? oc.ToSqlString() : "(SELECT 0)");

                        using (DbDataAdapter adapter = DbConnectionFactory.GetDbDataAdapter(conn, sqlCommand))
                        {
                            if (conn.State != ConnectionState.Open)
                                conn.Open();

                            adapter.Fill(dt);
                        }
                    }
                }
                else
                {
                    DbConnection conn = trans.Connection;

                    string sqlCommand = string.Format("SELECT TOP 1 * FROM [{0}] WHERE {1} ORDER BY {2}",
                            attrData.Name, wc.ToSqlString().Length > 0 ? wc.ToSqlString() : "1=1", oc.ToSqlString().Length > 0 ? oc.ToSqlString() : "(SELECT 0)");

                    DbDataAdapter adapter = DbConnectionFactory.GetDbDataAdapter(conn, sqlCommand, trans);

                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    adapter.Fill(dt);
                }

                if (dt.Rows.Count > 0)
                {
                    object obj = TypeCreator.CreateInstance(typeof(T));

                    DataToObject.GetDataFromDataRow(obj, dt.Rows[0]);

                    if (obj == null)
                        return new Result(ErrorType.OperationFailure, "Data Convert Error", string.Format("An error occurred while converting data object {0}", typeof(T).FullName));

                    return new Result(obj);
                }
                else
                {
                    return new Result(null);
                }
            }
            catch (TimeoutException)
            {
                return new Result(ErrorType.OperationTimeout, "Access", "Timeout");
            }
            catch (SqlException err)
            {
                return SQLExceptionHelper.SQLExceptionAnalyze(err);
            }
            catch (OutOfMemoryException)
            {
                return new Result(ErrorType.OperationFailure, "System", "A memory overflow occurred during a database query");
            }
            catch (DbException err)
            {
                return new Result(ErrorType.OperationFailure, "Data Engine", ExceptionHelper.GetRealException(err).Message);
            }
            catch (Exception err)
            {
                return new Result(ErrorType.SupportFailed, "Unknow", ExceptionHelper.GetRealException(err).Message);
            }
        }

        #endregion

        #region Data Delete
        /// <summary>
        /// Delete objects (delete all objects that meet the criteria)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="wc"></param>
        /// <returns></returns>
        public Result DeleteModels<T>(SQLWhereCondition wc)
        {
            return DeleteModels<T>(wc, null);
        }

        /// <summary>
        /// Delete objects (delete all objects that meet the criteria)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="wc"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public Result DeleteModels<T>(SQLWhereCondition wc, DbTransaction trans)
        {
            Type t = typeof(T);

            try
            {
                DataSchemaAttribute attrData = AttributeHelper.GetCustomerAttribute<DataSchemaAttribute>(typeof(T));

                if (attrData == null)
                    return new Result(ErrorType.DataDesignError, "Design error", "The specified Model does not contain the [DataSchemaAttribute] feature");

                if (trans == null)
                {
                    using (DbConnection conn = DbConnectionFactory.Create(attrData.DataSource))
                    {
                        string sqlCommand = string.Format("DELETE FROM [{0}] WHERE {1}", attrData.Name, wc.ToSqlString().Length > 0 ? wc.ToSqlString() : "1=1");

                        using (DbCommand command = DbConnectionFactory.GetDbCommand(conn, sqlCommand))
                        {
                            if (conn.State != ConnectionState.Open)
                                conn.Open();

                            command.Connection = conn;

                            int affectedRows = command.ExecuteNonQuery();

                            return new Result(affectedRows);
                        }
                    }
                }
                else
                {
                    DbConnection conn = trans.Connection;

                    string sqlCommand = string.Format("DELETE FROM [{0}] WHERE {1}", attrData.Name, wc.ToSqlString().Length > 0 ? wc.ToSqlString() : "1=1");

                    DbCommand command = DbConnectionFactory.GetDbCommand(conn, sqlCommand, trans);

                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    int affectedRows = command.ExecuteNonQuery();

                    return new Result(affectedRows);
                }
            }
            catch (TimeoutException)
            {
                return new Result(ErrorType.OperationTimeout, "Access", "Timeout");
            }
            catch (SqlException err)
            {
                return SQLExceptionHelper.SQLExceptionAnalyze(err);
            }
            catch (OutOfMemoryException)
            {
                return new Result(ErrorType.OperationFailure, "System", "A memory overflow occurred during a database query");
            }
            catch (DbException err)
            {
                return new Result(ErrorType.OperationFailure, "Data Engine", ExceptionHelper.GetRealException(err).Message);
            }
            catch (Exception err)
            {
                return new Result(ErrorType.SupportFailed, "Unknow", ExceptionHelper.GetRealException(err).Message);
            }
        }

        /// <summary>
        /// Deletes all rows in a table without recording a single row deletion.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Result TruncateTable<T>()
        {
            return TruncateTable<T>(null);
        }

        /// <summary>
        /// Deletes all rows in a table without recording a single row deletion.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="trans"></param>
        /// <returns></returns>
        public Result TruncateTable<T>(DbTransaction trans)
        {
            Type t = typeof(T);

            try
            {
                DataSchemaAttribute attrData = AttributeHelper.GetCustomerAttribute<DataSchemaAttribute>(typeof(T));

                if (attrData == null)
                   return new Result(ErrorType.DataDesignError, "Design error", "The specified Model does not contain the [DataSchemaAttribute] feature");

                if (trans == null)
                {
                    using (DbConnection conn = DbConnectionFactory.Create(attrData.DataSource))
                    {
                        string sqlCommand = string.Format("TRUNCATE TABLE [{0}]", attrData.Name);

                        using (DbCommand command = DbConnectionFactory.GetDbCommand(conn, sqlCommand))
                        {
                            if (conn.State != ConnectionState.Open)
                                conn.Open();

                            command.Connection = conn;

                            int affectedRows = command.ExecuteNonQuery();

                            return new Result(affectedRows);
                        }
                    }
                }
                else
                {
                    DbConnection conn = trans.Connection;

                    string sqlCommand = string.Format("TRUNCATE TABLE [{0}]", attrData.Name);

                    DbCommand command = DbConnectionFactory.GetDbCommand(conn, sqlCommand, trans);

                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    int affectedRows = command.ExecuteNonQuery();

                    return new Result(affectedRows);
                }
            }
             catch (TimeoutException)
            {
                return new Result(ErrorType.OperationTimeout, "Access", "Timeout");
            }
            catch (SqlException err)
            {
                return SQLExceptionHelper.SQLExceptionAnalyze(err);
            }
            catch (OutOfMemoryException)
            {
                return new Result(ErrorType.OperationFailure, "System", "A memory overflow occurred during a database query");
            }
            catch (DbException err)
            {
                return new Result(ErrorType.OperationFailure, "Data Engine", ExceptionHelper.GetRealException(err).Message);
            }
            catch (Exception err)
            {
                return new Result(ErrorType.SupportFailed, "Unknow", ExceptionHelper.GetRealException(err).Message);
            }
        }
        #endregion

        #region Data Count
        /// <summary>
        /// Calculates the number of objects that match specified conditions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="wc"></param>
        /// <returns></returns>
        public Result CountModels<T>(SQLWhereCondition wc)
        {
            return CountModels<T>(wc, null);
        }

        /// <summary>
        /// Calculates the number of objects that match specified conditions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="wc"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public Result CountModels<T>(SQLWhereCondition wc, DbTransaction trans)
        {
            Type t = typeof(T);

            try
            {
                DataSchemaAttribute attrData = AttributeHelper.GetCustomerAttribute<DataSchemaAttribute>(typeof(T));

                if (attrData == null)
                    return new Result(ErrorType.DataDesignError, "Design error", "The specified Model does not contain the [DataSchemaAttribute] feature");

                if (trans == null)
                {
                    using (DbConnection conn = DbConnectionFactory.Create(attrData.DataSource))
                    {
                        string sqlCommand = string.Format("SELECT COUNT(1) FROM [{0}] WHERE {1}", attrData.Name, wc.ToSqlString().Length > 0 ? wc.ToSqlString() : "1=1");

                        using (DbCommand command = DbConnectionFactory.GetDbCommand(conn, sqlCommand))
                        {
                            if (conn.State != ConnectionState.Open)
                                conn.Open();

                            command.Connection = conn;

                            int affectedRows = DataConvertHelper.ChangeType<object, int>(command.ExecuteScalar(), 0);

                            return new Result(affectedRows);
                        }
                    }
                }
                else
                {
                    DbConnection conn = trans.Connection;

                    string sqlCommand = string.Format("SELECT COUNT(1) FROM [{0}] WHERE {1}", attrData.Name, wc.ToSqlString().Length > 0 ? wc.ToSqlString() : "1=1");

                    DbCommand command = DbConnectionFactory.GetDbCommand(conn, sqlCommand, trans);

                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    int affectedRows = DataConvertHelper.ChangeType<object, int>(command.ExecuteScalar(), 0);

                    return new Result(affectedRows);
                }
            }
             catch (TimeoutException)
            {
                return new Result(ErrorType.OperationTimeout, "Access", "Timeout");
            }
            catch (SqlException err)
            {
                return SQLExceptionHelper.SQLExceptionAnalyze(err);
            }
            catch (OutOfMemoryException)
            {
                return new Result(ErrorType.OperationFailure, "System", "A memory overflow occurred during a database query");
            }
            catch (DbException err)
            {
                return new Result(ErrorType.OperationFailure, "Data Engine", ExceptionHelper.GetRealException(err).Message);
            }
            catch (Exception err)
            {
                return new Result(ErrorType.SupportFailed, "Unknow", ExceptionHelper.GetRealException(err).Message);
            }
        }
        #endregion

        #region Data Search(List)

        public Result LoadModels<T>(SQLWhereCondition wc, SQLOrderCondition oc)
        {
            return LoadModels<T>(wc, oc, null);
        }

        public Result LoadModels<T>(SQLWhereCondition wc, SQLOrderCondition oc, DbTransaction trans)
        {
            List<T> list = new List<T>();

            Result result = LoadModelsDataTable<T>(wc, oc, trans);

            if (!result.Succeed)
                return result;

            foreach (DataRow dr in result.GetData<DataTable>().Rows)
            {
                T target = default(T);

                DataToObject.GetDataFromDataRow(target, dr);

                list.Add(target);
            }

            return new Result(list);
        }

        /// <summary>
        /// Query an object (a DataTable returning a list of eligible objects)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="wc"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        public Result LoadModelsDataTable<T>(SQLWhereCondition wc, SQLOrderCondition oc)
        {
            return LoadModelsDataTable<T>(wc, oc, null);
        }

        /// <summary>
        /// Query an object (a DataTable returning a list of eligible objects)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="wc"></param>
        /// <param name="oc"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public Result LoadModelsDataTable<T>(SQLWhereCondition wc, SQLOrderCondition oc, DbTransaction trans)
        {
            DataTable dt = new DataTable();

            try
            {
                DataSchemaAttribute attrData = AttributeHelper.GetCustomerAttribute<DataSchemaAttribute>(typeof(T));

                if (trans == null)
                {
                    using (DbConnection conn = DbConnectionFactory.Create(attrData.DataSource))
                    {
                        string sqlCommand = string.Empty;

                        if (!string.IsNullOrEmpty(oc.ToSqlString()))
                            sqlCommand = string.Format("SELECT * FROM [{0}] WHERE {1} ORDER BY {2}", attrData.Name, wc.ToSqlString().Length > 0 ? wc.ToSqlString() : "1=1",
                                oc.ToSqlString());
                        else
                            sqlCommand = string.Format("SELECT * FROM [{0}] WHERE {1}", attrData.Name, wc.ToSqlString().Length > 0 ? wc.ToSqlString() : "1=1");

                        using (DbDataAdapter adapter = DbConnectionFactory.GetDbDataAdapter(conn, sqlCommand, trans))
                        {
                            if (conn.State != ConnectionState.Open)
                                conn.Open();
                            adapter.Fill(dt);
                        }
                    }
                }
                else
                {
                    DbConnection conn = trans.Connection;

                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    string sqlCommand = string.Empty;

                    if (!string.IsNullOrEmpty(oc.ToSqlString()))
                        sqlCommand = string.Format("SELECT * FROM [{0}] WHERE {1} ORDER BY {2}", attrData.Name, wc.ToSqlString().Length > 0 ? wc.ToSqlString() : "1=1",
                                oc.ToSqlString());
                    else
                        sqlCommand = string.Format("SELECT * FROM [{0}] WHERE {1}", attrData.Name, wc.ToSqlString().Length > 0 ? wc.ToSqlString() : "1=1");

                    DbDataAdapter adapter = DbConnectionFactory.GetDbDataAdapter(conn, sqlCommand, trans);

                    adapter.Fill(dt);
                }

                return new Result(dt);
            }
             catch (TimeoutException)
            {
                return new Result(ErrorType.OperationTimeout, "Access", "Timeout");
            }
            catch (SqlException err)
            {
                return SQLExceptionHelper.SQLExceptionAnalyze(err);
            }
            catch (OutOfMemoryException)
            {
                return new Result(ErrorType.OperationFailure, "System", "A memory overflow occurred during a database query");
            }
            catch (DbException err)
            {
                return new Result(ErrorType.OperationFailure, "Data Engine", ExceptionHelper.GetRealException(err).Message);
            }
            catch (Exception err)
            {
                return new Result(ErrorType.SupportFailed, "Unknow", ExceptionHelper.GetRealException(err).Message);
            }
        }

        /// <summary>
        /// Query objects (Returns a DataTable with a list of eligible objects, with paging support)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="wc"></param>
        /// <param name="oc"></param>
        /// <param name="pageSize"></param>
        /// <param name="absolutePage">Current page (must start with 1)</param>
        /// <param name="totalRecord">Total records</param>
        /// <param name="totalPage">Total Pages</param>
        /// <returns></returns>
        public Result LoadModelsDataTable<T>(SQLWhereCondition wc, SQLOrderCondition oc, int pageSize, int absolutePage, ref int totalRecord, ref int totalPage)
        {
            return LoadModelsDataTable<T>(wc, oc, null, pageSize, absolutePage, ref totalRecord, ref totalPage);
        }

        /// <summary>
        /// Query objects (Returns a DataTable with a list of eligible objects, with paging support)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="wc"></param>
        /// <param name="oc"></param>
        /// <param name="trans"></param>
        /// <param name="pageSize"></param>
        /// <param name="absolutePage">Current page (must start with 1)</param>
        /// <param name="totalRecord">Total records</param>
        /// <param name="totalPage">Total Pages</param>
        /// <returns></returns>
        public Result LoadModelsDataTable<T>(SQLWhereCondition wc, SQLOrderCondition oc, DbTransaction trans, int pageSize, int absolutePage, ref int totalRecord, ref int totalPage)
        {


            DataTable dt = new DataTable();

            try
            {
                DataSchemaAttribute attrData = AttributeHelper.GetCustomerAttribute<DataSchemaAttribute>(typeof(T));

                if (trans == null)
                {
                    using (DbConnection conn = DbConnectionFactory.Create(attrData.DataSource))
                    {
                        string sqlCommand = string.Empty;

                        sqlCommand = sqlCommand = string.Format("Select Count(1) FROM [{0}] Where {1}", attrData.Name, wc.ToSqlString().Length > 0 ? wc.ToSqlString() : "1=1");
                        if (conn.State != ConnectionState.Open)
                            conn.Open();
                        using (DbCommand command = DbConnectionFactory.GetDbCommand(conn, sqlCommand))
                        {
                            totalRecord = DataConvertHelper.ChangeType<object, int>(command.ExecuteScalar(), 0);
                        }

                        totalPage = (totalRecord % pageSize) > 0 ? (totalRecord / pageSize) + 1 : (totalRecord / pageSize);

                        if (totalPage > 0)
                        {
                            absolutePage = (absolutePage <= totalPage) ? absolutePage : totalPage;
                            absolutePage = (absolutePage > 0) ? absolutePage : 1;

                            if (!string.IsNullOrEmpty(oc.ToSqlString()))
                                sqlCommand = string.Format("SELECT * FROM (SELECT *,ROW_NUMBER() OVER (ORDER BY {0}) AS DNX_ROW_NUMBER FROM (SELECT * FROM {1} WHERE {4}) A) B WHERE [DNX_ROW_NUMBER] > {2} AND [DNX_ROW_NUMBER] <= {3}", oc.ToSqlString(), attrData.Name, pageSize * (absolutePage - 1), absolutePage * pageSize, wc.ToSqlString().Length > 0 ? wc.ToSqlString() : "1=1");
                            else
                                sqlCommand = string.Format("SELECT * FROM (SELECT *,ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS DNX_ROW_NUMBER FROM (SELECT * FROM {0} WHERE {3}) A) B WHERE [DNX_ROW_NUMBER] > {1} AND [DNX_ROW_NUMBER] <= {2}", attrData.Name, pageSize * (absolutePage - 1), absolutePage * pageSize, wc.ToSqlString().Length > 0 ? wc.ToSqlString() : "1=1");

                            using (DbDataAdapter adapter = DbConnectionFactory.GetDbDataAdapter(conn, sqlCommand, trans))
                            {
                                adapter.Fill(dt);
                            }

                            dt.Columns.Remove("DNX_ROW_NUMBER");
                        }
                    }
                }
                else
                {
                    DbConnection conn = trans.Connection;

                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    string sqlCommand = string.Empty;

                    sqlCommand = sqlCommand = string.Format("Select Count(1) FROM [{0}] Where {1} ", attrData.Name, wc.ToSqlString().Length > 0 ? wc.ToSqlString() : "1=1");
                    if (conn.State != ConnectionState.Open)
                        conn.Open();
                    DbCommand command = DbConnectionFactory.GetDbCommand(conn, sqlCommand, trans);

                    totalRecord = DataConvertHelper.ChangeType<object, int>(command.ExecuteScalar(), 0);

                    totalPage = (totalRecord % pageSize) > 0 ? (totalPage / pageSize) + 1 : (totalRecord / pageSize);
                    if (totalPage > 0)
                    {
                        absolutePage = (absolutePage <= totalPage) ? absolutePage : totalPage;
                        absolutePage = (absolutePage > 0) ? absolutePage : 1;

                        if (!string.IsNullOrEmpty(oc.ToSqlString()))
                        {
                            sqlCommand = string.Format("SELECT * FROM (SELECT *,ROW_NUMBER() OVER (ORDER BY {0}) AS DNX_ROW_NUMBER FROM (SELECT * FROM {1} WHERE {4}) A) B WHERE [DNX_ROW_NUMBER] > {2} AND [DNX_ROW_NUMBER] <= {3}", oc.ToSqlString(), attrData.Name, pageSize * (absolutePage - 1), absolutePage * pageSize, wc.ToSqlString().Length > 0 ? wc.ToSqlString() : "1=1");
                        }
                        else
                        {
                            sqlCommand = string.Format("SELECT * FROM (SELECT *,ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS DNX_ROW_NUMBER FROM (SELECT * FROM {1} WHERE {3}) A) B WHERE [DNX_ROW_NUMBER] > {1} AND [DNX_ROW_NUMBER] <= {2}", attrData.Name, pageSize * (absolutePage - 1), absolutePage * pageSize, wc.ToSqlString().Length > 0 ? wc.ToSqlString() : "1=1");
                        }

                        using (DbDataAdapter adapter = DbConnectionFactory.GetDbDataAdapter(conn, sqlCommand, trans))
                        {
                            adapter.Fill(dt);
                        }

                        dt.Columns.Remove("DNX_ROW_NUMBER");
                    }
                }

                return new Result(dt);
            }
             catch (TimeoutException)
            {
                return new Result(ErrorType.OperationTimeout, "Access", "Timeout");
            }
            catch (SqlException err)
            {
                return SQLExceptionHelper.SQLExceptionAnalyze(err);
            }
            catch (OutOfMemoryException)
            {
                return new Result(ErrorType.OperationFailure, "System", "A memory overflow occurred during a database query");
            }
            catch (DbException err)
            {
                return new Result(ErrorType.OperationFailure, "Data Engine", ExceptionHelper.GetRealException(err).Message);
            }
            catch (Exception err)
            {
                return new Result(ErrorType.SupportFailed, "Unknow", ExceptionHelper.GetRealException(err).Message);
            }
        }
        #endregion
    }
}
