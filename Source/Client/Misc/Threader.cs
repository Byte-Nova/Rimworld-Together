using System;
using System.Threading.Tasks;
using GameClient.Managers;
using TCPNetwork;

namespace GameClient.Misc
{
    public static class Threader
    {
        public enum Mode { Chat }

        public static Task GenerateThread(Mode mode, Listener listener = null)
        {
            return mode switch
            {
                Mode.Chat => Task.Run(ChatManager.ChatClock),
                _ => throw new NotImplementedException()
            };
        }
    }
}