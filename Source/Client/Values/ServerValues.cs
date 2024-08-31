using Shared;
using System.Collections.Generic;

namespace GameClient
{
    public static class ServerValues
    {
        public static bool isAdmin;

        public static bool hasFaction;

        public static int currentPlayers;

        public static List<string> currentPlayerNames = new List<string>();

        public static void SetValues(ServerGlobalData serverGlobalData)
        {
            isAdmin = serverGlobalData._isClientAdmin;

            hasFaction = serverGlobalData._isClientFactionMember;
        }

        public static void SetServerPlayers(Packet packet)
        {
            PlayerRecountData playerRecountData = Serializer.ConvertBytesToObject<PlayerRecountData>(packet.contents);
            currentPlayers = int.Parse(playerRecountData._currentPlayers);
            currentPlayerNames = playerRecountData._currentPlayerNames;
        }

        public static void CleanValues()
        {
            isAdmin = false;

            hasFaction = false;

            currentPlayers = 0;

            currentPlayerNames.Clear();
        }
    }
}
