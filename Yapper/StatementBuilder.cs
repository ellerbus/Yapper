using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using Augment;

namespace Yapper
{
    internal static class Extensions
    {
        public static string GetTableName(this Type type)
        {
            string table = type.Name;

            TableAttribute ta = type.GetCustomAttribute<TableAttribute>();

            if (ta != null)
            {
                table = ta.Name;
            }

            return table;
        }

        public static string GetColumnName(this PropertyInfo p)
        {
            string column = p.Name;

            ColumnAttribute ca = p.GetCustomAttribute<ColumnAttribute>();

            if (ca != null)
            {
                column = ca.Name;
            }

            return column;
        }

        public static bool IsColumn(this PropertyInfo p)
        {
            return Attribute.GetCustomAttribute(p, typeof(ColumnAttribute), true) != null;
        }

        public static bool IsPrimaryKey(this PropertyInfo p)
        {
            return Attribute.GetCustomAttribute(p, typeof(KeyAttribute), true) != null;
        }

        public static bool IsDatabaseGenerated(this PropertyInfo p)
        {
            return Attribute.GetCustomAttribute(p, typeof(DatabaseGeneratedAttribute), true) != null;
        }

        public static bool IsIdentity(this PropertyInfo p)
        {
            Attribute attr = Attribute.GetCustomAttribute(p, typeof(DatabaseGeneratedAttribute), true);

            DatabaseGeneratedAttribute dga = attr as DatabaseGeneratedAttribute;

            if (dga != null)
            {
                return dga.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity;
            }

            return false;
        }

        public static IList<PropertyInfo> GetColumnProperties(this Type type)
        {
            IList<PropertyInfo> properties = type
                .GetProperties()
                .Where(x => x.IsColumn())
                .ToList();

            return properties;
        }
    }

    /// <summary>
    ///
    /// </summary>
    public static class StatementBuilder
    {
        #region Members

        public static string P = "@";

        private static readonly object _lock = new object();

        private static readonly IDictionary<string, string> _statementCache = new Dictionary<string, string>();

        private static readonly string NL = "\n";
        private static readonly string S4 = new string(' ', 4);
        private static readonly string S8 = new string(' ', 8);
        private static readonly string S12 = new string(' ', 12);
        private static readonly string S16 = new string(' ', 16);

        #endregion

        #region Save SQL

        /// <summary>
        /// Generates the ONLY the Sql to insert into the database
        /// (typcially used with 'many' updates of individual objects and 'foreach' loop)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>SQL</returns>
        public static string SaveOne<T>()
        {
            Type type = typeof(T);

            string key = "V:1:" + typeof(T).FullName;

            string sql = null;

            if (!_statementCache.TryGetValue(key, out sql))
            {
                IList<PropertyInfo> properties = type.GetColumnProperties().ToList();

                string table = type.GetTableName();

                StringBuilder sqlb = new StringBuilder($"begin tran");

                sqlb
                    .Append(SaveUpdate<T>(table, properties))
                    .Append($"{NL}{S4}if @@rowcount = 0")
                    .Append($"{NL}{S4}begin")
                    .Append(SaveInsert<T>(table, properties))
                    .Append($"{NL}{S4}end");

                PropertyInfo ident = properties.FirstOrDefault(x => x.IsIdentity());

                if (ident != null)
                {
                    sqlb.Append($"{NL}{S4}select  {P}{ident.Name}");
                }

                sqlb.Append($"{NL}commit tran");

                sql = sqlb.ToString();

                _statementCache[key] = sql;
            }

            return sql;
        }

