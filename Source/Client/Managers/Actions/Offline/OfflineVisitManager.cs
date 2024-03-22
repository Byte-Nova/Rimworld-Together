﻿using System;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;
using Verse.AI.Group;


namespace GameClient
{
    //Handles all the functions of the offline visit feature

    public static class OfflineVisitManager
    {
        //Parses packets into useful orders

        public static void ParseOfflineVisitPacket(Packet packet)
        {
            OfflineVisitDetailsJSON offlineVisitDetails = (OfflineVisitDetailsJSON)Serializer.ConvertBytesToObject(packet.contents);

            switch (int.Parse(offlineVisitDetails.offlineVisitStepMode))
            {
                case (int)CommonEnumerators.OfflineVisitStepMode.Request:
                    OnRequestAccepted(offlineVisitDetails);
                    break;

                case (int)CommonEnumerators.OfflineVisitStepMode.Deny:
                    OnOfflineVisitDeny();
                    break;
            }
        }

        //Requests a raid to the server

        public static void RequestOfflineVisit()
        {
            DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for map"));

            OfflineVisitDetailsJSON offlineVisitDetailsJSON = new OfflineVisitDetailsJSON();
            offlineVisitDetailsJSON.offlineVisitStepMode = ((int)CommonEnumerators.OfflineVisitStepMode.Request).ToString();
            offlineVisitDetailsJSON.targetTile = ClientValues.chosenSettlement.Tile.ToString();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OfflineVisitPacket), offlineVisitDetailsJSON);
            Network.listener.EnqueuePacket(packet);
        }

        //Executes when offline visit is denied

        private static void OnOfflineVisitDeny()
        {
            DialogManager.clearStack();

            DialogManager.PushNewDialog(new RT_Dialog_OK("ERROR", "Player must not be connected!"));
        }

        //Executes when offline visit is accepted

        private static void OnRequestAccepted(OfflineVisitDetailsJSON offlineVisitDetailsJSON)
        {
            DialogManager.PopDialog();

            MapFileJSON mapFileJSON = (MapFileJSON)Serializer.ConvertBytesToObject(offlineVisitDetailsJSON.mapDetails);
            MapDetailsJSON mapDetailsJSON = (MapDetailsJSON)Serializer.ConvertBytesToObject(mapFileJSON.mapData);

            Action r1 = delegate {
                DialogManager.PushNewDialog(new RT_Dialog_Wait("Loading Map...",
                            delegate { PrepareMapForOfflineVisit(mapDetailsJSON); })); 
                            };

            if (ModManager.CheckIfMapHasConflictingMods(mapDetailsJSON))
            {
                DialogManager.PushNewDialog(new RT_Dialog_YesNo("Map received but contains unknown mod data, continue?", r1, null));
            }
            else DialogManager.PushNewDialog(new RT_Dialog_YesNo("Map received, continue?", r1, null));

        }

        //Prepares a map for the offline visit feature from a request

        private static void PrepareMapForOfflineVisit(MapDetailsJSON mapDetailsJSON)
        {
            Map map = MapScribeManager.StringToMap(mapDetailsJSON, false, true, true, false);

            //keep track of one pawn in the caravan to jump to later
            Pawn pawnToFocus = (ClientValues.chosenCaravan.pawns.Count > 0) ? ClientValues.chosenCaravan.pawns[0] : null;

            HandleMapFactions(map);

            CaravanEnterMapUtility.Enter(ClientValues.chosenCaravan, map, CaravanEnterMode.Edge,
                CaravanDropInventoryMode.DoNotDrop, draftColonists: true);

            PrepareMapLord(map);

            //Switch to the Map mode and focus on the caravan
            CameraJumper.TryJump(pawnToFocus);

            RT_Dialog_OK_Loop d1 = new RT_Dialog_OK_Loop("MESSAGE", new string[]
            {
                "You are now in offline visit mode!",
                "This mode allows you to visit an offline player!",
                "To stop the visit exit the map creating a caravan"
            },
            DialogManager.clearStack);
            DialogManager.PushNewDialog(d1);
        }

        //Handles the factions of a desired map for the offline visit

        private static void HandleMapFactions(Map map)
        {
            foreach (Pawn pawn in map.mapPawns.AllPawns.ToArray())
            {
                if (pawn.Faction == FactionValues.neutralPlayer)
                {
                    pawn.SetFaction(FactionValues.allyPlayer);
                }
            }

            foreach (Thing thing in map.listerThings.AllThings.ToArray())
            {
                if (thing.Faction == FactionValues.neutralPlayer)
                {
                    thing.SetFaction(FactionValues.allyPlayer);
                }
            }
        }

        //Prepares the map lord of a desired map for the offline visit

        private static void PrepareMapLord(Map map)
        {
            IntVec3 chillPlace = map.Center;
            Thing chillSpot = map.listerThings.AllThings.Find(x => x.def.defName == "RTChillSpot");
            if (chillSpot != null) chillPlace = chillSpot.Position;

            Pawn[] lordPawns = map.mapPawns.AllPawns.ToList().FindAll(fetch => fetch.Faction == FactionValues.allyPlayer).ToArray();
            LordJob_VisitColony job = new LordJob_VisitColony(FactionValues.allyPlayer, chillPlace, 999999999);
            LordMaker.MakeNewLord(FactionValues.allyPlayer, job, map, lordPawns);
        }
    }
}
