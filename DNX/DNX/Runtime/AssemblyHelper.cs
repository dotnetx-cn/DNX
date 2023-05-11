using System;
using System.Reflection;

namespace DNX.Runtime
{
    /// <summary>
    /// Through reflection, an operation definition that reads information about an assembly
    /// </summary>
    public static class AssemblyHelper
    {
 
        /// <summary>
        /// Get Property Value
        /// </summary>
        /// <param name="oData"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static object GetPropertyValue(object oData, PropertyInfo property)
        {
            object result = property.GetValue(oData, null);

            if (property.PropertyType == typeof(TimeSpan))
            {
                TimeSpan ts = (TimeSpan)result;

                if (ts != TimeSpan.Zero)
                    result = ts.TotalSeconds;
                else
                    result = null;
            }

            return result;
        }

        /// <summary>
        /// Set Property Value
        /// </summary>
        /// <param name="oData"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public static void SetPropertyValue(object oData, string propertyName, object value)
        {
            if (oData == null || string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("Run error: Object is empty or the specified property name is empty");

            PropertyInfo property = oData.GetType().GetProperty(propertyName);

            if (property == null)
                throw new ArgumentNullException(string.Format("Design error: An error occurred while retrieving the {0} attribute information for the object. The attribute did not exist.", propertyName));

            if (!property.CanWrite)
                throw new Exception("Design error: An error occurred while retrieving the {0} attribute information of an object. The object is not writable.");
            else if (property.PropertyType == typeof(DateTime?))
            {
                if (value != null && value != DBNull.Value)
                    property.SetValue(oData, value, null);
            }
            else if (property.PropertyType == typeof(DateTime))
            {
                if (value != null && value != DBNull.Value)
                    property.SetValue(oData, value, null);

            }
            else if (property.PropertyType == typeof(string))
            {
                if (value == null || value == DBNull.Value)
                    property.SetValue(oData, string.Empty, null);
                else
                    property.SetValue(oData, value, null);
            }
            else
                property.SetValue(oData, value, null);
        }
    }
}
