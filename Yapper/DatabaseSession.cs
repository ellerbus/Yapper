using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace Yapper.Core
{
    /// <summary>
    /// Represents an open connection to a specified database by wrapping
    /// an <see cref="IDbConnection"/> instance.
    /// </summary>
    public interface IDatabaseSession : IDisposable
    {
        /// <summary>
        /// Gets a handle to the underlying <see cref="IDbConnection"/>
        /// </summary>
        IDbConnection Connection { get; }

        /// <summary>
        /// Creates a unit of work (creates an underlying database transaction).
        /// </summary>
        /// <param name="isolationLevel">Specifies the <see cref="IsolationLevel"/> for this transaction.</param>
        /// <returns>A <see cref="IDatabaseTransaction"/></returns>
        IDatabaseTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    }

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

        #region Properties

        /// <summary>
        /// Gets a handle to the underlying <see cref="IDbConnection"/>
        /// </summary>
        public IDbConnection Connection { get; private set; }

        #endregion
    }
}