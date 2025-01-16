using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json;

namespace Shared 
{
    public class SiteInfoFile 
    {
        public string DefName;

        public string Label;

        public string Description;

        public string TexturePath;

        public string[] DefNameCost;

        public int[] Cost;

        public SiteRewardFile[] Rewards;

        public byte[]? Texture { get; set; } //For some reason NewtonSoft can only read Properties for the {ShouldSerialize}, no idea

        [JsonIgnore] public ThreadLocal<bool> shouldSerializeTexture = new ThreadLocal<bool>();

        public bool ShouldSerializeTexture() 
        {
            if (shouldSerializeTexture.Value)
            {
                if (File.Exists(TexturePath))
                {
                    Texture = File.ReadAllBytes(TexturePath);
                } 
                else
                {
                    Texture = null;
                }
            }
            else 
            {
                Texture = null;
            }
            return true;
        }

        public SiteInfoFile Clone() 
        {
            byte[] data = Serializer.ConvertObjectToBytes(this);
            return Serializer.ConvertBytesToObject<SiteInfoFile>(data, false);
        }
    }
}