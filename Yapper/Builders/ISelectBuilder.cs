using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Yamor.Builders
{
    /// <summary>
    /// The interface for a class which builds queries for selects
    /// </summary>
    public interface ISelectBuilder<T> : IQueryBuilder where T : class, new()
    {
        /// <summary>
        /// Executes the requested operation
        /// </summary>
        int Count();

        /// <summary>
        /// Executes the requested operation
        /// </summary>
        IList<T> ToList();

        /// <summary>
        /// Executes the requested operation
        /// </summary>
        /// <param name="page">1 based indexing</param>
        /// <param name="itemsPerPage"></param>
        /// <returns></returns>
        IPagedList<T> ToPagedList(int page, int itemsPerPage);

        /// <summary>
        /// Executes the requested operation
        /// </summary>
        T FirstOrDefault();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="top"></param>
        /// <returns></returns>
        ISelectBuilder<T> Limit(int top);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="field"></param>
        /// <returns></returns>
        ISelectBuilder<T> OrderBy<TField>(Expression<Func<T, TField>> field);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="field"></param>
        /// <returns></returns>
        ISelectBuilder<T> ThenBy<TField>(Expression<Func<T, TField>> field);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="field"></param>
        /// <returns></returns>
        ISelectBuilder<T> OrderByDescending<TField>(Expression<Func<T, TField>> field);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="field"></param>
        /// <returns></returns>
        ISelectBuilder<T> ThenByDescending<TField>(Expression<Func<T, TField>> field);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        ISelectBuilder<T> Where(object values);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        ISelectBuilder<T> Where(Expression<Func<T, bool>> where);
    }
}