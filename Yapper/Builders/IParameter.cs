using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yamor.Builders
{
    /// <summary>
    /// 
    /// </summary>
    public interface IParameter
    {
        /// <summary>
        /// 
        /// </summary>
        object Value { get; }
        /// <summary>
        /// 
        /// </summary>
        DbType Type { get; }
    }
}
