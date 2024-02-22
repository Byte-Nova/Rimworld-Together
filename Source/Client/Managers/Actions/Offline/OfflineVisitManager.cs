using System;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;
using Verse.AI.Group;


namespace GameClient
{
    public static class OfflineVisitManager
    {
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

        public static void OnOfflineVisitAccept()
        {
            DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for map"));

            OfflineVisitDetailsJSON offlineVisitDetailsJSON = new OfflineVisitDetailsJSON();
            offlineVisitDetailsJSON.offlineVisitStepMode = ((int)CommonEnumerators.OfflineVisitStepMode.Request).ToString();
            offlineVisitDetailsJSON.targetTile = ClientValues.chosenSettlement.Tile.ToString();

            Packet packet = Packet.CreatePacketFromJSON("OfflineVisitPacket", offlineVisitDetailsJSON);
            Network.listener.dataQueue.Enqueue(packet);
        }

        private static void OnOfflineVisitDeny()
        {
            DialogManager.PopDialog();

            DialogManager.PushNewDialog(new RT_Dialog_Error("Player must not be connected!", DialogManager.PopDialog));
        }

        private static void OnRequestAccepted(OfflineVisitDetailsJSON offlineVisitDetailsJSON)
        {
            DialogManager.PopDialog();

            MapFileJSON mapFileJSON = (MapFileJSON)Serializer.ConvertBytesToObject(offlineVisitDetailsJSON.mapDetails);

            MapDetailsJSON mapDetailsJSON = (MapDetailsJSON)Serializer.ConvertBytesToObject(mapFileJSON.mapData);

            Action r1 = delegate { PrepareMapForOfflineVisit(mapDetailsJSON); };

            if (ModManager.CheckIfMapHasConflictingMods(mapDetailsJSON))
            {
                DialogManager.WaitForDialogInput(new RT_Dialog_YesNo("Map received but contains unknown mod data, continue?", r1, DialogManager.PopDialog));
            }
            else DialogManager.WaitForDialogInput(new RT_Dialog_YesNo("Map received, continue?", r1, DialogManager.ClearStack));

            DialogManager.WaitForDialogInput(new RT_Dialog_OK("Game might hang temporarily depending on map complexity"));
        }

        private static void PrepareMapForOfflineVisit(MapDetailsJSON mapDetailsJSON)
        {
            Map map = DeepScribeManager.GetMapSimple(mapDetailsJSON, false, true, true, false);

            HandleMapFactions(map);

            CaravanEnterMapUtility.Enter(ClientValues.chosenCaravan, map, CaravanEnterMode.Edge,
                CaravanDropInventoryMode.DoNotDrop, draftColonists: true);

            PrepareMapLord(map);

            RT_Dialog_OK_Loop d1 = new RT_Dialog_OK_Loop(new string[]
            {
                "You are now in offline visit mode!",
                "This mode allows you to visit an offline player!",
                "To stop the visit exit the map creating a caravan"
            }, delegate { DialogManager.ClearStack(); });

            DialogManager.PushNewDialog(d1);
        }

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
