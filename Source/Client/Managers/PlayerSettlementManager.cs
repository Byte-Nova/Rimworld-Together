using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Shared;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public static class PlayerSettlementManager
    {
        public static List<Settlement> playerSettlements = new List<Settlement>();

        public static void ParsePacket(Packet packet)
        {
            PlayerSettlementData settlementData = Serializer.ConvertBytesToObject<PlayerSettlementData>(packet.contents);

            switch (settlementData._stepMode)
            {
                case SettlementStepMode.Add:
                    SpawnSingleSettlement(settlementData._settlementData);
                    break;

                case SettlementStepMode.Remove:
                    RemoveSingleSettlement(settlementData._settlementData);
                    break;
            }
        }

        public static void AddSettlements(SettlementFile[] settlements)
        {
            foreach (SettlementFile toAdd in settlements)
            {
                SpawnSingleSettlement(toAdd);
            }
        }

        public static void ClearAllSettlements()
        {
            Settlement[] settlements = Find.WorldObjects.Settlements.Where(fetch => FactionValues.playerFactions.Contains(fetch.Faction)).ToArray();
            foreach (Settlement settlement in settlements)
            {
                SettlementFile toRemove = new SettlementFile();
                toRemove.Tile = settlement.Tile;
                RemoveSingleSettlement(toRemove);
            }
        }

        public static void SpawnSingleSettlement(SettlementFile toAdd)
        {
            if (Find.WorldObjects.Settlements.FirstOrDefault(fetch => fetch.Tile == toAdd.Tile) != null) return;
            else
            {
                try
                {
                    Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                    settlement.Tile = toAdd.Tile;
                    settlement.Name = "RTSettlementName".Translate(toAdd.Owner);
                    settlement.SetFaction(PlanetManagerHelper.GetPlayerFactionFromGoodwill(toAdd.Goodwill));

                    playerSettlements.Add(settlement);
                    Find.WorldObjects.Add(settlement);
                }
                catch (Exception e) { Logger.Error($"Failed to spawn settlement at {toAdd.Tile}. Reason: {e}"); }
            }
        }

        public static void RemoveSingleSettlement(SettlementFile toRemove)
        {
            try
            {
                Settlement toGet = Find.WorldObjects.Settlements.Find(fetch => fetch.Tile == toRemove.Tile && FactionValues.playerFactions.Contains(fetch.Faction));
                if (!RimworldManager.CheckIfMapHasPlayerPawns(toGet.Map))
                {
                    if (playerSettlements.Contains(toGet)) playerSettlements.Remove(toGet);
                    Find.WorldObjects.Remove(toGet);
                }
                else Logger.Warning($"Ignored removal of settlement at {toGet.Tile} because player was inside");
            }
            catch (Exception e) { Logger.Error($"Failed to remove settlement at {toRemove.Tile}. Reason: {e}"); }
        }
    }

    public static class PlayerSettlementManagerHelper
    {
        public static SettlementFile[] tempSettlements;

        public static void SetValues(ServerGlobalData serverGlobalData)
        {
            tempSettlements = serverGlobalData._playerSettlements;
        }
    }
}
