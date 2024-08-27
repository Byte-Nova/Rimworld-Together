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

        public static JobDef GetJobFromDef(string defToFind) { return DefDatabase<JobDef>.AllDefs.First(fetch => fetch.defName == defToFind); }

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

        public static void PlaceThingIntoMap(Thing thing, Map map, ThingPlaceMode placeMode = ThingPlaceMode.Direct, bool useSpot = false)
        {
            IntVec3 positionToPlaceAt = IntVec3.Zero;
            if (useSpot) positionToPlaceAt = TransferManagerHelper.GetTransferLocationInMap(map);
            else positionToPlaceAt = thing.Position;

            if (thing is Pawn) GenSpawn.Spawn(thing, positionToPlaceAt, map, thing.Rotation);
            else GenPlace.TryPlaceThing(thing, positionToPlaceAt, map, placeMode, rot: thing.Rotation);
        }

        public static void PlaceThingIntoCaravan(Thing thing, Caravan caravan)
        {
            if (thing is Pawn)
            {
                Pawn pawn = thing as Pawn;

                if (!Find.WorldPawns.AllPawnsAliveOrDead.Contains(pawn)) Find.WorldPawns.PassToWorld(pawn);
                if (pawn.def.CanHaveFaction) pawn.SetFactionDirect(Faction.OfPlayer);
                caravan.AddPawn(pawn, false);
            }

            else
            {
                if (thing.stackCount == 0) return;

                caravan.AddPawnOrItem(thing, false);
            }
        }

        public static void RemoveThingFromCaravan(ThingDef thingDef, int requiredQuantity, Caravan caravan)
        {
            if (requiredQuantity == 0) return;

            List<Thing> caravanQuantity = CaravanInventoryUtility.AllInventoryItems(caravan)
                .FindAll(x => x.def == thingDef);

            int takenQuantity = 0;
            foreach (Thing thing in caravanQuantity)
            {
                if (takenQuantity + thing.stackCount >= requiredQuantity)
                {
                    thing.holdingOwner.Take(thing, requiredQuantity - takenQuantity);
                    break;
                }

                else if (takenQuantity + thing.stackCount < requiredQuantity)
                {
                    thing.holdingOwner.Take(thing, thing.stackCount);
                    takenQuantity += thing.stackCount;
                }
            }
        }

        public static void RemoveThingFromSettlement(Map map, ThingDef thingDef, int requiredQuantity)
        {
            if (requiredQuantity == 0) return;

            List<Thing> thingsInMap = new List<Thing>();
            foreach (Zone zone in map.zoneManager.AllZones)
            {
                foreach (Thing thing in zone.AllContainedThings.Where(fetch => fetch.def.category == ThingCategory.Item))
                {
                    if (thing.def == thingDef && !thing.Position.Fogged(map))
                    {
                        thingsInMap.Add(thing);
                    }
                }
            }

            int takenQuantity = 0;
            foreach (Thing thing in thingsInMap)
            {
                if (takenQuantity == requiredQuantity) return;

                else if (takenQuantity + thing.stackCount == requiredQuantity)
                {
                    takenQuantity = requiredQuantity;
                    thing.Destroy();
                    break;
                }

                else if (takenQuantity + thing.stackCount > requiredQuantity)
                {
                    int missingQuantity = requiredQuantity - takenQuantity;

                    takenQuantity += missingQuantity;
                    thing.stackCount -= missingQuantity;
                    if (thing.stackCount <= 0) thing.Destroy();
                    break;
                }

                else if (takenQuantity + thing.stackCount < requiredQuantity)
                {
                    takenQuantity += thing.stackCount;
                    thing.Destroy();
                    continue;
                }
            }
        }

        public static void RemovePawnFromGame(Pawn pawn)
        {
            if (pawn.Spawned) pawn.DeSpawn();
            if (Find.WorldPawns.AllPawnsAliveOrDead.Contains(pawn)) Find.WorldPawns.RemovePawn(pawn);
        }

        public static Pawn[] GetAllSettlementPawns(Faction faction, bool includeAnimals)
        {
            Settlement[] settlements = Find.World.worldObjects.Settlements.Where(fetch => fetch.Faction == faction).ToArray();

            List<Pawn> allPawns = new List<Pawn>();
            foreach(Settlement settlement in settlements)
            {
                allPawns.AddRange(GetPawnsFromMap(settlement.Map, faction, includeAnimals));
            }

            return allPawns.ToArray();
        }

        public static Pawn[] GetPawnsFromMap(Map map, Faction faction, bool includeAnimals)
        {
            if (includeAnimals) return map.mapPawns.AllPawns.Where(fetch => fetch.Faction == faction).ToArray();
            else return map.mapPawns.AllPawns.Where(fetch => fetch.Faction == faction && !DeepScribeHelper.CheckIfThingIsAnimal(fetch)).ToArray();
        }
    }
}
