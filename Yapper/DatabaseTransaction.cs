using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsureThat;

namespace Yapper.Core
{

    /// <summary>
    /// Represents a unit of work to be performed against the database by wrapping
    /// an <see cref="IDbTransaction"/> instance.
    /// </summary>
    public interface IDatabaseTransaction : IDisposable
    {
        /// <summary>
        /// Commits the Unit of Work
        /// </summary>
        void Commit();

        /// <summary>
        /// Rolls back the Unit of Work
        /// </summary>
        void Rollback();

        /// <summary>
        /// Gets a handle to the underlying <see cref="IDbTransaction"/>
        /// </summary>
        IDbTransaction Transaction { get; }
    }

    public sealed class DatabaseTransaction : IDatabaseTransaction
    {
        #region Members

        private readonly Action<DatabaseTransaction> OnCommit;
        private readonly Action<DatabaseTransaction> OnRollback;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="DatabaseTransaction"/> instance.
        /// </summary>
        /// <param name="transaction">The underlying <see cref="IDbTransaction"/> object used to either commit or roll back the statements that are being performed inside this unit of work.</param>
        /// <param name="onCommitOrRollback">An <see cref="Action{DatabaseUnitOfWork}"/> that will be executed when the unit of work is being committed or rolled back.</param>
        public DatabaseTransaction(IDbTransaction transaction, Action<DatabaseTransaction> onCommitOrRollback)
            : this(transaction, onCommitOrRollback, onCommitOrRollback)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DatabaseTransaction"/> instance.
        /// </summary>
        /// <param name="transaction">The underlying <see cref="IDbTransaction"/> object used to either commit or roll back the statements that are being performed inside this unit of work.</param>
        /// <param name="onCommit">An <see cref="Action{DatabaseUnitOfWork}"/> that will be executed when the unit of work is being committed.</param>
        /// <param name="onRollback">An <see cref="Action{DatabaseUnitOfWork}"/> that will be executed when the unit of work is being rolled back.</param>
        public DatabaseTransaction(IDbTransaction transaction, Action<DatabaseTransaction> onCommit, Action<DatabaseTransaction> onRollback)
        {
            Transaction = transaction;

            OnCommit = onCommit;
            OnRollback = onRollback;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Rollback will and rollback all statements that have been executed against the database inside this unit of work.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this unit of work has already been committed or rolled back.</exception>
        public void Rollback()
        {
            Ensure.That(Transaction, "Transaction").IsNotNull();

            try
            {
                Transaction.Rollback();

                OnRollback(this);
            }
            finally
            {
                Transaction.Dispose();

                Transaction = null;
            }
        }

        /// <summary>
        /// SaveChanges will try and commit all statements that have been executed against the database inside this unit of work.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this unit of work has already been committed or rolled back.</exception>
        public void Commit()
        {
            Ensure.That(Transaction, "Transaction").IsNotNull();

            try
            {
                Transaction.Commit();

                OnCommit(this);
            }
            finally
            {
                Transaction.Dispose();

                Transaction = null;
            }
        }

        /// <summary>
        /// Implements <see cref="IDisposable.Dispose"/>, and rolls back the statements executed inside this unit of work.
        /// This makes it easier to use a unit of work instance inside a <c>using</c> statement (<c>Using</c> in VB.Net).
        /// </summary>
        public void Dispose()
        {
            if (Transaction == null) return;

            try
            {
                Transaction.Rollback();

                OnRollback(this);
            }
            finally
            {
                Transaction.Dispose();

                Transaction = null;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a handle to the underlying <see cref="IDbTransaction"/>
        /// </summary>
        public IDbTransaction Transaction { get; private set; }

        #endregion
    }
}