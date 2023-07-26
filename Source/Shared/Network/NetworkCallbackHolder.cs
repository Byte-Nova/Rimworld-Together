using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RimworldTogether.Shared.Network
{
    public static class NetworkCallbackHolder
    {
        public static readonly Dictionary<int, Action<byte[], int>> Callbacks = new Dictionary<int, Action<byte[], int>>();
        public static readonly Dictionary<Type, ICommunicatorBase> CommunicatorTypes = new Dictionary<Type, ICommunicatorBase>();
 
        public static T GetType<T>() where T : ICommunicatorBase
        {
            return (T)CommunicatorTypes[typeof(T)];
        }

        static NetworkCallbackHolder()
        {
            var baseType = typeof(ICommunicatorBase);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => baseType.IsAssignableFrom(p) && (p.Attributes & TypeAttributes.Abstract) == 0 && p.IsGenericType == false).ToList();

            foreach (var type in types)
            {
                var instance = Activator.CreateInstance(type);
                CommunicatorTypes[type] = (ICommunicatorBase)instance;
            }
        }
    }
}