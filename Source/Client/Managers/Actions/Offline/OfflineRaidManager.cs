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
            RaidDetailsJSON raidDetailsJSON = (RaidDetailsJSON)Serializer.ConvertBytesToObject(packet.contents);

            switch (int.Parse(raidDetailsJSON.raidStepMode))
            {
                case (int)CommonEnumerators.RaidStepMode.Request:
                    OnRaidAccept(raidDetailsJSON);
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

            RaidDetailsJSON raidDetailsJSON = new RaidDetailsJSON();
            raidDetailsJSON.raidStepMode = ((int)CommonEnumerators.RaidStepMode.Request).ToString();
            raidDetailsJSON.targetTile = ClientValues.chosenSettlement.Tile.ToString();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.RaidPacket), raidDetailsJSON);
            Network.listener.EnqueuePacket(packet);
        }

        //Executes when raid request is accepted

        private static void OnRaidAccept(RaidDetailsJSON raidDetailsJSON)
        {
            DialogManager.PopDialog();

            MapFileJSON mapFileJSON = (MapFileJSON)Serializer.ConvertBytesToObject(raidDetailsJSON.mapDetails);
            MapDetailsJSON mapDetailsJSON = (MapDetailsJSON)Serializer.ConvertBytesToObject(mapFileJSON.mapData);

            Action r1 = delegate { PrepareMapForRaid(mapDetailsJSON); };

            if (ModManager.CheckIfMapHasConflictingMods(mapDetailsJSON))
            {
                DialogManager.PushNewDialog(new RT_Dialog_YesNo("Map received but contains unknown mod data, continue?", r1, null));
            }
            else DialogManager.PushNewDialog(new RT_Dialog_YesNo("Map received, continue?", r1, null));

            DialogManager.PushNewDialog(new RT_Dialog_OK("Game might hang temporarily depending on map complexity"));
        }

        //Executes when raid request is denied

        private static void OnRaidDeny()
        {
            DialogManager.PopDialog();

            DialogManager.PushNewDialog(new RT_Dialog_Error("Player must not be connected!"));
        }

        //Prepares a map for the raid order from a request

        private static void PrepareMapForRaid(MapDetailsJSON mapDetailsJSON)
        {
            Map map = MapScribeManager.StringToMap(mapDetailsJSON, true, true, true, true);

            HandleMapFactions(map);

            SettlementUtility.Attack(ClientValues.chosenCaravan, ClientValues.chosenSettlement);

            PrepareMapLord(map);

            RT_Dialog_OK_Loop d1 = new RT_Dialog_OK_Loop(new string[]
            {
                "You are now in raid mode!",
                "Raid mode allows you to raid player settlements",
                "Down all their enemy pawns and get loot for it!",
            });
            DialogManager.PushNewDialog(d1);
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
