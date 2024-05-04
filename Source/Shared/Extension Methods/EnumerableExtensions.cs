using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public static class EnumerableExtensions
    {
        public static T[] Add<T>(this T[] array, T item)
        {
            List<T> newList = array.ToList();
            newList.Add(item);
            return newList.ToArray();
        }

        public static T[] RemoveAt<T>(this T[] array, int index)
        {
            List<T> newList = array.ToList();
            newList.RemoveAt(index);
            return newList.ToArray();
        }

    }
}
