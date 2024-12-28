using System.Reflection;
using System;
using System.Linq;

namespace Shared
{
    public static class MethodManager
    {
        public static string latestException;

        public static bool TryExecuteMethod(string methodName, string typeName, object[] parameters)
        {
            try
            {
                Type fullType = GetTypeFromName(Assembly.GetExecutingAssembly(), typeName);
                MethodInfo methodInfo = GetMethodFromName(fullType, methodName);
                methodInfo.Invoke(methodInfo.Name, parameters);

                return true;
            }
            catch (Exception e) { latestException = e.ToString(); }

            return false;
        }

        public static bool TryExecuteModdedMethod(string methodName, string typeName, string assemblyName, object[] parameters)
        {
            try
            {
                Type execType = GetTypeFromName(Assembly.GetExecutingAssembly(), "Master");
                FieldInfo exectField = execType.GetField("loadedCompatibilityPatches");
                Assembly[] moddedAssemblies = (Assembly[])exectField.GetValue(null);

                Assembly toFind = moddedAssemblies.First(fetch => GetAssemblyName(fetch) == assemblyName);
                Type moddedType = GetTypeFromName(toFind, typeName);
                MethodInfo moddedMethod = GetMethodFromName(moddedType, methodName);
                moddedMethod.Invoke(moddedMethod.Name, parameters);

                return true;
            }
            catch (Exception e) { latestException = e.ToString(); }

            return false;
        }

        public static string GetAssemblyName(Assembly assembly)
        {
            return assembly.GetName().Name;
        }

        public static Type GetTypeFromName(Assembly assembly, string typeName)
        {
            return assembly.GetTypes().FirstOrDefault(T => T.Name == typeName);
        }

        public static MethodInfo GetMethodFromName(Type methodType, string methodName)
        {
            return methodType.GetMethod(methodName);
        }
    }
}