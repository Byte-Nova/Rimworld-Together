using RTServer.Core;
using RTServer.Hooks.TCPNetwork;
using RTShared.Files.Configs;
using RTNetwork.PacketManagers;
using RTNetwork.Packets.ServerBrowser;
using static RTServer.Managers.ServerBrowserManager;
using RTNetwork.Components;
using RTShared.Misc;

namespace RTServer.PacketManagers.ServerBrowser
{
    public class PM_Telemetry : PM_Base
    {
        [HandlesPacket(PacketHeader.ServerBrowserTelemetry)]
        public override void Receive(ServerClient client, byte[] bytes, PacketHeader header)
        {
            throw new NotImplementedException();
        }

        public static async void Send(BrowserMode mode)
        {
            if (!WasStartedOnce) ServerIPV4 = await GetPublicIP();

            PKT_ServerTelemetry telemetry = new PKT_ServerTelemetry();
            telemetry.Name = Master.ServerConfig.Name;
            telemetry.Description = Master.ServerConfig.Description;
            telemetry.DiscordURL = Master.ServerConfig.DiscordURL;
            telemetry.SteamWorkshopURL = Master.ServerConfig.SteamWorkshopURL;
            telemetry.Version = CommonValues.ExecutableVersion;
            telemetry.Endpoint = ServerIPV4;
            telemetry.Port = Master.ServerConfig.Port;
            telemetry.IsPrivate = mode == BrowserMode.Private;
            telemetry.CurrentPopulation = ServerNetwork.GetConnectedClients().Length;
            telemetry.MaxPopulation = Master.ServerConfig.MaxPlayers;
            telemetry.PasswordProtected = !string.IsNullOrWhiteSpace(Master.PasswordConfig.Password);
            telemetry.Mods = Master.ModConfig.ModConfigs.OrderBy(fetch => fetch.ModName).ToList();
            telemetry.PlayerNames = ServerNetwork.GetConnectedUsernames();

            Network.MultipurposeEndpoint?.EnqueuePacket(PacketHeader.ServerBrowserTelemetry, telemetry);
        }
    }
}
