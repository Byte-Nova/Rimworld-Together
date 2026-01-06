using GameClient.Misc;
using GameClient.WorldObjects;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Shared.Files;
using Shared.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using TCPNetwork.Packets;
using Verse;
using Verse.Noise;
using static Shared.CommonEnumerators;

namespace GameClient.Managers
{
    public static class SettlementManager
    {
        public static List<RTSettlement> PlayerSettlements { get; set; } = new List<RTSettlement>();

        [HandlesPacket(PacketHeader.SettlementManager)]
        private static void ParsePacket(byte[] bytes)
        {
            PlayerSettlementData data = Serializer.ConvertBytesToObject<PlayerSettlementData>(bytes);

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
        
            foreach (WorldObject settlement in Finder.GetAllRTSettlements())
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
                settlement.Name = $"{toAdd.Username}'s settlement";
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
                RTSettlement toGet = Finder.GetRTSettlementFromTile(toRemove.Tile);
                PlayerSettlements.Remove(toGet); 
                Find.WorldObjects.Remove(toGet);
                toGet.Destroy();
            }
            catch (Exception e) { Printer.Error($"Failed to remove settlement at {toRemove.Tile}. Reason: {e}"); }
        }

        public static void RegenSettlement(RTSettlement _)
        {
            SettlementFile file = new SettlementFile();
            file.Tile = _.Tile;
            file.Username = _.Label.Replace("'s settlement", "");

            if (_.Faction == SessionHandler.EnemyFaction) file.Goodwill = Goodwill.Enemy;
            else if (_.Faction == SessionHandler.AllyFaction) file.Goodwill = Goodwill.Ally;
            else if (_.Faction == SessionHandler.GuildFaction) file.Goodwill = Goodwill.Guild;
            else file.Goodwill = Goodwill.Neutral;

            RemoveSingleSettlement(file);
            SpawnSingleSettlement(file);
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
