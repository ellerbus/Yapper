using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using EnsureThat;
using Augment;
using Yapper.Dialects;
using Yapper.Mappers;
using System.Dynamic;

namespace Yapper.Builders
{
    /// <summary>
    /// Implementation for DeleteBuilder
    /// </summary>
    internal class DeleteBuilder<T> : SqlBuilder, IDeleteBuilder<T>, IDeleteAndOrBuilder<T>
    {
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="session"></param>
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
        private string GetQuery()
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

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public override string Query
        {
            get
            {
                if (_query.IsNullOrEmpty())
                {
                    _query = GetQuery();
                }
                return _query;
            }
        }
        private string _query;

        #endregion

        #region IWhereBuilder<IDeleteAndOrBuilder<T>,T> Members

        public ISqlQuery Where(T item)
        {
            AppendWhere(item, true);

            return this;
        }

        public IDeleteAndOrBuilder<T> Where(object where)
        {
            AppendWhere(where);

            return this;
        }

        public IDeleteAndOrBuilder<T> Where(Expression<Func<T, bool>> expression)
        {
            AppendWhere(expression);

            return this;
        }

        #endregion

        #region IWhereAndOrBuilder<IDeleteAndOrBuilder<T>,T> Members

        public IDeleteAndOrBuilder<T> And(object where)
        {
            AppendWhere(where);

            return this;
        }

        public IDeleteAndOrBuilder<T> And(Expression<Func<T, bool>> expression)
        {
            AppendWhere(expression);

            return this;
        }

        public IDeleteAndOrBuilder<T> Or(object where)
        {
            AppendWhere(where);

            return this;
        }

        public IDeleteAndOrBuilder<T> Or(Expression<Func<T, bool>> expression)
        {
            AppendWhere(expression);

            return this;
        }

        #endregion
    }
}