
using System;
using System.Collections.Generic;
using System.Reflection;
﻿using System.Reflection;
using GameClient.Core.Configs;
using GameClient.Misc;

namespace GameClient.Core
{
    // Class with all the critical variables for the client to work

    public static class Master
    {
        // Instances

        public static UnityMainThreadDispatcher threadDispatcher;

        public static ModExposer modConfigs = new ModExposer();

        public static Dictionary<string, MethodInfo> managerDictionary = new Dictionary<string, MethodInfo>();

        // Paths

        public static string appdataPath;

        public static string appdataRTPath;

        public static string appdataTempPath;

        public static string appdataTempVersionPath;

        public static string modMainPath;

        public static string modAssemblyPath;

        public static string modAddonsPath;

        public static string connectionDataPath;

        public static string loginDataPath;

        public static string clientPreferencesPath;

        public static string recentServersPath;

        public static string savesFolderPath;

        // Values

        public static readonly string modID = "RimWorld Together";
    }
}
