using System;
using System.Reflection;

namespace Shared
{
    [Serializable]
    public class Packet
    {
        public string header;

        public byte[] contents;
        
        public bool requiresMainThread;

        public ModdedData? moddedData;

        public Packet(string header, byte[] contents, bool requiresMainThread, ModdedData? moddedData)
        {
            this.header = header;
            this.contents = contents;
            this.requiresMainThread = requiresMainThread;
            this.moddedData = moddedData;
        }

        public static Packet CreatePacketFromObject(string header, object objectToUse = null, bool requiresMainThread = true, ModdedData? moddedData = null)
        {
            if (objectToUse == null) return new Packet(header, null, requiresMainThread, moddedData);
            else
            {
                byte[] contents = Serializer.ConvertObjectToBytes(objectToUse);
                if (moddedData == null)
                {
                    string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                    if (assemblyName != CommonValues.serverAssemblyName || assemblyName != CommonValues.clientAssemblyName) 
                    {
                        moddedData = new ModdedData(assemblyName);
                    }
                }
                return new Packet(header, contents, requiresMainThread, moddedData);
            }
        }
    }
}
