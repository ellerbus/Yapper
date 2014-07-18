using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsureThat;

namespace Yapper.Mappers
{
    /// <summary>
    /// Applied to dataobject boolean properties to convert true|false to Y|N or other values
    /// being stored in a DB
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class BooleanValueMapAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="trueValue"></param>
        /// <param name="falseValue"></param>
        public BooleanValueMapAttribute(object trueValue, object falseValue)
        {
            Ensure.That(trueValue.GetType() == falseValue.GetType())
                .WithExtraMessageOf(() => "Invalid Boolean Map, Type Mismatch (true.GetType() != false.GetType())")
                .IsTrue()
                ;

            TrueValue = trueValue;
            FalseValue = falseValue;
        }

        /// <summary>
        /// 
        /// </summary>
        public object TrueValue { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public object FalseValue { get; private set; }
    }
}
