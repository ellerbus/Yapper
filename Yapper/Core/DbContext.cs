using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

namespace Yapper.Core
{
    /// <summary>
    /// A database context class for Dapper (https://github.com/SamSaffron/dapper-dot-net), based on http://blog.gauffin.org/2013/01/ado-net-the-right-way/#.UpWLPMSkrd2
    /// </summary>
    /// <remarks>
    /// https://github.com/wcabus/DapperContext
    /// </remarks>
    sealed class DbContext : IContext
    {
        #region Members

        private IDbConnection _connection;
        private readonly DbFactory _connectionFactory;
        private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
        private readonly LinkedList<IUnitOfWork> _workItems = new LinkedList<IUnitOfWork>();

        #endregion

        #region Constructors

        /// <summary>
        /// <para>Default constructor.</para>
        /// <para>Uses the <paramref name="connectionStringName"/> to instantiate a <see cref="DbFactory"/>. This factory will be used to create connections to a database.</para>
        /// </summary>
        public DbContext(DbFactory factory)
        {
            _connectionFactory = factory;

            Sql.Dialect = factory.Dialect;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Ensures that a connection is ready for querying or creating transactions
        /// </summary>
        /// <remarks></remarks>
        private void CreateOrReuseConnection()
        {
            if (_connection != null)
            {
                return;
            }

            _connection = _connectionFactory.Create();
        }

        /// <summary>
        /// Creates a new <see cref="IUnitOfWork"/>.
        /// </summary>
        /// <param name="isolationLevel">The <see cref="IsolationLevel"/> used for the transaction inside this unit of work. Default value: <see cref="IsolationLevel.ReadCommitted"/></param>
        /// <returns></returns>
        public IUnitOfWork CreateUnitOfWork(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            CreateOrReuseConnection();

            //To create a transaction, our connection needs to be open.
            //If we need to open the connection ourselves, we're also in charge of
            //closing it when this transaction commits or rolls back.
            //This will be done by RemoveTransactionAndCloseConnection in that case.
            bool wasClosed = _connection.State == ConnectionState.Closed;

            if (wasClosed)
            {
                _connection.Open();
            }

            try
            {
                IUnitOfWork unit;

                IDbTransaction transaction = _connection.BeginTransaction(isolationLevel);

                if (wasClosed)
                {
                    unit = new DbUnitOfWork(transaction, RemoveTransactionAndCloseConnection);
                }
                else
                {
                    unit = new DbUnitOfWork(transaction, RemoveTransaction);
                }

                _rwLock.EnterWriteLock();

                _workItems.AddLast(unit);

                _rwLock.ExitWriteLock();

                return unit;
            }
            catch
            {
                //Close the connection if we're managing it, and if an
                //exception is thrown when creating the transaction.
                if (wasClosed)
                {
                    _connection.Close();
                }

                //Rethrow the original transaction
                throw;
            }
        }

        private IDbTransaction GetCurrentTransaction()
        {
            IDbTransaction currentTransaction = null;

            _rwLock.EnterReadLock();

            if (_workItems.Any())
            {
                currentTransaction = _workItems.First.Value.Transaction;
            }

            _rwLock.ExitReadLock();

            return currentTransaction;
        }

        private void RemoveTransaction(IUnitOfWork workItem)
        {
            _rwLock.EnterWriteLock();

            _workItems.Remove(workItem);

            _rwLock.ExitWriteLock();
        }

        private void RemoveTransactionAndCloseConnection(IUnitOfWork workItem)
        {
            _rwLock.EnterWriteLock();

            _workItems.Remove(workItem);

            _rwLock.ExitWriteLock();

            _connection.Close();
        }

        /// <summary>
        /// Implements <see cref="IDisposable.Dispose"/>.
        /// </summary>
        public void Dispose()
        {
            //Use an upgradeable lock, because when we dispose a unit of work,
            //one of the removal methods will be called (which enters a write lock)
            _rwLock.EnterUpgradeableReadLock();

            try
            {
                while (_workItems.Any())
                {
                    LinkedListNode<IUnitOfWork> workItem = _workItems.First;

                    //commit, will remove the item from the LinkedList because
                    //it calls either RemoveTransaction or
                    //RemoveTransactionAndCloseConnection

                    workItem.Value.Dispose();
                }
            }
            finally
            {
                _rwLock.ExitUpgradeableReadLock();
            }

            if (_connection != null)
            {
                _connection.Dispose();

                _connection = null;
            }
        }

        #endregion

        #region Dapper

        public IEnumerable<T> Query<T>(ISqlQuery query)
        {
            CreateOrReuseConnection();

            //Dapper will open and close the connection for us if necessary.

            return SqlMapper.Query<T>(_connection, query.Query, query.Parameters, GetCurrentTransaction());
        }

        public int Execute(ISqlQuery query)
        {
            CreateOrReuseConnection();

            //Dapper will open and close the connection for us if necessary.

            return SqlMapper.Execute(_connection, query.Query, query.Parameters, GetCurrentTransaction());
        }

        #endregion
    }
}
