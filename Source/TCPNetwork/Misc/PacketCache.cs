using System;
using System.Collections.Generic;
using System.Reflection;
using Shared;
using Shared.Misc;
using TCPNetwork.Files.Client;

namespace TCPNetwork.Misc
{
    public delegate void PacketHandler(byte[] bytes);

    public delegate void PacketHandlerServer(ServerClient client, byte[] bytes, PacketHeader handler);

    public static class PacketCache
    {
        public static Dictionary<PacketHeader, PacketHandler> ClientMethodDictionary { get; private set; } = new();

        public static Dictionary<PacketHeader, PacketHandlerServer> ServerMethodDictionary { get; private set; } = new();

        public static CommonEnumerators.AssemblyType AssemblyType { get; private set; } = CommonEnumerators.AssemblyType.None;

        public static void CacheAllPacketsInAppDomain(CommonEnumerators.AssemblyType assemblyType)
        {
            if (AssemblyType != CommonEnumerators.AssemblyType.None)
            {
                Printer.Warning($"Tried to cache assembly type {assemblyType}, but they were already cached. Invalidating cache");
                ClientMethodDictionary.Clear();
                ServerMethodDictionary.Clear();
            }

            AssemblyType = assemblyType;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
                    {
                        HandlesPacket attribute = method.GetCustomAttribute<HandlesPacket>();
                        if (attribute != null) AddMethod(attribute.header, method);
                    }
                }
            }
        }

        private static void AddMethod(PacketHeader header, MethodInfo method)
        {
            Printer.Warning($"Adding {GetMethodInfoStr(method)} to the packet cache", CommonEnumerators.LogImportanceMode.Extreme);

            switch (AssemblyType)
            {
                case CommonEnumerators.AssemblyType.None:
                    throw new Exception($"Tried adding type to the packet cache, but the type was not set yet");

                case CommonEnumerators.AssemblyType.Client:
                    if (ClientMethodDictionary.ContainsKey(header))
                    {
                        Printer.Error($"Tried registering header {header} twice!" +
                                      $"\n Original was: {GetMethodInfoStr(ClientMethodDictionary[header].Method)}" +
                                      $"\n Duplicate was: {GetMethodInfoStr(method)}");
                    }
                    ClientMethodDictionary.Add(header, (PacketHandler)Delegate.CreateDelegate(typeof(PacketHandler), method));
                    break;

                case CommonEnumerators.AssemblyType.Server:
                    if (ServerMethodDictionary.ContainsKey(header))
                    {
                        Printer.Error($"Tried registering header {header} twice!" +
                                      $"\n Original was: {GetMethodInfoStr(ClientMethodDictionary[header].Method)}" +
                                      $"\n Duplicate was: {GetMethodInfoStr(method)}");
                    }
                    ServerMethodDictionary.Add(header, (PacketHandlerServer)Delegate.CreateDelegate(typeof(PacketHandlerServer), method));
                    break;
            }
        }

        private static string GetMethodInfoStr(MethodInfo info) { return $"method {info.Name} from type {info.DeclaringType!.FullName}"; }
    }
}