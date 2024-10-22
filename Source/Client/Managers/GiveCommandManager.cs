using Shared;
using System;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace GameClient
{
    public class GiveCommandManager
    {
        public static void ParsePacket(Packet packet)
        {
            ThingDataFile giveData = Serializer.ConvertBytesToObject<ThingDataFile>(packet.contents);
            Logger.Message("Finding things");
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
                Map map = Find.AnyPlayerHomeMap;
                if (map == null)
                    Logger.Error("Hasn't found any player map");
                RimworldManager.PlaceThingIntoMap(thing, Find.AnyPlayerHomeMap, ThingPlaceMode.Direct, true, false);
                RimworldManager.GenerateLetter("Received Thing from Admin", "Admin gave a thing to you", LetterDefOf.PositiveEvent);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }
    }
}