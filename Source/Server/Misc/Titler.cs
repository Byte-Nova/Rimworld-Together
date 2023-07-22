using System;
using GameServer;

namespace Shared.Misc
{
    public static class Titler
    {
        public static void ChangeTitle()
        {
            Console.Title = $"Rimworld Together {Program.serverVersion} - " +
                $"Players [{Network.connectedClients.Count}/{Program.serverConfig.MaxPlayers}]";
        }
    }
}
