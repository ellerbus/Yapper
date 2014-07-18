using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yamor.Builders
{
    /// <summary>
    /// The interface for a class which builds queries for persistence (basically think CRUD'ing)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPersistenceBuilder<T> where T : class
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool Insert(T item);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool Update(T item);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool Delete(T item);
    }
}
