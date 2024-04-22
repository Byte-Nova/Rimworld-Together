﻿using Shared;
using System.Collections.Generic;

namespace GameClient
{
    public static class ServerValues
    {
        public static bool AllowCustomScenarios;

        public static bool isAdmin;

        public static bool hasFaction;

        public static int currentPlayers;

        public static List<string> currentPlayerNames = new List<string>();

        public static void SetServerParameters(ServerOverallJSON serverOverallJSON)
        {
            AllowCustomScenarios = serverOverallJSON.AllowCustomScenarios;
        }

        public static void SetAccountDetails(ServerOverallJSON serverOverallJSON)
        {
            isAdmin = serverOverallJSON.isClientAdmin;

            hasFaction = serverOverallJSON.isClientFactionMember;
        }

        public static void SetServerPlayers(Packet packet)
        {
            PlayerRecountJSON playerRecountJSON = (PlayerRecountJSON)Serializer.ConvertBytesToObject(packet.contents);
            currentPlayers = int.Parse(playerRecountJSON.currentPlayers);
            currentPlayerNames = playerRecountJSON.currentPlayerNames;
        }

        public static void CleanValues()
        {
            AllowCustomScenarios = false;

            isAdmin = false;

            hasFaction = false;

            currentPlayers = 0;

            currentPlayerNames.Clear();
        }
    }
}
