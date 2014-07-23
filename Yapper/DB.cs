using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsureThat;
using Augment;
using Yapper.Core;
using Yapper.Dialects;

namespace Yapper
{
    public static class DB
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ISession Open(string name = null)
        {
            if (name.IsNullOrEmpty())
            {
                ConnectionStringSettings first = ConfigurationManager.ConnectionStrings.Cast<ConnectionStringSettings>().FirstOrDefault();

                Ensure.That(first)
                    .WithExtraMessageOf(() => "No Connection Strings Found in app.config for web.config".FormatArgs())
                    .IsNotNull();

                name = first.Name;
            }

            DbFactory factory = new DbFactory(name);

            return new DbSession(factory);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="providerInvariantName"></param>
        /// <param name="dialect"></param>
        public static void MapDialect(string providerInvariantName, ISqlDialect dialect)
        {
            DialectCollection.Default[providerInvariantName] = dialect;
        }
    }
}
