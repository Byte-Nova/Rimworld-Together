using System;
using System.Threading.Tasks;
using GameClient.Managers;
using GameClient.TCP;

namespace GameClient.Misc
{
    public static class Threader
    {
        public enum Mode { Start, Listener, Sender, Health, KASender, Chat, Activity }

        public static Task GenerateThread(Mode mode)
        {
            return mode switch
            {
                Mode.Start => Task.Run(Network.StartConnection),
                Mode.Listener => Task.Run(Network.listener.Listen),
                Mode.Sender => Task.Run(Network.listener.SendData),
                Mode.Health => Task.Run(Network.listener.CheckConnectionHealth),
                Mode.KASender => Task.Run(Network.listener.SendKAFlag),
                Mode.Chat => Task.Run(ChatManager.ChatClock),
                Mode.Activity => Task.Run(OnlineActivityClock.StartBufferClock),
                _ => throw new NotImplementedException()
            };
        }
    }
}