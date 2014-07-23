using Augment;

namespace Yapper.Dialects
{
    /// <summary>
    /// Provides functionality common to all supported SQL Server versions
    /// </summary>
    public sealed class SQLiteDialect : SqlDialect
    {
        /// <summary>
        /// Provides the items used to construct a Select statement given a <see cref="ISqlDialect"/>
        /// </summary>
        /// <param name="selection">Select Clause</param>
        /// <param name="source">From Clause</param>
        /// <param name="conditions">Where Clause</param>
        /// <param name="order">Order By Clause</param>
        /// <param name="grouping">Group By Clause</param>
        /// <param name="limit">Top Clause</param>
        /// <param name="offset">Page offset index (zero based)</param>
        /// <param name="fetch">Page number of records to return</param>
        /// <returns>A SQL Select statement specific to a given <see cref="ISqlDialect"/></returns>
        public override string SelectStatement(string selection, string source, string conditions, string order, string grouping, int limit, int offset, int fetch)
        {
            if (offset > 0 || fetch > 0)
            {
                //  paging
                return "select {0} from {1} {2} {3} {4} limit {5}, {6}"
                    .FormatArgs(selection, source, conditions, grouping, order, offset, fetch);
            }

            if (limit > 0)
            {
                return "select {0} from {1} {2} {3} {4} limit {5}"
                    .FormatArgs(selection, source, conditions, grouping, order, limit);
            }

            return base.SelectStatement(selection, source, conditions, order, grouping, limit, offset, fetch);
        }

        /// <summary>
        /// Gets the delimiter used on the left hand side to escape an SQL identifier.
        /// </summary>
        public override string LeftDelimiter { get { return "\""; } }

        /// <summary>
        /// Gets the delimiter used on the right hand side to escape an SQL identifier.
        /// </summary>
        public override string RightDelimiter { get { return "\""; } }

        /// <summary>
        /// Gets the identity value on insert
        /// </summary>
        public override string SelectIdentity { get { return StatementSeparator + "select last_insert_rowid()"; } }
    }
}