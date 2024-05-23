using Shared;
using System.Collections.Generic;

namespace GameClient
{
    public static class ServerValues
    {
        public static bool AllowCustomScenarios;

        public static bool isAdmin;

        public static bool isOperator;

        public static bool hasFaction;

        public static int currentPlayers;

        public static List<string> currentPlayerNames = new List<string>();

        public static void SetServerParameters(ServerGlobalData serverGlobalData)
        {
            AllowCustomScenarios = serverGlobalData.AllowCustomScenarios;
        }

        public static void SetAccountData(ServerGlobalData serverGlobalData)
        {
            isAdmin = serverGlobalData.isClientAdmin;
            isOperator = serverGlobalData.isClientOperator;
            hasFaction = serverGlobalData.isClientFactionMember;
        }

        public static void SetServerPlayers(Packet packet)
        {
            PlayerRecountData playerRecountData = (PlayerRecountData)Serializer.ConvertBytesToObject(packet.contents);
            currentPlayers = int.Parse(playerRecountData.currentPlayers);
            currentPlayerNames = playerRecountData.currentPlayerNames;
        }

        public static void CleanValues()
        {
            AllowCustomScenarios = false;

            isAdmin = false;

            isOperator = false;

            hasFaction = false;

            currentPlayers = 0;

            currentPlayerNames.Clear();
        }
    }
}
