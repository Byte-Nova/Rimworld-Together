using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GameClient.Misc
{
    public static class ReflectionHandler
    {
        public static void SetPrivateField(Type type, object instance, string fieldName, object value, bool isStatic = false)
        {
            FieldInfo fieldInfo = null;
            if (isStatic) fieldInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            else fieldInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            fieldInfo.SetValue(instance, value);
        }

        public static object GetPrivateField(Type type, object instance, string fieldName, bool isStatic = false)
        {
            if (isStatic) return type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static).GetValue(instance);
            else return type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance);
        }

        public static void ExecutePrivateMethod(Type type, object instance, string methodName, bool isStatic = false, object[] parameters = null)
        {
            MethodInfo methodInfo = null;
            if (isStatic) methodInfo = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
            else methodInfo = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

            methodInfo.Invoke(instance, parameters);
        }
    }
}
