using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yapper.Builders;
using Yapper.Dialects;

namespace Yapper
{
    public class Sql
    {
        #region Defaults

        /// <summary>
        /// 
        /// </summary>
        [ThreadStatic]
        internal static ISqlDialect Dialect;

        #endregion

        #region Insert

        public static ISqlQuery Insert<T>(T item)
        {
            InsertBuilder<T> builder = new InsertBuilder<T>(Dialect, item);

            return builder;
        }

        #endregion

        #region Delete

        public static IDeleteBuilder<T> Delete<T>()
        {
            IDeleteBuilder<T> builder = new DeleteBuilder<T>(Dialect);

            return builder;
        }

        public static ISqlQuery Delete<T>(T item)
        {
            DeleteBuilder<T> builder = new DeleteBuilder<T>(Dialect, item);

            return builder;
        }

        #endregion

        #region Update

        public static IUpdateBuilder<T> Update<T>()
        {
            UpdateBuilder<T> builder = new UpdateBuilder<T>(Dialect);

            return builder;
        }

        public static ISqlQuery Update<T>(T item)
        {
            IUpdateBuilder<T> builder = new UpdateBuilder<T>(Dialect, item);

            return builder;
        }

        #endregion

        #region Select

        public static ISelectBuilder<T> Select<T>()
        {
            SelectBuilder<T> builder = new SelectBuilder<T>(Dialect);

            return builder;
        }

        #endregion
    }
}