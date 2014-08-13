using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Augment;

namespace Yapper.Dialects
{
    /// <summary>
    /// Provides functionality specific to SQL Server 2012
    /// </summary>
    public sealed class SqlServer2012Dialect : SqlServerDialect, ISqlDialect
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
                return "select {0} from {1} {2} {3} {4} offset {5} rows fetch next {6} rows only"
                    .FormatArgs(selection, source, conditions, grouping, order, offset * fetch, fetch);
            }

            return base.SelectStatement(selection, source, conditions, order, grouping, limit, offset, fetch);
        }
    }
}
