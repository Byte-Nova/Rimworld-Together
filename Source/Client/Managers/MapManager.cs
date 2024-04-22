﻿using Shared;
using System.Linq;
using Verse;

namespace GameClient
{
    //Class that handles map functions for the mod to use

    public static class MapManager
    {
        //Sends all the player maps to the server

        public static void SendPlayerMapsToServer()
        {
            foreach (Map map in Find.Maps.ToArray())
            {
                if (map.IsPlayerHome)
                {
                    SendMapToServerSingle(map);
                }
            }
        }

        //Sends a desired map to the server

        private static void SendMapToServerSingle(Map map)
        {
            MapData mapData = ParseMap(map, true, true, true, true);

            MapFileData mapFileData = new MapFileData();
            mapFileData.mapTile = mapData.mapTile;
            mapFileData.mapData = Serializer.ConvertObjectToBytes(mapData);

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.MapPacket), mapFileData);
            Network.listener.EnqueuePacket(packet);
        }

        //Parses a desired map into an usable mod class

        public static MapData ParseMap(Map map, bool includeItems, bool includeHumans, bool includeAnimals, bool includeMods)
        {
            MapData mapData = MapScribeManager.MapToString(map, includeItems, includeHumans, includeAnimals);

            if (includeMods) mapData.mapMods = ModManager.GetRunningModList().ToList();

            return mapData;
        }
    }
}
