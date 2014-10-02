using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using UniqueNamespace.Dapper;

namespace Yapper
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBuilderResults
    {
        /// <summary>
        /// 
        /// </summary>
        string Sql { get; }
        /// <summary>
        /// 
        /// </summary>
        object Parameters { get; }
    }

    /// <summary>
    /// Represents an open connection to a specified database by wrapping
    /// an <see cref="IDbConnection"/> instance.
    /// </summary>
    public interface IDatabaseSession : IDisposable
    {
        #region Members

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

        #endregion

        #region Plain Dapper

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        IEnumerable<T> Query<T>(string sql, object param = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        SqlMapper.GridReader QueryMultiple(string sql, object param = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns>Number of records affected</returns>
        int Execute(string sql, object param = null);

        #endregion

        #region Builder Ops

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="r"></param>
        /// <returns></returns>
        IEnumerable<T> Query<T>(IBuilderResults r);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        IEnumerable<T> Query<T>(SqlBuilder.Template sql);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        SqlMapper.GridReader QueryMultiple(SqlBuilder.Template sql);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        int Execute(SqlBuilder.Template sql);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        int Execute(IBuilderResults r);

        #endregion

        #region CUD Ops

        /// <summary>
        /// Inserts a new item w/o all properties assigned
        /// </summary>
        int Insert<T>(object data);

        /// <summary>
        /// Inserts a new item using all properties
        /// </summary>
        int Insert<T>(T item);

        /// <summary>
        /// Updates a new item w/o all properties assigned
        /// </summary>
        /// <param name="update">Properties to use for updating</param>
        /// <param name="where">Properties to use for filtering (all if null)</param>
        int Update<T>(object update, object where);

        /// <summary>
        /// Updates a specific item
        /// </summary>
        int Update<T>(T item);

        /// <summary>
        /// Deletes items based on filter
        /// </summary>
        /// <param name="where">Properties to use for filtering (all if null)</param>
        int Delete<T>(object where);

        /// <summary>
        /// Deletes a specific item
        /// </summary>
        int Delete<T>(T item);

        #endregion
    }

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
}
