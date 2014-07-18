using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using EnsureThat;
using Augment;
using Yamor.Dialects;
using Yamor.Mappers;
using Yamor.ResourceMessages;

namespace Yamor.Builders
{
    /// <summary>
    /// Implementation for UpdateBuilder
    /// </summary>
    internal class UpdateBuilder<T> : StatementBuilder, IUpdateBuilder<T> where T : class
    {
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="session"></param>
        public UpdateBuilder(ISession session)
            : base(session, typeof(T))
        {
            SetClause = new StringBuilder();
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IBuilderResults BuildQuery()
        {
            StringBuilder sb = new StringBuilder("update ")
                .Append(Dialect.EscapeIdentifier(ObjectMap.SourceName));

            Ensure.That(SetClause.Length > 0)
                .WithExtraMessageOf(() => "Missing Set Clause")
                .IsTrue()
                ;

            sb.Append(" set ").Append(SetClause.ToString());

            if (WhereClause.Length > 0)
            {
                sb.Append(" where ").Append(WhereClause.ToString());
            }

            IBuilderResults results = new BuilderResults();

            results.SqlQuery = sb.ToString();
            results.Parameters = new Dictionary<string, IParameter>(Parameters);

            return results;
        }

        #endregion

        #region IUpdateBuilder<T> Members

        public IUpdateBuilder<T> Set(object values)
        {
            foreach (PropertyInfo p in values.GetType().GetProperties())
            {
                PropertyMap pm = ObjectMap.Properties[p.Name];

                SetClause.AppendIf(SetClause.Length > 0, " ,")
                    .Append(Dialect.EscapeIdentifier(pm.SourceName))
                    .Append(" = ")
                    .Append(AppendParameter(pm, p.GetValue(values)))
                    ;

            }

            return this;
        }

        public IUpdateBuilder<T> Set<TField>(Expression<Func<T, TField>> field, object value)
        {
            string nm = GetPropertyName<T, TField>(field);

            PropertyMap pm = ObjectMap.Properties[nm];

            SetClause.AppendIf(SetClause.Length > 0, " ,")
                .Append(Dialect.EscapeIdentifier(pm.SourceName))
                .Append(" = ")
                .Append(AppendParameter(pm, value))
                ;

            return this;
        }

        public IUpdateBuilder<T> Set<TField, TValue>(Expression<Func<T, TField>> field, Expression<Func<T, TValue>> expression)
        {
            string nm = GetPropertyName<T, TField>(field);

            PropertyMap pm = ObjectMap.Properties[nm];

            Ensure.That(expression).IsNotNull();

            ExpressionBuilder e = new ExpressionBuilder(Driver, Parameters.Count);

            IBuilderResults results = e.Compile(expression);

            if (results.SqlQuery.IsNotEmpty())
            {
                SetClause.AppendIf(SetClause.Length > 0, " ,")
                   .Append(Dialect.EscapeIdentifier(pm.SourceName))
                   .Append(" = ").Append(results.SqlQuery)
                   ;

                AppendParameters(results.Parameters);
            }

            return this;
        }

        public int Execute()
        {
            IBuilderResults results = BuildQuery();

            return ExecuteNonQuery(results);
        }

        public IUpdateBuilder<T> Where(object values)
        {
            AppendWhere(values);

            return this;
        }

        public IUpdateBuilder<T> Where(Expression<Func<T, bool>> where)
        {
            AppendWhere(where);

            return this;
        }

        #endregion

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        protected StringBuilder SetClause { get; private set; }

        #endregion
    }
}