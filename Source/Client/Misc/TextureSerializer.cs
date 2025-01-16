using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameClient.Core;
using UnityEngine;
using UnityEngine.XR;

namespace GameClient.Misc
{
    public static class TextureSerializer
    {
        public static Texture2D MakeTexture(byte[] bytes) 
        {
            Texture2D texture = new Texture2D(2, 2);

            texture.LoadImage(bytes);

            return texture;
        }
    }
}
