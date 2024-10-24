using Shared;

namespace GameServer;

public class GameVictoryManager
{
    public static void ParsePacket(ServerClient client, Packet packet)
    {
        GameVictoryData chatData = Serializer.ConvertBytesToObject<GameVictoryData>(packet.contents);
        ChatManager.BroadcastServerNotification($"{chatData._playerName} won the game! Ending: {chatData._ending}");
    }
}