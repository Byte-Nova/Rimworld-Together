using RimWorld.Planet;
using RimWorld;
using System;
using System.Linq;
using Verse;
using Shared;
using static Shared.CommonEnumerators;
using System.Collections.Generic;

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
                    break;

                case SettlementStepMode.Remove:
                    RemoveNPCSettlementFromPacket(data._settlementData);
                    break;
            }
        }

        public static void AddSettlements(PlanetNPCSettlement[] settlements)
        {
            foreach(PlanetNPCSettlement settlement in settlements)
            {
                SpawnSingleSettlement(settlement);
            }
        }

        public static void SpawnSingleSettlement(PlanetNPCSettlement toAdd)
        {
            if (Find.WorldObjects.Settlements.FirstOrDefault(fetch => fetch.Tile == toAdd.tile) != null) return;
            else
            {
                Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                settlement.Tile = toAdd.tile;
                settlement.Name = toAdd.name;

                List<Faction> factions = PlanetManagerHelper.GetNPCFactionFromDefName(toAdd.defName);

                if (factions.Count == 0)
                {
                    Logger.Warning($"Could not find faction for settlement at tile {toAdd.tile} with faction {toAdd.defName}");
                    return;
                }

                else if (factions.Count == 1)
                {
                    settlement.SetFaction(factions.First());
                }

                else if (factions.Count > 1)
                {
                    foreach (Faction faction in factions)
                    {
                        if(faction.Name == toAdd.factionName) settlement.SetFaction(faction);
                    }

                    if(settlement.Faction == null) settlement.SetFaction(factions.First());
                } 

                Find.WorldObjects.Add(settlement);
            }
        }

        public static void ClearAllSettlements()
        {
            Settlement[] settlements = Find.WorldObjects.Settlements.Where(fetch => !FactionValues.playerFactions.Contains(fetch.Faction) &&
                fetch.Faction != Faction.OfPlayer).ToArray();

            foreach (Settlement settlement in settlements) RemoveSingleSettlement(settlement, null);

            DestroyedSettlement[] destroyedSettlements = Find.WorldObjects.DestroyedSettlements.Where(fetch => !FactionValues.playerFactions.Contains(fetch.Faction) &&
                fetch.Faction != Faction.OfPlayer).ToArray();

            foreach (DestroyedSettlement settlement in destroyedSettlements) RemoveSingleSettlement(null, settlement);
        }

        public static void RemoveNPCSettlementFromPacket(PlanetNPCSettlement data)
        {
            Settlement toRemove = Find.World.worldObjects.Settlements.FirstOrDefault(fetch => fetch.Tile == data.tile &&
                fetch.Faction != Faction.OfPlayer);

            if (toRemove != null) RemoveSingleSettlement(toRemove, null);
        }

        public static void RemoveSingleSettlement(Settlement settlement, DestroyedSettlement destroyedSettlement)
        {
            if (settlement != null)
            {
                try
                {
                    if (!RimworldManager.CheckIfMapHasPlayerPawns(settlement.Map))
                    {
                        NPCSettlementManagerHelper.lastRemovedSettlement = settlement;
                        Find.WorldObjects.Remove(settlement);
                    }
                    else Logger.Warning($"Ignored removal of settlement at {settlement.Tile} because player was inside");
                }
                catch (Exception e) { Logger.Warning($"Failed to remove NPC settlement at {settlement.Tile}. Reason: {e}"); }
            }

            else if (destroyedSettlement != null)
            {
                try
                {
                    if (!RimworldManager.CheckIfMapHasPlayerPawns(destroyedSettlement.Map))
                    {
                        Find.WorldObjects.Remove(destroyedSettlement);
                    }
                    else Logger.Warning($"Ignored removal of settlement at {destroyedSettlement.Tile} because player was inside");
                }
                catch (Exception e) { Logger.Warning($"Failed to remove NPC settlement at {destroyedSettlement.Tile}. Reason: {e}"); }       
            }
        }

        public static void RequestSettlementRemoval(Settlement settlement)
        {
            NPCSettlementData data = new NPCSettlementData();
            data._stepMode = SettlementStepMode.Remove;
            data._settlementData.tile = settlement.Tile;

            Packet packet = Packet.CreatePacketFromObject(nameof(NPCSettlementManager), data);
            Network.listener.EnqueuePacket(packet);
        }
    }

    public static class NPCSettlementManagerHelper
    {
        public static PlanetNPCSettlement[] tempNPCSettlements;
        
        public static Settlement lastRemovedSettlement;

        public static void SetValues(ServerGlobalData serverGlobalData)
        {
            tempNPCSettlements = serverGlobalData._npcSettlements;
        }
    }
}
