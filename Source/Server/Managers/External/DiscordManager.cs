using System.Text;
using Discord;
using Discord.WebSocket;

namespace GameServer
{
    public static class DiscordManager
    {
        private static DiscordSocketClient discordClient;

        private static readonly Queue<string> consoleBuffer = new Queue<string>();

        private static readonly int sendToConsoleDelay = 1000;
        
        private static readonly int updatePlayerCountDelay = 60000;

        private static readonly DiscordSocketConfig config = new() { GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent };

        public static void StartDiscordIntegration()
        {
            Logger.Title("Starting Discord integration");
            Logger.Title($"----------------------------------------");
            Threader.GenerateDiscordThread(Threader.DiscordMode.Start);
        }

        public static async void TryStartDiscordIntegration()
        {
            try
            {
                discordClient = new DiscordSocketClient(config);

                // Subscribing to events
                discordClient.Log += LogAsync;
                discordClient.MessageReceived += MessageReceivedAsync;

                //Starting Discord functions
                await discordClient.LoginAsync(TokenType.Bot, Master.discordConfig.Token);
                await discordClient.StartAsync();
                await discordClient.SetCustomStatusAsync("");

                //Start server functions
                Threader.GenerateDiscordThread(Threader.DiscordMode.Console);
                Threader.GenerateDiscordThread(Threader.DiscordMode.Count);
            }
            catch { Logger.Error("Failed to start Discord integration"); }
        }

        private static Task LogAsync(LogMessage log)
        {
            if (!Master.serverConfig.VerboseLogs) return Task.CompletedTask;
            else
            {
                Logger.Outsider("[Discord Integration] > " + log.ToString());
                return Task.CompletedTask;
            }
        }

        private static Task MessageReceivedAsync(SocketMessage message)
        {
            //Prevent bot from listening to itself
            if (message.Author.Id == discordClient.CurrentUser.Id) return Task.CompletedTask;

            //Prevent bot from listening to another webhook
            if (message.Author.IsWebhook) return Task.CompletedTask;

            //If message is from the chat channel
            if (message.Channel.Id == Master.discordConfig.ChatChannelId)
            {
                ChatManager.BroadcastDiscordMessage(message.Author.GlobalName == null ? message.Author.Username : message.Author.GlobalName, message.CleanContent);
            }

            //If message is from console channel
            else if (message.Channel.Id == Master.discordConfig.ConsoleChannelId)
            {
                Logger.Outsider($"[Discord Command] > {message.CleanContent}");
                ConsoleCommandManager.ParseServerCommands(message.CleanContent);
            }

            return Task.CompletedTask;
        }

        public static async void SendMessageToChatChannel(string user, string message)
        {
            if (discordClient.GetChannel(Master.discordConfig.ChatChannelId) is SocketTextChannel channel)
            {
                //If no webhook is available, primitive formatting is used
                if (string.IsNullOrWhiteSpace(Master.discordConfig.ChatWebhook)) await channel.SendMessageAsync($"{user}: {message}");
                else SendWebhookMessage(user, message);
            }
        }

        private static async void SendWebhookMessage(string user, string message)
        {
            using var httpClient = new HttpClient();
            var payload = new
            {
                username = user,
                content = message
            };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            using (var response = await httpClient.PostAsync(Master.discordConfig.ChatWebhook, httpContent))
            {
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Logger.Outsider($"Failed to send message to Discord webhook ({response.StatusCode}): {errorMessage}");
                }
            }
        }

        public static async void LoopMessagesToConsoleChannel()
        {
            if (Master.discordConfig.ConsoleChannelId == 0) return;

            while (true)
            {
                if (discordClient.GetChannel(Master.discordConfig.ConsoleChannelId) is SocketTextChannel channel)
                {
                    string condensedMessage = "";
                    bool readyToSend = consoleBuffer.Count == 0;

                    while (!readyToSend)
                    {
                        if (condensedMessage != "") condensedMessage += "\n";

                        condensedMessage += consoleBuffer.Dequeue();

                        if (consoleBuffer.Count == 0 || condensedMessage.Length + consoleBuffer.Peek().Length >= 2000) readyToSend = true;
                    }

                    if (condensedMessage != "") await channel.SendMessageAsync(condensedMessage);
                }

                await Task.Delay(sendToConsoleDelay);
            }
        }

        public static async void LoopUpdatePlayerCount()
        {
            while (true)
            {
                int count = NetworkHelper.GetConnectedClientsSafe().Count();
                string multiple = count > 1 ? "s" : "";
                
                await discordClient.SetCustomStatusAsync($"{count} Player{multiple} online");
                await Task.Delay(updatePlayerCountDelay);
            }
        }

        public static void SendMessageToConsoleChannelBuffer(string message) { consoleBuffer.Enqueue(message); }
    }
}