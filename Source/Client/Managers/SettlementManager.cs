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
using Shared.Files;
using GameClient.WorldObjects;
using TCPNetwork.Packets;

namespace GameClient.Managers
{
    public static class SettlementManager
    {
        public static List<RTSettlement> PlayerSettlements { get; set; } = new List<RTSettlement>();

        [HandlesPacket(PacketHeader.SettlementManager)]
        private static void ParsePacket(byte[] bytes)
        {
            PlayerSettlementData data = Serializer.ConvertBytesToObject<PlayerSettlementData>(bytes);

            Printer.Warning(data, LogImportanceMode.Extreme);

            switch (data._stepMode)
            {
                case SettlementStepMode.Add:
                    SpawnSingleSettlement(data._settlementFile);
                    break;

                case SettlementStepMode.Remove:
                    RemoveSingleSettlement(data._settlementFile);
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

            WorldObject[] settlements = (WorldObject[])Find.World.worldObjects.AllWorldObjects.FindAll(fetch => 
                fetch.def.defName == "RTSettlement").ToArray();

            foreach (RTSettlement settlement in settlements)
            {
                SettlementFile toRemove = new SettlementFile();
                toRemove.Tile = settlement.Tile;
                RemoveSingleSettlement(toRemove);
            }
        }

        public static void SpawnSingleSettlement(SettlementFile toAdd)
        {
            try
            {
                WorldObjectDef def = DefDatabase<WorldObjectDef>.AllDefs.First(fetch => fetch.defName == "RTSettlement");
                RTSettlement settlement = (RTSettlement)WorldObjectMaker.MakeWorldObject(def);
                settlement.Tile = toAdd.Tile;
                settlement.Name = $"{toAdd.Label}'s settlement";
                settlement.SetFaction(PlanetManagerHelper.GetPlayerFactionFromGoodwill(toAdd.Goodwill));

                PlayerSettlements.Add(settlement);
                Find.WorldObjects.Add(settlement);
            }
            catch (Exception e) { Printer.Error($"Failed to spawn settlement at {toAdd.Tile}. Reason: {e}"); }
        }

        public static void RemoveSingleSettlement(SettlementFile toRemove)
        {
            try
            {
                RTSettlement toGet = (RTSettlement)Find.WorldObjects.AllWorldObjects.First(fetch => fetch.Tile == toRemove.Tile && 
                    ClientValues.PlayerFactions.Contains(fetch.Faction));

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

            ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.SettlementManager, settlementData);
        }

        public static void AbandonSettlement(int settlementTile)
        {
            PlayerSettlementData settlementData = new PlayerSettlementData();
            settlementData._settlementFile.Tile = settlementTile;
            settlementData._stepMode = SettlementStepMode.Remove;

            ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.SettlementManager, settlementData);

            SaveManager.ForceSave();
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
