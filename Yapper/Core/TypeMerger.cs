﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Yapper.Core
{
    /// <summary>
    /// A Utility class used to merge the properties of
    /// heterogenious objects
    /// </summary>
    
    class TypeMerger
    {
        //assembly/module builders
        private static AssemblyBuilder asmBuilder = null;
        private static ModuleBuilder modBuilder = null;

        //object type cache
        private static IDictionary<string, Type> anonymousTypes = new Dictionary<string, Type>();

        //used for thread-safe access to Type Dictionary
        private static object _syncLock = new object();

        /// <summary>
        /// Merge two different object instances into a single
        /// object which is a super-set
        /// of the properties of both objects
        /// </summary>
        public static object MergeTypes(object values1, object values2)
        {
            //create a name from the names of both Types
            string name = string.Format("{0}_{1}", values1.GetType(), values2.GetType());
            string name2 = string.Format("{0}_{1}", values2.GetType(), values1.GetType());

            object newValues = CreateInstance(name, values1, values2);

            if (newValues != null)
            {
                return newValues;
            }

            newValues = CreateInstance(name2, values2, values1);

            if (newValues != null)
            {
                return newValues;
            }

            //lock for thread safe writing
            lock (_syncLock)
            {
                //now that we're inside the lock - check one more time
                newValues = CreateInstance(name, values1, values2);
                if (newValues != null)
                    return newValues;

                //merge list of PropertyDescriptors for both objects
                PropertyDescriptor[] pdc = GetProperties(values1, values2);

                //make sure static properties are properly initialized
                InitializeAssembly();

                //create the type definition
                Type newType = CreateType(name, pdc);

                //add it to the cache
                anonymousTypes.Add(name, newType);

                //return an instance of the new Type
                return CreateInstance(name, values1, values2);
            }
        }

        /// <summary>
        /// Instantiates an instance of an existing Type from cache
        /// </summary>
        private static object CreateInstance(string name, object values1, object values2)
        {
            object newValues = null;

            //merge all values together into an array
            object[] allValues = MergeValues(values1, values2);

            //check to see if type exists
            if (anonymousTypes.ContainsKey(name))
            {
                //get type
                Type type = anonymousTypes[name];

                //make sure it isn't null for some reason
                if (type != null)
                {
                    //create a new instance
                    newValues = Activator.CreateInstance(type, allValues);
                }
                else
                {
                    //remove null type entry
                    lock (_syncLock)
                    {
                        anonymousTypes.Remove(name);
                    }
                }
            }

            //return values (if any)
            return newValues;
        }

        /// <summary>
        /// Merge PropertyDescriptors for both objects
        /// </summary>
        private static PropertyDescriptor[] GetProperties(object values1, object values2)
        {
            //dynamic list to hold merged list of properties
            List<PropertyDescriptor> properties = new List<PropertyDescriptor>();

            //get the properties from both objects
            PropertyDescriptorCollection pdc1 = TypeDescriptor.GetProperties(values1);
            PropertyDescriptorCollection pdc2 = TypeDescriptor.GetProperties(values2);

            //add properties from values1
            for (int i = 0; i < pdc1.Count; i++)
            {
                properties.Add(pdc1[i]);
            }

            //add properties from values2
            for (int i = 0; i < pdc2.Count; i++)
            {
                properties.Add(pdc2[i]);
            }

            //return array
            return properties.ToArray();
        }

        /// <summary>
        /// Get the type of each property
        /// </summary>
        private static Type[] GetTypes(PropertyDescriptor[] pdc)
        {
            List<Type> types = new List<Type>();

            for (int i = 0; i < pdc.Length; i++)
            {
                types.Add(pdc[i].PropertyType);
            }

            return types.ToArray();
        }

        /// <summary>
        /// Merge the values of the two types into an object array
        /// </summary>
        private static object[] MergeValues(object values1,
            object values2)
        {
            PropertyDescriptorCollection pdc1 = TypeDescriptor.GetProperties(values1);
            PropertyDescriptorCollection pdc2 = TypeDescriptor.GetProperties(values2);

            List<object> values = new List<object>();

            for (int i = 0; i < pdc1.Count; i++)
            {
                values.Add(pdc1[i].GetValue(values1));
            }

            for (int i = 0; i < pdc2.Count; i++)
            {
                values.Add(pdc2[i].GetValue(values2));
            }

            return values.ToArray();
        }

        /// <summary>
        /// Initialize static objects
        /// </summary>
        private static void InitializeAssembly()
        {
            //check to see if we've already instantiated
            //the static objects
            if (asmBuilder == null)
            {
                //create a new dynamic assembly
                AssemblyName assembly = new AssemblyName
                {
                    Name = "AnonymousTypeExentions"
                };

                //get the current application domain
                AppDomain domain = Thread.GetDomain();

                //get a module builder object
                asmBuilder = domain.DefineDynamicAssembly(assembly, AssemblyBuilderAccess.Run);

                modBuilder = asmBuilder.DefineDynamicModule(asmBuilder.GetName().Name, false);
            }
        }

        /// <summary>
        /// Create a new Type definition from the list
        /// of PropertyDescriptors
        /// </summary>
        private static Type CreateType(string name, PropertyDescriptor[] pdc)
        {
            //create TypeBuilder
            TypeBuilder typeBuilder = CreateTypeBuilder(name);

            //get list of types for ctor definition
            Type[] types = GetTypes(pdc);

            //create priate fields for use w/in the ctor body and properties
            FieldBuilder[] fields = BuildFields(typeBuilder, pdc);

            //define/emit the Ctor
            BuildCtor(typeBuilder, fields, types);

            //define/emit the properties
            BuildProperties(typeBuilder, fields);

            //return Type definition
            return typeBuilder.CreateType();
        }

        /// <summary>
        /// Create a type builder with the specified name
        /// </summary>
        private static TypeBuilder CreateTypeBuilder(string typeName)
        {
            //define class attributes
            TypeBuilder typeBuilder = modBuilder.DefineType(typeName,
                        TypeAttributes.Public |
                        TypeAttributes.Class |
                        TypeAttributes.AutoClass |
                        TypeAttributes.AnsiClass |
                        TypeAttributes.BeforeFieldInit |
                        TypeAttributes.AutoLayout,
                        typeof(object));

            //return new type builder
            return typeBuilder;
        }

        /// <summary>
        /// Define/emit the ctor and ctor body
        /// </summary>
        private static void BuildCtor(TypeBuilder typeBuilder, FieldBuilder[] fields, Type[] types)
        {
            //define ctor()
            ConstructorBuilder ctor = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                types
                );

            //build ctor()
            ILGenerator ctorGen = ctor.GetILGenerator();

            //create ctor that will assign to private fields
            for (int i = 0; i < fields.Length; i++)
            {
                //load argument (parameter)
                ctorGen.Emit(OpCodes.Ldarg_0);
                ctorGen.Emit(OpCodes.Ldarg, (i + 1));

                //store argument in field
                ctorGen.Emit(OpCodes.Stfld, fields[i]);
            }

            //return from ctor()
            ctorGen.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Define fields based on the list of PropertyDescriptors
        /// </summary>
        private static FieldBuilder[] BuildFields(TypeBuilder typeBuilder, PropertyDescriptor[] pdc)
        {
            List<FieldBuilder> fields = new List<FieldBuilder>();

            //build/define fields
            for (int i = 0; i < pdc.Length; i++)
            {
                PropertyDescriptor pd = pdc[i];

                //define field as '_[Name]' with the object's Type
                FieldBuilder field = typeBuilder.DefineField(
                    string.Format("_{0}", pd.Name),
                    pd.PropertyType,
                    FieldAttributes.Private
                    );

                //add to list of FieldBuilder objects
                fields.Add(field);
            }

            return fields.ToArray();
        }

        /// <summary>
        /// Build a list of Properties to match the list of private fields
        /// </summary>
        private static void BuildProperties(TypeBuilder typeBuilder, FieldBuilder[] fields)
        {
            //build properties
            for (int i = 0; i < fields.Length; i++)
            {
                //remove '_' from name for public property name
                string propertyName = fields[i].Name.Substring(1);

                //define the property
                PropertyBuilder property = typeBuilder.DefineProperty(propertyName,
                    PropertyAttributes.None, fields[i].FieldType, null);

                //define 'Get' method only (anonymous types are read-only)
                MethodBuilder getMethod = typeBuilder.DefineMethod(
                    string.Format("Get_{0}", propertyName),
                    MethodAttributes.Public | MethodAttributes.SpecialName |                         MethodAttributes.HideBySig,
                    fields[i].FieldType,
                    Type.EmptyTypes
                    );

                //build 'Get' method
                ILGenerator methGen = getMethod.GetILGenerator();

                //method body
                methGen.Emit(OpCodes.Ldarg_0);
                //load value of corresponding field
                methGen.Emit(OpCodes.Ldfld, fields[i]);
                //return from 'Get' method
                methGen.Emit(OpCodes.Ret);

                //assign method to property 'Get'
                property.SetGetMethod(getMethod);
            }
        }
    }
}