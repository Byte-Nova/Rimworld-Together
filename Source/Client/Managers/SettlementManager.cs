using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Shared;
using static Shared.CommonEnumerators;
using GameClient.Misc;
using GameClient.Values;
using GameClient.TCP;

namespace GameClient.Managers
{
    public static class SettlementManager
    {
        public static List<Settlement> PlayerSettlements { get; set; } = new List<Settlement>();

        [HandlesPacket(PacketHeader.SettlementManager)]
        private static void ParsePacket(byte[] bytes)
        {
            PlayerSettlementData settlementData = Serializer.ConvertBytesToObject<PlayerSettlementData>(bytes);

            switch (settlementData._stepMode)
            {
                case SettlementStepMode.Add:
                    SpawnSingleSettlement(settlementData._settlementFile);
                    break;

                case SettlementStepMode.Remove:
                    RemoveSingleSettlement(settlementData._settlementFile);
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
            PlayerSettlements.Clear();

            Settlement[] settlements = Find.WorldObjects.Settlements.Where(fetch => ClientValues.PlayerFactions.Contains(fetch.Faction)).ToArray();
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
                    settlement.Name = $"{toAdd.Label}'s settlement";
                    settlement.SetFaction(PlanetManagerHelper.GetPlayerFactionFromGoodwill(toAdd.Goodwill));

                    PlayerSettlements.Add(settlement);
                    Find.WorldObjects.Add(settlement);
                }
                catch (Exception e) { Printer.Error($"Failed to spawn settlement at {toAdd.Tile}. Reason: {e}"); }
            }
        }

        public static void RemoveSingleSettlement(SettlementFile toRemove)
        {
            try
            {
                Settlement toGet = Find.WorldObjects.Settlements.Find(fetch => fetch.Tile == toRemove.Tile && ClientValues.PlayerFactions.Contains(fetch.Faction));
                if (!RimworldManager.CheckIfMapHasPlayerPawns(toGet.Map))
                {
                    if (PlayerSettlements.Contains(toGet)) PlayerSettlements.Remove(toGet);
                    Find.WorldObjects.Remove(toGet);
                }
                else Printer.Warning($"Ignored removal of settlement at {toGet.Tile} because player was inside");
            }
            catch (Exception e) { Printer.Error($"Failed to remove settlement at {toRemove.Tile}. Reason: {e}"); }
        }

        public static void SendNewPlayerSettlement(int settlementTile)
        {
            PlayerSettlementData settlementData = new PlayerSettlementData();
            settlementData._settlementFile.Tile = settlementTile;
            settlementData._stepMode = SettlementStepMode.Add;

            Network.Listener.EnqueuePacket(PacketHeader.SettlementManager, settlementData);
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
