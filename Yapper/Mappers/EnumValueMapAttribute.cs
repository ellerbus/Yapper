using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yapper.Mappers
{
    /// <summary>
    /// Applied to enum fields to convert related dataobject property values
    /// being stored in a DB (ie. Status.Active can be "A" or 0)
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class EnumValueMapAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public EnumValueMapAttribute(object value)
        {
            Value = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public object Value { get; private set; }
    }
}
