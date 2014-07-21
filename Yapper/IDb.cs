using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yapper
{
    public interface IContext : IDisposable
    {
        IUnitOfWork CreateUnitOfWork(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        IEnumerable<T> Query<T>(ISqlQuery query);

        int Execute(ISqlQuery query);
    }

    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Retrieves the underlying <see cref="IDbTransaction"/> instance.
        /// </summary>
        IDbTransaction Transaction { get; }

        /// <summary>
        /// 
        /// </summary>
        void Commit();

        /// <summary>
        /// 
        /// </summary>
        void Rollback();
    }
}
