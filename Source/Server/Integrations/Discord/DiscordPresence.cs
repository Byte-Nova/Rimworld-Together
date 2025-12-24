using Discord;
using Discord.WebSocket;
using GameServer.Core;
using Shared.Misc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GameServer.Integrations.Discord
{
    public static class DiscordPresence
    {
        private static readonly SemaphoreSlim StartStopSemaphore = new SemaphoreSlim(1, 1);

        private static DiscordSocketClient Client { get; set; }
        private static CancellationTokenSource TokenSource { get; set; }
        private static Task LoopTask { get; set; }

        private static int UpdateIntervalMs { get; set; } = 15000;
        private static string LastPresenceText { get; set; } = string.Empty;

        public static void TryStart(DiscordSocketClient client)
        {
            _ = Task.Run(() => StartAsync(client));
        }

        private static async Task StartAsync(DiscordSocketClient client)
        {
            await StartStopSemaphore.WaitAsync();
            try
            {
                if (client == null) return;

                if (Client == client && TokenSource != null) return;

                await StopInternalAsync();

                Client = client;
                TokenSource = new CancellationTokenSource();

                LoopTask = Task.Run(() => PresenceLoopAsync(TokenSource.Token));

                await UpdatePresenceAsync();
            }
            catch (Exception e)
            {
                Printer.Warning($"[Discord] Presence failed to start: {e}");
            }
            finally
            {
                StartStopSemaphore.Release();
            }
        }

        public static void TryStop()
        {
            _ = Task.Run(StopAsync);
        }

        private static async Task StopAsync()
        {
            await StartStopSemaphore.WaitAsync();
            try
            {
                await StopInternalAsync();
            }
            finally
            {
                StartStopSemaphore.Release();
            }
        }

        private static async Task StopInternalAsync()
        {
            try
            {
                if (TokenSource != null)
                {
                    try { TokenSource.Cancel(); } catch { }
                }

                if (LoopTask != null)
                {
                    try { await LoopTask; } catch { }
                }
            }
            finally
            {
                try { TokenSource?.Dispose(); } catch { }

                TokenSource = null;
                LoopTask = null;
                Client = null;
                LastPresenceText = string.Empty;
            }
        }

        private static async Task PresenceLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await UpdatePresenceAsync();
                }
                catch (Exception e)
                {
                    Printer.Warning($"[Discord] Presence update error: {e}");
                }

                try { await Task.Delay(UpdateIntervalMs, token); }
                catch { }
            }
        }

        private static async Task UpdatePresenceAsync()
        {
            if (Client == null) return;
            if (Client.ConnectionState != ConnectionState.Connected) return;

            int online = 0;
            try
            {
                online = ServerNetwork.Instance?.GetConnectedClientsSafe()?.Length ?? 0;
            }
            catch { online = 0; }

            string text = BuildPresenceText(online);

            if (text == LastPresenceText) return;

            try
            {
                await Client.SetGameAsync(text, null, ActivityType.Watching);
                LastPresenceText = text;
            }
            catch { }
        }
        // Used to have max but dropped cause who gives a frick about knowing the max....
        private static string BuildPresenceText(int online)
        {
            if (online <= 0) return "No players online";
            if (online == 1) return "1 player online";
            return $"{online} players online";
        }
    }
}