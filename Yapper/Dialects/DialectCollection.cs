using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsureThat;
using Augment;
using System.Collections.Concurrent;

namespace Yapper.Dialects
{
    sealed class DialectCollection : ConcurrentDictionary<string, ISqlDialect>
    {
        #region Members

        static DialectCollection()
        {
            Default = new DialectCollection();
        }

        private Dictionary<string, ISqlDialect> _activeDialects = new Dictionary<string, ISqlDialect>();

        #endregion

        #region Constructors

        private DialectCollection()
            : base(StringComparer.InvariantCultureIgnoreCase)
        {
            this["System.Data.SqlClient"] = new SqlServer2012Dialect();
            this["System.Data.SqlServerCE.4.0"] = new SqlCeDialect();
            this["System.Data.SQLite"] = new SQLiteDialect();
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="providerInvariantName"></param>
        /// <returns></returns>
        public ISqlDialect GetDialectFor(string providerInvariantName)
        {
            Ensure.That(ContainsKey(providerInvariantName))
                .WithExtraMessageOf(() => "Missing Dialect for '{0}'".FormatArgs(providerInvariantName))
                .IsTrue();

            return this[providerInvariantName];
        }

        #endregion

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public static DialectCollection Default { get; private set; }

        #endregion
    }
}
