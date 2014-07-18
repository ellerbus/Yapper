using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yapper
{
   static class Extensions
   {
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
