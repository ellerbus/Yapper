using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Yapper.Core
{
    /// <summary>
    /// Defines methods for resolving table names of entities. 
    /// Custom implementations can be registerd with <see cref="M:SetTableNameResolver()"/>.
    /// </summary>
    public interface ITableNameResolver
    {
        /// <summary>
        /// Resolves the table name for the specified type.
        /// </summary>
        /// <param name="type">The type to resolve the table name for.</param>
        /// <returns>A string containing the resolved table name for for <paramref name="type"/>.</returns>
        string ResolveTableName(Type type);
    }

    /// <summary>
    /// Implements the <see cref="T:Dommel.ITableNameResolver"/> interface by resolving table names 
    /// by making the type name plural and removing the 'I' prefix for interfaces. 
    /// </summary>
    sealed class DefaultTableNameResolver : ITableNameResolver
    {
        /// <summary>
        /// Resolves the table name by using type.Name or searching for <see cref="TableAttribute"/>
        /// </summary>
        /// <param name="type"></param>
        public string ResolveTableName(Type type)
        {
            string name = type.Name;

            TableAttribute ta = type.GetCustomAttribute<TableAttribute>(true);

            if (ta != null)
            {
                name = ta.Name ?? name;
            }

            return name;
        }
    }
}