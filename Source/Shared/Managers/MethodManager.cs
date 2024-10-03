using System.Reflection;
using System;
using static Shared.CommonEnumerators;
using System.Diagnostics;

namespace Shared
{
    public static class MethodManager
    {
        public static AssemblyType GetAssemblyType()
        {
            string assemblyName = GetExecutingAssemblyName();

            if (assemblyName == CommonValues.serverAssemblyName) return AssemblyType.Server;
            else if (assemblyName == CommonValues.clientAssemblyName) return AssemblyType.Client;
            else throw new NotImplementedException();
        }

        public static void ExecuteMethod(string typeName, string methodName, object[] parameters)
        {
            try
            {
                Type fullType = GetTypeFromName(typeName);
                MethodInfo methodInfo = GetMethodFromName(fullType, methodName);

                methodInfo.Invoke(methodInfo.Name, parameters);
            }
            catch(System.Exception e) { Debug.WriteLine(e); }
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