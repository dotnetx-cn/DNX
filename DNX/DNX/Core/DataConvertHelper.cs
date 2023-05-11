using System;

namespace DNX.Core
{
    /// <summary>
    /// A helper class that casts the specified data
    /// </summary>
    public static class DataConvertHelper
    {
        /// <summary>
        /// Method to convert a string to and from an enumeration type, Guid, TimeSpan to an integer type.
        /// </summary>
        /// <typeparam name="TSource">Source data type</typeparam>
        /// <typeparam name="TResult">Target data type</typeparam>
        /// <param name="value">The value of source data</param>
        /// <returns></returns>
        public static TResult ChangeType<TSource, TResult>(TSource value)
        {
            return ChangeType<TSource, TResult>(value, default(TResult));
        }

        /// <summary>
        /// Method to convert a string to and from an enumeration type, Guid, TimeSpan to an integer type.
        /// </summary>
        /// <typeparam name="TSource">Source data type</typeparam>
        /// <typeparam name="TResult">Target data type</typeparam>
        /// <param name="value">The value of source data</param>
        /// <param name="defaultValue">The default value returned in case of a conversion error</param>
        /// <returns></returns>
        public static TResult ChangeType<TSource, TResult>(TSource value, TResult defaultValue)
        {
            try
            {
                return (TResult)ChangeType<TSource>(value, typeof(TResult));
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Method to convert a string to and from an enumeration type, Guid, TimeSpan to an integer type.
        /// </summary>
        /// <typeparam name="T">Source data type</typeparam>
        /// <param name="value">The value of source data</param>
        /// <param name="targetType">Target data type</param>
        /// <returns></returns>
        private static object ChangeType<T>(T value, Type targetType)
        {
            bool dealed = false;
            object result = null;

            Type sourceType = typeof(T);

            #region object
            if (sourceType == typeof(object) && sourceType != null)
                sourceType = value.GetType();
            #endregion

            #region Enum
            if (targetType.IsEnum)
            {
                if (sourceType == typeof(string) || sourceType == typeof(int))
                {
                    result = Enum.Parse(targetType, value.ToString());
                    dealed = true;
                }
            }
            #endregion

            #region Guid
            if (targetType == typeof(Guid))
                return new Guid((string)Convert.ChangeType(value, typeof(string)));
            #endregion

            #region TimeSpan
            if (targetType == typeof(TimeSpan))
            {
                if (sourceType == typeof(TimeSpan))
                    result = value;
                else
                    result = TimeSpan.FromSeconds((double)Convert.ChangeType(value, typeof(double)));

                dealed = true;
            }
            #endregion

            if (!dealed)
            {
                if (targetType != typeof(object) && targetType.IsAssignableFrom(sourceType))
                    result = value;
                else
                    result = Convert.ChangeType(value, targetType);
            }
            

            return result;
        }
    }
}
