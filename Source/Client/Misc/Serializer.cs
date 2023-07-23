using System.IO;
using RimworldTogether.Shared.Network;
using UnityEngine;

namespace RimworldTogether.GameClient.Misc
{
    public static class Serializer
    {
        public static string SerializeToString(object serializable)
        {
            return JsonUtility.ToJson(serializable);
        }

        public static T SerializeFromString<T>(string serializable)
        {
            return JsonUtility.FromJson<T>(serializable);
        }

        public static Packet SerializeToPacket(string serializable)
        {
            return JsonUtility.FromJson<Packet>(serializable);
        }

        public static void SerializeToFile(string path, object serializable)
        {
            File.WriteAllText(path, JsonUtility.ToJson(serializable, true));
        }

        public static T SerializeFromFile<T>(string path)
        {
            return JsonUtility.FromJson<T>(File.ReadAllText(path));
        }
    }
}
