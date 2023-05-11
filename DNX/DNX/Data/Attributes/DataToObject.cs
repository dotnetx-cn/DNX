
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace DNX.Data.Attributes
{
    /// <summary>
    /// 用于数据行至数据对象的快速转换
    /// </summary>
    public static class DataToObject
    {
        /// <summary>
        /// 从DataRow中读出值转换为指定对象
        /// </summary>
        /// <param name="oData">指定对象</param>
        /// <param name="row">数据源</param>
        /// <param name="ignoreProperties">可以忽略的列</param>
        public static void GetDataFromDataRow(object oData, DataRow row, params string[] ignoreProperties)
        {
            List<PropertyAttribute> list = DataPropertyAttribute.GetObjectProperties(oData, BindingFlagType.Select, ignoreProperties);
            foreach (PropertyAttribute pa in list)
            {
                try
                {
                    string fld = pa.Attribute.Name;
                    if (row.Table.Columns.Contains(fld))
                    {
                        PropertyInfo pi = pa.PropertyInfo;

                        object oValue = row[fld];

                        if (pi.CanWrite)
                        {
                            object oChangeValue = ChangeType(oValue, pi.PropertyType);
                            if (oChangeValue != DBNull.Value)
                                pi.SetValue(oData, oChangeValue, null);
                        }
                    }
                }
                catch (Exception err)
                {
                    throw new Exception(string.Format("从DataReader的字段{0}转换到对象属性{1}出错，字段值为{2}。\n错误信息：{3}",
                        pa.Attribute.Name, pa.PropertyInfo.Name, pa.Value, err.Message));
                }

            }
        }

        private static object ChangeType(object oValue, Type t)
        {
            if (oValue is Guid)
                oValue = oValue.ToString();
            else if (t.IsEnum)
                oValue = System.Enum.Parse(t, oValue.ToString(), true);
            else if (t == typeof(bool))
                oValue = (oValue.ToString() == "1" || oValue.ToString().ToLower() == "true") ? true : false;
            else if (t == typeof(TimeSpan))
                oValue = ConvertToTimeSpan(oValue);
            else if (t == typeof(Guid))
                oValue = Guid.Parse(oValue.ToString());
            else if (oValue != DBNull.Value)
                oValue = Convert.ChangeType(oValue, t);

            return oValue;

        }

        private static TimeSpan ConvertToTimeSpan(object oValue)
        {
            TimeSpan ts = TimeSpan.Zero;
            if (oValue != null && oValue != DBNull.Value)
            {
                if (!(oValue is TimeSpan))
                {
                    double dbl = 0;
                    if (oValue is decimal)
                        dbl = decimal.ToDouble((decimal)oValue);
                    else if (oValue is double)
                        dbl = (double)oValue;
                    else if (oValue is float)
                        dbl = (double)oValue;
                    else if (oValue is int)
                        dbl = (double)oValue;
                    else if (oValue is long)
                        dbl = (double)oValue;
                    ts = TimeSpan.FromSeconds(dbl);
                }
                else
                {
                    ts = (TimeSpan)oValue;
                }

            }
            return ts;
        }
    }
}
