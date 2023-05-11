using System;
using System.Collections.Generic;
using System.Text;

namespace DNX.Data.SQL
{
    [Serializable]
    public class SQLWhereCondition : SQLConditionBase
    {
        private List<Condition> list;
        private List<string> listString;

        #region constructor
        public SQLWhereCondition()
        {
            list = new List<Condition>();
            listString = new List<string>();
        }
        #endregion

        /// <summary>
        /// Add special conditions such as 1=1
        /// </summary>
        /// <param name="sCon"></param>
        public void AppendCondition(string sCon)
        {
            if (!string.IsNullOrEmpty(sCon.Trim()))
                listString.Add(sCon);
        }

        /// <summary>
        /// Increment a condition based on the Format mechanism
        /// </summary>
        /// <param name="sCon"></param>
        /// <param name="args"></param>
        public void AppendConditionFormat(string sCon, params object[] args)
        {
            AppendCondition(string.Format(sCon, args));
        }

        /// <summary>
        /// Add a query condition in equality mode
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AppendCondition(string name, object value)
        {
            if (value != null)
                AppendCondition(name, value, value.GetType(), "=");
            else
                AppendCondition(name, "NULL", typeof(int), "=");
        }

        /// <summary>
        /// Add a query condition in equality mode
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public void AppendCondition(string name, object value, Type type)
        {
            if (value == null)
                AppendCondition(name, "NULL", typeof(int), "=");
            else
                AppendCondition(name, value, type, "=");
        }

        public void AppendCondition(string name, object value, Type type, string operation)
        {
            if (name != "DNX_ROW_NUMBER")
            {
                Condition con = new Condition();
                con.Name = name;
                con.Value = value;
                con.Type = type;
                con.Operation = operation;
                list.Add(con);
            }
            else
                throw new Exception("Do not use the reserved DNX_ROW_NUMBER field");
        }

        /// <summary>
        /// Generate SQL statement
        /// </summary>
        /// <returns></returns>
        public override string ToSqlString()
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                Array.ForEach<Condition>(list.ToArray(), target =>
                {
                    string sValue = string.Empty;

                    if (target.Value == null || target.Value == DBNull.Value)
                        sb.AppendFormat("[{0}] IS NULL", target.Name);
                    else
                    {
                        if (!target.Type.IsValueType || target.Type == typeof(DateTime))
                        {
                            // If the value type is value, single quotation marks are not required. Otherwise, single quotation marks are added
                            if (target.Type == typeof(DateTime))
                            {
                                if (DateTime.Parse(target.Value.ToString()) == DateTime.MinValue ||
                                    DateTime.Parse(target.Value.ToString()) == DateTime.MaxValue)
                                    sValue = "NULL";
                                else
                                {
                                    if (!target.Name.Contains("(") && !target.Name.Contains(" "))
                                        sValue = string.Format("'{0}'", target.Value.ToString().Replace("'", "''"));
                                    else
                                        sValue = string.Format("'{0}'", target.Value.ToString());
                                }
                            }
                            else
                            {
                                if (!target.Name.Contains("(") && !target.Name.Contains(" "))
                                    sValue = string.Format("'{0}'", target.Value.ToString().Replace("'", "''"));
                                else
                                    sValue = string.Format("'{0}'", target.Value.ToString());
                            }

                            if (target.Type == typeof(byte[]))
                            {
                                sValue = "0x";

                                Array.ForEach<byte>(target.Value as byte[], item =>
                                {
                                    sValue += item.ToString("X2");
                                });
                            }
                            else
                                sValue = string.Format("'{0}'", target.Value.ToString().Replace("'", "''"));
                        }
                        else
                        {
                            // For byte and byte[] binary data processing alone,
                            // the value should be in the format 0xFFFFFFF without single quotes when querying the database
                            if (target.Type == typeof(byte) || target.Type == typeof(byte[]))
                                sValue = string.Format("0x{0}", ((byte)target.Value).ToString("X2"));
                            else
                            {
                                if (target.Type == typeof(bool))
                                {
                                    if ((bool)target.Value == true)
                                        sValue = "1";
                                    else
                                        sValue = "0";
                                }
                                else if (target.Type == typeof(Guid))
                                    sValue = string.Format("'{0}'", target.Value.ToString());
                                else
                                {
                                    if (!target.Name.Contains("(") && !target.Name.Contains(" "))
                                        sValue = target.Value.ToString().Replace("'", "''");
                                    else
                                        sValue = target.Value.ToString();
                                }
                            }
                        }
                    }

                    string s = string.Format("{0} {1} {2}", (target.Name.Contains(".") || target.Name.Contains("(") || target.Name.Contains(" ")) ? target.Name.Trim() : "[" + target.Name.Trim() + "]", target.Operation, sValue);

                    sb.Append(s);
                    sb.Append("\0");
                });

                if (listString.Count > 0)
                {
                    Array.ForEach<string>(listString.ToArray(), target =>
                    {
                        sb.Append(target.Trim());
                        sb.Append("\0");
                    });
                }

                string sSQL = sb.ToString().TrimEnd('\0').Replace("\0", " And ");

                if (sSQL.Trim().Length > 0)
                    return sSQL;
                else
                    return "1=1";
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Removes all elements from an object
        /// </summary>
        public void Clear()
        {
            this.list.Clear();
            this.listString.Clear();
        }
    }
}
