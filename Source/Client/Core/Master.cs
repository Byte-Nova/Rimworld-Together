using System.Reflection;

namespace GameClient
{
    //Class with all the critical variables for the client to work

    public static class Master
    {
        //Instances

        public static UnityMainThreadDispatcher threadDispatcher;
        
        public static ModConfigs modConfigs = new ModConfigs();

        public static Assembly SOS2;

        //Paths

        public static string mainPath;
        
        public static string modFolderPath;

        public static string connectionDataPath;

        public static string loginDataPath;

        public static string clientPreferencesPath;

        public static string savesFolderPath;
    }
}
