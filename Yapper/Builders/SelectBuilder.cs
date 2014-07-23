using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using EnsureThat;
using Augment;
using Yapper.Dialects;
using Yapper.Mappers;

namespace Yapper.Builders
{
    /// <summary>
    /// Implementation for SelectBuilder
    /// </summary>
    internal sealed class SelectBuilder<T> : SqlBuilder,
        ISelectBuilder<T>,
        ISelectAndOrOrderByBuilder<T>,
        ISelectOrderByBuilder<T>,
        ISelectThenByBuilder<T>,
        ISelectPageBuilder<T>
    {
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="session"></param>
        public SelectBuilder(ISqlDialect dialect, T item)
            : this(dialect)
        {
            AppendWhereAnd(item, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="session"></param>
        public SelectBuilder(ISqlDialect dialect)
            : base(dialect, typeof(T))
        {
            SelectClause = new StringBuilder();
            FromClause = new StringBuilder();
            OrderByClause = new StringBuilder();
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override string GetQuery()
        {
            if (SelectClause.Length == 0)
            {
                foreach (PropertyMap pm in ObjectMap.Properties.Where(x => x.IsSelectable))
                {
                    AppendSelect(pm);
                }
            }

            if (FromClause.Length == 0)
            {
                FromClause.Append(Dialect.EscapeIdentifier(ObjectMap.SourceName));
            }

            if (WhereClause.Length > 0)
            {
                WhereClause.Insert(0, "where ");
            }

            if (OrderByClause.Length == 0 && IsPaging && ObjectMap.HasPrimaryKey)
            {
                //  default to PK
                foreach (PropertyMap pm in ObjectMap.Properties.Where(x => x.IsPrimaryKey))
                {
                    AppendOrderBy(pm, false);
                }
            }

            if (OrderByClause.Length > 0)
            {
                OrderByClause.Insert(0, "order by ");
            }

            Parameters = BuilderParameters.ToExpando();

            return Dialect.SelectStatement(
                SelectClause.ToString(),
                FromClause.ToString(),
                WhereClause.ToString(),
                OrderByClause.ToString(),
                null,
                Limit,
                PageNumber > 0 ? PageNumber - 1 : 0, ItemsPerPage
                );
        }

        private void AppendSelect<TField>(Expression<Func<T, TField>> field, string sqlFunction = null)
        {
            string nm = GetPropertyName(field);

            PropertyMap pm = ObjectMap.Properties[nm];

            AppendSelect(pm, sqlFunction);
        }

        private void AppendSelect(PropertyMap pm, string sqlFunction = null)
        {
            SelectClause.AppendIf(SelectClause.Length > 0, ", ");

            string source = Dialect.EscapeIdentifier(pm.SourceName);

            if (sqlFunction.IsNullOrEmpty())
            {
                if (pm.UsesEnumMap)
                {
                    SelectClause.Append(pm.EnumMap.FromSql.FormatArgs(source));
                }
                else
                {
                    SelectClause.Append(source);
                }
            }
            else
            {
                SelectClause.Append(sqlFunction).Append("(").Append(source).Append(")");
            }

            SelectClause.Append(" as ").Append(Dialect.EscapeIdentifier(pm.Name));
        }

        private void AppendOrderBy<TField>(Expression<Func<T, TField>> field, bool clear, bool desc)
        {
            string nm = GetPropertyName(field);

            PropertyMap pm = ObjectMap.Properties[nm];

            if (clear)
            {
                OrderByClause.Clear();
            }

            AppendOrderBy(pm, desc);
        }

        private void AppendOrderBy(PropertyMap pm, bool desc)
        {
            OrderByClause.AppendIf(OrderByClause.Length > 0, ", ")
                .Append(Dialect.EscapeIdentifier(pm.SourceName))
                .AppendIf(desc, " desc")
                ;
        }

        #endregion

        #region ISelectBuilder<T> Members

        public ISelectBuilder<T> Top(int top)
        {
            Ensure.That(top)
                .WithExtraMessageOf(() => "Top must be greater than zero")
                .IsGt(0);

            Limit = top;

            PageNumber = 0;

            ItemsPerPage = 0;

            return this;
        }

        public ISelectBuilder<T> Column<K>(Expression<Func<T, K>> column)
        {
            AppendSelect(column);

            return this;
        }

        public ISelectBuilder<T> Max<K>(Expression<Func<T, K>> max)
        {
            AppendSelect(max, "MAX");

            return this;
        }

        public ISelectBuilder<T> Min<K>(Expression<Func<T, K>> min)
        {
            AppendSelect(min, "MIN");

            return this;
        }

        public ISelectBuilder<T> Avg<K>(Expression<Func<T, K>> avg)
        {
            AppendSelect(avg, "AVG");

            return this;
        }

        public ISelectBuilder<T> Sum<K>(Expression<Func<T, K>> sum)
        {
            AppendSelect(sum, "SUM");

            return this;
        }

        public ISelectBuilder<T> Count<K>(Expression<Func<T, K>> count)
        {
            AppendSelect(count, "COUNT");

            return this;
        }

        public ISelectBuilder<T> Count()
        {
            SelectClause.AppendIf(SelectClause.Length > 0, ", ")
                .Append("COUNT(1)")
                ;

            return this;
        }

        #endregion

        #region IWhereBuilder<ISelectAndOrOrderByBuilder<T>,T> Members

        public ISelectAndOrOrderByBuilder<T> Where(object where)
        {
            AppendWhereAnd(where);

            return this;
        }

        public ISelectAndOrOrderByBuilder<T> Where(Expression<Func<T, bool>> expression)
        {
            AppendWhereAnd(expression);

            return this;
        }

        #endregion

        #region IWhereAndOrBuilder<ISelectAndOrOrderByBuilder<T>,T> Members

        public ISelectAndOrOrderByBuilder<T> And(object where)
        {
            AppendWhereAnd(where);

            return this;
        }

        public ISelectAndOrOrderByBuilder<T> And(Expression<Func<T, bool>> expression)
        {
            AppendWhereAnd(expression);

            return this;
        }

        public ISelectAndOrOrderByBuilder<T> Or(object where)
        {
            AppendWhereOr(where);

            return this;
        }

        public ISelectAndOrOrderByBuilder<T> Or(Expression<Func<T, bool>> expression)
        {
            AppendWhereOr(expression);

            return this;
        }

        #endregion

        #region ISelectOrderByBuilder<T> Members

        public ISelectThenByBuilder<T> OrderBy<K>(Expression<Func<T, K>> orderby)
        {
            AppendOrderBy(orderby, true, false);

            return this;
        }

        public ISelectThenByBuilder<T> OrderByDescending<K>(Expression<Func<T, K>> orderby)
        {
            AppendOrderBy(orderby, true, true);

            return this;
        }

        #endregion

        #region ISelectThenByBuilder<T> Members

        public ISelectThenByBuilder<T> ThenBy<K>(Expression<Func<T, K>> thenby)
        {
            AppendOrderBy(thenby, false, false);

            return this;
        }

        public ISelectThenByBuilder<T> ThenByDescending<K>(Expression<Func<T, K>> thenby)
        {
            AppendOrderBy(thenby, false, true);

            return this;
        }

        #endregion

        #region ISelectPageBuilder<T> Members

        public ISqlQuery Page(int page, int itemsPerPage)
        {
            Ensure.That(page)
                .WithExtraMessageOf(() => "Top Page be greater than one")
                .IsGt(0);

            Ensure.That(itemsPerPage)
                .WithExtraMessageOf(() => "Item per Page be greater than zero")
                .IsGt(0);

            Limit = 0;

            PageNumber = page;

            ItemsPerPage = itemsPerPage;

            return this;
        }

        #endregion

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        private StringBuilder SelectClause { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private StringBuilder FromClause { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private StringBuilder OrderByClause { get; set; }

        /// <summary>
        /// 1 based index
        /// </summary>
        private int PageNumber { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private int ItemsPerPage { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private int Limit { get; set; }

        /// <summary>
        /// The start for a paged result set
        /// </summary>
        private int Start
        {
            get { return ((PageNumber - 1) * ItemsPerPage) + 1; }
        }

        /// <summary>
        /// The offset for a paged result set
        /// </summary>
        private int Offset
        {
            get { return ItemsPerPage * PageNumber; }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool IsPaging
        {
            get { return PageNumber > 0 && ItemsPerPage > 0; }
        }

        #endregion
    }
}