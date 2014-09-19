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

namespace Yapper.Core
{
    /// <summary>
    /// 
    /// </summary>
    public static class QueryBuilder
    {
        #region Members

        private static readonly IDictionary<int, string> _sqlCache = new Dictionary<int, string>();

        #endregion

        #region SQL

        /// <summary>
        /// Generates the Sql to retrieve an item from the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where">The where clause</param>
        /// <returns>SQL</returns>
        public static string Select<T>(object where)
        {
            Type type = typeof(T);

            int key = CreateKey("S", typeof(T), where == null ? null : where.GetType());

            string sql = null;

            if (!_sqlCache.TryGetValue(key, out sql))
            {
                MapperHelper.InitializeTypeMap(type);

                StringBuilder sqlb = new StringBuilder("select * from ");

                sqlb.Append(MapperHelper.GetTableName(type)).Append(GetWhere(type, where));

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
        public static string Delete<T>(T item)
        {
            Type type = typeof(T);

            int key = CreateKey("D", typeof(T), item == null ? null : item.GetType());

            string sql = null;

            if (!_sqlCache.TryGetValue(key, out sql))
            {
                MapperHelper.InitializeTypeMap(type);

                StringBuilder sqlb = new StringBuilder("delete from ");

                sqlb.Append(MapperHelper.GetTableName(type)).Append(GetWhere(type, item));

                sql = sqlb.ToString();

                _sqlCache[key] = sql;
            }

            return sql;
        }

        /// <summary>
        /// Generates the Sql to delete an item from the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where">The id of the item in the database.</param>
        /// <returns>The item with the corresponding id.</returns>
        public static string Delete<T>(object where)
        {
            Type type = typeof(T);

            int key = CreateKey("D", typeof(T), where == null ? null : where.GetType());

            string sql = null;

            if (!_sqlCache.TryGetValue(key, out sql))
            {
                MapperHelper.InitializeTypeMap(type);

                StringBuilder sqlb = new StringBuilder("delete from ");

                sqlb.Append(MapperHelper.GetTableName(type)).Append(GetWhere(type, where));

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
        public static string Update<T>(T item)
        {
            Type type = typeof(T);

            int key = CreateKey("U", typeof(T), item == null ? null : item.GetType());

            string sql = null;

            if (!_sqlCache.TryGetValue(key, out sql))
            {
                MapperHelper.InitializeTypeMap(type);

                StringBuilder sqlb = new StringBuilder("update ");

                sqlb.Append(MapperHelper.GetTableName(type))
                    .Append(GetSet(type, item))
                    .Append(GetWhere(type, item));

                sql = sqlb.ToString();

                _sqlCache[key] = sql;
            }

            return sql;
        }

        /// <summary>
        /// Generates the Sql to update an item from the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="set">The set clause</param>
        /// <param name="where">The where clause</param>
        /// <returns>SQL</returns>
        public static string Update<T>(object set, object where)
        {
            Ensure.That(set, "set").IsNotNull();

            Type type = typeof(T);

            int key = CreateKey("U", typeof(T), set.GetType(), where == null ? null : where.GetType());

            string sql = null;

            if (!_sqlCache.TryGetValue(key, out sql))
            {
                MapperHelper.InitializeTypeMap(type);

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

                sqlb.Append(MapperHelper.GetTableName(type))
                    .Append(GetSet(type, set))
                    .Append(GetWhere(type, where));

                sql = sqlb.ToString();

                _sqlCache[key] = sql;
            }

            return sql;
        }

        /// <summary>
        /// Generates the Sql to insert an item from the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item to insert</param>
        /// <returns>SQL</returns>
        public static string Insert<T>(T item)
        {
            Type type = typeof(T);

            int key = CreateKey("I", typeof(T), item == null ? null : item.GetType());

            string sql = null;

            if (!_sqlCache.TryGetValue(key, out sql))
            {
                MapperHelper.InitializeTypeMap(type);

                StringBuilder sqlb = new StringBuilder("insert into ");

                sqlb.Append(MapperHelper.GetTableName(type)).Append(GetInsert(type, item));

                sql = sqlb.ToString();

                _sqlCache[key] = sql;
            }

            return sql;
        }

        /// <summary>
        /// Generates the Sql to insert an item from the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The id of the item in the database.</param>
        /// <returns>The item with the corresponding id.</returns>
        public static string Insert<T>(object item)
        {
            Ensure.That(item, "item").IsNotNull();

            Type type = typeof(T);

            int key = CreateKey("I", typeof(T), item.GetType());

            string sql = null;

            if (!_sqlCache.TryGetValue(key, out sql))
            {
                MapperHelper.InitializeTypeMap(type);

                StringBuilder sqlb = new StringBuilder("insert into ");

                sqlb.Append(MapperHelper.GetTableName(type)).Append(GetInsert(type, item));

                sql = sqlb.ToString();

                _sqlCache[key] = sql;
            }

            return sql;
        }

        private static int CreateKey(string prefix, Type t1, Type t2, Type t3 = null)
        {
            int hash = 17;

            hash = hash * 23 + prefix.GetHashCode();
            hash = hash * 23 + t1.FullName.GetHashCode();
            hash = hash * 23 + (t2 == null ? 1 : t2.FullName.GetHashCode());
            hash = hash * 23 + (t3 == null ? 1 : t3.FullName.GetHashCode());

            return hash;
        }

        #endregion

        #region Helpers

        private static string GetInsert(Type type, object item)
        {
            Ensure.That(item, "item").IsNotNull();

            StringBuilder sql = new StringBuilder();

            IDictionary<string, PropertyInfo> columns =MapperHelper. GetColumnProperties(type);

            PropertyInfo[] properties = null;

            if (type == item.GetType())
            {
                //  NOT database generated
                properties = columns.Values
                    .Where(x => x.GetCustomAttribute<DatabaseGeneratedAttribute>() == null)
                    .ToArray();
            }
            else
            {
                properties = item.GetType().GetProperties()
                    .Where(x => columns.ContainsKey(x.Name) && x.GetCustomAttribute<DatabaseGeneratedAttribute>() == null)
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
            Ensure.That(set, "set").IsNotNull();

            StringBuilder sql = new StringBuilder();

            IDictionary<string, PropertyInfo> columns = MapperHelper.GetColumnProperties(type);

            PropertyInfo[] properties = null;

            if (type == set.GetType())
            {
                //  NOT primary key
                properties = columns.Values
                    .Where(x => x.GetCustomAttribute<KeyAttribute>() == null && x.GetCustomAttribute<DatabaseGeneratedAttribute>() == null)
                    .ToArray();
            }
            else
            {
                properties = set.GetType().GetProperties()
                    .Where(x => columns.ContainsKey(x.Name) && x.GetCustomAttribute<DatabaseGeneratedAttribute>() == null)
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

        private static string GetWhere(Type type, object where)
        {
            if (where == null)
            {
                return "";
            }

            StringBuilder sql = new StringBuilder();

            IDictionary<string, PropertyInfo> columns = MapperHelper.GetColumnProperties(type);

            PropertyInfo[] properties = null;

            if (type == where.GetType())
            {
                //  primary key ONLY
                properties = columns.Values
                    .Where(x => x.GetCustomAttribute<KeyAttribute>() != null)
                    .ToArray();
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