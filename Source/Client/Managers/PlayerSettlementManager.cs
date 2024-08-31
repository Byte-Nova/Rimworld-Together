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
            if (settlementData.settlementData.isShip)
            {
                switch (settlementData.stepMode)
                {
                    case SettlementStepMode.Add:
                        PlayerShipData data = (PlayerShipData)settlementData;
                        GameClient.SOS2.PlayerShipManager.SpawnSingleSettlement(data);
                        break;

                    case SettlementStepMode.Remove:
                        RemoveSingleSettlement(settlementData);
                        break;
                }
            }
            else
            {
                switch (settlementData.stepMode)
                {
                    case SettlementStepMode.Add:
                        SpawnSingleSettlement(settlementData);
                        break;

                    case SettlementStepMode.Remove:
                        RemoveSingleSettlement(settlementData);
                        break;
                }
            }
        }

        public static void AddSettlements(SettlementFile[] toAdd)
        {
            if (toAdd == null) return;

            for (int i = 0; i < PlayerSettlementManagerHelper.tempSettlements.Count(); i++)
            {
                SettlementFile settlementFile = PlayerSettlementManagerHelper.tempSettlements[i];
                {
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
            for (int i = 0; i < PlayerSettlementManagerHelper.tempSpaceSettlements.Count(); i++)
            {
                SpaceSettlementFile settlementFile = PlayerSettlementManagerHelper.tempSpaceSettlements[i];
                {
                    try
                    {
                        GameClient.SOS2.PlayerShipManager.AddSettlementFromFile(settlementFile);
                    }
                    catch (Exception e) { Logger.Error($"Failed to build ship at {settlementFile.Tile}. Reason: {e}"); }
                }
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
                    settlement.Tile = toAdd.settlementData.Tile;
                    settlement.Name = $"{toAdd.settlementData.Owner}'s settlement";
                    settlement.SetFaction(PlanetManagerHelper.GetPlayerFactionFromGoodwill(toAdd.settlementData.Goodwill));

                    playerSettlements.Add(settlement);
                    Find.WorldObjects.Add(settlement);
                }
                catch (Exception e) { Logger.Error($"Failed to spawn settlement at {toAdd.settlementData.Tile}. Reason: {e}"); }
            }
        }

        public static void RemoveSingleSettlement(PlayerSettlementData toRemove)
        {
            if (ClientValues.isReadyToPlay)
            {
                try
                {
                    if(!toRemove.settlementData.isShip) {
                        Settlement toGet = playerSettlements.Find(x => x.Tile == toRemove.settlementData.Tile);

                        playerSettlements.Remove(toGet);
                        Find.WorldObjects.Remove(toGet);
                    } else 
                    {
                        GameClient.SOS2.PlayerShipManager.RemoveFromTile(toRemove.settlementData.Tile);
                    }
                }
                catch (Exception e) { Logger.Error($"Failed to remove settlement at {toRemove.settlementData.Tile}. Reason: {e}"); }
            }
        }

        public static WorldObject GetWorldObjectFromTile(int tile)
        {
            try
            {
                WorldObject toGet = Find.WorldObjects.AllWorldObjects.Where(x => x.Tile == tile).FirstOrDefault();
                if (toGet != null)
                {
                    if (toGet.def.defName == "RT_Ship" || toGet.def.defName == "RT_ShipEnemy" || toGet.def.defName == "RT_ShipNeutral")
                    {
                        return (WorldObjectFakeOrbitingShip)toGet;
                    }
                    else
                    {
                        return (Settlement)toGet;
                    }
                }
                return null;
            }
            catch (Exception e) { GameClient.Logger.Error($"Failed to find WorldObject at {tile}. Reason: {e}"); }
            return null;
        }
    }

    public static class PlayerSettlementManagerHelper
    {
        public static SettlementFile[] tempSettlements;
        public static SpaceSettlementFile[] tempSpaceSettlements;

        public static void SetValues(ServerGlobalData serverGlobalData)
        {
            tempSpaceSettlements = serverGlobalData.playerSpaceSettlements;
            tempSettlements = serverGlobalData._playerSettlements;
        }
    }
}
