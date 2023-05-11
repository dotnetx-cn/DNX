using System;

namespace DNX.Data.SQL
{
    /// <summary>
    /// SQL collation rule
    /// </summary>
    [Flags]
    public enum SQLSortOrder
    {
        /// <summary>
        /// Ascending
        /// </summary>
        Ascending,
        /// <summary>
        /// Descending
        /// </summary>
        Descending,
        /// <summary>
        /// Unspecified
        /// </summary>
        Unspecified
    }

    /// <summary>
    /// Contains conditional base classes for SQL query optimization
    /// </summary>
    [Serializable]
    public class SQLConditionBase
    {
        /// <summary>
        /// Returns a standard SQL statement
        /// </summary>
        /// <returns></returns>
        public virtual string ToSqlString()
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Conditional class infrastructure
    /// </summary>
    [Serializable]
    public struct Condition
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name;
        /// <summary>
        /// Value
        /// </summary>
        public object Value;
        /// <summary>
        /// Type， Allow Set For string、int
        /// </summary>
        public Type Type;
        /// <summary>
        /// Operators such as =, &gt;
        /// </summary>
        public string Operation;
    }
}
