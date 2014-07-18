using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Yamor.Builders
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Parameter : IParameter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public Parameter(object value, Type type)
        {
            Value = value;
            Type = type.ToDbType();
        }
        
        /// <summary>
        /// 
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DbType Type { get; set; }
    }
}
