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
        public override string SelectStatement(string selection, string source, string conditions, string order, string grouping, int limit, int offset, int fetch)
        {
            if (offset > 0 || fetch > 0)
            {
                //  paging
                return "select {0} from {1} {2} {3} {4} offset {5} rows fetch next {6} rows only"
                    .FormatArgs(selection, source, conditions, grouping, order, offset, fetch);
            }

            return base.SelectStatement(selection, source, conditions, order, grouping, limit, offset, fetch);
        }
    }
}
