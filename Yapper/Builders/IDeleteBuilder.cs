using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Yamor.Builders
{
    /// <summary>
    /// The interface for a class which builds queries for updates
    /// </summary>
    public interface IDeleteBuilder<T> where T : class
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        IDeleteBuilder<T> Where(object values);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        IDeleteBuilder<T> Where(Expression<Func<T, bool>> where);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int Execute();
    }
}
