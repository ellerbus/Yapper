/* License: http://www.apache.org/licenses/LICENSE-2.0 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yapper.Dialects
{
    /// <summary>
    /// Provides functionality common to all supported SQL Server versions
    /// </summary>
    public sealed class SQLiteDialect : SqlDialect
    {
        #region ISqlDialect

        public override string LeftDelimiter { get { return "\""; } }

        public override string RightDelimiter { get { return "\""; } }

        #endregion
    }
}