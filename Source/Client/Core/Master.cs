using System.Collections.Generic;
using System.Reflection;

namespace GameClient
{
    //Class with all the critical variables for the client to work

    public static class Master
    {
        //Instances

        public static UnityMainThreadDispatcher threadDispatcher;
        
        public static ModConfigs modConfigs = new ModConfigs();

        // DO NOT RENAME 'loadedCompatibilityPatches'
        // IT HAS A HARDCODED REFERENCE WITH THE METHOD MANAGER

        public static Assembly[] loadedCompatibilityPatches;

        //Paths

        public static string mainPath;
        
        public static string modFolderPath;

        public static string modAssemblyPath;

        public static string connectionDataPath;

        public static string loginDataPath;

        public static string clientPreferencesPath;

        public static string savesFolderPath;
    }
}
