using RimworldTogether.GameServer.Managers;
using RimworldTogether.GameServer.Managers.Actions;
using RimworldTogether.GameServer.Network.Listener;

namespace RimworldTogether.GameServer.Misc
{
    public static class Threader
    {
        public enum ServerMode
        {
            Start,
            Sites,
            Console
        }

        public static Task GenerateServerThread(ServerMode mode, CancellationToken cancellationToken)
        {
            switch (mode)
            {
                case ServerMode.Start:
                    return Task.Run(Network.Network.ReadyServer, cancellationToken);

                case ServerMode.Sites:
                    return Task.Run(SiteManager.StartSiteTicker, cancellationToken);

                case ServerMode.Console:
                    return Task.Run(ServerCommandManager.ListenForServerCommands, cancellationToken);

                default:
                    throw new NotImplementedException();
            }
        }

        public enum ClientMode
        {
            Listener,
            Health,
            KAFlag
        }

        public static Task GenerateClientThread(ClientListener listener, ClientMode mode, CancellationToken cancellationToken)
        {
            switch (mode)
            {
                case ClientMode.Listener:
                    return Task.Run(listener.ListenToClient, cancellationToken);

                case ClientMode.Health:
                    return Task.Run(listener.CheckForConnectionHealth, cancellationToken);

                case ClientMode.KAFlag:
                    return Task.Run(listener.CheckForKAFlag, cancellationToken);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}