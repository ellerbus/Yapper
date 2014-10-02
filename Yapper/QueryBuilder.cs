using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using Augment;
using Dapper;
using EnsureThat;
using UniqueNamespace;
using UniqueNamespace.Dapper;
using Yapper.Core;

namespace Yapper
{
    static class Extensions
    {
        public static bool IsPrimaryKey(this PropertyInfo p)
        {
            return p.GetCustomAttribute<KeyAttribute>() != null;
        }
        public static bool IsDatabaseGenerated(this PropertyInfo p)
        {
            return p.GetCustomAttribute<DatabaseGeneratedAttribute>() != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class SqlBuilderExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="b"></param>
        /// <returns></returns>
        public static SqlBuilder From<T>(this SqlBuilder b)
        {
            b.From(MapperHelper.GetTableName(typeof(T)));

            return b;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class QueryBuilder
    {
        #region Members

        private static readonly IDictionary<int, string> _sqlCache = new Dictionary<int, string>();

        class BuilderResults : IBuilderResults
        {
            public BuilderResults(string sql, object parameters = null)
            {
                Sql = sql;
                Parameters = parameters;
            }
            public string Sql { get; private set; }
            public object Parameters { get; private set; }
        }

        #endregion

        #region Insert

        /// <summary>
        /// Generates the ONLY the Sql to insert into the database
        /// (typcially used with 'many' updates of individual objects and 'foreach' loop)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>SQL</returns>
        public static string Insert<T>()
        {
            Type type = typeof(T);

            int key = CreateKey("I", typeof(T), null);

            string sql = null;

            if (!_sqlCache.TryGetValue(key, out sql))
            {
                StringBuilder sqlb = new StringBuilder("insert into ");

                sqlb.Append(MapperHelper.GetTableName(type)).Append(GetInsert(type, null));

                sql = sqlb.ToString();

                _sqlCache[key] = sql;
            }

            return sql;
        }

        /// <summary>
        /// Generates the Sql to insert an item into the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item to insert</param>
        /// <returns>SQL</returns>
        public static IBuilderResults Insert<T>(T item)
        {
            Ensure.That(item != null, "item").IsTrue();

            Type type = typeof(T);

            int key = CreateKey("I", typeof(T), item.GetType());

            string sql = null;

            if (!_sqlCache.TryGetValue(key, out sql))
            {
                StringBuilder sqlb = new StringBuilder("insert into ");

                sqlb.Append(MapperHelper.GetTableName(type)).Append(GetInsert(type, item));

                sql = sqlb.ToString();

                _sqlCache[key] = sql;
            }

            return new BuilderResults(sql, item);
        }

        /// <summary>
        /// Generates the Sql to insert an item into the database (only using selected fields from anonymous object)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The object to insert</param>
        /// <returns>SQL</returns>
        public static IBuilderResults Insert<T>(object data)
        {
            Ensure.That(data, "data").IsNotNull();

            Type type = typeof(T);

            int key = CreateKey("I", typeof(T), data.GetType());

            string sql = null;

            if (!_sqlCache.TryGetValue(key, out sql))
            {
                StringBuilder sqlb = new StringBuilder("insert into ");

                sqlb.Append(MapperHelper.GetTableName(type)).Append(GetInsert(type, data));

                sql = sqlb.ToString();

                _sqlCache[key] = sql;
            }

            return new BuilderResults(sql, data);
        }

        #endregion

        #region Update SQL

        /// <summary>
        /// Generates the ONLY the Sql to update the database
        /// (typcially used with 'many' updates of individual objects and 'foreach' loop)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>SQL</returns>
        public static string Update<T>()
        {
            Type type = typeof(T);

            int key = CreateKey("U", typeof(T), null);

            string sql = null;

            if (!_sqlCache.TryGetValue(key, out sql))
            {
                StringBuilder sqlb = new StringBuilder("update ");

                sqlb.Append(MapperHelper.GetTableName(type))
                    .Append(GetSet(type, null))
                    .Append(GetWhere(type, null));

                sql = sqlb.ToString();

                _sqlCache[key] = sql;
            }

            return sql;
        }

        /// <summary>
        /// Generates the Sql to update an item from the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item to update</param>
        /// <returns>SQL</returns>
        public static IBuilderResults Update<T>(T item)
        {
            Ensure.That(item != null, "item").IsTrue();

            Type type = typeof(T);

            int key = CreateKey("U", typeof(T), item.GetType());

            string sql = null;

            if (!_sqlCache.TryGetValue(key, out sql))
            {
                StringBuilder sqlb = new StringBuilder("update ");

                sqlb.Append(MapperHelper.GetTableName(type))
                    .Append(GetSet(type, item))
                    .Append(GetWhere(type, item));

                sql = sqlb.ToString();

                _sqlCache[key] = sql;
            }

            return new BuilderResults(sql, item);
        }

        /// <summary>
        /// Generates the Sql to update an item from the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="set">The set clause</param>
        /// <param name="where">The where clause</param>
        /// <returns>SQL</returns>
        public static IBuilderResults Update<T>(object set, object where)
        {
            Ensure.That(set, "set").IsNotNull();

            Type type = typeof(T);

            int key = CreateKey("U", typeof(T), set.GetType(), where == null ? null : where.GetType());

            string sql = null;

            if (!_sqlCache.TryGetValue(key, out sql))
            {
                StringBuilder sqlb = new StringBuilder("update ");

                if (where != null)
                {
                    HashSet<string> setProperties = new HashSet<string>(set.GetType().GetProperties().Select(x => x.Name));
                    HashSet<string> whereProperties = new HashSet<string>(where.GetType().GetProperties().Select(x => x.Name));

                    //  if any where is in the set - NOGO
                    if (whereProperties.Any(x => setProperties.Contains(x)))
                    {
                        throw new InvalidOperationException("The 'where' condition contains a Property that is in the 'set' condition.  Properties must be unique to each condition.");
                    }
                }

                sqlb.Append(MapperHelper.GetTableName(type)).Append(GetSet(type, set));

                if (where != null)
                {
                    sqlb.Append(GetWhere(type, where));
                }

                sql = sqlb.ToString();

                _sqlCache[key] = sql;
            }

            if (where == null)
            {
                return new BuilderResults(sql, set);
            }
            
            DynamicParameters parms = new DynamicParameters(set);

            parms.AddDynamicParams(where);

            return new BuilderResults(sql, parms);
        }

        #endregion

        #region Delete SQL

        /// <summary>
        /// Generates the ONLY the Sql to delete from the database
        /// (typcially used with 'many' deletes of individual objects and 'foreach' loop)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>SQL</returns>
        public static string Delete<T>()
        {
            Type type = typeof(T);

            int key = CreateKey("D", typeof(T), null);

            string sql = null;

            if (!_sqlCache.TryGetValue(key, out sql))
            {
                StringBuilder sqlb = new StringBuilder("delete from ");

                sqlb.Append(MapperHelper.GetTableName(type));

                sql = sqlb.ToString();

                _sqlCache[key] = sql;
            }

            return sql;
        }

        /// <summary>
        /// Generates the Sql to delete an item from the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item to delete</param>
        /// <returns>SQL</returns>
        public static IBuilderResults Delete<T>(T item)
        {
            Ensure.That(item != null, "item").IsTrue();

            Type type = typeof(T);

            int key = CreateKey("D", typeof(T), item.GetType());

            string sql = null;

            if (!_sqlCache.TryGetValue(key, out sql))
            {
                StringBuilder sqlb = new StringBuilder("delete from ");

                sqlb.Append(MapperHelper.GetTableName(type)).Append(GetWhere(type, item));

                sql = sqlb.ToString();

                _sqlCache[key] = sql;
            }

            return new BuilderResults(sql, item);
        }

        /// <summary>
        /// Generates the Sql to delete from the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where">Conditions to delete</param>
        /// <returns>SQL</returns>
        public static IBuilderResults Delete<T>(object where)
        {
            Ensure.That(where, "where").IsNotNull();

            Type type = typeof(T);

            int key = CreateKey("D", typeof(T), where.GetType());

            string sql = null;

            if (!_sqlCache.TryGetValue(key, out sql))
            {
                StringBuilder sqlb = new StringBuilder("delete from ");

                sqlb.Append(MapperHelper.GetTableName(type)).Append(GetWhere(type, where));

                sql = sqlb.ToString();

                _sqlCache[key] = sql;
            }

            return new BuilderResults(sql, where);
        }

        #endregion

        #region Select SQL

        /// <summary>
        /// Generates the ONLY the Sql to select from the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>SQL</returns>
        public static string Select<T>()
        {
            Type type = typeof(T);

            int key = CreateKey("S", typeof(T), null);

            string sql = null;

            if (!_sqlCache.TryGetValue(key, out sql))
            {
                StringBuilder sqlb = new StringBuilder("select * from ");

                sqlb.Append(MapperHelper.GetTableName(type));

                sql = sqlb.ToString();

                _sqlCache[key] = sql;
            }

            return sql;
        }

        /// <summary>
        /// Generates the Sql to select from the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where">Conditions to select</param>
        /// <returns>SQL</returns>
        public static IBuilderResults Select<T>(object where)
        {
            Ensure.That(where, "where").IsNotNull();

            Type type = typeof(T);

            int key = CreateKey("S", typeof(T), where.GetType());

            string sql = null;

            if (!_sqlCache.TryGetValue(key, out sql))
            {
                StringBuilder sqlb = new StringBuilder("select * from ");

                sqlb.Append(MapperHelper.GetTableName(type)).Append(GetWhere(type, where));

                sql = sqlb.ToString();

                _sqlCache[key] = sql;
            }

            return new BuilderResults(sql, where);
        }

        #endregion

        #region Helpers

        private static int CreateKey(string prefix, Type t1, Type t2, Type t3 = null)
        {
            int hash = 17;

            hash = hash * 23 + prefix.GetHashCode();
            hash = hash * 23 + t1.FullName.GetHashCode();
            hash = hash * 23 + (t2 == null ? 57 : t2.FullName.GetHashCode());
            hash = hash * 23 + (t3 == null ? 113 : t3.FullName.GetHashCode());

            return hash;
        }

        private static string GetInsert(Type type, object item)
        {
            StringBuilder sql = new StringBuilder();

            IDictionary<string, PropertyInfo> columns = MapperHelper.GetColumnProperties(type);

            PropertyInfo[] properties = null;

            if (item == null || type == item.GetType())
            {
                //  NOT database generated
                properties = columns.Values
                    .Where(x => !x.IsDatabaseGenerated())
                    .ToArray();
            }
            else
            {
                properties = item.GetType().GetProperties()
                    .Where(x => columns.ContainsKey(x.Name) && !x.IsDatabaseGenerated())
                    .ToArray();
            }

            string cols = properties.Select(x => MapperHelper.GetColumnName(columns[x.Name])).Join(", ");

            string parms = properties.Select(x => "@{0}".FormatArgs(x.Name)).Join(", ");

            sql.Append(" (").Append(cols).Append(")")
                .Append(" values ")
                .Append("(").Append(parms).Append(")");

            return sql.ToString();
        }

        private static string GetSet(Type type, object set)
        {
            StringBuilder sql = new StringBuilder();

            IDictionary<string, PropertyInfo> columns = MapperHelper.GetColumnProperties(type);

            PropertyInfo[] properties = null;

            if (set == null || type == set.GetType())
            {
                //  NOT primary key
                properties = columns.Values
                    .Where(x => !x.IsPrimaryKey() && !x.IsDatabaseGenerated())
                    .ToArray();
            }
            else
            {
                properties = set.GetType().GetProperties()
                    .Where(x => columns.ContainsKey(x.Name) && !x.IsDatabaseGenerated())
                    .ToArray();
            }

            foreach (PropertyInfo p in properties)
            {
                sql.AppendIf(sql.Length > 0, ", ")
                    .Append(MapperHelper.GetColumnName(columns[p.Name]))
                    .AppendFormat(" = @{0}", p.Name);
            }

            return " set " + sql.ToString();
        }

        private static string GetWhere(Type mappedType, object where)
        {
            StringBuilder sql = new StringBuilder();

            IDictionary<string, PropertyInfo> columns = MapperHelper.GetColumnProperties(mappedType);

            PropertyInfo[] properties = null;

            if (where == null || mappedType == where.GetType())
            {
                properties = columns.Values.Where(x => x.IsPrimaryKey()).ToArray();
            }
            else
            {
                properties = where.GetType().GetProperties()
                    .Where(x => columns.ContainsKey(x.Name))
                    .ToArray();
            }

            foreach (PropertyInfo p in properties)
            {
                sql.AppendIf(sql.Length > 0, " and ")
                    .Append(MapperHelper.GetColumnName(columns[p.Name]))
                    .AppendFormat(" = @{0}", p.Name);
            }

            return " where " + sql.ToString();
        }

        #endregion
    }
}