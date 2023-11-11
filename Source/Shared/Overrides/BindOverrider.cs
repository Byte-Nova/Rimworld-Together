using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace RimworldTogether.Shared.Misc
{
    public class BindOverrider : SerializationBinder
    {
        private static readonly string clientAssembly = "GameClient";
        private static readonly string clientVersion = "Version=4.0.0.0";

        private static readonly string serverAssembly = "GameServer";
        private static readonly string serverVersion = "Version=7.0.0.0";

        public override Type BindToType(string assemblyName, string typeName)
        {
            if (assemblyName.Contains(clientVersion) || assemblyName.Contains(clientAssembly))
            {
                return Type.GetType(typeName);
            }

            else if (assemblyName.Contains(serverAssembly) || assemblyName.Contains(serverVersion))
            {
                return Type.GetType(typeName);
            }

            else throw new Exception("Unknown assembly version");
        }
    }
}
