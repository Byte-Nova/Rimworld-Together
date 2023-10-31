using RimworldTogether.GameServer.Managers;
using RimworldTogether.GameServer.Managers.Actions;
using RimworldTogether.GameServer.Network;

namespace RimworldTogether.GameServer.Misc
{
    public static class Threader
    {
        public enum ServerMode
        {
            Start,
            Heartbeat,
            Sites,
            Console
        }

        public static Task GenerateServerThread(ServerMode mode, CancellationToken cancellationToken)
        {
            switch (mode)
            {
                case ServerMode.Start:
                    return Task.Run(Network.Network.ReadyServer, cancellationToken);

                case ServerMode.Heartbeat:
                    return Task.Run(Network.Network.HearbeatClients, cancellationToken);

                case ServerMode.Sites:
                    return Task.Run(SiteManager.StartSiteTicker, cancellationToken);

                case ServerMode.Console:
                    return Task.Run(ServerCommandManager.ListenForServerCommands, cancellationToken);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}