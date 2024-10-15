using System.Reflection;
using System;
using static Shared.CommonEnumerators;
using System.Diagnostics;

namespace Shared
{
    public static class MethodManager
    {
        public static bool TryExecuteMethod(string methodName, string typeName, object[] parameters)
        {
            try
            {
                Type fullType = GetTypeFromName(typeName);
                MethodInfo methodInfo = GetMethodFromName(fullType, methodName);
                methodInfo.Invoke(methodInfo.Name, parameters);

                return true;
            }
            catch (Exception e) { Debug.WriteLine(e); }

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