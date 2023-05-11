
using System.Reflection;

namespace DNX.Data.Attributes
{
    /// <summary>
    /// 扩展类
    /// </summary>
    public struct PropertyAttribute
    {
        public DataPropertyAttribute Attribute;

        public object Value;

        public PropertyInfo PropertyInfo;
    }
}
