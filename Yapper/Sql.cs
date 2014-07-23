using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yapper.Builders;
using Yapper.Dialects;

namespace Yapper
{
    /// <summary>
    /// Starting point for building SQL Statements
    /// </summary>
    public static class Sql
    {
        #region Defaults

        /// <summary>
        /// Default Dialect used for building SQL Statements (<see cref="ThreadStaticAttribute">ThreadStatic</see>)
        /// </summary>
        [ThreadStatic]
        internal static ISqlDialect Dialect;

        #endregion

        #region Insert

        /// <summary>
        /// Starting point for building an Insert statement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">T to be deleted from the database</param>
        /// <returns>An instance of <see cref="ISqlQuery"/></returns>
        public static ISqlQuery Insert<T>(T item)
        {
            InsertBuilder<T> builder = new InsertBuilder<T>(Dialect, item);

            return builder;
        }

        #endregion

        #region Delete

        /// <summary>
        /// Starting point for building a Delete statement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IDeleteBuilder<T> Delete<T>()
        {
            IDeleteBuilder<T> builder = new DeleteBuilder<T>(Dialect);

            return builder;
        }

        /// <summary>
        /// Starting point for building a Delete statement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">T to be deleted from the database</param>
        /// <returns>An instance of <see cref="ISqlQuery"/></returns>
        public static ISqlQuery Delete<T>(T item)
        {
            DeleteBuilder<T> builder = new DeleteBuilder<T>(Dialect, item);

            return builder;
        }

        #endregion

        #region Update

        /// <summary>
        /// Starting point for building an Update statement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IUpdateBuilder<T> Update<T>()
        {
            UpdateBuilder<T> builder = new UpdateBuilder<T>(Dialect);

            return builder;
        }

        /// <summary>
        /// Starting point for building an Update statement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">T to be deleted from the database</param>
        /// <returns>An instance of <see cref="ISqlQuery"/></returns>
        public static ISqlQuery Update<T>(T item)
        {
            IUpdateBuilder<T> builder = new UpdateBuilder<T>(Dialect, item);

            return builder;
        }

        #endregion

        #region Select

        /// <summary>
        /// Starting point for building a Select statement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>An instance of <see cref="ISelectBuilder{T}"/></returns>
        public static ISelectBuilder<T> Select<T>()
        {
            SelectBuilder<T> builder = new SelectBuilder<T>(Dialect);

            return builder;
        }

        #endregion
    }
}