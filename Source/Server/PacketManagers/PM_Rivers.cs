using RTNetwork.Components;
using RTNetwork.PacketManagers;
using RTNetwork.Packets;
using RTServer.Core;
using RTServer.Managers;
using RTShared.Details.Planet;
using RTShared.Files.Player;
using RTShared.Misc;

namespace RTServer.PacketManagers
{
    public class PM_Rivers : PM_Base
    {
        [HandlesPacket(PacketHeader.River)]
        public override void Receive(ServerClient client, byte[] bytes, PacketHeader header)
        {
            if (!FL_PlayerCooldown.CheckIfCanRiver(client.GetData<FL_Player>(), Master.ActionConfigs.RiverAction)) ResponseShortcutManager.SendUnavailablePacket(client);
            else
            {
                PKT_River data = Serializer.ConvertBytesToObject<PKT_River>(bytes);

                switch (data.CurrentStepMode)
                {
                    case PKT_River.StepMode.Add:
                        throw new NotImplementedException();
                        break;

                    case PKT_River.StepMode.Remove:
                        throw new NotImplementedException();
                        break;
                    
                    case PKT_River.StepMode.Bulk:
                        AddRiversBulk(client, data);
                        break;
                }

                client.GetData<FL_Player>().Cooldowns.SetRiverTimer(client.GetData<FL_Player>());
            }
        }
        
        private static void AddRiversBulk(ServerClient client, PKT_River packet)
        {
            if (!client.GetData<FL_Player>().IsAdmin && PM_World.CheckIfWorldExists()) client.Listener.MarkForDisconnect();
            else
            {
                Master.RiverFile.AddBulk(packet.Rivers);
                client.Listener.EnqueuePacket(PacketHeader.River, packet);
                Printer.Warning($"[Set rivers] > {client.GetData<FL_Player>().Username}");   
            }
        }
        
        public static List<RiverDetail> GetAllRivers() { return Master.RiverFile.Rivers; }
    }
}