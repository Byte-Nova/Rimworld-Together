using RTServer.Core;
using RTServer.Managers;
using RTServer.Misc;
using RTShared.Misc;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using RTNetwork.PacketManagers;
using static RTNetwork.Packets.PKT_Login;
using RTNetwork.Components;
using RTServer.PacketManagers;
using RTShared.Files.Player;

namespace RTServer.Hooks.TCPNetwork
{
    public class ServerNetwork
    {
        private static Action<PacketHeader, byte[], ServerClient> OnReadPacket { get; set; } = delegate (PacketHeader header, byte[] buffer, ServerClient client)
        {
            MethodInfo method = (MethodInfo)PM_Base.PacketDictionary[header][1];
            method.Invoke(PM_Base.PacketDictionary[header][0], new object[] { client, buffer, header });
        };

        private static Action<ServerClient> OnDisconnect { get; set; } = delegate (ServerClient client) 
        {
            try
            {
                Network.ServerClients.Remove(client, out _);
                UserManager.SendPlayerRecount();
                
                InformationDisplayer.DisplayDisconnect(client);
                if (client.GetData<FL_Player>() != null) InformationDisplayer.DisplayLogOut(client);
                if (Master.ChatConfig.DisconnectNotifications) PM_Chat.BroadcastServerNotification($"{client.GetData<FL_Player>().Username} has left the server!");
            }
            catch (Exception ex) { Printer.Error(ex); }
        };

        public static void StartFeature()
        {
            try
            {
                Network.Ip = Master.ServerConfig.IP;
                Network.Port = Master.ServerConfig.Port;

                if (Master.ServerConfig.UseUPnP) { _ = new UPnP(); }

                Network.ServerListener = new TcpListener(IPAddress.Parse(Network.Ip), Network.Port);
                Network.ServerListener.Start();

                Printer.Warning("Server launched");
                Printer.Warning($"Listening for users at {Network.Ip}:{Network.Port}");
                Printer.Warning("Type 'help' to get a list of available commands");

                Task.Run(delegate { while (true) ListenForNewClients(); });
            }
            catch (Exception e) { Printer.Error(e); }
        }

        private static void ListenForNewClients()
        {
            ServerClient client = new ServerClient(Network.ServerListener.AcceptTcpClient(), new NetworkRuleset(null, OnDisconnect, OnReadPacket, null));

            if (GetConnectedClients().Length >= Master.ServerConfig.MaxPlayers) PM_Login.DenyConnectionWithReason(client, LoginResponse.Full);
            else if (Master.WorldValues == null && GetConnectedClients().Length > 0) PM_Login.DenyConnectionWithReason(client, LoginResponse.NoWorld);
            else
            {
                Network.TotalClientsSinceLaunch++;
                Network.ServerClients.TryAdd(client, -1);
                InformationDisplayer.DisplayConnect(client);
                client.Listener.Ruleset.OnConnect?.Invoke(client);
            }
        }

        public static ServerClient[] GetConnectedClients(ServerClient toExclude = null)
        {
            if (toExclude != null)
            {
                return Network.ServerClients.Keys.Where(fetch => fetch.GetData<FL_Player>().Username !=
                    toExclude.GetData<FL_Player>().Username).ToArray();
            }
            else return Network.ServerClients.Keys.ToArray();
        }
        
        public static List<string> GetConnectedUsernames()
        {
            ServerClient[] connectedClients = GetConnectedClients(); 
            List<string> usernames = [];
            
            foreach (ServerClient client in connectedClients)
            {
                if (client.GetData<FL_Player>() == null) usernames.Add("Unknown");
                else usernames.Add(client.GetData<FL_Player>().Username);
            }

            return usernames;
        }

        public static ServerClient GetConnectedClientFromUsername(string username)
        {
            return GetConnectedClients().FirstOrDefault(fetch => fetch.GetData<FL_Player>().Username == username);
        }

        public static ServerClient GetClientFromID(int id) { return Network.ServerClients.Keys.First(fetch => fetch.ID == id); }

        public static void SendPacketToAllClients(PacketHeader header, object obj, ServerClient toExclude = null)
        {
            foreach (ServerClient client in GetConnectedClients(toExclude))
            {
                client.Listener.EnqueuePacket(header, obj);
            }
        }
    }
}