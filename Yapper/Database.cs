using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using Augment;
using EnsureThat;
using Yapper.Core;

namespace Yapper
{
    /// <summary>
    /// Handler to open an instance of <see cref="OpenConnection"/>
    /// </summary>
    public static class Database
    {
        /// <summary>
        /// Opens a connection to a database given a <see cref="ConfigurationManager.ConnectionStrings"/> name.
        /// </summary>
        /// <param name="name">If *null* uses first found connection string from <see cref="ConfigurationManager"/>.</param>
        /// <returns>An instance of <see cref="IDatabaseSession"/></returns>
        public static IDatabaseSession OpenSession(string name = null)
        {
            if (name.IsNullOrEmpty())
            {
                IList<ConnectionStringSettings> connections = ConfigurationManager.ConnectionStrings.Cast<ConnectionStringSettings>().ToList();

                Ensure.That(connections.Count)
                    .WithExtraMessageOf(() => "A Connection String name is required when more than one ConnectionString element found")
                    .Is(1);

                ConnectionStringSettings first = connections.FirstOrDefault();

                Ensure.That(first)
                    .WithExtraMessageOf(() => "No Connection Strings Found in app.config or web.config")
                    .IsNotNull();

                name = first.Name;
            }

            IDbConnection c = OpenConnection(name);

            return new DatabaseSession(c);
        }

        private static IDbConnection OpenConnection(string connectionStringName)
        {
            Ensure.That(connectionStringName, "connectionStringName").IsNotNullOrWhiteSpace();

            ConnectionStringSettings css = ConfigurationManager.ConnectionStrings[connectionStringName];

            Ensure.That(css)
                .WithExtraMessageOf(() => "Failed to find connection string named '{0}' in app.config or web.config.".FormatArgs(connectionStringName))
                .IsNotNull();

            DbProviderFactory factory = DbProviderFactories.GetFactory(css.ProviderName);

            Ensure.That(factory)
                .WithExtraMessageOf(() => "Failed to create a provider factory using the connection-string named '{0}' in app.config or web.config.".FormatArgs(css.ProviderName))
                .IsNotNull();

            IDbConnection connection = factory.CreateConnection();

            connection.ConnectionString = css.ConnectionString;

            connection.Open();

            return connection;
        }
    }
}
