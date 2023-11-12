using RimworldTogether.Shared.Misc;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

#pragma warning disable SYSLIB0011 // Type or member is obsolete

namespace RimworldTogether.Shared.Serializers
{
    public static class ObjectConverter
    {
        private static BindOverrider bindOverrider = new BindOverrider();

        public static byte[] ConvertObjectToBytes(object toConvert)
        {
            if (toConvert == null) return null;

            MemoryStream memoryStream = new MemoryStream();

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Binder = bindOverrider;

            binaryFormatter.Serialize(memoryStream, toConvert);
            return memoryStream.ToArray();
        }

        public static object ConvertBytesToObject(byte[] bytes)
        {
            MemoryStream memoryStream = new MemoryStream();

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Binder = bindOverrider;

            memoryStream.Write(bytes, 0, bytes.Length);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return binaryFormatter.Deserialize(memoryStream);
        }
    }
}
