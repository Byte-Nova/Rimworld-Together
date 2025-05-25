namespace GameServer.Core.Configs;

[Serializable]
public class ChatConfigFile
{
    public bool EnableMoTD = false;

    public string MessageOfTheDay = "Remember to drink water";
    // Sends join/leave notifications to Discord chat (not console, which is always logged) and in-game chat, displayed under “[SERVER]” in blue.
    public bool LoginNotifications = false;

    public bool DisconnectNotifications = false;
}