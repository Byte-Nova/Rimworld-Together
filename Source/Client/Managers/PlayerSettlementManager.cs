﻿using RimWorld.Planet;
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
                    SpawnSingleSettlement(settlementData);
                    break;

                case SettlementStepMode.Remove:
                    RemoveSingleSettlement(settlementData);
                    break;
            }
        }

        public static void AddSettlements(SettlementFile[] toAdd)
        {
            if (toAdd == null) return;

            for (int i = 0; i < PlayerSettlementManagerHelper.tempSettlements.Count(); i++)
            {
                SettlementFile settlementFile = PlayerSettlementManagerHelper.tempSettlements[i];

                try
                {
                    Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                    settlement.Tile = settlementFile.Tile;
                    settlement.Name = $"{settlementFile.Owner}'s settlement";
                    settlement.SetFaction(PlanetManagerHelper.GetPlayerFactionFromGoodwill(settlementFile.Goodwill));

                    playerSettlements.Add(settlement);
                    Find.WorldObjects.Add(settlement);
                }
                catch (Exception e) { Logger.Error($"Failed to build settlement at {settlementFile.Tile}. Reason: {e}"); }
            }
        }

        public static void ClearAllSettlements()
        {
            playerSettlements.Clear();

            Settlement[] settlements = Find.WorldObjects.Settlements.Where(fetch => FactionValues.playerFactions.Contains(fetch.Faction)).ToArray();
            foreach (Settlement settlement in settlements) Find.WorldObjects.Remove(settlement);
        }

        public static void SpawnSingleSettlement(PlayerSettlementData toAdd)
        {
            if (ClientValues.isReadyToPlay)
            {
                try
                {
                    Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                    settlement.Tile = toAdd._settlementData.Tile;
                    settlement.Name = $"{toAdd._settlementData.Owner}'s settlement";
                    settlement.SetFaction(PlanetManagerHelper.GetPlayerFactionFromGoodwill(toAdd._settlementData.Goodwill));

                    playerSettlements.Add(settlement);
                    WorldObjectManagerHelper.lastWorldObjectAdded = settlement.Tile;
                    Find.WorldObjects.Add(settlement);
                }
                catch (Exception e) { Logger.Error($"Failed to spawn settlement at {toAdd._settlementData.Tile}. Reason: {e}"); }
            }
        }

        public static void RemoveSingleSettlement(PlayerSettlementData toRemove)
        {
            if (ClientValues.isReadyToPlay)
            {
                try
                {
                    Settlement toGet = playerSettlements.Find(x => x.Tile == toRemove._settlementData.Tile);

                    playerSettlements.Remove(toGet);
                    WorldObjectManagerHelper.lastWorldObjectAdded = toGet.Tile;
                    Find.WorldObjects.Remove(toGet);
                }
                catch (Exception e) { Logger.Error($"Failed to remove settlement at {toRemove._settlementData.Tile}. Reason: {e}"); }
            }
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
