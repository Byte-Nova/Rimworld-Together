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
            OfflineVisitData offlineVisitData = (OfflineVisitData)Serializer.ConvertBytesToObject(packet.contents);

            switch (int.Parse(offlineVisitData.offlineVisitStepMode))
            {
                case (int)CommonEnumerators.OfflineVisitStepMode.Request:
                    OnRequestAccepted(offlineVisitData);
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

            OfflineVisitData offlineVisitData = new OfflineVisitData();
            offlineVisitData.offlineVisitStepMode = ((int)CommonEnumerators.OfflineVisitStepMode.Request).ToString();
            offlineVisitData.targetTile = ClientValues.chosenSettlement.Tile;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OfflineVisitPacket), offlineVisitData);
            Network.listener.EnqueuePacket(packet);
        }

        //Executes when offline visit is denied

        private static void OnOfflineVisitDeny()
        {
            DialogManager.PopWaitDialog();

            DialogManager.PushNewDialog(new RT_Dialog_Error("Player must not be connected!"));
        }

        //Executes when offline visit is accepted

        private static void OnRequestAccepted(OfflineVisitData offlineVisitData)
        {
            DialogManager.PopWaitDialog();

            MapFileData mapFileData = (MapFileData)Serializer.ConvertBytesToObject(offlineVisitData.mapData);
            MapData mapData = (MapData)Serializer.ConvertBytesToObject(mapFileData.mapData);

            Action r1 = delegate { PrepareMapForOfflineVisit(mapData); };

            if (ModManager.CheckIfMapHasConflictingMods(mapData))
            {
                DialogManager.PushNewDialog(new RT_Dialog_YesNo("Map received but contains unknown mod data, continue?", r1, null));
            }
            else DialogManager.PushNewDialog(new RT_Dialog_YesNo("Map received, continue?", r1, null));
        }

        //Prepares a map for the offline visit feature from a request

        private static void PrepareMapForOfflineVisit(MapData mapData)
        {
            Map map = MapScribeManager.StringToMap(mapData, false, true, true, true, true, true);

            HandleMapFactions(map);

            CaravanEnterMapUtility.Enter(ClientValues.chosenCaravan, map, CaravanEnterMode.Edge,
                CaravanDropInventoryMode.DoNotDrop, draftColonists: true);

            PrepareMapLord(map);
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
