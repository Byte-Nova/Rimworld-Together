using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RimworldTogether.Shared.Misc
{
    public class BindOverrider : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (assemblyName.Equals("NA")) return Type.GetType(typeName);
            else return BindToType(assemblyName, typeName);
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            // specify a neutral code for the assembly name to be recognized by the BindToType method.
            assemblyName = "NA";
            typeName = serializedType.FullName;
        }
    }
}
