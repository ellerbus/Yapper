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
using Yamor.Dialects;
using Yamor.Logging;
using Yamor.Mappers;
using Yamor.ResourceMessages;

namespace Yamor.Builders
{
    /// <summary>
    /// Base class for building select statements (non-crud like ie. UpdateMany, DeleteMany, SelectMany)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class SelectBuilder<T> : StatementBuilder, ISelectBuilder<T> where T : class, new()
    {
        #region Members

        private static readonly ILog log = LogManager.GetCurrentClassLog();

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="session"></param>
        public SelectBuilder(ISession session)
            : base(session, typeof(T))
        {
            SelectClause = new StringBuilder();
            OrderByClause = new StringBuilder();

            foreach (PropertyMap p in ObjectMap.Properties.Where(x => x.HasPropertySetter))
            {
                SelectClause.AppendIf(SelectClause.Length > 0, ", ")
                    .Append(Dialect.EscapeIdentifier(p.SourceName))
                    .Append(" as ")
                    .Append(Dialect.EscapeIdentifier(p.Name))
                    ;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IBuilderResults BuildQuery()
        {
            IBuilderResults results = new BuilderResults();

            if (IsPaging)
            {
                results.SqlQuery = BuildPagedSelect();
            }
            else
            {
                results.SqlQuery = BuildSelect(Top);
            }

            results.Parameters = new Dictionary<string, IParameter>(Parameters);

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IBuilderResults BuildCountQuery()
        {
            IBuilderResults results = new BuilderResults();

            results.SqlQuery = BuildCountSelect();

            results.Parameters = new Dictionary<string, IParameter>(Parameters);

            return results;
        }

        protected virtual string BuildPagedSelect()
        {
            StringBuilder sb = new StringBuilder("select ");

            sb.Append(SelectClause)
                .Append(" from ").Append(Dialect.EscapeIdentifier(ObjectMap.SourceName))
                ;

            if (WhereClause.Length > 0)
            {
                sb.Append(" where ").Append(WhereClause.ToString());
            }

            if (OrderByClause.Length == 0)
            {
                DefaultOrderByClause();
            }

            sb.Append(" order by ").Append(OrderByClause.ToString());

            sb.Append(" limit ").Append(Start).Append(", ").Append(Offset);

            return sb.ToString();
        }

        protected void DefaultOrderByClause()
        {
            OrderByClause.Clear();

            foreach (PropertyMap p in ObjectMap.Properties.Where(x => x.IsPrimaryKey))
            {
                AppendOrderBy(p, false);
            }
        }

        protected virtual string BuildSelect(int top)
        {
            StringBuilder sb = new StringBuilder("select ");

            sb.Append(SelectClause)
                .Append(" from ").Append(Dialect.EscapeIdentifier(ObjectMap.SourceName))
                ;

            if (WhereClause.Length > 0)
            {
                sb.Append(" where ").Append(WhereClause.ToString());
            }

            if (OrderByClause.Length > 0)
            {
                sb.Append(" order by ").Append(OrderByClause.ToString());
            }

            if (top > 0)
            {
                sb.Append(" limit ").Append(top);
            }

            return sb.ToString();
        }

        protected virtual string BuildCountSelect(int top = 0)
        {
            StringBuilder sb = new StringBuilder("select ");

            sb.Append("count(1)")
                .Append(" from ").Append(Dialect.EscapeIdentifier(ObjectMap.SourceName))
                ;

            if (WhereClause.Length > 0)
            {
                sb.Append(" where ").Append(WhereClause.ToString());
            }

            return sb.ToString();
        }

        #endregion

        #region ISelectBuilder<T> Members

        public int Count()
        {
            IBuilderResults results = BuildCountQuery();

            object value = ExecuteScalar(results);

            return (int)Convert.ChangeType(value, typeof(int));
        }

        public IList<T> ToList()
        {
            IBuilderResults results = BuildQuery();

            IList<T> items = ExecuteObjects(results);

            return items;
        }

        public IPagedList<T> ToPagedList(int page, int itemsPerPage)
        {
            PageNumber = page;
            ItemsPerPage = itemsPerPage;
            Top = 0;

            IBuilderResults results = BuildQuery();

            IList<T> items = ExecuteObjects(results);

            PagedList<T> pagedList = new PagedList<T>()
            {
                Items = items,
                Page = page,
                ItemsPerPage = itemsPerPage,
                TotalItems = Count()
            };

            return pagedList;
        }

        public T FirstOrDefault()
        {
            IBuilderResults results = BuildQuery();

            T item = ExecuteObject(results);

            return item;
        }

        public ISelectBuilder<T> Limit(int top)
        {
            PageNumber = 0;
            ItemsPerPage = 0;
            Top = top;

            return this;
        }

        public ISelectBuilder<T> OrderBy<TField>(Expression<Func<T, TField>> field)
        {
            AppendOrderBy(field, true, false);

            return this;
        }

        public ISelectBuilder<T> ThenBy<TField>(Expression<Func<T, TField>> field)
        {
            AppendOrderBy(field, false, false);

            return this;
        }

        public ISelectBuilder<T> OrderByDescending<TField>(Expression<Func<T, TField>> field)
        {
            AppendOrderBy(field, true, true);

            return this;
        }

        public ISelectBuilder<T> ThenByDescending<TField>(Expression<Func<T, TField>> field)
        {
            AppendOrderBy(field, false, true);

            return this;
        }

        public ISelectBuilder<T> Where(object values)
        {
            AppendWhere(values);

            return this;
        }

        public ISelectBuilder<T> Where(Expression<Func<T, bool>> expression)
        {
            AppendWhere(expression);

            return this;
        }

        #endregion

        #region Methods

        private IList<T> ExecuteObjects(IBuilderResults results)
        {
            using (IDbCommand cmd = Session.CreateCommand(results))
            {
                Func<IDbCommand, IList<T>> action = (x) =>
                {
                    IList<T> items = new List<T>();

                    using (IDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(CreateObject(reader));
                        }
                    }

                    return items;
                };

                return ExecuteCommand(cmd, action);
            }
        }

        private T ExecuteObject(IBuilderResults results)
        {
            using (IDbCommand cmd = Session.CreateCommand(results))
            {
                Func<IDbCommand, T> action = (x) =>
                {
                    T item = null;

                    using (IDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            item = CreateObject(reader);
                        }
                    }

                    return item;
                };

                return ExecuteCommand(cmd, action);
            }
        }

        private T CreateObject(IDataReader reader)
        {
            T item = new T();

            foreach (PropertyMap p in ObjectMap.Properties.Where(x => x.IsSelectable))
            {
                object value = p.ConvertToClrValue(reader[p.Name]);

                p.SetValue(item, value);
            }

            return item;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pm"></param>
        /// <param name="desc"></param>
        protected void AppendOrderBy(PropertyMap pm, bool desc)
        {
            OrderByClause.AppendIf(OrderByClause.Length > 0, ", ")
                .Append(Dialect.EscapeIdentifier(pm.SourceName))
                .AppendIf(desc, " desc")
                ;
        }

        #endregion

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        protected StringBuilder SelectClause { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        protected StringBuilder OrderByClause { get; private set; }

        /// <summary>
        /// 1 based index
        /// </summary>
        protected int PageNumber { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        protected int ItemsPerPage { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        protected int Top { get; private set; }

        /// <summary>
        /// The start for a paged result set
        /// </summary>
        protected virtual int Start
        {
            get { return ((PageNumber - 1) * ItemsPerPage) + 1; }
        }

        /// <summary>
        /// The offset for a paged result set
        /// </summary>
        protected virtual int Offset
        {
            get { return ItemsPerPage * PageNumber; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected bool IsPaging
        {
            get { return PageNumber > 0 && ItemsPerPage > 0; }
        }

        #endregion
    }
}