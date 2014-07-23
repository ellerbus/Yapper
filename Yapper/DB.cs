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
    public static class DB
    {
        /// <summary>
        /// Opens a connection to a database given a <see cref="ConfigurationManager.ConnectionStrings"/> name.
        /// </summary>
        /// <param name="name">If *null* uses first found connection string from <see cref="ConfigurationManager"/>.</param>
        /// <returns>An instance of <see cref="ISession"/></returns>
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
