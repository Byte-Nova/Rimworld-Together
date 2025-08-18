using GameClient.Dialogs;
using GameClient.Misc;
using GameClient.Patches;
using GameClient.Values;
using GameClient.WorldObjects;
using TCPNetwork.Packets;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Shared.Files;
using System.Reflection;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Managers
{
    public static class ActivityManager
    {
        [HandlesPacket(PacketHeader.ActivityManager)]
        private static void ParsePacket(byte[] bytes)
        {
            ActivityData data = Serializer.ConvertBytesToObject<ActivityData>(bytes);

            Printer.Warning(data, LogImportanceMode.Extreme);

            switch (data._stepMode)
            {
                case ActivityStepMode.Request:
                    OnAccept(data);
                    break;

                case ActivityStepMode.Deny:
                    OnDeny();
                    break;
            }
        }

        public static void RequestActivity(ActivityType type, int targetTile)
        {
            if (!SessionValues.ActionValues.EnableActivities)
            {
                RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "This feature has been disabled in this server!" }));
                return;
            }

            SessionValues.ToggleActivity(type);

            SendRequest(targetTile);
        }

        private static void SendRequest(int targetTile)
        {
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Wait("Waiting for map"));

            ActivityData data = new ActivityData();
            data._stepMode = ActivityStepMode.Request;
            data._targetTile = targetTile;

            ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.ActivityManager, data);
        }

        private static void OnAccept(ActivityData data) 
        {
            RT_Dialog_Wait.Instance.Close();

            PrepareMap(data._mapFile); 
        }

        private static void OnDeny()
        {
            RT_Dialog_Wait.Instance.Close();

            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "This map is currently unavailable!" }));
        }

        private static void PrepareMap(MapFile mapFile)
        {
            Map map = null;

            if (SessionValues.latestActivity == ActivityType.Raid)
            {
                map = MapSaveLoader.StringToMap(mapFile, true, true, true, true, true, true, true);
            }

            else if (SessionValues.latestActivity == ActivityType.Zoom)
            {
                map = MapSaveLoader.StringToMap(mapFile, true, true, true, true, true, true, false);
            }

            Faction faction;
            if (SessionValues.latestActivity == ActivityType.Raid) faction = ClientValues.EnemyPlayer;
            else faction = ClientValues.NeutralPlayer;

            RimworldManager.SetMapFactions(map, faction);

            RimworldManager.SetMapLord(map, faction);

            if (SessionValues.latestActivity == ActivityType.Raid)
            {
                CaravanEnterMapUtility.Enter(SessionValues.ChosenCaravan, SessionValues.ChosenSettlement.Map, 
                    CaravanEnterMode.Edge, CaravanDropInventoryMode.DoNotDrop, draftColonists: true);

                CameraJumper.TryJump(map.Center, map, CameraJumper.MovementMode.Pan);
            }

            else if (SessionValues.latestActivity == ActivityType.Zoom)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);
                Caravan caravan = CaravanMaker.MakeCaravan(new Pawn[] { pawn }, Faction.OfPlayer, map.Tile, true);
                CaravanEnterMapUtility.Enter(caravan, map, CaravanEnterMode.Edge);

                CameraJumper.TryJump(map.Center, map, CameraJumper.MovementMode.Pan);
                pawn.Destroy();
            }
        }
    }
}
