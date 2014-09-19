using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Yapper.Core
{
    //private static IColumnNameResolver _columnNameResolver = new DefaultColumnNameResolver();

    ///// <summary>
    ///// Sets the <see cref="T:Dommel.IColumnNameResolver"/> implementation for resolving column names.
    ///// </summary>
    ///// <param name="resolver">An instance of <see cref="T:Dommel.IColumnNameResolver"/>.</param>
    //public static void SetColumnNameResolver(IColumnNameResolver resolver)
    //{
    //    _columnNameResolver = resolver;
    //}

    /// <summary>
    /// Defines methods for resolving column names for entities. 
    /// Custom implementations can be registerd with <see cref="M:SetColumnNameResolver()"/>.
    /// </summary>
    public interface IColumnNameResolver
    {
        /// <summary>
        /// Resolves the column name for the specified property.
        /// </summary>
        /// <param name="propertyInfo">The property of the item.</param>
        /// <returns>The column name for the property.</returns>
        string ResolveColumnName(PropertyInfo propertyInfo);
    }

    /// <summary>
    /// Implements the <see cref="IColumnNameResolver"/>.
    /// </summary>
    sealed class DefaultColumnNameResolver : IColumnNameResolver
    {
        /// <summary>
        /// Resolves the column name for the property. This is just the name of the property.
        /// </summary>
        public string ResolveColumnName(PropertyInfo propertyInfo)
        {
            string name = propertyInfo.Name;

            ColumnAttribute ca = propertyInfo.GetCustomAttribute<ColumnAttribute>(true);

            if (ca != null)
            {
                name = ca.Name ?? name;
            }

            return name;
        }
    }
}
