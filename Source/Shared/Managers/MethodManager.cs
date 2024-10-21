using System.Reflection;
using System;
using static Shared.CommonEnumerators;
using System.Diagnostics;

namespace Shared
{
    public static class MethodManager
    {
        public static string TryExecuteMethod(string methodName, string typeName, object[] parameters)
        {
            string exception = "";
            try
            {
                Type fullType = GetTypeFromName(typeName);
                MethodInfo methodInfo = GetMethodFromName(fullType, methodName);
                methodInfo.Invoke(methodInfo.Name, parameters);
                return "";
            }
            catch (Exception e) { exception = e.ToString(); }

            return exception;
        }

        public static string TryExecuteMethod(Assembly assembly, string methodName, string typeName, object[] parameters = null)
        {
            string exception = "";
            try
            {
                Type fullType = assembly.GetType($"{GetExecutingAssemblyName()}.{typeName}");
                MethodInfo methodInfo = GetMethodFromName(fullType, methodName);
                methodInfo.Invoke(methodInfo.Name, parameters);
                return "";
            }
            catch (Exception e) { exception = e.ToString(); }

            return exception;
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