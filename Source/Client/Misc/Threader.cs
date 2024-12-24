using System;
using System.Threading.Tasks;

namespace GameClient
{
    public static class Threader
    {
        public enum Mode { Listener, Sender, Health, KASender, Chat, Activity }

        public static Task GenerateThread(Mode mode)
        {
            return mode switch
            {
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