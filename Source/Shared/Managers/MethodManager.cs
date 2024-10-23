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
                Type fullType = GetTypeFromName(typeName);
                MethodInfo methodInfo = GetMethodFromName(fullType, methodName);
                methodInfo.Invoke(methodInfo.Name, parameters);

                return true;
            }
            catch (Exception e) { latestException = e.ToString(); }

            return false;
        }

        public static bool TryExecuteModdedMethod(string methodName, string typeName, object[] parameters)
        {
            try
            {
                Type fullType = GetTypeFromName("Master");
                FieldInfo fieldInfo = fullType.GetField("loadedCompatibilityPatches");
                Assembly[] assemblyArray = (Assembly[])fieldInfo.GetValue(null);
                Packet packet = (Packet)parameters[1];

                Assembly toFind = assemblyArray.First(fetch => fetch.GetName().Name.ToString() == packet.modTargetAssembly);
                Type moddedType = toFind.GetType(typeName);
                MethodInfo moddedMethod = GetMethodFromName(fullType, methodName);
                moddedMethod.Invoke(moddedMethod.Name, parameters);
            }
            catch (Exception e) { latestException = e.ToString(); }

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