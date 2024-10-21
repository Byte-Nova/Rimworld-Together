using System.Reflection;
using System;

namespace Shared
{
    public static class MethodManager
    {
        public static string latestException;

        public static bool TryExecuteMethod(string methodName, string typeName, object[] parameters)
        {
            try
            {
                Type fullType = GetTypeFromName(typeName);
                MethodInfo methodInfo = GetMethodFromName(fullType, methodName);
                methodInfo.Invoke(methodInfo.Name, parameters);

                return true;
            }
            catch (Exception e) { latestException = e.ToString(); }

            return false;
        }

        //TODO
        //MAKE IT SO IT LOOPS THROUGH THE MODDED ASSEMBLIES

        public static bool TryExecuteModdedMethod(string methodName, string typeName, object[] parameters)
        {
            // try
            // {
            //     Type fullType = GetTypeFromName(typeName);
            //     MethodInfo methodInfo = GetMethodFromName(fullType, methodName);
            //     methodInfo.Invoke(methodInfo.Name, parameters);

            //     return true;
            // }
            // catch (Exception e) { latestException = e.ToString(); }

            return false;
        }
        
        public static string GetExecutingAssemblyName()
        {
            return Assembly.GetExecutingAssembly().GetName().Name;
        }

        public static Type GetTypeFromName(string typeName)
        {
            return Assembly.GetExecutingAssembly().GetType($"{GetExecutingAssemblyName()}.{typeName}");
        }

        public static MethodInfo GetMethodFromName(Type methodType, string methodName)
        {
            return methodType.GetMethod(methodName);
        }
    }
}