using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System.IO;

namespace Shared
{
    //Class that handles all of the mod's serialization functions

    public static class Serializer
    {
        // Variables

        private static JsonSerializerSettings DefaultSettings => new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.None };

        private static JsonSerializerSettings IndentedSettings => new JsonSerializerSettings() 
        { 
            TypeNameHandling = TypeNameHandling.None,
            Formatting = Formatting.Indented
        };

        //Serialize from and to byte arrays

        public static byte[] ConvertObjectToBytes(object toConvert, bool compression = false)
        {
            JsonSerializer serializer = JsonSerializer.Create(DefaultSettings);
            MemoryStream memoryStream = new MemoryStream();

            using (BsonWriter writer = new BsonWriter(memoryStream)) 
            { 
                serializer.Serialize(writer, toConvert); 
            }

            if (compression) return GZip.Compress(memoryStream.ToArray());
            else return memoryStream.ToArray();
        }

        public static T ConvertBytesToObject<T>(byte[] bytes, bool compression = true)
        {
            if (compression) bytes = GZip.Decompress(bytes);

            JsonSerializer serializer = JsonSerializer.Create(DefaultSettings);
            MemoryStream memoryStream = new MemoryStream(bytes);

            using BsonReader reader = new BsonReader(memoryStream);
            return serializer.Deserialize<T>(reader);
        }

        // Serialize from and to strings

        public static string SerializeToString(object serializable) { return JsonConvert.SerializeObject(serializable, DefaultSettings); }

        public static T SerializeFromString<T>(string serializable) { return JsonConvert.DeserializeObject<T>(serializable, DefaultSettings); }

        // Serialize from and to files text

        public static void SerializeToFile(string path, object serializable) { File.WriteAllText(path, JsonConvert.SerializeObject(serializable, IndentedSettings)); }

        public static T SerializeFromFile<T>(string path) { return JsonConvert.DeserializeObject<T>(File.ReadAllText(path), DefaultSettings); }

        // Serialize from and to file bytes

        public static void ObjectBytesToFile(string path, object serializable) { File.WriteAllBytes(path, ConvertObjectToBytes(serializable, true)); }

        public static T FileBytesToObject<T>(string path) { return ConvertBytesToObject<T>(File.ReadAllBytes(path)); }
    }
}