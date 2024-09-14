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

        public static void AddSettlements(SettlementFile[] settlements)
        {
            foreach (SettlementFile toAdd in settlements)
            {
                SpawnSingleSettlement(toAdd);
            }
            foreach (SpaceSettlementFile settlementFile in PlayerSettlementManager.tempSpaceSettlements)
            {
                try
                {
                    GameClient.SOS2.PlayerShipManager.AddSettlementFromFile(settlementFile);
                }
                catch (Exception e) { Logger.Error($"Failed to build ship at {settlementFile.Tile}. Reason: {e}"); }
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
                    settlement.Name = $"{toAdd.Owner}'s settlement";
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
                    if(!toRemove.settlementData.isShip) {
                        Settlement toGet = playerSettlements.Find(x => x.Tile == toRemove.settlementData.Tile);

                        playerSettlements.Remove(toGet);
                        Find.WorldObjects.Remove(toGet);
                    } else 
                    {
                        GameClient.SOS2.PlayerShipManager.RemoveFromTile(toRemove.settlementData.Tile);
                    }
                }
                else Logger.Warning($"Ignored removal of settlement at {toGet.Tile} because player was inside");
            }
            catch (Exception e) { Logger.Error($"Failed to remove settlement at {toRemove.Tile}. Reason: {e}"); }
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
