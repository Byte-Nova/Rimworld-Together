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
            SettlementData settlementData = Serializer.ConvertBytesToObject<SettlementData>(packet.contents);

            switch (settlementData.settlementStepMode)
            {
                case SettlementStepMode.Add:
                    SpawnSingleSettlement(settlementData);
                    break;

                case SettlementStepMode.Remove:
                    RemoveSingleSettlement(settlementData);
                    break;
            }
        }

        public static void AddSettlements(OnlineSettlementFile[] settlements)
        {
            if (settlements == null) return;

            for (int i = 0; i < PlayerSettlementManagerHelper.tempSettlements.Count(); i++)
            {
                OnlineSettlementFile settlementFile = PlayerSettlementManagerHelper.tempSettlements[i];

                try
                {
                    Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                    settlement.Tile = settlementFile.tile;
                    settlement.Name = "RTSettlementName".Translate(settlementFile.owner);
                    settlement.SetFaction(PlanetManagerHelper.GetPlayerFactionFromGoodwill(settlementFile.goodwill));

                    playerSettlements.Add(settlement);
                    Find.WorldObjects.Add(settlement);
                }
                catch (Exception e) { Logger.Error($"Failed to build settlement at {settlementFile.tile}. Reason: {e}"); }
            }
        }

        public static void ClearAllSettlements()
        {
            playerSettlements.Clear();

            Settlement[] settlements = Find.WorldObjects.Settlements.Where(fetch => FactionValues.playerFactions.Contains(fetch.Faction)).ToArray();
            foreach (Settlement settlement in settlements) Find.WorldObjects.Remove(settlement);
        }

        public static void SpawnSingleSettlement(SettlementData newSettlementJSON)
        {
            if (ClientValues.isReadyToPlay)
            {
                try
                {
                    Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                    settlement.Tile = newSettlementJSON.tile;
                    settlement.Name = "RTSettlementName".Translate(newSettlementJSON.owner);
                    settlement.SetFaction(PlanetManagerHelper.GetPlayerFactionFromGoodwill(newSettlementJSON.goodwill));

                    playerSettlements.Add(settlement);
                    Find.WorldObjects.Add(settlement);
                }
                catch (Exception e) { Logger.Error($"Failed to spawn settlement at {newSettlementJSON.tile}. Reason: {e}"); }
            }
        }

        public static void RemoveSingleSettlement(SettlementData newSettlementJSON)
        {
            if (ClientValues.isReadyToPlay)
            {
                try
                {
                    Settlement toGet = playerSettlements.Find(x => x.Tile == newSettlementJSON.tile);

                    playerSettlements.Remove(toGet);
                    Find.WorldObjects.Remove(toGet);
                }
                catch (Exception e) { Logger.Error($"Failed to remove settlement at {newSettlementJSON.tile}. Reason: {e}"); }
            }
        }
    }

    public static class PlayerSettlementManagerHelper
    {
        public static OnlineSettlementFile[] tempSettlements;

        public static void SetValues(ServerGlobalData serverGlobalData)
        {
            tempSettlements = serverGlobalData.playerSettlements;
        }
    }
}
