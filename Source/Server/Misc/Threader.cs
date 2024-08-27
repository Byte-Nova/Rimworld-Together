namespace GameServer
{
    public static class Threader
    {
        public enum ServerMode { Start, Sites, Caravans, Console }

        public static Task GenerateServerThread(ServerMode mode)
        {
            return mode switch
            {
                ServerMode.Start => Task.Run(Network.ReadyServer),
                ServerMode.Sites => Task.Run(SiteManager.StartSiteTicker),
                ServerMode.Caravans => Task.Run(CaravanManager.StartCaravanTicker),
                ServerMode.Console => Task.Run(ConsoleCommandManager.ListenForServerCommands),
                _ => throw new NotImplementedException(),
            };
        }

        public enum ClientMode { Listener, Sender, Health, KAFlag }

        public static Task GenerateClientThread(Listener listener, ClientMode mode)
        {
            return mode switch
            {
                ClientMode.Listener => Task.Run(listener.Listen),
                ClientMode.Sender => Task.Run(listener.SendData),
                ClientMode.Health => Task.Run(listener.CheckConnectionHealth),
                ClientMode.KAFlag => Task.Run(listener.CheckKAFlag),
                _ => throw new NotImplementedException(),
            };
        }

        public enum DiscordMode { Start, Console, Count }

        public static Task GenerateDiscordThread(DiscordMode mode)
        {
            return mode switch
            {
                DiscordMode.Start => Task.Run(DiscordManager.TryStartDiscordIntegration),
                DiscordMode.Console => Task.Run(DiscordManager.LoopMessagesToConsoleChannel),
                DiscordMode.Count => Task.Run(DiscordManager.LoopUpdatePlayerCount),
                _ => throw new NotImplementedException(),
            };
        }
    }
}