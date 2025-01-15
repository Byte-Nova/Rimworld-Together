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
        public static void MakeTextureOnMainThread(byte[] bytes, Action<Texture2D> callback) 
        {
            Master.threadDispatcher.Enqueue(() =>
            {
                Texture2D texture = new Texture2D(2, 2);

                texture.LoadImage(bytes);

                Renderer renderer = new Renderer();
                renderer.material.mainTexture = texture;

                callback.Invoke(texture);
            });
        }
    }
}
