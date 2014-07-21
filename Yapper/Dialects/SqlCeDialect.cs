using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Augment;

namespace Yapper.Dialects
{
    /// <summary>
    /// Provides functionality common to all supported SQL Server versions
    /// </summary>
    public sealed class SqlCeDialect : SqlDialect
    {
        public override string SelectStatement(string selection, string source, string conditions, string order, string grouping, int limit, int offset, int fetch)
        {
            if (offset > 0 || fetch > 0)
            {
                //  paging
                return "select {0} from {1} {2} {3} {4} offset {5} rows fetch next {6} rows only"
                    .FormatArgs(selection, source, conditions, grouping, order, offset, fetch);
            }

            if (limit > 0)
            {
                return "select top({0}) {1} from {2} {3} {4} {5}"
                    .FormatArgs(limit, selection, source, conditions, grouping, order);
            }

            return base.SelectStatement(selection, source, conditions, order, grouping, limit, offset, fetch);
        }

        public override string LeftDelimiter { get { return "\""; } }

        public override string RightDelimiter { get { return "\""; } }

        public override string SelectIdentity { get { return StatementSeparator + "select @@IDENTITY"; } }
    }
}