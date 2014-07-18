using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yapper.Mappers
{
    /// <summary>
    /// Collection of object maps, keyed by Type
    /// </summary>
    public sealed class ObjectMapCollection
    {
        #region Members

        static ObjectMapCollection()
        {
            Default = new ObjectMapCollection();
        }

        private static readonly object _lock = new object();

        private Dictionary<Type, ObjectMap> _maps = new Dictionary<Type, ObjectMap>();

        #endregion

        #region Constructors

        private ObjectMapCollection()
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ObjectMap GetMapFor<T>() where T : class
        {
            return GetMapFor(typeof(T));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ObjectMap GetMapFor(Type type)
        {
            lock (_lock)
            {
                ObjectMap map = null;

                if (!_maps.TryGetValue(type, out map))
                {
                    map = new ObjectMap(type);

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
        public static ObjectMapCollection Default { get; private set; }

        #endregion
    }
}