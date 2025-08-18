using System.Net;
using System.Text;
using GameServer.Core;
using GameServer.Misc;
using Shared;
using static Shared.CommonEnumerators;
using Shared.Files;
using TCPNetwork.Packets;

namespace GameServer.Managers
{
    public static class ServerBrowserManager
    {
        private const string MasterServer = "https://rimworldtogether.eragon.dev";
        private const int MaxDescriptionLength = 200;
        private const int MaxNameLength = 40;
        private const int DelayBetweenRequest = 520000; //Temporary testing timer, should be 5 minutes, aka 520000 miliseconds
        private const int DelayBetweenErrors = 18000000;
        private static HttpClientHandler handler = new HttpClientHandler() { UseProxy = false };
        private static HttpClient Client = new HttpClient(handler) 
        {
            DefaultRequestVersion = HttpVersion.Version11
        };
        public static void StartLoops()
        {
            Task.Run(async () =>
            {
                if (Master.ServerBrowserConfig.EnableServerBrowser)
                {
                    if (ValidateServerInfos())
                    {
                        Printer.Warning($"You have enabled the server browser feature. By doing so, you understand that:" +
                            $"\n- Your server's information (name, description, player count, ect... will be shared to possibly all Rimworld Together's users." +
                            $"\n- Your server's contact information (public ip adress and port) will be shared to possibly all Rimworld Together's users." +
                            $"\n If you do not want to share this information, you can disable the server browser in:\n{Path.Combine(Master.ConfigsPath, "ServerBrowserConfig.json")} " +
                            "\nunder the `EnableServerBrowser` setting and then restart the server.");
                        Console.CancelKeyPress += SendClosureSignalFromConsole;
                        AppDomain.CurrentDomain.ProcessExit += SendClosureSignalFromApplicationShutdown;
                        while (true)
                        {
                            bool result = await SendServerInfo();
                            if (result)
                            {
                                await Task.Delay(DelayBetweenRequest);
                            }
                            else
                            {
                                await Task.Delay(DelayBetweenErrors);
                            }
                        }
                    }
                }
                else
                {
                    Printer.Warning($"The server browser is currently disabled. " +
                        $"If you want to advertise your server to all Rimworld Together's users, you can turn on the server browser.");
                    while (true)
                    {
                        bool result = await SendServerPlayerCount();
                        if (result)
                        {
                            await Task.Delay(DelayBetweenRequest);
                        }
                        else
                        {
                            await Task.Delay(DelayBetweenErrors);
                        }
                    }
                }
            });
        }

        private static bool ValidateServerInfos() 
        {
            ServerConfigFile serverInfo = Master.ServerConfig;
            ServerBrowserConfig serverBrowserInfo = Master.ServerBrowserConfig;

            if (serverInfo.Description.Length > MaxDescriptionLength) 
            {
                Printer.Error($"Server description is above {MaxDescriptionLength} characters, please shorten it. Server browser features have been turned off.");
                return false;
            }

            if (string.IsNullOrEmpty(serverBrowserInfo.PublicEndPoint)) 
            {
                Printer.Error($"Public endpoint is empty. Please set your public ip adress or domain. Server browser features have been turned off.");
                return false;
            }

            if (serverInfo.Name.Length > MaxNameLength)
            {
                Printer.Error($"Server name is above {MaxNameLength} characters, please shorten it. Server browser features have been turned off.");
                return false;
            }

            if(serverInfo.Name == "RimWorld-Together-Server") 
            {
                Printer.Error($"Server name is the default name of {serverInfo.Name}. Please change the server name to something unique!. Server browser features have been turned off.");
                return false;
            }

            return true;
        }

        private static async Task<bool> SendServerInfo()
        {
            try
            {
                Client.DefaultRequestHeaders.Clear();
                Client.DefaultRequestHeaders.Add("action", "Add-Server-Browser");
                ServerInfo info = new ServerInfo()
                {
                    _ip = Master.ServerBrowserConfig.PublicEndPoint,
                    _port = int.Parse(Master.ServerConfig.Port),
                    _name = Master.ServerConfig.Name,
                    _description = Master.ServerConfig.Description,
                    _maximumPlayerCount = int.Parse(Master.ServerConfig.MaxPlayers),
                    _currentPlayerCount = ServerNetwork.Instance.ServerClients.Count,
                    _config = Master.ModConfig
                };
                HttpResponseMessage response = await Client.PostAsync(MasterServer, 
                    new StringContent(Serializer.SerializeToString(info), Encoding.UTF8, "application/json"));

                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                Printer.Error($"Error while notifying the Master Server\n {ex}");
                Printer.Error($"Will retry in 30 minutes");
                return false;
            }
        }

        private static async Task<bool> SendServerPlayerCount() 
        {
            try
            {
                Client.DefaultRequestHeaders.Clear();
                Client.DefaultRequestHeaders.Add("action", "Player-Count");

                HttpResponseMessage response = await Client.PostAsync(MasterServer,
                    new StringContent(ServerNetwork.Instance.ServerClients.Count.ToString()));

                response.EnsureSuccessStatusCode();
                return true;
            }
            catch(Exception ex)
            {
                Printer.Error(ex, LogImportanceMode.Verbose);
                return false;
            }
        }

        private static void SendClosureSignalFromConsole(object sender, ConsoleCancelEventArgs e) 
        {
            e.Cancel = true;
            SendClosureSignal().Wait();
        }
        private static void SendClosureSignalFromApplicationShutdown(object sender, EventArgs e)
        {
            SendClosureSignal().Wait();
        }
        private static async Task SendClosureSignal() 
        {
            Client.DefaultRequestHeaders.Clear();
            Client.DefaultRequestHeaders.Add("action", "Remove-Server-Browser");
            ServerInfo info = new ServerInfo()
            {
                _ip = Master.ServerBrowserConfig.PublicEndPoint,
                _port = int.Parse(Master.ServerConfig.Port),
                _name = Master.ServerConfig.Name
            };
            HttpResponseMessage response = await Client.PostAsync(MasterServer,
                new StringContent(Serializer.SerializeToString(info)));
        }
    }
}
