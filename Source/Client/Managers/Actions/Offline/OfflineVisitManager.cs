using System;
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
            DialogManager.PushNewDialog(new RT_Dialog_Wait("RimworldTogether.WaitingMap".Translate()));

            OfflineVisitDetailsJSON offlineVisitDetailsJSON = new OfflineVisitDetailsJSON();
            offlineVisitDetailsJSON.offlineVisitStepMode = ((int)CommonEnumerators.OfflineVisitStepMode.Request).ToString();
            offlineVisitDetailsJSON.targetTile = ClientValues.chosenSettlement.Tile.ToString();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OfflineVisitPacket), offlineVisitDetailsJSON);
            Network.listener.EnqueuePacket(packet);
        }

        //Executes when offline visit is denied

        private static void OnOfflineVisitDeny()
        {
            DialogManager.PopWaitDialog();

            DialogManager.PushNewDialog(new RT_Dialog_Error("RimworldTogether.PlayerOffline".Translate()));
        }

        //Executes when offline visit is accepted

        private static void OnRequestAccepted(OfflineVisitDetailsJSON offlineVisitDetailsJSON)
        {
            DialogManager.PopWaitDialog();

            MapFileJSON mapFileJSON = (MapFileJSON)Serializer.ConvertBytesToObject(offlineVisitDetailsJSON.mapDetails);
            MapDetailsJSON mapDetailsJSON = (MapDetailsJSON)Serializer.ConvertBytesToObject(mapFileJSON.mapData);

            Action r1 = delegate { PrepareMapForOfflineVisit(mapDetailsJSON); };

            if (ModManager.CheckIfMapHasConflictingMods(mapDetailsJSON))
            {
                DialogManager.PushNewDialog(new RT_Dialog_YesNo("RimworldTogether.DataDifference".Translate(), r1, null));
            }
            else DialogManager.PushNewDialog(new RT_Dialog_YesNo("RimworldTogether.MapReady".Translate(), r1, null));

            DialogManager.PushNewDialog(new RT_Dialog_OK("RimworldTogether.MapLoadHang".Translate()));
        }

        //Prepares a map for the offline visit feature from a request

        private static void PrepareMapForOfflineVisit(MapDetailsJSON mapDetailsJSON)
        {
            Map map = MapScribeManager.StringToMap(mapDetailsJSON, false, true, true, false);

            HandleMapFactions(map);

            CaravanEnterMapUtility.Enter(ClientValues.chosenCaravan, map, CaravanEnterMode.Edge,
                CaravanDropInventoryMode.DoNotDrop, draftColonists: true);

            PrepareMapLord(map);

            RT_Dialog_OK_Loop d1 = new RT_Dialog_OK_Loop(new string[]
            {
                "RimworldTogether.OfflineVisitMod".Translate(),
                "RimworldTogether.OfflineModNote".Translate(),
                "RimworldTogether.MapLeaveNote".Translate()
            });
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
