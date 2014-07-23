using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yapper.Mappers
{
    /// <summary>
    /// Applied to enum fields to convert related dataobject property values
    /// being stored in a DB (ie. Status.Active = "A")
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class EnumMapAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public EnumMapAttribute(string value)
        {
            Value = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Value { get; private set; }
    }
}
