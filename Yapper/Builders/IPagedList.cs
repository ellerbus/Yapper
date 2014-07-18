using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yamor.Builders
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPagedList<T>
    {
        /// <summary>
        /// Gets the items in the page.
        /// </summary>
        IList<T> Items { get; }

        /// <summary>
        /// Gets the page number for the items.
        /// </summary>
        int Page { get; }

        /// <summary>
        /// Gets the number of items per page.
        /// </summary>
        int ItemsPerPage { get; }

        /// <summary>
        /// Gets the total number of pages for the query.
        /// </summary>
        int TotalPages { get; }

        /// <summary>
        /// Gets the total number of items for the query.
        /// </summary>
        int TotalItems { get; }
    }
}
