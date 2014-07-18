using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Yamor.Builders
{
    /// <summary>
    /// A class which contains the result of a paged query.
    /// </summary>
    /// <typeparam name="T">The type of object the contained in the results.</typeparam>
    [DebuggerDisplay("Page={Page},TotalPages={TotalPages},PerPage={ResultsPerPage},TotalItems={TotalResults}")]
    public sealed class PagedList<T> : IPagedList<T>
    {
        #region IPagedList<T> Members

        /// <summary>
        /// 
        /// </summary>
        public IList<T> Items { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public int Page { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public int ItemsPerPage { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public int TotalItems { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public int TotalPages { get { return ((TotalItems - 1) / ItemsPerPage) + 1; } }

        #endregion
    }
}