using System;
using System.Collections.Generic;
using System.Text;

namespace DNX.Data.SQL
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class SQLInsertCondition : SQLConditionBase
    {
        private List<Condition> list;
        private List<string> listString;

        /// <summary>
        /// constructor
        /// </summary>
        public SQLInsertCondition()
        {
            list = new List<Condition>();
            listString = new List<string>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sCon"></param>
        public void AppendCondition(string sCon)
        {
            if (!string.IsNullOrEmpty(sCon.Trim()))
                listString.Add(sCon);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sCon"></param>
        /// <param name="args"></param>
        public void AppendConditionFormat(string sCon, params object[] args)
        {
            AppendCondition(string.Format(sCon, args));
        }

        /// <summary>
        /// 
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
        /// 
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
                throw new Exception("DNX_ROW_NUMBER");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToSqlString()
        {
            try
            {
                StringBuilder sbField = new StringBuilder();
                StringBuilder sbValue = new StringBuilder();

                Array.ForEach<Condition>(list.ToArray(), target =>
                {
                    sbField.AppendFormat("[{0}],", target.Name);

                    string sValue = string.Empty;

                    if (!target.Type.IsValueType || target.Type == typeof(DateTime))
                    {
                       
                        if (target.Type == typeof(DateTime))
                        {
                            if (DateTime.Parse(target.Value.ToString()) == DateTime.MinValue ||
                                DateTime.Parse(target.Value.ToString()) == DateTime.MaxValue)
                            {
                                sValue = "null";
                            }
                            else
                            {
                                sValue = string.Format("'{0}'", target.Value.ToString().Replace("'", "''"));
                            }
                        }
                        else if (target.Type == typeof(byte[]))
                        {
                            sValue = "0x";

                            Array.ForEach<byte>(target.Value as byte[], item =>
                            {
                                sValue += item.ToString("X2");
                            });
                        }
                        else
                        {
                            sValue = string.Format("N'{0}'", target.Value.ToString().Replace("'", "''"));
                        }
                    }
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
                        {
                            sValue = string.Format("'{0}'", target.Value.ToString());
                        }
                        else
                        {
                            sValue = target.Value.ToString().Replace("'", "''");
                        }
                    }
                    sbValue.AppendFormat("{0},", sValue);
                });

                if (sbField.ToString().Trim().Length > 0)
                {
                    return string.Format("({0}) Values ({1})", sbField.ToString().TrimEnd(','), sbValue.ToString().TrimEnd(','));
                }
                else
                {
                    return string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            this.list.Clear();
            this.listString.Clear();
        }
    }
}