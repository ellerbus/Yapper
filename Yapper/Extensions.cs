using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Yapper
{
   static class Extensions
   {
       public static Type CreateDynamicType(this ExpandoObject expando, int id)
       {
           Type baseClassType = typeof(object);

           string dynamicClassName = "D" + id;

           AssemblyName assemblyName = new AssemblyName(dynamicClassName + "Assembly");

           AssemblyBuilder assemblyBuilder = System.Threading.Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

           ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(dynamicClassName + "Module");

           TypeBuilder typeBuilder = moduleBuilder.DefineType(dynamicClassName, TypeAttributes.Public | TypeAttributes.Class, baseClassType);

           var baseClassObj = Activator.CreateInstance(baseClassType);

           IDictionary<string, object> propertyList = expando as IDictionary<string, object>;

           foreach (KeyValuePair<string, object> prop in propertyList)
           {
               string propertyName = prop.Key;

               Type propertyType = prop.Value.GetType();

               FieldBuilder fieldBuilder = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

               PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(
                   propertyName,
                   System.Reflection.PropertyAttributes.None,
                   propertyType,
                   new Type[] { propertyType }
                   );

               MethodAttributes getSetAttributes = MethodAttributes.Public | MethodAttributes.HideBySig;

               MethodBuilder getPropertyBuilder = typeBuilder.DefineMethod("geter", getSetAttributes, propertyType, null);

               ILGenerator getterIL = getPropertyBuilder.GetILGenerator();
               getterIL.Emit(OpCodes.Ldarg_0);
               getterIL.Emit(OpCodes.Ldfld, fieldBuilder);
               getterIL.Emit(OpCodes.Ret);

               MethodBuilder setPropertyBuilder = typeBuilder.DefineMethod("seter", getSetAttributes, null, new Type[] { propertyType });

               ILGenerator setterIL = setPropertyBuilder.GetILGenerator();
               setterIL.Emit(OpCodes.Ldarg_0);
               setterIL.Emit(OpCodes.Ldarg_1);
               setterIL.Emit(OpCodes.Stfld, fieldBuilder);
               setterIL.Emit(OpCodes.Ret);

               propertyBuilder.SetGetMethod(getPropertyBuilder);
               propertyBuilder.SetSetMethod(setPropertyBuilder);
           }

           return typeBuilder.CreateType();
       }

       /// <summary>
       /// Extension method that turns a dictionary of string and object to an ExpandoObject
       /// </summary>
       public static ExpandoObject ToExpando(this IDictionary<string, object> dictionary)
       {
           ExpandoObject expando = new ExpandoObject();

           IDictionary<string, object> expandoDic = (IDictionary<string, object>)expando;

           // go through the items in the dictionary and copy over the key value pairs)
           foreach (var kvp in dictionary)
           {
               // if the value can also be turned into an ExpandoObject, then do it!
               if (kvp.Value is IDictionary<string, object>)
               {
                   ExpandoObject expandoValue = ((IDictionary<string, object>)kvp.Value).ToExpando();

                   expandoDic.Add(kvp.Key, expandoValue);
               }
               else if (kvp.Value is ICollection)
               {
                   // iterate through the collection and convert any strin-object dictionaries
                   // along the way into expando objects
                   List<object> itemList = new List<object>();

                   foreach (var item in (ICollection)kvp.Value)
                   {
                       if (item is IDictionary<string, object>)
                       {
                           ExpandoObject expandoItem = ((IDictionary<string, object>)item).ToExpando();

                           itemList.Add(expandoItem);
                       }
                       else
                       {
                           itemList.Add(item);
                       }
                   }

                   expandoDic.Add(kvp.Key, itemList);
               }
               else
               {
                   expandoDic.Add(kvp);
               }
           }

           return expando;
       }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="type"></param>
       /// <param name="genericTypeDefinition"></param>
       /// <returns></returns>
       public static bool IsOrHasGenericInterfaceTypeOf(this Type type, Type genericTypeDefinition)
       {
           if (type == genericTypeDefinition)
           {
               return true;
           }

           if (type.GetTypeWithGenericTypeDefinitionOf(genericTypeDefinition) != null)
           {
               return true;
           }

           return false;
       }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="type"></param>
       /// <param name="genericTypeDefinition"></param>
       /// <returns></returns>
       public static Type GetTypeWithGenericTypeDefinitionOf(this Type type, Type genericTypeDefinition)
       {
           foreach (Type t in type.GetInterfaces())
           {
               if (t.IsGenericType && t.GetGenericTypeDefinition() == genericTypeDefinition)
               {
                   return t;
               }
           }

           Type genericType = type.GetGenericType();

           if (genericType != null && genericType.GetGenericTypeDefinition() == genericTypeDefinition)
           {
               return genericType;
           }

           return null;
       }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="type"></param>
       /// <returns></returns>
       private static Type GetGenericType(this Type type)
       {
           while (type != null)
           {
               if (type.IsGenericType)
               {
                   return type;
               }

               type = type.BaseType;
           }

           return null;
       }
    }
}
