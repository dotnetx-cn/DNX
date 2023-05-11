using System;
using System.Collections.Generic;
using System.Text;

namespace DNX.Data.SQL
{
    /// <summary>
    /// Concatenate database collation fast method classes
    /// </summary>
    public class SQLOrderCondition : SQLConditionBase
    {
        private List<Condition> list;

        /// <summary>
        /// constructor
        /// </summary>
        public SQLOrderCondition()
        {
            list = new List<Condition>();
        }
        /// <summary>
        /// Add a collation rule
        /// </summary>
        /// <param name="name"></param>
        /// <param name="order"></param>
        public void AppendCondtion(string name, SQLSortOrder order)
        {
            Condition con = new Condition();
            con.Name = name;

            switch (order)
            {
                case SQLSortOrder.Ascending:
                    con.Operation = "ASC";
                    break;
                case SQLSortOrder.Descending:
                    con.Operation = "DESC";
                    break;
                case SQLSortOrder.Unspecified:
                    con.Operation = "";
                    break;
            }

            list.Add(con);
        }

        /// <summary>
        /// Removes all elements from the SQLOrderCondtion object
        /// </summary>
        public void Clear()
        {
            list.Clear();
        }

        /// <summary>
        /// Returns an executable T-SQL statement
        /// </summary>
        /// <returns></returns>
        public override string ToSqlString()
        {
            StringBuilder builder = new StringBuilder();

            Array.ForEach<Condition>(list.ToArray(), target =>
            {
                builder.AppendFormat("[{0}] {1},", target.Name, target.Operation);
            });

            return builder.ToString().TrimEnd(',');
        }

    }
}
