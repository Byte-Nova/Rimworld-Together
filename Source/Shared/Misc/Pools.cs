using System.Collections.Generic;

namespace Shared.Misc
{
    public static class Pools
    {
        //This pool is used for packet defnames and names. There's no need to store the same defname hundreds of times in memory, since they are just strings and mean nothing...
        public static class StringPool
        {
            
            public static Dictionary<string, string> AllStrings = new  Dictionary<string, string>();
            
            private static object Lock = new object();
            
            public static string GetOrAddString(string str)
            {
                if (str == null)
                    return null;
                lock (Lock)
                {
                    if (!AllStrings.TryGetValue(str, out string existingStr))
                    {
                        existingStr = str;
                        AllStrings.Add(existingStr, existingStr);
                    }

                    return existingStr;
                }
            }
        }
    }
}