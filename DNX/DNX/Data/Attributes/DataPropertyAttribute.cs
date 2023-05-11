using DNX.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DNX.Data.Attributes
{
    #region 自定义特性执行枚举
    [Flags]
    public enum BindingFlagType
    {
        /// <summary>
        /// 任何情况下都不出现
        /// </summary>
        None = 0,
        /// <summary>
        /// 表示属性会出现在Insert中
        /// </summary>
        Insert = 1,
        /// <summary>
        /// 表示属性会出现在Update中
        /// </summary>
        Update = 2,
        /// <summary>
        /// 表示属性会出现在Where语句部分
        /// </summary>
        Where = 4,
        /// <summary>
        /// 表示属性会出现在查询的返回字段中
        /// </summary>
        Select = 8,
        /// <summary>
        /// 当使用枚举时，如果设置该属性，则取其整型值，否则取字符串
        /// </summary>
        UseEnumValue = 16,
        /// <summary>
        /// 在所有情况下都会出现
        /// </summary>
        All = Insert | Update | Where | Select | UseEnumValue,
    }
    #endregion

    /// <summary>
    /// 描述数据对象的属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class DataPropertyAttribute : Attribute
    {
        public DataPropertyAttribute()
        {
        }

        public DataPropertyAttribute(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// 数据库中对象名称
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// 数据库中对象的长度
        /// </summary>
        public int Length
        {
            get;
            set;
        }

        /// <summary>
        /// 数据库对象中的数据类型描述
        /// </summary>
        public string NativeType
        {
            get;
            set;
        }

        /// <summary>
        /// 数据库中对象的说明
        /// </summary>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// 如果该值为空，对应的数据库缺省表达式
        /// （注意：如果该值为字符串，不要忘记在前后加上单引号）
        /// </summary>
        public string DefaultValue
        {
            get;
            set;
        }

        /// <summary>
        /// 是否为标识，默认为false
        /// </summary>
        public bool Identity
        {
            get;
            set;
        }

        /// <summary>
        /// 是否为主键
        /// </summary>
        public bool PrimaryKey
        {
            get;
            set;
        }

        // <summary>
        /// 是否允许为空，默认值为true
        /// </summary>
        public bool AllowNull
        {
            get;
            set;
        }

        /// <summary>
        /// 数据精度
        /// </summary>
        public int Scale
        {
            get;
            set;
        }

        /// <summary>
        /// 对象属性的绑定标志
        /// 标识出该属性会出现在Insert、Update或Where中，也可以指定为None，不出现在SQL中
        /// </summary>
        public BindingFlagType BindingFlag
        {
            get;
            set;
        }

        /// <summary>
        /// 得到某一个对象的某个属性的DataProperty的描述
        /// </summary>
        /// <param name="oData"></param>
        /// <param name="strPropertyName"></param>
        /// <returns></returns>
        public static DataPropertyAttribute GetDataPropertyFromObjProperty(object oData, string strPropertyName)
        {
            PropertyInfo[] piArray = oData.GetType().GetProperties(BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
            DataPropertyAttribute dpa = null;
            for (int i = 0; i < piArray.Length; i++)
            {
                PropertyInfo pi = piArray[i];
                if (pi.Name.Equals(strPropertyName, StringComparison.OrdinalIgnoreCase))
                {
                    dpa = (DataPropertyAttribute)Attribute.GetCustomAttribute(pi, typeof(DataPropertyAttribute));
                    break;
                }
            }
            return dpa;
        }

        public static List<PropertyAttribute> GetObjectProperties(object oData, BindingFlagType bindingFlag, params string[] ignoreProperties)
        {
            PropertyInfo[] piArray = oData.GetType().GetProperties(BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
            if (piArray.Length <= 0)
                throw new Exception("This object has no Public property and cannot construct data field information");

            List<PropertyAttribute> list = new List<PropertyAttribute>();
            for (int i = 0; i < piArray.Length; i++)
            {
                PropertyAttribute pa;
                PropertyInfo pi = piArray[i];

                if (pi.CanRead && Array.Exists<string>(ignoreProperties, target => (string.Compare(pi.Name, target, true) == 0)) == false)
                {
                    DataPropertyAttribute dpa = (DataPropertyAttribute)Attribute.GetCustomAttribute(pi, typeof(DataPropertyAttribute));
                    if (dpa == null)
                        dpa = new DataPropertyAttribute(pi.Name);

                    if ((dpa.BindingFlag & bindingFlag) != 0)
                    {
                        if (dpa.Name == string.Empty)
                            dpa.Name = pi.Name;

                        pa.Attribute = dpa;
                        pa.Value = AssemblyHelper.GetPropertyValue(oData, pi);
                        pa.PropertyInfo = pi;
                        if (pa.Value != null)
                        {
                            if (pa.Value is Enum)
                                if ((dpa.BindingFlag & BindingFlagType.UseEnumValue) != 0)
                                    pa.Value = (int)pa.Value;
                        }
                        list.Add(pa);
                    }
                }
            }
            return list;
        }
    }
}
