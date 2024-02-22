using System;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;
using RimworldTogether.GameClient.Misc;


namespace GameClient
{
    public static class OfflineSpyManager
    {
        public static int spyCost;

        public static void ParseSpyPacket(Packet packet)
        {
            SpyDetailsJSON spyDetailsJSON = (SpyDetailsJSON)Serializer.ConvertBytesToObject(packet.contents);

            switch(int.Parse(spyDetailsJSON.spyStepMode))
            {
                case (int)CommonEnumerators.SpyStepMode.Request:
                    OnSpyAccept(spyDetailsJSON);
                    break;

                case (int)CommonEnumerators.SpyStepMode.Deny:
                    OnSpyDeny();
                    break;
            }
        }

        public static void SetSpyCost(ServerOverallJSON serverOverallJSON)
        {
            try { spyCost = int.Parse(serverOverallJSON.SpyCost); }
            catch
            {
                Logs.Warning("Server didn't have spy cost set, defaulting to 0");

                spyCost = 0;
            }
        }

        public static void RequestSpy()
        {
            Action r1 = delegate
            {
                if (!RimworldManager.CheckIfHasEnoughSilverInCaravan(spyCost))
                {
                    DialogManager.PushNewDialog(new RT_Dialog_Error("You do not have enough silver!", DialogManager.PopDialog));
                }

                else
                {
                    RimworldManager.RemoveThingFromCaravan(ThingDefOf.Silver, spyCost);

                    DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for map"));

                    SpyDetailsJSON spyDetailsJSON = new SpyDetailsJSON();
                    spyDetailsJSON.spyStepMode = ((int)CommonEnumerators.SpyStepMode.Request).ToString();
                    spyDetailsJSON.targetTile = ClientValues.chosenSettlement.Tile.ToString();

                    Packet packet = Packet.CreatePacketFromJSON("SpyPacket", spyDetailsJSON);
                    Network.listener.dataQueue.Enqueue(packet);
                }
            };

            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo($"Spying a settlement costs {spyCost} silver, continue?", r1, DialogManager.PopDialog);
            DialogManager.PushNewDialog(d1);
        }

        private static void OnSpyAccept(SpyDetailsJSON spyDetailsJSON)
        {
            DialogManager.PopDialog();

            MapFileJSON mapFileJSON = (MapFileJSON)Serializer.ConvertBytesToObject(spyDetailsJSON.mapDetails);
            MapDetailsJSON mapDetailsJSON = (MapDetailsJSON)Serializer.ConvertBytesToObject(mapFileJSON.mapData);

            Action r1 = delegate { PrepareMapForSpy(mapDetailsJSON); };

            if (ModManager.CheckIfMapHasConflictingMods(mapDetailsJSON))
            {
                DialogManager.PushNewDialog(new RT_Dialog_YesNo("Map received but contains unknown mod data, continue?", r1, DialogManager.PopDialog));
            }
            else DialogManager.PushNewDialog(new RT_Dialog_YesNo("Map received, continue?", r1, DialogManager.PopDialog));

            DialogManager.PushNewDialog(new RT_Dialog_OK("Game might hang temporarily depending on map complexity"));
        }

        private static void OnSpyDeny()
        {
            DialogManager.PopDialog();

            TransferManager.SendSilverToCaravan(spyCost);

            DialogManager.PushNewDialog(new RT_Dialog_OK("Spent silver has been recovered"));

            DialogManager.PushNewDialog(new RT_Dialog_Error("Player must not be connected!", DialogManager.PopDialog));
        }

        private static void PrepareMapForSpy(MapDetailsJSON mapDetailsJSON)
        {
            Map map = DeepScribeManager.GenerateCustomMap(mapDetailsJSON, false, false, false, false);

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
    }
}
