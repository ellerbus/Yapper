using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnsureThat;
using Augment;
using Yamor.Dialects;
using Yamor.Drivers;
using Yamor.Listeners;
using Yamor.Mappers;
using Yamor.ResourceMessages;

namespace Yamor.Builders
{
    /// <summary>
    /// Implementation for PersistenceBuilder
    /// </summary>
    internal sealed class PersistenceBuilder<T> : StatementBuilder, IPersistenceBuilder<T> where T : class
    {
        #region Members

        private static IDictionary<string, string> _cache = new Dictionary<string, string>();

        private static readonly object _lock = new object();

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="session"></param>
        public PersistenceBuilder(ISession session)
            : base(session, typeof(T))
        {
            RequiresPrimaryKey();
        }

        #endregion

        #region Insert Methods

        public bool Insert(T item)
        {
            Ensure.That(item)
                .WithExtraMessageOf(() => ExceptionMessages.CannotUpdateNullItem)
                .IsNotNull();

            IBuilderResults results = BuildInsertQuery(item);

            BeforeInsertListeners(item);

            bool executed = false;

            if (ObjectMap.HasIdentity)
            {
                object id = ExecuteScalar(results);

                ObjectMap.Identity.SetValue(item, id);

                executed =  true;
            }
            else
            {
                executed = ExecuteNonQuery(results) == 1;
            }

            if (executed)
            {
                AfterInsertListeners(item);
            }

            return executed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public IBuilderResults BuildInsertQuery(object data)
        {
            IBuilderResults results = new BuilderResults();

            string sql = null;

            lock (_lock)
            {
                string key = GetKey("I");

                if (!_cache.TryGetValue(key, out sql))
                {
                    StringBuilder sb = new StringBuilder("insert into ");

                    sb.Append(Dialect.EscapeIdentifier(ObjectMap.SourceName));

                    IList<PropertyMap> properties = GetInsertProperties().ToList();

                    IList<string> fields = properties.Select(x => Dialect.EscapeIdentifier(x.SourceName)).ToList();
                    IList<string> parms = properties.Select(x => Driver.GetParameterName(x.Name)).ToList();

                    sb.Append(" (")
                        .Append(fields.Join(", "))
                        .Append(") values (")
                        .Append(parms.Join(", "))
                        .Append(")")
                        ;

                    if (ObjectMap.HasIdentity)
                    {
                        Ensure.That(Dialect.SupportsIdentity)
                            .WithExtraMessageOf(() => ExceptionMessages.NotSupportedIdentity.FormatArgs(Dialect.GetType().FullName, ObjectMap.ObjectType.FullName))
                            .IsTrue();

                        string columnName = Dialect.EscapeIdentifier(ObjectMap.Identity.SourceName);

                        sb.Append(Syntax.SelectIdentityCommand.FormatArgs(columnName));
                    }

                    sql = sb.ToString();

                    _cache.Add(key, sql);
                }
            }

            results.SqlQuery = sql;
            results.Parameters = GetInsertParameters(data);

            return results;
        }

        private IDictionary<string, IParameter> GetInsertParameters(object data)
        {
            IEnumerable<PropertyMap> properties = GetInsertProperties();

            AppendParameters(properties, data);

            return Parameters;
        }

        private IEnumerable<PropertyMap> GetInsertProperties()
        {
            return ObjectMap.Properties.Where(x => x.IsInsertable);
        }

        #endregion

        #region Update Methods

        public bool Update(T item)
        {
            Ensure.That(item)
                .WithExtraMessageOf(() => ExceptionMessages.CannotUpdateNullItem)
                .IsNotNull();

            IBuilderResults results = BuildUpdateQuery(item);

            BeforeUpdateListeners(item);

            bool executed = ExecuteNonQuery(results) == 1;

            if (executed)
            {
                AfterUpdateListeners(item);
            }

            return executed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public IBuilderResults BuildUpdateQuery(object data)
        {
            BuilderResults results = new BuilderResults();

            string sql = null;

            lock (_lock)
            {
                string key = GetKey("U");

                if (!_cache.TryGetValue(key, out sql))
                {
                    StringBuilder sb = new StringBuilder("update ");

                    sb.Append(Dialect.EscapeIdentifier(ObjectMap.SourceName))
                        .Append(" set ");

                    bool delim = false;

                    foreach (PropertyMap p in ObjectMap.Properties.Where(x => x.IsUpdatable))
                    {
                        sb.AppendIf(delim, ", ")
                            .Append(Dialect.EscapeIdentifier(p.SourceName))
                            .Append(" = ")
                            .Append(Driver.GetParameterName(p.Name))
                            ;

                        delim = true;
                    }

                    AppendPrimaryKeyWhere(sb);

                    sql = sb.ToString();

                    _cache.Add(key, sql);
                }
            }

            results.SqlQuery = sql;
            results.Parameters = GetUpdateParameters(data);

            return results;
        }

        private IDictionary<string, IParameter> GetUpdateParameters(object data)
        {
            IEnumerable<PropertyMap> properties = ObjectMap.Properties.Where(x => x.IsUpdatable || x.IsPrimaryKey);

            AppendParameters(properties, data);

            return Parameters;
        }

        #endregion

        #region Delete Methods

        public bool Delete(T item)
        {
            Ensure.That(item)
                .WithExtraMessageOf(() => ExceptionMessages.CannotDeleteNullItem)
                .IsNotNull();

            IBuilderResults results = BuildDeleteQuery(item);

            BeforeDeleteListeners(item);

            bool executed = ExecuteNonQuery(results) == 1;

            if (executed)
            {
                AfterDeleteListeners(item);
            }

            return executed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public IBuilderResults BuildDeleteQuery(object data)
        {
            IBuilderResults results = new BuilderResults();

            string sql = null;

            lock (_lock)
            {
                string key = GetKey("D");

                if (!_cache.TryGetValue(key, out sql))
                {
                    StringBuilder sb = new StringBuilder("delete from ")
                        .Append(Dialect.EscapeIdentifier(ObjectMap.SourceName));

                    AppendPrimaryKeyWhere(sb);

                    sql = sb.ToString();

                    _cache.Add(key, sql);
                }
            }

            results.SqlQuery = sql;
            results.Parameters = GetDeleteParameters(data);

            return results;
        }

        private IDictionary<string, IParameter> GetDeleteParameters(object data)
        {
            IEnumerable<PropertyMap> properties = ObjectMap.Properties.Where(x => x.IsPrimaryKey);

            AppendParameters(properties, data);

            return Parameters;
        }

        #endregion

        #region Common Methods

        private void BeforeInsertListeners(T item)
        {
            if (Session.Listeners.Count > 0)
            {
                foreach (IListener listener in Session.Listeners)
                {
                    listener.BeforeInsert(item);
                }
            }
        }

        private void AfterInsertListeners(T item)
        {
            if (Session.Listeners.Count > 0)
            {
                foreach (IListener listener in Session.Listeners)
                {
                    listener.AfterInsert(item);
                }
            }
        }

        private void BeforeUpdateListeners(T item)
        {
            if (Session.Listeners.Count > 0)
            {
                foreach (IListener listener in Session.Listeners)
                {
                    listener.BeforeUpdate(item);
                }
            }
        }

        private void AfterUpdateListeners(T item)
        {
            if (Session.Listeners.Count > 0)
            {
                foreach (IListener listener in Session.Listeners)
                {
                    listener.AfterUpdate(item);
                }
            }
        }

        private void BeforeDeleteListeners(T item)
        {
            if (Session.Listeners.Count > 0)
            {
                foreach (IListener listener in Session.Listeners)
                {
                    listener.BeforeDelete(item);
                }
            }
        }

        private void AfterDeleteListeners(T item)
        {
            if (Session.Listeners.Count > 0)
            {
                foreach (IListener listener in Session.Listeners)
                {
                    listener.AfterDelete(item);
                }
            }
        }

        #endregion
    }
}