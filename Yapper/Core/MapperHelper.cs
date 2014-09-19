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
    static class MapperHelper
    {
        #region Members

        private static readonly HashSet<Type> _initialized = new HashSet<Type>();
        private static readonly IDictionary<Type, string> _tableNameCache = new Dictionary<Type, string>();
        private static readonly IDictionary<string, string> _columnNameCache = new Dictionary<string, string>();
        private static readonly IDictionary<Type, IDictionary<string, PropertyInfo>> _propertyCache = new Dictionary<Type, IDictionary<string, PropertyInfo>>();

        #endregion

        #region Helper Methods

        public static string GetTableName(Type type)
        {
            string name = null;

            lock (_tableNameCache)
            {
                if (!_tableNameCache.TryGetValue(type, out name))
                {
                    name = new DefaultTableNameResolver().ResolveTableName(type);

                    _tableNameCache[type] = name;
                }
            }

            return name;
        }

        public static string GetColumnName(PropertyInfo property)
        {
            string key = "{0}.{1}".FormatArgs(property.DeclaringType.FullName, property.Name);

            string name = null;

            lock (_columnNameCache)
            {
                if (!_columnNameCache.TryGetValue(key, out name))
                {
                    name = new DefaultColumnNameResolver().ResolveColumnName(property);

                    _columnNameCache[key] = name;
                }
            }

            return name;
        }

        public static IDictionary<string, PropertyInfo> GetColumnProperties(Type type)
        {
            IDictionary<string, PropertyInfo> properties = null;

            lock (_propertyCache)
            {
                if (!_propertyCache.TryGetValue(type, out properties))
                {
                    properties = type.GetProperties()
                        .Where(x => x.GetCustomAttribute<ColumnAttribute>() != null)
                        .ToDictionary(x => x.Name);

                    _propertyCache.Add(type, properties);
                }
            }

            return _propertyCache[type];
        }

        public static void InitializeTypeMap(Type type)
        {
            lock (_initialized)
            {
                if (!_initialized.Contains(type))
                {
                    IDictionary<string, PropertyInfo> properties = GetColumnProperties(type);

                    SqlMapper.ITypeMap originalMap = SqlMapper.GetTypeMap(type);

                    CustomTypeMap map = new CustomTypeMap(type, originalMap);

                    foreach (var item in properties)
                    {
                        map.MapColumn(GetColumnName(item.Value), item.Value.Name);
                    }

                    SqlMapper.SetTypeMap(map.Type, map);
                }
            }
        }

        #endregion
    }
}