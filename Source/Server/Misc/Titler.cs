using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
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
