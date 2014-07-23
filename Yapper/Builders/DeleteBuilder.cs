using System;
using System.Linq.Expressions;
using System.Text;
using Yapper.Dialects;
using Yapper.Mappers;

namespace Yapper.Builders
{
    /// <summary>
    /// Implementation for DeleteBuilder
    /// </summary>
    sealed class DeleteBuilder<T> : SqlBuilder, IDeleteBuilder<T>, IDeleteAndOrBuilder<T>
    {
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dialect"></param>
        /// <param name="item"></param>
        public DeleteBuilder(ISqlDialect dialect, T item)
            : this(dialect)
        {
            AppendWhereAnd(item, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dialect"></param>
        public DeleteBuilder(ISqlDialect dialect)
            : base(dialect, typeof(T))
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override string GetQuery()
        {
            StringBuilder sb = new StringBuilder("delete from ")
                .Append(Dialect.EscapeIdentifier(ObjectMap.SourceName));

            if (WhereClause.Length > 0)
            {
                sb.Append(" where ").Append(WhereClause.ToString());
            }

            Parameters = BuilderParameters.ToExpando();

            return sb.ToString();
        }

        #endregion

        #region IWhereBuilder<IDeleteAndOrBuilder<T>,T> Members

        public IDeleteAndOrBuilder<T> Where(object where)
        {
            AppendWhereAnd(where);

            return this;
        }

        public IDeleteAndOrBuilder<T> Where(Expression<Func<T, bool>> expression)
        {
            AppendWhereAnd(expression);

            return this;
        }

        #endregion

        #region IWhereAndOrBuilder<IDeleteAndOrBuilder<T>,T> Members

        public IDeleteAndOrBuilder<T> And(object where)
        {
            AppendWhereAnd(where);

            return this;
        }

        public IDeleteAndOrBuilder<T> And(Expression<Func<T, bool>> expression)
        {
            AppendWhereAnd(expression);

            return this;
        }

        public IDeleteAndOrBuilder<T> Or(object where)
        {
            AppendWhereOr(where);

            return this;
        }

        public IDeleteAndOrBuilder<T> Or(Expression<Func<T, bool>> expression)
        {
            AppendWhereOr(expression);

            return this;
        }

        #endregion
    }
}