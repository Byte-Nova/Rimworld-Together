using System;
using System.Runtime.Serialization;

namespace RimworldTogether.Shared.Misc
{
    public class BindOverrider : SerializationBinder
    {
        private static readonly string neutralAssembly = "NA";
        private static readonly string neutralVersion = "Version=1.0.0.0";

        private static readonly string clientAssembly = "GameClient";
        private static readonly string clientVersion = "Version=4.0.0.0";

        private static readonly string serverAssembly = "GameServer";
        private static readonly string serverVersion = "Version=7.0.0.0";

        public override Type BindToType(string assemblyName, string typeName)
        {
            if (assemblyName.Contains(clientAssembly))
            {
                assemblyName = assemblyName.Replace(clientAssembly, neutralAssembly);
                return Type.GetType(typeName);
            }

            else if (assemblyName.Contains(clientVersion))
            {
                typeName = typeName.Replace(clientVersion, neutralVersion);
                return Type.GetType(typeName);
            }

            else if (assemblyName.Contains(serverAssembly))
            {
                assemblyName = assemblyName.Replace(serverAssembly, neutralAssembly);
                return Type.GetType(typeName);
            }

            else if (assemblyName.Contains(serverVersion))
            {
                typeName = typeName.Replace(serverVersion, neutralVersion);
                return Type.GetType(typeName);
            }

            else if (assemblyName.Contains(neutralAssembly) || assemblyName.Contains(neutralVersion))
            {
                return Type.GetType(typeName);
            }

            else throw new Exception("Unknown assembly");
        }
    }
}
