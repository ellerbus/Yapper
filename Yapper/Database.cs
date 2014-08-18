using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using Augment;
using EnsureThat;
using Yapper.Core;
using Yapper.Dialects;

namespace Yapper
{
    /// <summary>
    /// Handler to open an instance of <see cref="IDbConnection"/> or maps a <see cref="ISqlDialect"/>
    /// </summary>
    public static class Database
    {
        /// <summary>
        /// Opens a connection to a database given a <see cref="ConfigurationManager.ConnectionStrings"/> name.
        /// </summary>
        /// <param name="name">If *null* uses first found connection string from <see cref="ConfigurationManager"/>.</param>
        /// <returns>An instance of <see cref="ISession"/></returns>
        public static ISession OpenSession(string name = null)
        {
            if (name.IsNullOrEmpty())
            {
                IList<ConnectionStringSettings> connections = ConfigurationManager.ConnectionStrings.Cast<ConnectionStringSettings>().ToList();

                Ensure.That(connections.Count == 1)
                    .WithExtraMessageOf(() => "A ConnectionStringSettings name is required when more than one ConnectionStringSettings found".FormatArgs())
                    .IsTrue();

                ConnectionStringSettings first = connections.FirstOrDefault();

                Ensure.That(first)
                    .WithExtraMessageOf(() => "No Connection Strings Found in app.config for web.config".FormatArgs())
                    .IsNotNull();

                name = first.Name;
            }

            DbFactory factory = new DbFactory(name);

            return new DbSession(factory);
        }

        /// <summary>
        /// Maps an instance of <see cref="ISqlDialect"/> to a given providerInvariantName.
        /// </summary>
        /// <param name="providerInvariantName">Invariant name of a provider.</param>
        /// <param name="dialect">An instance of <see cref="ISqlDialect"/></param>
        public static void MapDialect(string providerInvariantName, ISqlDialect dialect)
        {
            DialectCollection.Default[providerInvariantName] = dialect;
        }
    }
}
