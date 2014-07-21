using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsureThat;

namespace Yapper.Core
{
    sealed class DbUnitOfWork : IUnitOfWork
    {
        #region Members

        private readonly Action<DbUnitOfWork> OnCommit;
        private readonly Action<DbUnitOfWork> OnRollback;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="DbUnitOfWork"/> instance.
        /// </summary>
        /// <param name="transaction">The underlying <see cref="IDbTransaction"/> object used to either commit or roll back the statements that are being performed inside this unit of work.</param>
        /// <param name="onCommitOrRollback">An <see cref="Action{UnitOfWork}"/> that will be executed when the unit of work is being committed or rolled back.</param>
        public DbUnitOfWork(IDbTransaction transaction, Action<DbUnitOfWork> onCommitOrRollback)
            : this(transaction, onCommitOrRollback, onCommitOrRollback)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DbUnitOfWork"/> instance.
        /// </summary>
        /// <param name="transaction">The underlying <see cref="IDbTransaction"/> object used to either commit or roll back the statements that are being performed inside this unit of work.</param>
        /// <param name="onCommit">An <see cref="Action{UnitOfWork}"/> that will be executed when the unit of work is being committed.</param>
        /// <param name="onRollback">An <see cref="Action{UnitOfWork}"/> that will be executed when the unit of work is being rolled back.</param>
        public DbUnitOfWork(IDbTransaction transaction, Action<DbUnitOfWork> onCommit, Action<DbUnitOfWork> onRollback)
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
            Ensure.That(Transaction).IsNotNull();

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
            Ensure.That(Transaction).IsNotNull();

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
        /// Retrieves the underlying <see cref="IDbTransaction"/> instance.
        /// </summary>
        public IDbTransaction Transaction { get; private set; }

        #endregion
    }
}
