using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yamor.Builders
{
    /// <summary>
    /// The interface for a class which produces the builder results
    /// </summary>
    public interface IQueryBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IBuilderResults BuildQuery();
    }
}
