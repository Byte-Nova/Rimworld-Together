using GameServer.Core;
using RTShared;
using RTNetwork.Packets;
using RTShared.Files.ServerClient;
using GameServer.Managers;
using RTNetwork.PacketManagers;
using static RTNetwork.Packets.PKT_Zoom;
using RTShared.Files.Player;
using RTNetwork.Components;

namespace GameServer.PacketManager
{
    public class PM_Zoom : PM_Base
    {
        [HandlesPacket(PacketHeader.Zoom)]
        public override void Receive(ServerClient client, byte[] bytes, PacketHeader header)
        {
            if (!FL_PlayerCooldown.CheckIfCanZoom(client.GetData<FL_Player>(), Master.ActionConfigs.ZoomAction)) ResponseShortcutManager.SendUnavailablePacket(client);
            else
            {
                PKT_Zoom data = Serializer.ConvertBytesToObject<PKT_Zoom>(bytes);

                switch (data.CurrentStepMode)
                {
                    case StepMode.Request:
                        SendRequestedMap(client, data);
                        break;
                }

                client.GetData<FL_Player>().Cooldowns.SetZoomTimer(client.GetData<FL_Player>());
            }
        }

        private static void SendRequestedMap(ServerClient client, PKT_Zoom data)
        {
            if (!PM_Map.CheckIfMapExists(data.TargetTile))
            {
                data.CurrentStepMode = StepMode.Deny;
                client.Listener.EnqueuePacket(PacketHeader.Zoom, data);
            }

            else
            {
                data.CurrentStepMode = StepMode.Request;
                data.Map = PM_Map.GetMapFromTile(data.TargetTile);
                client.Listener.EnqueuePacket(PacketHeader.Zoom, data);
            }
        }
    }
}
