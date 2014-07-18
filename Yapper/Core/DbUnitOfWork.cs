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

        private readonly Action<DbUnitOfWork> _onCommit;
        private readonly Action<DbUnitOfWork> _onRollback;

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
            
            _onCommit = onCommit;
            _onRollback = onRollback;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Implements <see cref="IDisposable.Dispose"/>, and commits the statements executed inside this unit of work.
        /// This makes it easier to use a unit of work instance inside a <c>using</c> statement (<c>Using</c> in VB.Net).
        /// </summary>
        public void Dispose()
        {
            if (Transaction == null)
            {
                return;
            }

            try
            {
                Transaction.Commit();

                _onCommit(this);
            }
            catch(Exception)
            {
                Transaction.Rollback();

                _onRollback(this);

                throw;
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
