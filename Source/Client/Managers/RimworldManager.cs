using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;

namespace GameClient
{
    public static class RimworldManager
    {
        public static bool CheckForAnySocialPawn(CommonEnumerators.SearchLocation location)
        {
            if (location == CommonEnumerators.SearchLocation.Caravan)
            {
                Caravan caravan = ClientValues.chosenCaravan;

                Pawn playerNegotiator = caravan.PawnsListForReading.Find(fetch => fetch.IsColonist && !fetch.skills.skills[10].PermanentlyDisabled);
                if (playerNegotiator != null) return true;
            }

            else if (location == CommonEnumerators.SearchLocation.Settlement)
            {
                Map map = Find.AnyPlayerHomeMap;

                Pawn playerNegotiator = map.mapPawns.AllPawns.Find(fetch => fetch.IsColonist && !fetch.skills.skills[10].PermanentlyDisabled);
                if (playerNegotiator != null) return true;
            }

            return false;
        }

        public static bool CheckIfPlayerHasMap()
        {
            Map map = Find.AnyPlayerHomeMap;
            if (map != null) return true;
            else return false;
        }

        public static bool CheckIfHasEnoughSilverInMap(Map map, int requiredQuantity)
        {
            if (requiredQuantity == 0) return true;

            List<Thing> silverInMap = new List<Thing>();
            foreach (Zone zone in map.zoneManager.AllZones)
            {
                foreach (Thing thing in zone.AllContainedThings.Where(fetch => fetch.def.category == ThingCategory.Item))
                {
                    if (thing.def == ThingDefOf.Silver && !thing.Position.Fogged(map))
                    {
                        silverInMap.Add(thing);
                    }
                }
            }

            int totalSilver = 0;
            foreach (Thing silverStack in silverInMap) totalSilver += silverStack.stackCount;

            if (totalSilver >= requiredQuantity) return true;
            else return false;
        }

        public static bool CheckIfHasEnoughSilverInCaravan(int requiredQuantity)
        {
            if (requiredQuantity == 0) return true;

            List<Thing> caravanSilver = CaravanInventoryUtility.AllInventoryItems(ClientValues.chosenCaravan)
                .FindAll(x => x.def == ThingDefOf.Silver);

            int silverInCaravan = 0;
            foreach (Thing silverStack in caravanSilver) silverInCaravan += silverStack.stackCount;

            if (silverInCaravan >= requiredQuantity) return true;
            else return false;
        }

        public static void GenerateLetter(string title, string description, LetterDef letterType)
        {
            Find.LetterStack.ReceiveLetter(title,
                description,
                letterType);
        }
    }
}
