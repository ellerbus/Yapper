using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Augment;

namespace Yapper.Dialects
{
    /// <summary>
    /// Provides functionality specific to SQL Server 2008
    /// </summary>
    public sealed class SqlServer2008Dialect : SqlServerDialect, ISqlDialect
    {
        public override string SelectStatement(string selection, string source, string conditions, string order, string grouping, int limit, int offset, int fetch)
        {
            if (offset > 0 || fetch > 0)
            {
                string innerSql = "select {0}, row_number() over ({1}) as rownbr from {2} {3}"
                    .FormatArgs(selection, order, source, conditions);

                return "select * from ({0}) x where x.rownbr between {1} and {2}"
                    .FormatArgs(innerSql, offset + 1, offset + fetch);
            }

            return base.SelectStatement(selection, source, conditions, order, grouping, limit, offset, fetch);
        }
    }
}
