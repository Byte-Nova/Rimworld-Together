using RimWorld.Planet;
using RimWorld;
using System;
using System.Linq;
using Verse;
using Shared;
using static Shared.CommonEnumerators;
using System.Collections.Generic;
using UnityEngine;

namespace GameClient
{
    public static class NPCSettlementManager
    {
        public static void ParsePacket(Packet packet)
        {
            NPCSettlementData data = Serializer.ConvertBytesToObject<NPCSettlementData>(packet.contents);

            switch (data._stepMode)
            {
                case SettlementStepMode.Add:
                    SpawnSettlement(data._settlementData);
                    break;

                case SettlementStepMode.Remove:
                    RemoveNPCSettlementFromPacket(data._settlementData);
                    break;
            }
        }

        public static void AddSettlements(PlanetNPCSettlement[] settlements)
        {
            if (settlements == null) return;

            foreach (PlanetNPCSettlement settlement in NPCSettlementManagerHelper.tempNPCSettlements)
            {
                SpawnSettlement(settlement);
            }
        }

        public static void SpawnSettlement(PlanetNPCSettlement toAdd)
        {
            try
            {
                Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                settlement.Tile = toAdd.tile;
                settlement.Name = toAdd.name;

                //TODO
                //THIS FUNCTION WILL ALWAYS ASSIGN ALL SETTLEMENTS TO THE FIRST INSTANCE OF A FACTION IF THERE'S MORE OF ONE OF THE SAME TIME
                //HAVING MULTIPLE GENTLE TRIBES WILL SYNC ALL THE SETTLEMENTS OF THE GENTLE TRIBES TO THE FIRST ONE. FIX!!
                settlement.SetFaction(PlanetManagerHelper.GetNPCFactionFromDefName(toAdd.defName));

                WorldObjectManagerHelper.lastWorldObjectAdded = settlement.Tile;
                Find.WorldObjects.Add(settlement);
            }
            catch (Exception e) { Logger.Error($"Failed to build NPC settlement at {toAdd.tile}. Reason: {e}"); }
        }

        public static void ClearAllSettlements()
        {
            Settlement[] settlements = Find.WorldObjects.Settlements.Where(fetch => !FactionValues.playerFactions.Contains(fetch.Faction) &&
                fetch.Faction != Faction.OfPlayer).ToArray();

            foreach (Settlement settlement in settlements)
            {
                RemoveSettlement(settlement, null);
                NPCSettlementManagerHelper.lastSettlements.Add(settlement.Tile);
            }

            DestroyedSettlement[] destroyedSettlements = Find.WorldObjects.DestroyedSettlements.Where(fetch => !FactionValues.playerFactions.Contains(fetch.Faction) &&
                fetch.Faction != Faction.OfPlayer).ToArray();

            foreach (DestroyedSettlement settlement in destroyedSettlements)
            {
                RemoveSettlement(null, settlement);
            }
        }

        public static void RemoveNPCSettlementFromPacket(PlanetNPCSettlement data)
        {
            Settlement toRemove = Find.World.worldObjects.Settlements.FirstOrDefault(fetch => fetch.Tile == data.tile &&
                fetch.Faction != Faction.OfPlayer);
            try
            {
                if (toRemove != null) RemoveSettlement(toRemove, null);
                WorldObjectManagerHelper.lastWorldObjectRemoved = data.tile;
            }
            catch (Exception ex) { Logger.Error(ex.ToString()); }
        }

        public static void RemoveSettlement(Settlement settlement, DestroyedSettlement destroyedSettlement)
        {
            if (settlement != null)
            {
                Find.WorldObjects.Remove(settlement);
            }
            else if (destroyedSettlement != null) Find.WorldObjects.Remove(destroyedSettlement);
        }
    }

    public static class NPCFactionManager
    {
        public static List<PlanetNPCFaction> newFactions = new List<PlanetNPCFaction>();
        public static void QueueFactionToServer(Faction faction) 
        {
            PlanetNPCFaction data = new PlanetNPCFaction();
            data.defName = faction.def.defName;
            data.name = faction.Name;
            data.color = new float[] { faction.Color.r, faction.Color.g, faction.Color.b, faction.Color.a };
            newFactions.Add(data);
            if (ClientValues.verboseBool) Logger.Message("Sending new faction to server");
        }
        public static bool DoesFactionExist(string def)
        {
            if (Find.FactionManager.AllFactions.FirstOrDefault(f => f.def.defName == def) == null)
            {
                return false;
            }
            return true;
        }

        public static void SpawnFaction(PlanetNPCFaction faction)
        {
            try
            {
                FactionDef toSpawn = DefDatabase<FactionDef>.GetNamed(faction.defName);
                bool hidden = toSpawn.hidden;
                toSpawn.hidden = true;
                FactionGeneratorParms parms = new FactionGeneratorParms(toSpawn);
                Faction newFaction = FactionGenerator.NewGeneratedFaction(parms);
                newFaction.Name = faction.name;
                newFaction.color = new Color(faction.color[0],
                        faction.color[1],
                        faction.color[2],
                        faction.color[3]);
                toSpawn.hidden = hidden;
                Find.FactionManager.Add(newFaction);
            }
            catch (Exception ex) { Logger.Warning($"Failed generating new faction. Reason:{ex.ToString()}"); }
        }

        //public static FactionRelationKind GetFactionRelation(Faction faction) 
        //{
        //    FactionRelationKind relation = FactionRelationKind.Hostile;
        //    if (faction.def.CanEverBeNonHostile)
        //        if (!faction.def.mustStartOneEnemy)
        //            if (faction.NaturalGoodwill > 0)
        //                return FactionRelationKind.Neutral;
        //    return relation;
        //}
    }
    public static class NPCSettlementManagerHelper
    {
        public static PlanetNPCSettlement[] tempNPCSettlements;

        public static List<int> lastSettlements = new List<int>();
        public static void SetValues(ServerGlobalData serverGlobalData)
        {
            tempNPCSettlements = serverGlobalData._npcSettlements;
        }
    }
}
