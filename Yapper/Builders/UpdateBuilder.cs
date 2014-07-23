using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using EnsureThat;
using Augment;
using Yapper.Dialects;
using Yapper.Mappers;

namespace Yapper.Builders
{
    /// <summary>
    /// Implementation for UpdateBuilder
    /// </summary>
    sealed class UpdateBuilder<T> : SqlBuilder, IUpdateBuilder<T>, IUpdateWhereAndOrBuilder<T>
    {
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dialect"></param>
        /// <param name="item"></param>
        public UpdateBuilder(ISqlDialect dialect, T item)
            : this(dialect)
        {
            AppendSet(item, true);
            AppendWhereAnd(item, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dialect"></param>
        public UpdateBuilder(ISqlDialect dialect)
            : base(dialect, typeof(T))
        {
            SetClause = new StringBuilder();
        }

        #endregion

        #region Methods

        private void AppendSet(object values, bool excludePrimaryKeys = false)
        {
            foreach (PropertyInfo p in values.GetType().GetProperties())
            {
                PropertyMap pm = ObjectMap.Properties[p.Name];

                bool add = true;

                if (excludePrimaryKeys)
                {
                    //  don't add the PK
                    add = !pm.IsPrimaryKey;
                }

                if (add && pm.IsUpdatable)
                {
                    SetClause.AppendIf(SetClause.Length > 0, " ,")
                        .Append(Dialect.EscapeIdentifier(pm.SourceName))
                        .Append(" = ")
                        .Append(AppendParameter(pm, p.GetValue(values)))
                        ;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override string GetQuery()
        {
            StringBuilder sb = new StringBuilder("update ")
                .Append(Dialect.EscapeIdentifier(ObjectMap.SourceName))
                .Append(" set ").Append(SetClause.ToString());

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
        private StringBuilder SetClause { get; set; }

        #endregion

        #region IUpdateBuilder<T> Members

        public IUpdateBuilder<T> Set(object values)
        {
            AppendSet(values, false);

            return this;
        }

        public IUpdateBuilder<T> Set<K, V>(Expression<Func<T, K>> field, Expression<Func<T, V>> expression)
        {
            string nm = GetPropertyName<T, K>(field);

            PropertyMap pm = ObjectMap.Properties[nm];

            Ensure.That(expression).IsNotNull();

            ExpressionBuilder e = new ExpressionBuilder(Dialect, BuilderParameters.Count);

            e.Compile(expression);

            if (e.Query.IsNotEmpty())
            {
                SetClause.AppendIf(SetClause.Length > 0, " ,")
                   .Append(Dialect.EscapeIdentifier(pm.SourceName))
                   .Append(" = ").Append(e.Query)
                   ;

                AppendParameters(e.Parameters);
            }

            return this;
        }

        #endregion

        #region IWhereBuilder<IUpdateWhereAndOrBuilder<T>,T> Members

        public ISqlQuery Where(T item)
        {
            AppendWhereAnd(item, true);

            return this;
        }

        public IUpdateWhereAndOrBuilder<T> Where(object where)
        {
            AppendWhereAnd(where);

            return this;
        }

        public IUpdateWhereAndOrBuilder<T> Where(Expression<Func<T, bool>> expression)
        {
            AppendWhereAnd(expression);

            return this;
        }

        #endregion

        #region IWhereAndOrBuilder<IUpdateWhereAndOrBuilder<T>,T> Members

        public IUpdateWhereAndOrBuilder<T> And(object where)
        {
            AppendWhereAnd(where);

            return this;
        }

        public IUpdateWhereAndOrBuilder<T> And(Expression<Func<T, bool>> expression)
        {
            AppendWhereAnd(expression);

            return this;
        }

        public IUpdateWhereAndOrBuilder<T> Or(object where)
        {
            AppendWhereOr(where);

            return this;
        }

        public IUpdateWhereAndOrBuilder<T> Or(Expression<Func<T, bool>> expression)
        {
            AppendWhereOr(expression);

            return this;
        }

        #endregion
    }
}