        private static string SaveInsert<T>(string table, IList<PropertyInfo> properties)
        {
            string cols = properties
                .Where(x => !x.IsDatabaseGenerated())
                .Select(x => x.GetColumnName())
                .Join($",{NL}{S12}");

            string parms = properties
                .Where(x => !x.IsDatabaseGenerated())
                .Select(x => P + x.Name)
                .Join($",{NL}{S12}");

            StringBuilder sqlb = new StringBuilder($"{NL}{S8}insert into {table}")
                .Append($"{NL}{S8}(")
                .Append($"{NL}{S12}" + cols)
                .Append($"{NL}{S8})")
                .Append($"{NL}{S8}values")
                .Append($"{NL}{S8}(")
                .Append($"{NL}{S12}" + parms)
                .Append($"{NL}{S8})");

            PropertyInfo ident = properties.FirstOrDefault(x => x.IsIdentity());

            if (ident != null)
            {
                sqlb.Append($"{NL}{S8}set {P}{ident.Name} = ident_current('dbo.{table}')");
            }

            return sqlb.ToString();
        }

        private static string SaveUpdate<T>(string table, IList<PropertyInfo> properties)
        {
            StringBuilder sqlb = new StringBuilder($"{NL}{S4}update  {table} with (serializable)");

            string sets = properties
                .Where(x => !x.IsDatabaseGenerated())
                .Select(x => $"{x.GetColumnName()} = {P}{x.Name}")
                .Join($",{NL}{S12}");

            sqlb.Append($"{NL}{S4}set {S4}{sets}")
                .Append(GetWhere(typeof(T), null, S4));

            return sqlb.ToString();
        }

        #endregion

        #region Insert SQL

        /// <summary>
        /// Generates the ONLY the Sql to insert into the database
        /// (typcially used with 'many' updates of individual objects and 'foreach' loop)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>SQL</returns>
        public static string InsertOne<T>()
        {
            Type type = typeof(T);

            string key = "I:1:" + typeof(T).FullName;

            string sql = null;

            if (!_statementCache.TryGetValue(key, out sql))
            {
                IList<PropertyInfo> properties = type.GetColumnProperties().ToList();

                string table = type.GetTableName();

                string cols = properties
                    .Where(x => !x.IsDatabaseGenerated())
                    .Select(x => x.GetColumnName())
                    .Join($",{NL}{S8}");

                string parms = properties
                    .Where(x => !x.IsDatabaseGenerated())
                    .Select(x => P + x.Name)
                    .Join($",{NL}{S8}");

                StringBuilder sqlb = new StringBuilder($"insert into {table}")
                    .Append($"{NL}{S4}(")
                    .Append($"{NL}{S8}" + cols)
                    .Append($"{NL}{S4})")
                    .Append($"{NL}{S4}values")
                    .Append($"{NL}{S4}(")
                    .Append($"{NL}{S8}" + parms)
                    .Append($"{NL}{S4})");

                if (properties.Any(x => x.IsDatabaseGenerated()))
                {
                    sqlb.Append($"{NL}select ident_current('dbo.{table}')");
                }

                sql = sqlb.ToString();

                _statementCache[key] = sql;
            }

            return sql;
        }

        #endregion

        #region Update SQL

        /// <summary>
        /// Generates the ONLY the Sql to update the database
        /// (typcially used with 'many' updates of individual objects and 'foreach' loop)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>SQL</returns>
        public static string UpdateOne<T>()
        {
            Type type = typeof(T);

            string key = "U:1:" + typeof(T).FullName;

            string sql = null;

            if (!_statementCache.TryGetValue(key, out sql))
            {
                IList<PropertyInfo> properties = type.GetColumnProperties().ToList();

                string table = type.GetTableName();

                StringBuilder sqlb = new StringBuilder($"update  {table}");

                string sets = properties
                    .Where(x => !x.IsDatabaseGenerated())
                    .Select(x => $"{x.GetColumnName()} = {P}{x.Name}")
                    .Join($",{NL}{S8}");

                sqlb.Append($"{NL}set {S4}{sets}")
                    .Append(GetWhere(type, null));

                sql = sqlb.ToString();

                _statementCache[key] = sql;
            }

            return sql;
        }

        #endregion

        #region Delete SQL

