using GameClient;
using GameClient.Hooks.TCPNetwork;
using GameClient.Misc;
using RimWorld;
using Shared;
using Synchronous.Misc;
using Synchronous.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Synchronous.Managers
{
    public static class SWeatherManager
    {
        public static void Ask(byte value)
        {
            PlayerWeather weather = new PlayerWeather();
            weather.MapTile = Find.CurrentMap.Tile;
            weather.WeatherByte = value;

            ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.SPlayerWeather, weather);
        }

        [HandlesPacket(PacketHeader.SPlayerWeather)]
        private static void Receive(byte[] bytes)
        {
            PlayerWeather data = Serializer.ConvertBytesToObject<PlayerWeather>(bytes);

            PatchHandler.ExecuteInBypass(delegate
            {
                WeatherDef def = Finder.GetWeatherDefFromByte(data.WeatherByte);
                Finder.GetMapFromTile(data.MapTile).weatherManager.TransitionTo(def);
            });
        }
    }
}
