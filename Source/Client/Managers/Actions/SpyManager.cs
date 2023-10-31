using System;
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

namespace RimworldTogether.GameClient.Managers.Actions
{
    public static class SpyManager
    {
        private enum SpyStepMode { Request, Deny }

        public static int spyCost;

        public static void ParseSpyPacket(Packet packet)
        {
            SpyDetailsJSON spyDetailsJSON = (SpyDetailsJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

            switch(int.Parse(spyDetailsJSON.spyStepMode))
            {
                case (int)SpyStepMode.Request:
                    OnSpyAccept(spyDetailsJSON);
                    break;

                case (int)SpyStepMode.Deny:
                    OnSpyDeny();
                    break;
            }
        }

        public static void SetSpyCost(ServerOverallJSON serverOverallJSON)
        {
            try { spyCost = int.Parse(serverOverallJSON.SpyCost); }
            catch
            {
                Log.Warning("Server didn't have spy cost set, defaulting to 0");

                spyCost = 0;
            }
        }

        public static void RequestSpy()
        {
            Action r1 = delegate
            {
                if (!RimworldManager.CheckIfHasEnoughSilverInCaravan(spyCost))
                {
                    DialogManager.PushNewDialog(new RT_Dialog_Error("You do not have enough silver!"));
                }

                else
                {
                    RimworldManager.RemoveThingFromCaravan(ThingDefOf.Silver, spyCost);

                    DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for map"));

                    SpyDetailsJSON spyDetailsJSON = new SpyDetailsJSON();
                    spyDetailsJSON.spyStepMode = ((int)SpyStepMode.Request).ToString();
                    spyDetailsJSON.spyData = ClientValues.chosenSettlement.Tile.ToString();

                    Packet packet = Packet.CreatePacketFromJSON("SpyPacket", spyDetailsJSON);
                    Network.Network.serverListener.SendData(packet);
                }
            };

            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo($"Spying a settlement costs {spyCost} silver, continue?", r1, null);
            DialogManager.PushNewDialog(d1);
        }

        private static void OnSpyAccept(SpyDetailsJSON spyDetailsJSON)
        {
            DialogManager.PopWaitDialog();

            MapDetailsJSON dummyDetails = Serializer.SerializeFromString<MapDetailsJSON>(spyDetailsJSON.spyData);
            byte[] inflatedBytes = GZip.Decompress(dummyDetails.deflatedMapData);
            string inflatedString = Encoding.UTF8.GetString(inflatedBytes);

            MapDetailsJSON mapDetailsJSON = Serializer.SerializeFromString<MapDetailsJSON>(inflatedString);

            Action r1 = delegate { PrepareMapForSpy(mapDetailsJSON); };

            if (ModManager.CheckIfMapHasConflictingMods(mapDetailsJSON))
            {
                DialogManager.PushNewDialog(new RT_Dialog_YesNo("Map received but contains unknown mod data, continue?", r1, null));
            }
            else DialogManager.PushNewDialog(new RT_Dialog_YesNo("Map received, continue?", r1, null));

            DialogManager.PushNewDialog(new RT_Dialog_OK("Game might hang temporarily depending on map complexity"));
        }

        private static void OnSpyDeny()
        {
            DialogManager.PopWaitDialog();

            TransferManager.SendSilverToCaravan(spyCost);

            DialogManager.PushNewDialog(new RT_Dialog_OK("Spent silver has been recovered"));

            DialogManager.PushNewDialog(new RT_Dialog_Error("Player must not be connected!"));
        }

        private static void PrepareMapForSpy(MapDetailsJSON mapDetailsJSON)
        {
            Map map = DeepScribeManager.GetMapSimple(mapDetailsJSON, false, false, false, false);

            HandleMapFactions(map);

            CaravanEnterMapUtility.Enter(ClientValues.chosenCaravan, map, CaravanEnterMode.Edge,
                CaravanDropInventoryMode.DoNotDrop, draftColonists: true);

            RT_Dialog_OK_Loop d1 = new RT_Dialog_OK_Loop(new string[]
            {
                "You are now in spy mode!",
                "Spy mode allows you to check out another player's base",
                "To stop the spy exit the map creating a caravan"
            });
            DialogManager.PushNewDialog(d1);
        }

        private static void HandleMapFactions(Map map)
        {
            foreach (Pawn pawn in map.mapPawns.AllPawns.ToArray())
            {
                if (pawn.Faction == PlanetFactions.neutralPlayer)
                {
                    pawn.SetFaction(PlanetFactions.enemyPlayer);
                }
            }

            foreach (Thing thing in map.listerThings.AllThings.ToArray())
            {
                if (thing.Faction == PlanetFactions.neutralPlayer)
                {
                    thing.SetFaction(PlanetFactions.enemyPlayer);
                }
            }
        }
    }
}
