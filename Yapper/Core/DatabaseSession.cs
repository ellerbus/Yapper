using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using System.Reflection.Emit;
using System.Reflection;

namespace Yapper.Core
{
    /// <summary>
    /// A database context class for Dapper (https://github.com/SamSaffron/dapper-dot-net),
    /// based on http://blog.gauffin.org/2013/01/ado-net-the-right-way/#.UpWLPMSkrd2
    /// and
    /// https://github.com/wcabus/DapperContext
    /// </summary>
    sealed class DatabaseSession : IDatabaseSession
    {
        #region Members

        private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
        private readonly LinkedList<IDatabaseTransaction> _workItems = new LinkedList<IDatabaseTransaction>();

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        public DatabaseSession(IDbConnection connection)
        {
            Connection = connection;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new <see cref="IDatabaseTransaction"/>.
        /// </summary>
        /// <param name="isolationLevel">The <see cref="IsolationLevel"/> used for the transaction inside this unit of work. Default value: <see cref="IsolationLevel.ReadCommitted"/></param>
        /// <returns></returns>
        public IDatabaseTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            //To create a transaction, our connection needs to be open.
            //If we need to open the connection ourselves, we're also in charge of
            //closing it when this transaction commits or rolls back.
            //This will be done by RemoveTransactionAndCloseConnection in that case.
            bool wasClosed = Connection.State == ConnectionState.Closed;

            if (wasClosed)
            {
                Connection.Open();
            }

            try
            {
                IDatabaseTransaction unit;

                IDbTransaction transaction = Connection.BeginTransaction(isolationLevel);

                if (wasClosed)
                {
                    unit = new DatabaseTransaction(transaction, RemoveTransactionAndCloseConnection);
                }
                else
                {
                    unit = new DatabaseTransaction(transaction, RemoveTransaction);
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
                    Connection.Close();
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

        private void RemoveTransaction(IDatabaseTransaction workItem)
        {
            _rwLock.EnterWriteLock();

            _workItems.Remove(workItem);

            _rwLock.ExitWriteLock();
        }

        private void RemoveTransactionAndCloseConnection(IDatabaseTransaction workItem)
        {
            _rwLock.EnterWriteLock();

            _workItems.Remove(workItem);

            _rwLock.ExitWriteLock();

            Connection.Close();
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
                    LinkedListNode<IDatabaseTransaction> workItem = _workItems.First;

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

            if (Connection != null)
            {
                Connection.Dispose();

                Connection = null;
            }
        }

        #endregion

        #region Dapper

        public IEnumerable<T> Query<T>(string sql, object param = null)
        {
            //Dapper will open and close the connection for us if necessary.

            return Connection.Query<T>(sql: sql, param: param, transaction: GetCurrentTransaction());
        }

        public SqlMapper.GridReader QueryMultiple(string sql, object param = null)
        {
            //Dapper will open and close the connection for us if necessary.

            return Connection.QueryMultiple(sql: sql, param: param, transaction: GetCurrentTransaction());
        }

        public int Execute(string sql, object param = null)
        {
            //Dapper will open and close the connection for us if necessary.

            return Connection.Execute(sql: sql, param: param, transaction: GetCurrentTransaction());
        }

        /// <summary>
        /// Inserts a new item w/o all properties assigned
        /// </summary>
        public int Insert<T>(object item)
        {
            string sql = QueryBuilder.Insert<T>(item);

            return Execute(sql, item);
        }

        /// <summary>
        /// Inserts a new item w/ all properties assigned
        /// </summary>
        public int Insert<T>(T item)
        {
            string sql = QueryBuilder.Insert<T>(item);

            return Execute(sql, item);
        }

        /// <summary>
        /// Updates a new item w/o all properties assigned
        /// </summary>
        public int Update<T>(object set, object where)
        {
            string sql = QueryBuilder.Update<T>(set, where);

            DynamicParameters parms = new DynamicParameters(set);

            parms.AddDynamicParams(where);

            return Execute(sql, parms);
        }

        /// <summary>
        /// Updates a new item w/ all properties assigned
        /// </summary>
        public int Update<T>(T item)
        {
            string sql = QueryBuilder.Update<T>(item);

            return Execute(sql, item);
        }

        /// <summary>
        /// Inserts a new item w/o all properties assigned
        /// </summary>
        public int Delete<T>(object where)
        {
            string sql = QueryBuilder.Delete<T>(where);

            return Execute(sql, where);
        }

        /// <summary>
        /// Inserts a new item w/ all properties assigned
        /// </summary>
        public int Delete<T>(T item)
        {
            string sql = QueryBuilder.Delete<T>(item);

            return Execute(sql, item);
        }

        /// <summary>
        /// Selects an based on parameters
        /// </summary>
        public IEnumerable<T> Select<T>(object where)
        {
            string sql = QueryBuilder.Select<T>(where);

            return Query<T>(sql, where);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a handle to the underlying <see cref="IDbConnection"/>
        /// </summary>
        public IDbConnection Connection { get; private set; }

        #endregion
    }
}