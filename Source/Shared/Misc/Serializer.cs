using System;
using MessagePack;
using MessagePack.Resolvers;
using Newtonsoft.Json;
using System.IO;
using Shared.Misc;

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

        public static byte[] ConvertObjectToBytes(object toConvert, bool compression = true)
        {
            try 
            { 
                if (compression) return MessagePackSerializer.Serialize(toConvert, ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4Block));
                else return MessagePackSerializer.Serialize(toConvert, ContractlessStandardResolver.Options); 
            }

            catch (Exception e) 
            { 
                Printer.Error(e);
                throw null;
            }
        }

        public static T ConvertBytesToObject<T>(byte[] bytes, bool compression = true)
        {
            try 
            { 
                if (compression) return MessagePackSerializer.Deserialize<T>(bytes, ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4Block));
                else return MessagePackSerializer.Deserialize<T>(bytes, ContractlessStandardResolver.Options);
            }

            catch (Exception e)
            {
                Printer.Error(e);
                throw null;
            }
        }

        // Serialize from and to strings

        public static string SerializeToString(object serializable)
        {
            try { return JsonConvert.SerializeObject(serializable, DefaultSettings); }
            catch (Exception e)
            {
                Printer.Error(e);
                throw null;
            }
        }

        public static T SerializeFromString<T>(string serializable)
        {
            try { return JsonConvert.DeserializeObject<T>(serializable, DefaultSettings); }
            catch (Exception e)
            {
                Printer.Error(e);
                throw null;
            }
        }

        /// <summary>
        /// Serializes an object to a JSON at the path specified, deserialized by <see cref="SerializeToFile"/>
        /// </summary>
        public static void SerializeToFile(string path, object serializable)
        {
            try { File.WriteAllText(path, JsonConvert.SerializeObject(serializable, IndentedSettings)); }
            catch (Exception e)
            {
                Printer.Error(e);
                throw null;
            }
        }

        /// <summary>
        /// Deserializes an object from JSON, serialized by <see cref="SerializeFromString"/>
        /// </summary>
        public static T SerializeFromFile<T>(string path)
        {
            try { return JsonConvert.DeserializeObject<T>(File.ReadAllText(path), DefaultSettings); }
            catch (Exception e)
            {
                Printer.Error(e);
                throw null;
            }
        }

        /// <summary>
        /// Serializes an object to binary to a file, deserialized by <see cref="FileBytesToObject{T}"/>
        /// </summary>
        public static void ObjectBytesToFile(string path, object serializable)
        {
            try { File.WriteAllBytes(path, ConvertObjectToBytes(serializable)); }
            catch (Exception e)
            {
                Printer.Error(e);
                throw null;
            }
        }

        /// <summary>
        /// Deserializes an object from binary from a file, serialize by <see cref="ObjectBytesToFile"/>
        /// </summary>
        public static T FileBytesToObject<T>(string path)
        {
            try { return ConvertBytesToObject<T>(File.ReadAllBytes(path)); }
            catch (Exception e)
            {
                Printer.Error(e);
                throw null;
            }
        }
    }
}