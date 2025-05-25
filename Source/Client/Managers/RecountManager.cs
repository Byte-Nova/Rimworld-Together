using System.Collections.Generic;
using GameClient.Misc;
using Shared;
using static Shared.CommonEnumerators;

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
            PlayerRecountData data = Serializer.ConvertBytesToObject<PlayerRecountData>(bytes);

            Printer.Warning(data, LogImportanceMode.Extreme);

            CurrentPlayers = data._currentPlayerCount;
            CurrentPlayerNames = data._currentPlayerNames;
        }
    }
}