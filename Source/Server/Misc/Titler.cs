using Shared;

namespace GameServer
{
    public static class Titler
    {
        public static void ChangeTitle()
        {
            Console.Title = $"Rimworld Together {CommonValues.executableVersion} - " +
                $"Players [{Network.connectedClients.Count}/{Program.serverConfig.MaxPlayers}]";
        }
    }
}
