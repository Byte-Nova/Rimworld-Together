using System.Collections.Generic;
using Shared;

namespace GameClient.Managers
{

    public static class RecountManager
    {
        public static int CurrentPlayers { get; private set; }

        public static List<string>? CurrentPlayerNames { get; private set; }

        [HandlesPacket(PacketHeader.RecountManager)]
        private static void ParsePacket(byte[] bytes) { SetServerPlayers(bytes); }

        public static void SetServerPlayers(byte[] bytes)
        {
            PlayerRecountData playerRecountData = Serializer.ConvertBytesToObject<PlayerRecountData>(bytes);
            CurrentPlayers = playerRecountData._currentPlayerCount;
            CurrentPlayerNames = playerRecountData._currentPlayerNames;
        }
    }
}