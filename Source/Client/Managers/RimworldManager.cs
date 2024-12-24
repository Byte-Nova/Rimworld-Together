using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using static Shared.CommonEnumerators;

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

            int silverInMap = GetSpecificThingCountInMap(ThingDefOf.Silver, map);
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

        public static bool CheckIfHasEnoughItemInCaravan(Caravan caravan, string defName, int quantity) 
        {
            if(quantity == 0) return true;

            List<Thing> caravanItems = CaravanInventoryUtility.AllInventoryItems(caravan).FindAll(x => x.def.defName == defName);
            int totalItem = 0;

            foreach (Thing itemStack in caravanItems) totalItem += itemStack.stackCount;
            if(totalItem >= quantity) return true;

            return false;
        }

        public static Thing[] GetAllThingsInMap(Map map)
        {
            return map.listerThings.AllThings.Where(fetch => fetch.def.category == ThingCategory.Item 
                && fetch.IsInAnyStorage() && fetch.def.category == ThingCategory.Item && !fetch.Position.Fogged(map)).ToArray();
        }

        public static int GetSpecificThingCountInMap(ThingDef thingDef, Map map)
        {
            int totalCount = 0;

            Thing[] allFetchedThings = map.listerThings.AllThings.Where(fetch => fetch.def == thingDef && !fetch.Position.Fogged(map)).ToArray();

            foreach (Thing thing in allFetchedThings)
            {
                totalCount += thing.stackCount;
            }

            return totalCount;
        }

        public static int GetSilverInCaravan(Caravan caravan)
        {
            List<Thing> caravanSilver = CaravanInventoryUtility.AllInventoryItems(caravan)
                .FindAll(x => x.def == ThingDefOf.Silver);

            int totalSilver = 0;
            foreach (Thing silverStack in caravanSilver) totalSilver += silverStack.stackCount;

            return totalSilver;
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

        public static void PlaceThingIntoMap(Thing thing, Map map, ThingPlaceMode placeMode = ThingPlaceMode.Direct, bool useSpot = false, bool byDropPod = false)
        {
            IntVec3 positionToPlaceAt = IntVec3.Zero;
            if (useSpot) positionToPlaceAt = TransferManagerHelper.GetTransferLocationInMap(map);
            else positionToPlaceAt = thing.Position;

            if (byDropPod) TradeUtility.SpawnDropPod(FindVectorNear(positionToPlaceAt, map), map, thing);
            else
            {
                if (thing is Pawn) GenSpawn.Spawn(thing, positionToPlaceAt, map, thing.Rotation);
                else GenPlace.TryPlaceThing(thing, positionToPlaceAt, map, placeMode, rot: thing.Rotation);
            }
        }

        private static IntVec3 FindVectorNear(IntVec3 center, Map map)
        {
            if (!DropCellFinder.TryFindDropSpotNear(center, map, out IntVec3 vectorForUse, false, true))
            {
                Logger.Warning("Couldn't find any good drop spot near " + center + "Will use random valid location instead.", LogImportanceMode.Verbose);
                vectorForUse = CellFinderLoose.RandomCellWith((Predicate<IntVec3>)(c => c.Standable(map) && !c.Fogged(map)), map);
            }
            
            return vectorForUse;
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

        public static void RemoveThingFromCaravan(Caravan caravan, ThingDef thingDef, int requiredQuantity)
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
            while (requiredQuantity > 0)
            {
                List<Thing> things = map.listerThings.ThingsOfDef(thingDef).Where(fetch => fetch.IsInAnyStorage())
                    .ToList();

                while (requiredQuantity > 0)
                {
                    Thing thing = things.First();
                    int stackDeleting = Mathf.Min(requiredQuantity, thing.stackCount);
                    thing.SplitOff(stackDeleting);
                    requiredQuantity -= stackDeleting;
                    things.Remove(thing);
                }
            }
        }

        public static void RemovePawnFromGame(Pawn pawn)
        {
            if (pawn.Spawned) pawn.DeSpawn();
            pawn.Destroy();
        }

        public static Pawn[] GetAllSettlementsPawns(Faction faction, bool includeAnimals)
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
            if (map == null || map.mapPawns == null) return new Pawn[0];
            else
            {
                if (includeAnimals) return map.mapPawns.AllPawns.Where(fetch => fetch.Faction == faction).ToArray();
                else return map.mapPawns.AllPawns.Where(fetch => fetch.Faction == faction && !ScriberHelper.CheckIfThingIsAnimal(fetch)).ToArray();
            }
        }

        public static bool CheckIfMapHasPlayerPawns(Map map)
        {
            if (map == null || map.mapPawns == null) return false;
            else if (map.mapPawns.AllPawns.FirstOrDefault(fetch => fetch.Faction == Faction.OfPlayer) != null) return true;
            else return false;
        }

        public static void SetGameSpeed(TimeSpeed timeSpeed) { Find.TickManager.CurTimeSpeed = timeSpeed; }
    }
}
