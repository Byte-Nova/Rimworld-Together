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
    public static class RaidManager
    {
        private enum RaidStepMode { Request, Deny }

        public static void ParseRaidPacket(Packet packet)
        {
            RaidDetailsJSON raidDetailsJSON = (RaidDetailsJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

            switch (int.Parse(raidDetailsJSON.raidStepMode))
            {
                case (int)RaidStepMode.Request:
                    OnRaidAccept(raidDetailsJSON);
                    break;

                case (int)RaidStepMode.Deny:
                    OnRaidDeny();
                    break;
            }
        }

        public static void RequestRaid()
        {
            DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for map"));

            RaidDetailsJSON raidDetailsJSON = new RaidDetailsJSON();
            raidDetailsJSON.raidStepMode = ((int)RaidStepMode.Request).ToString();
            raidDetailsJSON.raidData = ClientValues.chosenSettlement.Tile.ToString();

            Packet packet = Packet.CreatePacketFromJSON("RaidPacket", raidDetailsJSON);
            Network.Network.serverListener.SendData(packet);
        }

        private static void OnRaidAccept(RaidDetailsJSON raidDetailsJSON)
        {
            DialogManager.PopWaitDialog();

            MapDetailsJSON dummyDetails = Serializer.SerializeFromString<MapDetailsJSON>(raidDetailsJSON.raidData);
            byte[] inflatedBytes = GZip.Decompress(dummyDetails.deflatedMapData);
            string inflatedString = Encoding.UTF8.GetString(inflatedBytes);

            MapDetailsJSON mapDetailsJSON = Serializer.SerializeFromString<MapDetailsJSON>(inflatedString);

            Action r1 = delegate { PrepareMapForRaid(mapDetailsJSON); };

            if (ModManager.CheckIfMapHasConflictingMods(mapDetailsJSON))
            {
                DialogManager.PushNewDialog(new RT_Dialog_YesNo("Map received but contains unknown mod data, continue?", r1, null));
            }
            else DialogManager.PushNewDialog(new RT_Dialog_YesNo("Map received, continue?", r1, null));

            DialogManager.PushNewDialog(new RT_Dialog_OK("Game might hang temporarily depending on map complexity"));
        }

        private static void OnRaidDeny()
        {
            DialogManager.PopWaitDialog();

            DialogManager.PushNewDialog(new RT_Dialog_Error("Player must not be connected!"));
        }

        private static void PrepareMapForRaid(MapDetailsJSON mapDetailsJSON)
        {
            Map map = DeepScribeManager.GetMapSimple(mapDetailsJSON, true, true, true, true);

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

        private static void PrepareMapLord(Map map)
        {
            IntVec3 defensePlace = map.Center;
            Thing defenseSpot = map.listerThings.AllThings.Find(x => x.def.defName == "RTDefenseSpot");
            if (defenseSpot != null) defensePlace = defenseSpot.Position;

            Pawn[] lordPawns = map.mapPawns.AllPawns.ToList().FindAll(fetch => fetch.Faction == PlanetFactions.enemyPlayer).ToArray();
            LordJob_DefendBase job = new LordJob_DefendBase(PlanetFactions.enemyPlayer, defensePlace, true);
            LordMaker.MakeNewLord(PlanetFactions.enemyPlayer, job, map, lordPawns);
        }
    }
}
