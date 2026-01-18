using GameClient.Dialogs;
using GameClient.Misc;
using GameClient.Patches;
using GameClient.WorldObjects;
using TCPNetwork.Packets;
using RimWorld;
using RimWorld.Planet;
using Shared;
using System.Reflection;
using Verse;
using static Shared.CommonEnumerators;
using Shared.Files.Maps;
using GameClient.Hooks.TCPNetwork;

namespace GameClient.Managers
{
    public static class ActivityManager
    {
        [HandlesPacket(PacketHeader.ActivityManager)]
        private static void ParsePacket(byte[] bytes)
        {
            ActivityData data = Serializer.ConvertBytesToObject<ActivityData>(bytes);

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
            if (!SessionHandler.CurrentActionValues.ActivityAction.IsEnabled)
            {
                RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "This feature has been disabled in this server!" }));
                return;
            }

            SessionHandler.latestActivity = type;

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

            PrepareMap(Serializer.ConvertBytesToObject<MapFile>(data._mapRawData)); 
        }

        private static void OnDeny()
        {
            RT_Dialog_Wait.Instance.Close();

            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "This map is currently unavailable!" }));
        }

        private static void PrepareMap(MapFile mapFile)
        {
            Map map = null;

            if (SessionHandler.latestActivity == ActivityType.Raid)
            {
                map = MapSaveLoader.StringToMap(mapFile, true, true, true, true, true);
            }

            else if (SessionHandler.latestActivity == ActivityType.Zoom)
            {
                map = MapSaveLoader.StringToMap(mapFile, true, true, true, true, false);
            }

            Faction faction;
            if (SessionHandler.latestActivity == ActivityType.Raid) faction = SessionHandler.EnemyFaction;
            else faction = SessionHandler.NeutralFaction;

            RimworldManager.SetMapFactions(map, faction);

            RimworldManager.SetMapLord(map, faction);

            if (SessionHandler.latestActivity == ActivityType.Raid)
            {
                CaravanEnterMapUtility.Enter(SessionHandler.ChosenCaravan, SessionHandler.ChosenSettlement.Map, 
                    CaravanEnterMode.Edge, CaravanDropInventoryMode.DoNotDrop, draftColonists: true);

                CameraJumper.TryJump(map.Center, map, CameraJumper.MovementMode.Pan);
            }

            else if (SessionHandler.latestActivity == ActivityType.Zoom)
            {
                CameraJumper.TryJump(map.Center, map, CameraJumper.MovementMode.Pan);
            }
        }
    }
}
