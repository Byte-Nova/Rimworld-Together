using Shared;

namespace GameServer;

public class GameVictoryManager
{
    public static void ParsePacket(ServerClient client, Packet packet)
    {
        GameVictoryData victoryData = Serializer.ConvertBytesToObject<GameVictoryData>(packet.contents);
        SendVictoryMessage(victoryData);
    }

    private static void SendVictoryMessage(GameVictoryData victoryData)
    {
        if (!Master.chatConfig.EndGameNotifications) return;
        ChatManager.BroadcastServerNotification($"{victoryData._playerName} won the game! Ending: {victoryData._ending}");
    }
}