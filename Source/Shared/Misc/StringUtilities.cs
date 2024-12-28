using System.Reflection;
using System;
using System.Collections;
namespace Shared
{
    /// <summary>
    /// Class to convert objects into strings for debugging purposes. Only use it with <see cref="CommonEnumerators.LogImportanceMode.Verbose"/>.
    /// </summary>
    public static class StringUtilities
    {
        /// <summary>
        /// This function processes an object and return every field as a string. Only use it with <see cref="CommonEnumerators.LogImportanceMode.Verbose"/>.
        /// </summary>
        /// <param name="obj">The object to convert to a string.</param>
        /// <param name="tabbing">The number of tab levels for indentation. 0 by default</param>
        /// <returns>A string representing the object's fields and values.</returns>
        public static string ToString(object obj, int tabbing = 0)
        {
            string str = "";
            if (tabbing == 0)
            {
                str += Indent(tabbing) + $"{obj.GetType().Name}:\n";
                tabbing++;
            }
            foreach (FieldInfo field in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object value = field.GetValue(obj);
                str += Indent(tabbing);

                if (value != null)
                {
                    if (field.FieldType.IsPrimitive || field.FieldType.IsValueType || field.FieldType == typeof(string))
                    {
                        str += $"{field.Name}: {value}\n"; // Is Primitive
                    }
                    else if (value is Array) // Is Array
                    {
                        str += $"{field.Name}: {HandleArray((Array)value, tabbing + 1)}";
                    }
                    else if (typeof(IEnumerable).IsAssignableFrom(field.FieldType) && field.FieldType != typeof(string)) // Is List
                    {
                        str += $"{field.Name}: {HandleIEnumerable((IEnumerable)value, tabbing + 1)}";
                    }
                    else //Is Object
                    {
                        str += $"{field.Name}: \n{ToString(value, tabbing + 1)}";
                    }
                }
                else
                {
                    str += $"{field.Name}: was null\n"; // Is Null
                }
            }
            return str;
        }

        private static string HandleArray(Array array, int tabbing)
        {
            string str = "";
            if(array.Length == 0) 
            {
                return str += Indent(tabbing) + "Empty";
            }
            str += "\n";
            foreach (var item in array)
            {
                str += "" + ProcessItem(item, tabbing);
            }

            return str;
        }

        private static string HandleIEnumerable(IEnumerable enumerable, int tabbing)
        {
            string str = "";
            if (!enumerable.GetEnumerator().MoveNext()) 
            {
                return str += "Empty\n";
            }
            foreach (var item in enumerable)
            {
                str += ProcessItem(item, tabbing);
            }

            return str;
        }

        private static string ProcessItem(object item, int tabbing) 
        {
            string str = "";
            str += Indent(tabbing);
            if (item != null)
            {
                if (item.GetType().IsPrimitive || item.GetType().IsValueType || item.GetType() == typeof(string)) // Is primitive
                {
                    str += $"{item.GetType().Name}: {item}\n";
                }
                else if (item.GetType() == typeof(IEnumerable)) //Is List
                {
                    str += $"{item.GetType().Name}: \n{HandleIEnumerable((IEnumerable)item, tabbing + 1)}";
                }
                else if (item.GetType() == typeof(Array)) //Is Array
                {
                    str += $"{item.GetType().Name}: \n{HandleArray((Array)item, tabbing + 1)}";
                }
                else //Is Object
                {
                    str += $"{item.GetType().Name}: \n{ToString(item, tabbing + 1)}";
                }
            }
            else
            {
                str += $"{item.GetType().Name}: was null\n"; // Is Null
            }
            return str;
        }
        private static string Indent(int level)
        {
            return new string('\t', level);
        }
    }
}
