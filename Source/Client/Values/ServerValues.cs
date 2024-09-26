using Shared;
using System.Collections.Generic;

namespace GameClient
{
    public static class ServerValues
    {
        public static bool isAdmin;

        public static bool hasFaction;

        public static bool isNPCModificationAllowed;

        public static int currentPlayers;

        public static List<string> currentPlayerNames = new List<string>();

        public static void SetValues(ServerGlobalData serverGlobalData)
        {
            isAdmin = serverGlobalData._isClientAdmin;

            hasFaction = serverGlobalData._isClientFactionMember;

            if (serverGlobalData._isNPCModificaitonAllowedForEveryone) isNPCModificationAllowed = true;
            else if (serverGlobalData._isNPCModificationAllowed && isAdmin) isNPCModificationAllowed = true;
            else isNPCModificationAllowed = false;
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
