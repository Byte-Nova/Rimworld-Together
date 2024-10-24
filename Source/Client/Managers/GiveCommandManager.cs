using Shared;
using System;
using Verse;
using UnityEngine;
using RimWorld;
using RimWorld.Planet;

namespace GameClient
{
    public class GiveCommandManager
    {
        public static void ParsePacket(Packet packet)
        {
            ThingDataFile giveData = Serializer.ConvertBytesToObject<ThingDataFile>(packet.contents);
            GiveThingToPlayer(giveData);
        }

        private static void GiveThingToPlayer(ThingDataFile giveData)
        {
            try
            {
                ThingDataFile thingData = new ThingDataFile();
                thingData.DefName = giveData.DefName;
                thingData.Quantity = giveData.Quantity;
                thingData.Quality = giveData.Quality;
                Thing thing = ThingScribeManager.StringToItem(thingData);
                if (thing.HitPoints == 0) thing.HitPoints = thing.MaxHitPoints;
                Map map = Find.AnyPlayerHomeMap;
                if (map == null)
                    Logger.Error("Hasn't found any player map");
                    RimworldManager.PlaceThingIntoMap(thing, Find.AnyPlayerHomeMap, ThingPlaceMode.Near, true);
                RimworldManager.GenerateLetter("Admin notification", $"Admin gave you \"{thing.LabelNoCount} x{thingData.Quantity}\"", LetterDefOf.PositiveEvent);
                
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }
    }
}