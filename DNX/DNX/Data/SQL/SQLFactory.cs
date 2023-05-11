using DNX.Data.Attributes;
using DNX.Runtime;
using System;
using System.Reflection;

namespace DNX.Data.SQL
{
    /// <summary>
    /// Core SQL statement generation
    /// </summary>
    internal class SQLFactory
    {
        /// <summary>
        /// Gets a T-SQL INSERT statement based on the SQL92 standard
        /// </summary>
        /// <param name="oData"></param>
        /// <param name="tableName"></param>
        /// <param name="ignoreProperties"></param>
        /// <returns></returns>
        public static string GetInsertSqlStr(object oData, string tableName, params string[] ignoreProperties)
        {
            return string.Format("INSERT INTO [{0}] {1}", tableName, GetSQLInsertCondition(oData, tableName, ignoreProperties).ToSqlString());
        }

        /// <summary>
        /// Gets a T-SQL UPDATE statement based on the SQL92 standard
        /// </summary>
        /// <param name="oData"></param>
        /// <param name="sTableName"></param>
        /// <param name="ignoreProperties"></param>
        /// <returns></returns>
        public static string GetUpdateSqlStr(object oData, string sTableName, params string[] ignoreProperties)
        {
            SQLUpdateCondition uc = GetSQLUpdateCondition(oData, sTableName);
            SQLWhereCondition wc = GetSQLWhereCondition(oData);

            return string.Format("UPDATE [{0}] SET {1} WHERE {2}", sTableName, uc.ToSqlString(), wc.ToSqlString());
        }

        /// <summary>
        /// Gets a T-SQL DELETE statement based on the SQL92 standard
        /// </summary>
        /// <param name="oData"></param>
        /// <param name="mTableName"></param>
        /// <returns></returns>
        public static string GetDeleteSqlStr(object oData, string mTableName)
        {
            SQLWhereCondition wc = GetSQLWhereCondition(oData);

            return string.Format("DELETE FROM [{0}] WHERE {1}", mTableName, wc.ToSqlString());
        }

        /// <summary>
        /// Gets a T-SQL INSERT statement based on the SQL92 standard
        /// </summary>
        /// <param name="oData"></param>
        /// <param name="tableName"></param>
        /// <param name="ignoreProperties"></param>
        /// <returns></returns>
        public static SQLInsertCondition GetSQLInsertCondition(object oData, string tableName, params string[] ignoreProperties)
        {
            SQLInsertCondition ic = new SQLInsertCondition();

            Array.ForEach<PropertyAttribute>(DataPropertyAttribute.GetObjectProperties(oData, BindingFlagType.Insert, ignoreProperties).ToArray(), target =>
            {
                if ((target.Value == null || target.Value is DBNull ||
                 (
                     target.Value is System.DateTime &&
                     ((DateTime)target.Value == DateTime.MinValue || (DateTime)target.Value == DateTime.MaxValue)
                     ) ||
                     (
                         target.Value is System.TimeSpan && (TimeSpan)target.Value == TimeSpan.Zero))
                     && target.Attribute.DefaultValue != string.Empty)
                {
                    if (target.Attribute.DefaultValue != null && target.Attribute.NativeType.ToLower() != "timestamp")
                        ic.AppendCondition(target.Attribute.Name, target.Attribute.DefaultValue);
                }
                else
                {
                    ic.AppendCondition(target.Attribute.Name, target.Value);
                }
            });

            return ic;
        }

        /// <summary>
        /// Gets a T-SQL UPDATE statement based on the SQL92 standard
        /// </summary>
        /// <param name="oData"></param>
        /// <param name="tableName"></param>
        /// <param name="ignoreProperties"></param>
        /// <returns></returns>
        public static SQLUpdateCondition GetSQLUpdateCondition(object oData, string tableName, params string[] ignoreProperties)
        {
            SQLUpdateCondition uc = new SQLUpdateCondition();

            Array.ForEach<PropertyAttribute>(DataPropertyAttribute.GetObjectProperties(oData,BindingFlagType.Update,ignoreProperties).ToArray(), target =>
            {
                if ((target.Value == null || target.Value is DBNull ||
                 (
                     target.Value is System.DateTime &&
                     ((DateTime)target.Value == DateTime.MinValue || (DateTime)target.Value == DateTime.MaxValue)
                     ) ||
                     (
                         target.Value is System.TimeSpan && (TimeSpan)target.Value == TimeSpan.Zero))
                     && target.Attribute.DefaultValue != string.Empty)
                {
                    if (target.Attribute.DefaultValue != null && target.Attribute.NativeType.ToLower() != "timestamp")
                        uc.AppendCondition(target.Attribute.Name, target.Attribute.DefaultValue);
                }
                else
                {
                    uc.AppendCondition(target.Attribute.Name, target.Value);
                }
            });

            return uc;
        }


        /// <summary>
        /// Gets the condition portion of the query statement
        /// </summary>
        /// <param name="oData"></param>
        /// <returns></returns>
        public static SQLWhereCondition GetSQLWhereCondition(object oData)
        {
            SQLWhereCondition wc = new SQLWhereCondition();

            Array.ForEach<PropertyInfo>(oData.GetType().GetProperties(),target=>{

                DataPropertyAttribute attrData = AttributeHelper.GetCustomerAttribute<DataPropertyAttribute>(target);

                if (attrData != null)
                    if (attrData.PrimaryKey)
                        wc.AppendCondition(target.Name, target.GetValue(oData, null));
            });

            return wc;
        }

        /// <summary>
        /// Gets the default values for the database fields
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static object GetDefaultValue(string dataType)
        {
            if (string.IsNullOrEmpty(dataType))
                return null;

            switch (dataType.ToLower())
            {
                case "binary":
                case "varbinary":
                case "image":
                case "sql_variant":
                    return null;
                case "ntext":
                case "text":
                case "char":
                case "varchar":
                case "nchar":
                case "nvarchar":
                case "xml":
                    return string.Empty;
                case "bit":
                    return false;
                case "tinyint":
                case "smallint":
                case "int":
                case "bigint":
                    return 0;
                case "smallmoney":
                case "money":
                case "float":
                case "real":
                case "numeric":
                case "decimal":
                    return 0.0000;
                case "datetime":
                    return DateTime.Now;
                case "smalldatetime":
                    return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                case "timestamp":   // timestamp that cannot be null equals binary(8), timestamp that can be null equals varbinary(8)
                    return null;
                case "uniqueidentifier":
                    return Guid.Empty;
                default:
                    return null;
            }
        }
    }
}
