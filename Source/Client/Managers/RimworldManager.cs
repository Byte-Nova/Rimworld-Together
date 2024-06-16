using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace GameClient
{
    public static class RimworldManager
    {
        public static bool CheckIfPlayerHasMap()
        {
            Map map = Find.AnyPlayerHomeMap;
            if (map != null) return true;
            else return false;
        }

        public static bool CheckIfSocialPawnInMap(Map map)
        {
            Pawn playerNegotiator = map.mapPawns.AllPawns.Find(fetch => fetch.IsColonist && !fetch.skills.skills[10].PermanentlyDisabled);
            if (playerNegotiator != null) return true;
            else return false;
        }

        public static bool CheckIfSocialPawnInCaravan(Caravan caravan)
        {
            Pawn playerNegotiator = caravan.PawnsListForReading.Find(fetch => fetch.IsColonist && !fetch.skills.skills[10].PermanentlyDisabled);
            if (playerNegotiator != null) return true;
            else return false;
        }

        public static bool CheckIfHasEnoughSilverInMap(Map map, int requiredQuantity)
        {
            if (requiredQuantity == 0) return true;

            int silverInMap = GetSilverInMap(map);
            if (silverInMap >= requiredQuantity) return true;
            else return false;
        }

        public static bool CheckIfHasEnoughSilverInCaravan(Caravan caravan, int requiredQuantity)
        {
            if (requiredQuantity == 0) return true;

            int silverInCaravan = GetSilverInCaravan(caravan);
            if (silverInCaravan >= requiredQuantity) return true;
            else return false;
        }

        public static int GetSilverInMap(Map map)
        {
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

            return totalSilver;
        }

        public static int GetSilverInCaravan(Caravan caravan)
        {
            List<Thing> caravanSilver = CaravanInventoryUtility.AllInventoryItems(caravan)
                .FindAll(x => x.def == ThingDefOf.Silver);

            int totalSilver = 0;
            foreach (Thing silverStack in caravanSilver) totalSilver += silverStack.stackCount;

            return totalSilver;
        }

        public static bool CheckIfPlayerHasConsoleInMap(Map map)
        {
            foreach (Thing thing in map.listerThings.AllThings)
            {
                if (thing.def == ThingDefOf.CommsConsole && thing.Faction == Faction.OfPlayer) return true;
            }

            return false;
        }

        public static void GenerateLetter(string title, string description, LetterDef letterType)
        {
            Find.LetterStack.ReceiveLetter(title,
                description,
                letterType);
        }

        public static int GetGameTicks() { return Find.TickManager.TicksSinceSettle; }

        public static void SetGameTicks(int newGameTicks) { Find.TickManager.DebugSetTicksGame(newGameTicks); }

        public static JobDef GetJobFromDef(string defToFind) { return DefDatabase<JobDef>.AllDefs.ToList().Find(fetch => fetch.defName == defToFind); }

        public static Job SetJobFromDef(JobDef jobDef, LocalTargetInfo targetA, LocalTargetInfo targetB, LocalTargetInfo targetC)
        {
            return JobMaker.MakeJob(jobDef, targetA, targetB, targetC);
        }

        public static Thing[] GetThingsInMap(Map map)
        {
            return map.listerThings.AllThings.Where(fetch =>
                !DeepScribeHelper.CheckIfThingIsHuman(fetch) &&
                !DeepScribeHelper.CheckIfThingIsAnimal(fetch))
                .ToArray();
        }

        public static void PlaceThingInMap(Thing thing, Map map, ThingPlaceMode placeMode = ThingPlaceMode.Direct)
        {
            if (thing is Pawn) GenSpawn.Spawn(thing, thing.Position, map, thing.Rotation);
            else GenPlace.TryPlaceThing(thing, thing.Position, map, placeMode, rot: thing.Rotation);
        }
    }
}
