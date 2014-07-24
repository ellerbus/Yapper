using System;
using System.Collections.Generic;
using System.Data;
using Dapper;

namespace Yapper
{
    /// <summary>
    /// Represents an open connection to a specified database by wrapping
    /// an <see cref="IDbConnection"/> instance.
    /// </summary>
    public interface ISession : IDisposable
    {
        /// <summary>
        /// Creates a unit of work (creates an underlying database transaction).
        /// </summary>
        /// <param name="isolationLevel">Specifies the <see cref="IsolationLevel"/> for this transaction.</param>
        /// <returns>A <see cref="IUnitOfWork"/></returns>
        IUnitOfWork CreateUnitOfWork(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        /// <summary>
        /// Executes a query, returning the data typed as per T (wrapping Dapper.SqlMapper.Query).
        /// </summary>
        /// <typeparam name="T">The Type to be returned</typeparam>
        /// <param name="query">The <see cref="ISqlQuery"/> that builds the underlying SQL statement and parameters.</param>
        /// <returns>A sequence of data of the supplied type</returns>
        IEnumerable<T> Query<T>(ISqlQuery query);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="q0"></param>
        /// <param name="q1"></param>
        /// <returns></returns>
        SqlMapper.GridReader Query(ISqlQuery q0, ISqlQuery q1);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="q0"></param>
        /// <param name="q1"></param>
        /// <param name="queries"></param>
        /// <returns></returns>
        SqlMapper.GridReader Query(ISqlQuery q0, ISqlQuery q1, params ISqlQuery[] queries);

        /// <summary>
        /// Executes a <see cref="ISqlQuery"/> against the Connection object,
        /// and returns the number of rows affected.
        /// </summary>
        /// <param name="query">The <see cref="ISqlQuery"/> that builds the underlying SQL statement and parameters.</param>
        /// <returns>The number of rows affected.</returns>
        int Execute(ISqlQuery query);

        /// <summary>
        /// Retrieves the underlying <see cref="IDbConnection"/> instance.
        /// </summary>
        IDbConnection Connection { get; }
    }

    /// <summary>
    /// Represents a unit of work to be performed against the database by wrapping
    /// an <see cref="IDbTransaction"/> instance.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Retrieves the underlying <see cref="IDbTransaction"/> instance.
        /// </summary>
        IDbTransaction Transaction { get; }

        /// <summary>
        /// Commits the Unit of Work
        /// </summary>
        void Commit();

        /// <summary>
        /// Rolls back the Unit of Work
        /// </summary>
        void Rollback();
    }
}
