using System.Collections.Generic;
using Shared;

namespace GameClient
{
    public static class PlayerRecountManager
    {
        //Variables

        public static int currentPlayers;

        public static List<string> currentPlayerNames = new List<string>();

        public static void ParsePacket(Packet packet)
        {
            SetServerPlayers(packet);
        }

        public static void SetServerPlayers(Packet packet)
        {
            PlayerRecountData playerRecountData = Serializer.ConvertBytesToObject<PlayerRecountData>(packet.contents);
            currentPlayers = int.Parse(playerRecountData._currentPlayers);
            currentPlayerNames = playerRecountData._currentPlayerNames;
        }

        public static void CleanValues()
        {
            currentPlayers = 0;

            currentPlayerNames.Clear();
        }
    }
}