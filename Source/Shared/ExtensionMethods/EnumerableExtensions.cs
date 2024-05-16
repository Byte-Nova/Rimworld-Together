



using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    public static class EnumerableExtensions
    {

        public static T[] Add<T>(this T[] array, T item)
        {
            List<T> returnList = array.ToList();
            returnList.Add(item);
            return  returnList.ToArray();
        }

        public static T[] RemoveAt<T>(this T[] array, int index)
        {
            List<T> returnList = array.ToList();
            returnList.RemoveAt(index);
            return returnList.ToArray();
        }

    }






}