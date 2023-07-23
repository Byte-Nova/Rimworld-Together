using Newtonsoft.Json;
using RimworldTogether.Shared.Network;

namespace RimworldTogether.GameServer.Misc
{
    public static class Serializer
    {
        public static string SerializeToString(object serializable)
        {
            return JsonConvert.SerializeObject(serializable);
        }

        public static T SerializeFromString<T>(string serializable)
        {
            return JsonConvert.DeserializeObject<T>(serializable);
        }

        public static Packet SerializeToPacket(string serializable)
        {
            return JsonConvert.DeserializeObject<Packet>(serializable);
        }

        public static void SerializeToFile(string path, object serializable)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(serializable, Formatting.Indented));
        }

        public static T SerializeFromFile<T>(string path)
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
        }
    }
}
