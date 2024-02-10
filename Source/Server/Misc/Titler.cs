using RimworldTogether.GameServer.Core;

namespace RimworldTogether.GameServer.Misc
{
    public static class Titler
    {
        public static void ChangeTitle()
        {
            Console.Title = $"Rimworld Together {Program.serverVersion} - " +
                $"Players [{Network.Network.connectedClients.Count}/{Program.serverConfig.MaxPlayers}]";
        }
    }
}
