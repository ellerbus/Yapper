using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsureThat;
using Augment;
using System.Data;
using Yapper.Dialects;

namespace Yapper.Core
{
    /// <summary>
    /// A DbConnectionFactory allows you to create <see cref="IDbConnection"/> instances by configuring
    /// a connection in the connectionstrings section inside a app/web.config file.
    /// </summary>
    sealed class DbFactory
    {
        #region Constructors

        /// <summary>
        /// Creates a new DbConnectionFactory instance.
        /// </summary>
        /// <param name="connectionStringName">A key of one of the connectionstring settings inside the connectionstrings section of an app/web.config file.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="connectionStringName"/> is a null value.4</exception>
        /// <exception cref="ConfigurationErrorsException">Thrown if <paramref name="connectionStringName"/> is not found in any app/web.config file available to the application.</exception>
        public DbFactory(string connectionStringName)
        {
            Ensure.That(connectionStringName, "connectionStringName").IsNotNullOrWhiteSpace();

            ConnectionStringSettings css = ConfigurationManager.ConnectionStrings[connectionStringName];

            Ensure.That(css)
                .WithExtraMessageOf(() => "Failed to find connection string named '{0}' in app.config or web.config.".FormatArgs(connectionStringName))
                .IsNotNull();

            Name = connectionStringName;

            ProviderInvariantName = css.ProviderName;

            Provider = DbProviderFactories.GetFactory(css.ProviderName);

            ConnectionString = css.ConnectionString;

            Dialect = DialectCollection.Default.GetDialectFor(ProviderInvariantName);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new instance of <see cref="IDbConnection"/>.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException">Thrown if the connectionstring entry in the app/web.config file is missing information, contains errors or is missing entirely.</exception>
        /// <returns></returns>
        public IDbConnection Create()
        {
            IDbConnection connection = Provider.CreateConnection();

            Ensure.That(connection)
                .WithExtraMessageOf(() => "Failed to create a connection using the connection string named '{0}' in app.config or web.config.".FormatArgs(ProviderInvariantName))
                .IsNotNull();

            connection.ConnectionString = ConnectionString;

            return connection;
        }

        #endregion

        #region Properties

        public DbProviderFactory Provider { get; private set; }
        public string ProviderInvariantName { get; private set; }
        public string ConnectionString { get; private set; }
        public string Name { get; private set; }

        public ISqlDialect Dialect { get; private set; }

        #endregion
    }
}
