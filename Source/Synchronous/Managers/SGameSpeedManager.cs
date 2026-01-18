using GameClient;
using GameClient.Hooks.TCPNetwork;
using GameClient.Misc;
using RimWorld;
using Shared;
using Shared.Misc;
using Synchronous.Misc;
using Synchronous.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Synchronous.Managers
{
    public static class SGameSpeedManager
    {
        public static TimeSpeed LatestGameSpeed { get; private set; } = TimeSpeed.Paused;

        [OnSynchronousStart]
        private static void SendFirstSpeed()
        {
            if (SessionHandler.IsSynchronousHost)
            {
                PlayerGameSpeed data = new PlayerGameSpeed();
                data.CurrentGameSpeed = (int)TimeSpeed.Paused;
                data.TimeTicks = Find.TickManager.TicksSinceSettle;
                ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.SPlayerGameSpeed, data);
            }
        }

        public static void Ask(TimeSpeed speed)
        {
            if (speed == LatestGameSpeed) return;
            else
            {
                PlayerGameSpeed data = new PlayerGameSpeed();
                data.CurrentGameSpeed = (int)speed;
                data.TimeTicks = Find.TickManager.TicksSinceSettle;

                ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.SPlayerGameSpeed, data);
            }
        }

        [HandlesPacket(PacketHeader.SPlayerGameSpeed)]
        private static void Receive(byte[] bytes)
        {
            PlayerGameSpeed data = Serializer.ConvertBytesToObject<PlayerGameSpeed>(bytes);

            PatchHandler.ExecuteInBypass(delegate
            {
                Find.TickManager.CurTimeSpeed = (TimeSpeed)data.CurrentGameSpeed;
                Find.TickManager.DebugSetTicksGame(data.TimeTicks);
                LatestGameSpeed = Find.TickManager.CurTimeSpeed;
            });
        }
    }
}
