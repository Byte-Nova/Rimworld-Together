using System;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using RimworldTogether.GameClient.Dialogs;
using RimworldTogether.GameClient.Misc;
using RimworldTogether.GameClient.Planet;
using RimworldTogether.GameClient.Values;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.JSON.Actions;
using RimworldTogether.Shared.Misc;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using Verse;
using Verse.AI.Group;

namespace RimworldTogether.GameClient.Managers.Actions
{
    public static class OfflineVisitManager
    {
        private enum OfflineVisitStepMode { Request, Deny }

        public static void ParseOfflineVisitPacket(Packet packet)
        {
            OfflineVisitDetailsJSON offlineVisitDetails = (OfflineVisitDetailsJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

            switch (int.Parse(offlineVisitDetails.offlineVisitStepMode))
            {
                case (int)OfflineVisitStepMode.Request:
                    OnRequestAccepted(offlineVisitDetails);
                    break;

                case (int)OfflineVisitStepMode.Deny:
                    OnOfflineVisitDeny();
                    break;
            }
        }

        public static void OnOfflineVisitAccept()
        {
            DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for map"));

            OfflineVisitDetailsJSON offlineVisitDetailsJSON = new OfflineVisitDetailsJSON();
            offlineVisitDetailsJSON.offlineVisitStepMode = ((int)OfflineVisitStepMode.Request).ToString();
            offlineVisitDetailsJSON.offlineVisitData = ClientValues.chosenSettlement.Tile.ToString();

            Packet packet = Packet.CreatePacketFromJSON("OfflineVisitPacket", offlineVisitDetailsJSON);
            Network.Network.serverListener.SendData(packet);
        }

        private static void OnOfflineVisitDeny()
        {
            DialogManager.PopWaitDialog();

            DialogManager.PushNewDialog(new RT_Dialog_Error("Player must not be connected!"));
        }

        private static void OnRequestAccepted(OfflineVisitDetailsJSON offlineVisitDetailsJSON)
        {
            DialogManager.PopWaitDialog();

            MapDetailsJSON dummyDetails = Serializer.SerializeFromString<MapDetailsJSON>(offlineVisitDetailsJSON.offlineVisitData);
            byte[] inflatedBytes = GZip.Decompress(dummyDetails.deflatedMapData);
            string inflatedString = Encoding.UTF8.GetString(inflatedBytes);

            MapDetailsJSON mapDetailsJSON = Serializer.SerializeFromString<MapDetailsJSON>(inflatedString);

            Action r1 = delegate { PrepareMapForOfflineVisit(mapDetailsJSON); };

            if (ModManager.CheckIfMapHasConflictingMods(mapDetailsJSON))
            {
                DialogManager.PushNewDialog(new RT_Dialog_YesNo("Map received but contains unknown mod data, continue?", r1, null));
            }
            else DialogManager.PushNewDialog(new RT_Dialog_YesNo("Map received, continue?", r1, null));

            DialogManager.PushNewDialog(new RT_Dialog_OK("Game might hang temporarily depending on map complexity"));
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
            });
            DialogManager.PushNewDialog(d1);
        }

        private static void HandleMapFactions(Map map)
        {
            foreach (Pawn pawn in map.mapPawns.AllPawns.ToArray())
            {
                if (pawn.Faction == PlanetFactions.neutralPlayer)
                {
                    pawn.SetFaction(PlanetFactions.allyPlayer);
                }
            }

            foreach (Thing thing in map.listerThings.AllThings.ToArray())
            {
                if (thing.Faction == PlanetFactions.neutralPlayer)
                {
                    thing.SetFaction(PlanetFactions.allyPlayer);
                }
            }
        }

        private static void PrepareMapLord(Map map)
        {
            IntVec3 chillPlace = map.Center;
            Thing chillSpot = map.listerThings.AllThings.Find(x => x.def.defName == "RTChillSpot");
            if (chillSpot != null) chillPlace = chillSpot.Position;

            Pawn[] lordPawns = map.mapPawns.AllPawns.ToList().FindAll(fetch => fetch.Faction == PlanetFactions.allyPlayer).ToArray();
            LordJob_VisitColony job = new LordJob_VisitColony(PlanetFactions.allyPlayer, chillPlace, 999999999);
            LordMaker.MakeNewLord(PlanetFactions.allyPlayer, job, map, lordPawns);
        }
    }
}
