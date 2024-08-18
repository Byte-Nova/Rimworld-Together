using System.Text;
using Discord;
using Discord.WebSocket;

namespace GameServer
{
    public static class DiscordManager
    {
        private static DiscordSocketClient? _client;
        private static Queue<string> _consoleBuffer = new();
        private static Task sendToConsoleChannel = new(SendMessageToConsoleChannel);
        private static Task updatePlayerCount = new(UpdatePlayerCount);
        private static int _sendToConsoleDelay = 1000;
        private static int _updatePlayerCountDelay = 60000;

        private static DiscordSocketConfig Config = new() { GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent };

        public static async void StartDiscordIntegration()
        {
            if (Master.serverConfig == null) return;

            _client = new DiscordSocketClient(Config);

            // Subscribing to events
            _client.Log += LogAsync;
            _client.MessageReceived += MessageReceivedAsync;

            await _client.LoginAsync(TokenType.Bot, Master.serverConfig.DiscordIntegration.Token);
            await _client.StartAsync();

            await _client.SetCustomStatusAsync("");
            sendToConsoleChannel.Start();
            updatePlayerCount.Start();
        }

        private static Task LogAsync(LogMessage log)
        {
            if (Master.serverConfig != null && !Master.serverConfig.VerboseLogs) return Task.CompletedTask;
            
            Logger.Message("[Discord] " + log.ToString()[9..]);
            return Task.CompletedTask;
        }

        private static Task MessageReceivedAsync(SocketMessage message)
        {
            if (_client == null) return Task.CompletedTask;
            if (message.Author.Id == _client.CurrentUser.Id) return Task.CompletedTask; // The bot should never respond to itself.
            if (message.Author.IsWebhook) return Task.CompletedTask; // Same as above if it's from a webhook

            if (message.Channel.Id == Master.serverConfig.DiscordIntegration.ChatChannelId)
            {
                ChatManager.BroadcastDiscordMessage(message.Author.GlobalName == null ? message.Author.Username : message.Author.GlobalName, message.CleanContent);
            }
            else if (message.Channel.Id == Master.serverConfig.DiscordIntegration.ConsoleChannelId)
            {
                ServerCommandManager.ParseServerCommands(message.CleanContent);
            }

            return Task.CompletedTask;
        }

        public static async void SendMessageToChatChannel(string user, string message)
        {
            if (Master.serverConfig == null) return;
            if (_client == null) return;

            if (_client.GetChannel(Master.serverConfig.DiscordIntegration.ChatChannelId) is SocketTextChannel channel)
            {
                if (Master.serverConfig.DiscordIntegration.ChatWebhook == "") await channel.SendMessageAsync($"{user}: {message}");
                else SendWebhookMessage(user, message);
            }
        }

        public static void SendMessageToConsoleChannelBuffer(string message)
        {
            _consoleBuffer.Enqueue(message);
        }

        private static async void SendMessageToConsoleChannel()
        {
            if (Master.serverConfig == null) return;
            if (_client == null) return;

            while(Master.serverConfig.DiscordIntegration.Enabled)
            {
                if (_client.GetChannel(Master.serverConfig.DiscordIntegration.ConsoleChannelId) is SocketTextChannel channel)
                {
                    string condensedMessage = "";
                    bool readyToSend = _consoleBuffer.Count == 0;

                    while (!readyToSend)
                    {
                        if (condensedMessage != "") condensedMessage += "\n";
                        condensedMessage += _consoleBuffer.Dequeue();

                        if (_consoleBuffer.Count == 0 || condensedMessage.Length + _consoleBuffer.Peek().Length >= 2000) readyToSend = true;
                    }

                    if (condensedMessage != "") await channel.SendMessageAsync(condensedMessage);
                }

                await Task.Delay(_sendToConsoleDelay);
            }
        }

        private static async void SendWebhookMessage(string user, string message)
        {
            if (!Master.serverConfig.DiscordIntegration.Enabled) return;
            if (Master.serverConfig.DiscordIntegration.ChatWebhook == "") return;

            using var httpClient = new HttpClient();
            var payload = new
            {
                username = user,
                content = message
            };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            using (var response = await httpClient.PostAsync(Master.serverConfig.DiscordIntegration.ChatWebhook, httpContent))
            {
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to send message to Discord webhook ({response.StatusCode}): {errorMessage}");
                }
            }
        }

        public static async void UpdatePlayerCount()
        {
            if (_client == null) return;

            while(Master.serverConfig.DiscordIntegration.Enabled)
            {
                int count = Network.connectedClients.Count;
                string multiple = count > 1 ? "s" : "";
                
                await _client.SetCustomStatusAsync($"{count} Player{multiple} online");
                await Task.Delay(_updatePlayerCountDelay);
            }
        }

        public static void RestartDiscordClient()
        {
            _client?.Dispose();
            sendToConsoleChannel.Dispose();
            updatePlayerCount.Dispose();

            StartDiscordIntegration();
        }
    }
}