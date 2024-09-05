using RimWorld.Planet;
using RimWorld;
using System;
using System.Linq;
using Verse;
using Shared;
using static Shared.CommonEnumerators;

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
            if (settlements == null) return;

            foreach(PlanetNPCSettlement settlement in NPCSettlementManagerHelper.tempNPCSettlements)
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

            if (toRemove != null) RemoveSettlement(toRemove, null);
        }

        public static void RemoveSettlement(Settlement settlement, DestroyedSettlement destroyedSettlement)
        {
            if (settlement != null)
            {
                NPCSettlementManagerHelper.lastRemovedSettlement = settlement;
                Find.WorldObjects.Remove(settlement);
            }
            else if (destroyedSettlement != null) Find.WorldObjects.Remove(destroyedSettlement);
        }

        public static void RequestSettlementRemoval(Settlement settlement)
        {
            NPCSettlementData data = new NPCSettlementData();
            data._stepMode = SettlementStepMode.Remove;
            data._settlementData.tile = settlement.Tile;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.NPCSettlementPacket), data);
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
