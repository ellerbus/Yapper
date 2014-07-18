using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yapper.Mappers
{
    /// <summary>
    /// Collection of enum maps
    /// </summary>
    public sealed class EnumMapCollection
    {
        #region Members

        static EnumMapCollection()
        {
            Default = new EnumMapCollection();
        }

        private static readonly object _lock = new object();

        private Dictionary<Type, ValueMap> _maps = new Dictionary<Type, ValueMap>();

        #endregion

        #region Constructors

        private EnumMapCollection()
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ValueMap GetMapFor<T>() where T : class
        {
            return GetMapFor(typeof(T));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ValueMap GetMapFor(Type type)
        {
            lock (_lock)
            {
                ValueMap map = null;

                if (!_maps.TryGetValue(type, out map))
                {
                    map = new ValueMap(type);

                    _maps.Add(type, map);
                }

                return map;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public static EnumMapCollection Default { get; private set; }

        #endregion
    }
}