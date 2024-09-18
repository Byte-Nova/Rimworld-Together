using RimWorld;
using RimWorld.Planet;
using Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public static class WorldObjectManager
    {
        private static List<PlayerSettlementData> newPlayerSettlements = new List<PlayerSettlementData>();
        private static List<NPCSettlementData> newNPCSettlements = new List<NPCSettlementData>();
        public static void NewWorldObjectAdded(WorldObject o)
        {
            if (!(WorldObjectManagerHelper.lastWorldObjectAdded == o.Tile))
            {
                switch (o.def.defName)
                {
                    case "Settlement":
                        HandleSettlement((Settlement)o, SettlementStepMode.Add);
                        break;
                    case "Site":
                        break;
                    default:
                        break;
                }
            } 
            else 
            {

            }
        }

        public static void WorldObjectRemoved(WorldObject o) 
        {
            if (!(WorldObjectManagerHelper.lastWorldObjectRemoved == o.Tile))
            {
                switch (o.def.defName)
                {
                    case "Settlement":
                        HandleSettlement((Settlement)o, SettlementStepMode.Remove);
                        break;
                    case "Site":
                        break;
                    default:
                        break;
                }
            }
            else
            {

            }
        }

        private static void HandleSettlement(Settlement settlement, SettlementStepMode stepMode)
        {
            if ((settlement.Faction == FactionValues.enemyPlayer || settlement.Faction == FactionValues.neutralPlayer || settlement.Faction == FactionValues.allyPlayer || settlement.Faction == FactionValues.yourOnlineFaction)) // If NPC
            {
                // Do nothing, these were probably added by the server
            }
            else if (settlement.Faction == Find.FactionManager.OfPlayer) //Player Settlement
            {
                QueuePlayerSettlementToServer(settlement, stepMode);
            }
            else//NPC settlement, should sync if admin
            {
                if (ServerValues.isAdmin)QueueNPCSettlementToServer(settlement, stepMode);
            }
        }
        // Queue a player settlement to be modified
        private static void QueuePlayerSettlementToServer(Settlement settlement, SettlementStepMode stepMode)
        {
            PlayerSettlementData data = new PlayerSettlementData();
            data._settlementData.Tile = settlement.Tile;
            data._stepMode = stepMode;
            newPlayerSettlements.Add(data);
            if (ClientValues.verboseBool) Logger.Message("Sending new player settlement to server");
        }
        // Queue a settlement to be modified
        private static void QueueNPCSettlementToServer(Settlement settlement, SettlementStepMode stepMode)
        {
            NPCSettlementData data = new NPCSettlementData();
            data._settlementData.tile = settlement.Tile;
            Logger.Warning(settlement.Tile.ToString());
            data._settlementData.name = settlement.Name;
            data._settlementData.defName = settlement.Faction.def.defName;
            data._stepMode = stepMode;
            newNPCSettlements.Add(data);
            if (ClientValues.verboseBool) Logger.Message("Sending new npc settlement to server");
        }
        // Send data to the server if it changed
        private static void SendDataToServer() 
        {
            while (true)
            {
                if (newNPCSettlements.Count + newPlayerSettlements.Count > 0)
                {
                    NewWorldObjects data = new NewWorldObjects();
                    data._playerSettlements = newPlayerSettlements.ToArray();
                    data._npcSettlements = newNPCSettlements.ToArray();
                    data._planetNPCFaction = NPCFactionManager.newFactions.ToArray();
                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.NewWorldObjectData), data);
                    Network.listener.EnqueuePacket(packet);
                    newNPCSettlements.Clear();
                    newPlayerSettlements.Clear();
                    NPCFactionManager.newFactions.Clear();
                    SaveManager.ForceSave();
                }
                Thread.Sleep(5000);
            }
        }

        static WorldObjectManager() 
        {
            Task.Run(SendDataToServer);
        }
    }

    public static class WorldObjectManagerHelper 
    {
        public static int lastWorldObjectAdded;
        public static int lastWorldObjectRemoved; 
    }
}
