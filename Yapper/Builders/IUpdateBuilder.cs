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
    public interface IUpdateBuilder<T> where T : class
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        IUpdateBuilder<T> Set(object values);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        IUpdateBuilder<T> Set<TField>(Expression<Func<T, TField>> field, object value);
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="field"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        IUpdateBuilder<T> Set<TField, TValue>(Expression<Func<T, TField>> field, Expression<Func<T, TValue>> expression);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        IUpdateBuilder<T> Where(object values);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        IUpdateBuilder<T> Where(Expression<Func<T, bool>> where);
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int Execute();
    }
}
