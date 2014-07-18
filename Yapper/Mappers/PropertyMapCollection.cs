using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Yapper.Mappers
{
    /// <summary>
    /// Collection of property maps, keyed by Property.Name
    /// </summary>
    public sealed class PropertyMapCollection : KeyedCollection<string, PropertyMap>
    {
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectMap"></param>
        /// <param name="optInProperties">Using annotations to denote property/columns</param>
        public PropertyMapCollection(ObjectMap objectMap, bool optInProperties)
        {
            ObjectMap = objectMap;

            IEnumerable<PropertyInfo> properties = ObjectMap.ObjectType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                ;

            foreach (PropertyInfo p in properties)
            {
                ColumnAttribute colattr = p.GetCustomAttribute<ColumnAttribute>(true);

                if (optInProperties && colattr == null)
                {
                    continue;
                }

                NotMappedAttribute notmapped = p.GetCustomAttribute<NotMappedAttribute>(true);

                if (!optInProperties && notmapped != null)
                {
                    continue;
                }

                Add(new PropertyMap(p, colattr));
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override string GetKeyForItem(PropertyMap item)
        {
            return item.Name;
        }

        #endregion

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public ObjectMap ObjectMap { get; private set; }

        #endregion
    }
}