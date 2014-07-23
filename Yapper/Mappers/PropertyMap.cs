using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using EnsureThat;
using Augment;
using System.Diagnostics;

namespace Yapper.Mappers
{
    /// <summary>
    /// Used to map a property to a table.column
    /// </summary>
    [DebuggerDisplay("Name={Name}")]
    public sealed class PropertyMap
    {
        #region Members

        private Func<object, object> _getter;
        private Action<object, object> _setter;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="col"></param>
        public PropertyMap(PropertyInfo p, ColumnAttribute col)
        {
            Ensure.That(p).IsNotNull();

            ObjectProperty = p;

            if (col == null)
            {
                SourceName = p.Name;
            }
            else
            {
                SourceName = col.Name ?? p.Name;
            }

            EvaluatePrimaryKey();
            EvaluateValueMapping();
            EvaluateDatabaseGenerated();

            MaxLengthAttribute len = p.GetCustomAttribute<MaxLengthAttribute>(true);

            if (len != null)
            {
                MaxLength = len.Length;
            }

            if (IsIdentity && !IsPrimaryKey)
            {
                string msg = "When using an Identity it but be a Primary Key {0}::{1}"
                    .FormatArgs(ObjectProperty.DeclaringType.FullName, ObjectProperty.Name)
                    ;

                throw new InvalidOperationException(msg);
            }

            _setter = CreateSetterDelegate(p);
            _getter = CreateGetterDelegate(p);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public PropertyMap(PropertyInfo p)
            : this(p, p.GetCustomAttribute<ColumnAttribute>(true))
        {
        }

        #endregion

        #region Evaluations

        private void EvaluatePrimaryKey()
        {
            KeyAttribute key = ObjectProperty.GetCustomAttribute<KeyAttribute>(true);

            IsPrimaryKey = key != null;
        }

        private void EvaluateValueMapping()
        {
            if (ObjectProperty.PropertyType == typeof(string))
            {
                IsNullable = true;
            }
            else if (ObjectProperty.PropertyType.IsGenericType && ObjectProperty.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                IsNullable = true;
            }

            if (ObjectProperty.PropertyType.IsEnum)
            {
                EnumMap = EnumMapCollection.Default.GetMapFor(ObjectProperty.PropertyType);
            }
        }

        private void EvaluateDatabaseGenerated()
        {
            HasPropertySetter = ObjectProperty.SetMethod != null;

            DatabaseGeneratedAttribute dbgen = ObjectProperty.GetCustomAttribute<DatabaseGeneratedAttribute>(true);

            if (dbgen != null)
            {
                IsDatabaseGenerated = true;

                IsIdentity = dbgen.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity;

                IsComputed = dbgen.DatabaseGeneratedOption == DatabaseGeneratedOption.Computed;
            }
        }

        #endregion

        #region Reflection Methods

        private static Func<object, object> CreateGetterDelegate(PropertyInfo info)
        {
            // This may seem like a lot of nasty complexity, but a simple release-mode test shows that using a delegate like
            // this is over 300 times faster than calling PropertyInfo.GetValue().  Doing this via generating IL is actually
            // simpler than trying to do it by specialising generic methods (e.g. using ReflectionUtil.CreateStaticMethodDelegate).
            // See the TypeDataTest.PerformanceComparison for a comparison.
            MethodInfo getMethod = info.GetGetMethod();
            
            if (getMethod == null)
            {
                return null;
            }

            // Generate a dynamic method that simply does "return getMethod(obj)", boxing if necessary
            DynamicMethod dynamicMethod = CreateDynamicMethod(info.DeclaringType, typeof(object), new Type[] { typeof(object) });
           
            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
           
            ilGenerator.Emit(OpCodes.Ldarg_0);              // Push the object onto the stack
            ilGenerator.Emit(OpCodes.Call, getMethod);      // Call the getter
           
            if (info.PropertyType.IsValueType)
            {
                // Need to box the result
                ilGenerator.Emit(OpCodes.Box, info.PropertyType);
            }
         
            ilGenerator.Emit(OpCodes.Ret);

            Func<object, object> getter = (Func<object, object>)dynamicMethod.CreateDelegate(typeof(Func<object, object>));
           
            return getter;
        }
        
        private static Action<object, object> CreateSetterDelegate(PropertyInfo info)
        {
            MethodInfo setMethod = info.GetSetMethod();
            
            if (setMethod == null)
            {
                return null;
            }
            
            // Generate a dynamic method that simply does "setMethod(obj, value)", unboxing if necessary
            DynamicMethod dynamicMethod = CreateDynamicMethod(info.DeclaringType, typeof(void), new Type[] { typeof(object), typeof(object) });
            
            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
            
            ilGenerator.Emit(OpCodes.Ldarg_0);              // Push the object onto the stack
            ilGenerator.Emit(OpCodes.Ldarg_1);              // Push the value onto the stack
            
            if (info.PropertyType.IsValueType)
            {
                // Need to unbox the value
                ilGenerator.Emit(OpCodes.Unbox_Any, info.PropertyType);
            }
            
            ilGenerator.Emit(OpCodes.Call, setMethod);      // Call the setter
            ilGenerator.Emit(OpCodes.Ret);

            Action<object, object> setter = (Action<object, object>)dynamicMethod.CreateDelegate(typeof(Action<object, object>));
            
            return setter;
        }

        private static DynamicMethod CreateDynamicMethod(Type owningType, Type returnType, Type[] argTypes)
        {
            DynamicMethod dynamicMethod;
            if (owningType.IsInterface)
            {
                // An interface can't own any code, but then it can't have any private properties either so that's fine
                dynamicMethod = new DynamicMethod("", returnType, argTypes);
            }
            else
            {
                // Make the owner the class so we can see private properties
                dynamicMethod = new DynamicMethod("", returnType, argTypes, owningType, true);
            }
            return dynamicMethod;
        }

        #endregion

        #region Methods

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //internal void SetValue(object item, object value)
        //{
        //    value = Convert.ChangeType(value, ObjectProperty.PropertyType);

        //    _setter(item, value);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal object GetValue(object item)
        {
            return _getter(item);
        }

        /// <summary>
        /// Translates the value to a SQL value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal object ConvertToSqlValue(object value)
        {
            if (UsesEnumMap)
            {
                return EnumMap.ToSql(value);
            }

            return value;
        }

        ///// <summary>
        ///// Translates the value to a SQL value
        ///// </summary>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //internal object ConvertToClrValue(object value)
        //{
        //    if (UsesEnumMap)
        //    {
        //        return EnumMap.ToClr(value);
        //    }

        //    if (value != DBNull.Value)
        //    {
        //        return value;
        //    }

        //    //  it's DBNull if here
        //    return null;
        //}

        #endregion

        #region Properties

        /// <summary>
        /// Source Name=SQL
        /// </summary>
        public string SourceName { get; private set; }

        /// <summary>
        /// Used for datatype mapping between SQL and CLR
        /// </summary>
        public EnumMap EnumMap { get; private set; }

        /// <summary>
        /// Is a value map used
        /// </summary>
        public bool UsesEnumMap { get { return EnumMap != null; } }

        /// <summary>
        /// Name=ObjectProperty Name
        /// </summary>
        public string Name { get { return ObjectProperty.Name; } }

        /// <summary>
        /// Property Name
        /// </summary>
        public PropertyInfo ObjectProperty { get; private set; }

        /// <summary>
        /// Whether or not the property has a setter
        /// </summary>
        public bool HasPropertySetter { get; private set; }

        /// <summary>
        /// Whether or not the property has a setter
        /// </summary>
        public bool IsReadOnly { get { return !HasPropertySetter; } }

        /// <summary>
        /// 
        /// </summary>
        public int MaxLength { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsPrimaryKey { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsDatabaseGenerated { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsIdentity { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsComputed { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsNullable { get; private set; }

        /// <summary>
        /// As part of CRUD operation
        /// </summary>
        public bool IsSelectable
        {
            get
            {
                if (IsReadOnly) return false;

                return true;
            }
        }

        /// <summary>
        /// As part of CRUD operation
        /// </summary>
        public bool IsInsertable
        {
            get
            {
                if (IsDatabaseGenerated) return false;
                if (IsIdentity) return false;
                if (IsComputed) return false;

                return true;
            }
        }

        /// <summary>
        /// As part of CRUD operation
        /// </summary>
        public bool IsUpdatable
        {
            get
            {
                if (IsPrimaryKey) return false;

                return IsInsertable;
            }
        }

        #endregion
    }
}