        /// <summary>
        /// Generates the ONLY the Sql to delete from the database
        /// (typcially used with 'many' deletes of individual objects and 'foreach' loop)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>SQL</returns>
        public static string DeleteOne<T>()
        {
            Type type = typeof(T);

            string key = "D:1:" + typeof(T).FullName;

            string sql = null;

            if (!_statementCache.TryGetValue(key, out sql))
            {
                string table = type.GetTableName();

                StringBuilder sqlb = new StringBuilder($"delete{NL}from{S4}{table}")
                    .Append(GetWhere(type, null));

                sql = sqlb.ToString();

                _statementCache[key] = sql;
            }

            return sql;
        }

        #endregion

        #region Select SQL

        /// <summary>
        /// Generates the ONLY the Sql to select from the database
        /// (typcially used with 'many' deletes of individual objects and 'foreach' loop)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>SQL</returns>
        public static string SelectOne<T>(object where = null)
        {
            Type type = typeof(T);

            string key = "S:1:" + typeof(T).FullName;

            if (where != null)
            {
                key += "+" + where.GetType().FullName;
            }

            string sql = null;

            if (!_statementCache.TryGetValue(key, out sql))
            {
                string table = type.GetTableName();

                IList<PropertyInfo> properties = type.GetColumnProperties().ToList();

                string cols = properties
                    .Where(x => !x.IsDatabaseGenerated())
                    .Select(x => $"{x.GetColumnName()} {x.Name}")
                    .Join($",{NL}{S8}");

                StringBuilder sqlb = new StringBuilder($"select  {cols}{NL}from{S4}{table}")
                    .Append(GetWhere(type, where));

                sql = sqlb.ToString();

                _statementCache[key] = sql;
            }

            return sql;
        }

        /// <summary>
        /// Generates the ONLY the Sql to select from the database
        /// (typcially used with 'many' deletes of individual objects and 'foreach' loop)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>SQL</returns>
        public static string SelectMany<T>(object where = null)
        {
            Type type = typeof(T);

            string key = "S:*:" + typeof(T).FullName;

            if (where != null)
            {
                key += "+" + where.GetType().FullName;
            }

            string sql = null;

            if (!_statementCache.TryGetValue(key, out sql))
            {
                string table = type.GetTableName();

                IList<PropertyInfo> properties = type.GetColumnProperties().ToList();

                string cols = properties
                    .Where(x => !x.IsDatabaseGenerated())
                    .Select(x => $"{x.GetColumnName()} {x.Name}")
                    .Join($",{NL}{S8}");

                StringBuilder sqlb = new StringBuilder($"select  {cols}{NL}from{S4}{table}");

                if (where != null)
                {
                    sqlb.Append(GetWhere(type, where));
                }

                sql = sqlb.ToString();

                _statementCache[key] = sql;
            }

            return sql;
        }

        #endregion

        #region Helpers

        private static string GetWhere(Type mappedType, object where, string pad = "")
        {
            StringBuilder sql = new StringBuilder();

            IList<PropertyInfo> properties = mappedType.GetColumnProperties();

            if (where == null || mappedType == where.GetType())
            {
                foreach (PropertyInfo p in properties.Where(x => x.IsPrimaryKey()))
                {
                    sql.AppendIf(sql.Length > 0, $"{NL}{pad}  and   ")
                        .Append(p.GetColumnName())
                        .Append(" = ")
                        .Append(P)
                        .Append(p.Name);
                }
            }
            else
            {
                foreach (PropertyInfo pi in where.GetType().GetProperties())
                {
                    PropertyInfo p = properties.First(x => x.Name.IsSameAs(pi.Name));

                    sql.AppendIf(sql.Length > 0, $"{NL}{pad}  and   ")
                        .Append(p.GetColumnName())
                        .Append(" = ")
                        .Append(P)
                        .Append(p.Name);
                }
            }

            return $"{NL}{pad}where   " + sql.ToString();
        }

        #endregion
    }
}