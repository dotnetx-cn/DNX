
using System;

namespace DNX.Data.Attributes
{
    /// <summary>
    /// 用于描述数据库信息的自定义特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DataSchemaAttribute : Attribute
    {
        #region constructor
        /// <summary>
        /// constructor
        /// </summary>
        public DataSchemaAttribute()
        {
        }
        #endregion

        #region 属性
        /// <summary>
        /// 数据源（需要对应Web.config中的connectionString名称设置）
        /// </summary>
        public string DataSource
        {
            get;
            set;
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get;
            set;
        }
        /// <summary>
        /// 架构
        /// </summary>
        public string Schema
        {
            get;
            set;
        }

        /// <summary>
        /// 说明
        /// </summary>
        public string Description
        {
            get;
            set;
        }
        /// <summary>
        /// 是否为数据视图
        /// </summary>
        public bool IsDataView
        {
            get;
            set;
        }
        /// <summary>
        /// 返回当前实例的表现形式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}.[1]", this.Schema, this.Name);
        }

        #endregion
    }
}
