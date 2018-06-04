using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

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
}
