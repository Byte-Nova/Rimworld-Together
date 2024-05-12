using System;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;
using Verse.AI.Group;

namespace GameClient
{
    //Class that handles all functions of the offline raid feature

    public static class OfflineRaidManager
    {
        //Parses a packet into a useful order

        public static void ParseRaidPacket(Packet packet)
        {
            RaidData raidData = (RaidData)Serializer.ConvertBytesToObject(packet.contents);

            switch (int.Parse(raidData.raidStepMode))
            {
                case (int)CommonEnumerators.RaidStepMode.Request:
                    OnRaidAccept(raidData);
                    break;

                case (int)CommonEnumerators.RaidStepMode.Deny:
                    OnRaidDeny();
                    break;
            }
        }

        //Requests a raid to the server

        public static void RequestRaid()
        {
            DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for map"));

            RaidData raidData = new RaidData();
            raidData.raidStepMode = ((int)CommonEnumerators.RaidStepMode.Request).ToString();
            raidData.targetTile = ClientValues.chosenSettlement.Tile;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.RaidPacket), raidData);
            Network.listener.EnqueuePacket(packet);
        }

        //Executes when raid request is accepted

        private static void OnRaidAccept(RaidData raidData)
        {
            DialogManager.PopWaitDialog();

            MapFileData mapFileData = (MapFileData)Serializer.ConvertBytesToObject(raidData.mapData);
            MapData mapData = (MapData)Serializer.ConvertBytesToObject(mapFileData.mapData);

            Action r1 = delegate { PrepareMapForRaid(mapData); };

            if (ModManager.CheckIfMapHasConflictingMods(mapData))
            {
                DialogManager.PushNewDialog(new RT_Dialog_YesNo("Map received but contains unknown mod data, continue?", r1, null));
            }
            else DialogManager.PushNewDialog(new RT_Dialog_YesNo("Map received, continue?", r1, null));
        }

        //Executes when raid request is denied

        private static void OnRaidDeny()
        {
            DialogManager.PopWaitDialog();

            DialogManager.PushNewDialog(new RT_Dialog_Error("Player must not be connected!"));
        }

        //Prepares a map for the raid order from a request

        private static void PrepareMapForRaid(MapData mapData)
        {
            Map map = MapScribeManager.StringToMap(mapData, true, true, true, true, true, true, true);

            HandleMapFactions(map);

            SettlementUtility.Attack(ClientValues.chosenCaravan, ClientValues.chosenSettlement);

            PrepareMapLord(map);
        }

        //Handles the factions of a desired map for the raid order

        private static void HandleMapFactions(Map map)
        {
            foreach (Pawn pawn in map.mapPawns.AllPawns.ToArray())
            {
                if (pawn.Faction == FactionValues.neutralPlayer)
                {
                    pawn.SetFaction(FactionValues.enemyPlayer);
                }
            }

            foreach (Thing thing in map.listerThings.AllThings.ToArray())
            {
                if (thing.Faction == FactionValues.neutralPlayer)
                {
                    thing.SetFaction(FactionValues.enemyPlayer);
                }
            }
        }

        //Prepares the map lords of a desired map for the raid order

        private static void PrepareMapLord(Map map)
        {
            IntVec3 defensePlace = map.Center;
            Thing defenseSpot = map.listerThings.AllThings.Find(x => x.def.defName == "RTDefenseSpot");
            if (defenseSpot != null) defensePlace = defenseSpot.Position;

            Pawn[] lordPawns = map.mapPawns.AllPawns.ToList().FindAll(fetch => fetch.Faction == FactionValues.enemyPlayer).ToArray();
            LordJob_DefendBase job = new LordJob_DefendBase(FactionValues.enemyPlayer, defensePlace, true);
            LordMaker.MakeNewLord(FactionValues.enemyPlayer, job, map, lordPawns);
        }
    }
}
