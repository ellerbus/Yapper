using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using Yapper.Dialects;

namespace Yapper
{
    /// <summary>
    /// Represents an open connection to a specified database by wrapping
    /// an <see cref="IDbConnection"/> instance.
    /// </summary>
    public interface ISession : IDisposable
    {
        #region Database

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
        /// Execute a command that returns multiple result sets, that can be accessed sequentially 
        /// via the SqlMapper.GridReader
        /// </summary>
        /// <param name="q0">The first query</param>
        /// <param name="q1">The second query</param>
        /// <returns>A <see cref="SqlMapper.GridReader"/> to access returned result sets</returns>
        SqlMapper.GridReader Query(ISqlQuery q0, ISqlQuery q1);

        /// <summary>
        /// Execute a command that returns multiple result sets, that can be accessed sequentially 
        /// via the SqlMapper.GridReader
        /// </summary>
        /// <param name="q0">The first query</param>
        /// <param name="q1">The second query</param>
        /// <param name="queries">Variable list of remaining queries</param>
        /// <returns>A <see cref="SqlMapper.GridReader"/> to access returned result sets</returns>
        SqlMapper.GridReader Query(ISqlQuery q0, ISqlQuery q1, params ISqlQuery[] queries);

        /// <summary>
        /// Send multiple commands to be executed
        /// </summary>
        /// <param name="q0">The first command</param>
        /// <param name="q1">The second command</param>
        void ExecuteMany(ISqlQuery q0, ISqlQuery q1);

        /// <summary>
        /// Send multiple commands to be executed
        /// </summary>
        /// <param name="q0">The first command</param>
        /// <param name="q1">The second command</param>
        /// <param name="queries">Variable list of remaining commands</param>
        void ExecuteMany(ISqlQuery q0, ISqlQuery q1, params ISqlQuery[] queries);

        /// <summary>
        /// Executes a <see cref="ISqlQuery"/> against the Connection object,
        /// and returns the number of rows affected.
        /// </summary>
        /// <param name="query">The <see cref="ISqlQuery"/> that builds the underlying SQL statement and parameters.</param>
        /// <returns>The number of rows affected.</returns>
        int Execute(ISqlQuery query);

        #endregion

        #region Properties

        /// <summary>
        /// Retrieves the underlying <see cref="IDbConnection"/> instance.
        /// </summary>
        IDbConnection Connection { get; }

        /// <summary>
        /// Retrieves a Sql Builder using an assigned <see cref="ISqlDialect"/>
        /// </summary>
        IDbSql Sql { get; }

        #endregion
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

    /// <summary>
    /// Represents a SQL Builder to a specified database by wrapping
    /// a <see cref="ISqlDialect"/> instance.
    /// </summary>
    public interface IDbSql
    {
        /// <summary>
        /// Starting point for building an Insert statement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">T to be deleted from the database</param>
        /// <returns>An instance of <see cref="ISqlQuery"/></returns>
        ISqlQuery Insert<T>(T item);

        /// <summary>
        /// Starting point for building a Delete statement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IDeleteBuilder<T> Delete<T>();

        /// <summary>
        /// Starting point for building a Delete statement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">T to be deleted from the database</param>
        /// <returns>An instance of <see cref="ISqlQuery"/></returns>
        ISqlQuery Delete<T>(T item);

        /// <summary>
        /// Starting point for building an Update statement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IUpdateBuilder<T> Update<T>();

        /// <summary>
        /// Starting point for building an Update statement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">T to be deleted from the database</param>
        /// <returns>An instance of <see cref="ISqlQuery"/></returns>
        ISqlQuery Update<T>(T item);

        /// <summary>
        /// Starting point for building a Select statement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>An instance of <see cref="ISelectBuilder{T}"/></returns>
        ISelectBuilder<T> Select<T>();
    }
}
