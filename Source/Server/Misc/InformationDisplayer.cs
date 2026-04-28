using Shared.Misc;
using TCPNetwork.Files.Client;

namespace GameServer.Misc
{
    public static class InformationDisplayer
    {
        public static void DisplayConnect(ServerClient client) { Printer.Message($"[Connect] > {(client.CurrentIP.Contains(':') && client.CurrentIP.Contains('.') ? client.CurrentIP.Split(":")[3] : client.CurrentIP)}"); }

        public static void DisplayDisconnect(ServerClient client) { Printer.Message($"[Disconnect] > {(client.CurrentIP.Contains(':') && client.CurrentIP.Contains('.') ? client.CurrentIP.Split(":")[3] : client.CurrentIP)}"); }

        public static void DisplayLogin(ServerClient client) { Printer.Message($"[Log in] > {client.GetOrSetClientData<UserFile>().Username}"); }

        public static void DisplayRegister(ServerClient client) { Printer.Message($"[Register] > {client.GetOrSetClientData<UserFile>().Username}"); }

        public static void DisplaySaveGame(ServerClient client) { Printer.Message($"[Save game] > {client.GetOrSetClientData<UserFile>().Username}"); }

        public static void DisplaySaveMap(ServerClient client) { Printer.Message($"[Save Map] > {client.GetOrSetClientData<UserFile>().Username}"); }

        public static void DisplaySetMods(ServerClient client) { Printer.Warning($"[Set mods] > {client.GetOrSetClientData<UserFile>().Username}"); }

        public static void DisplaySetWorld(ServerClient client) { Printer.Warning($"[Set world] > {client.GetOrSetClientData<UserFile>().Username}"); }

        public static void DisplaySetEvents(ServerClient client) { Printer.Warning($"[Set events] > {client.GetOrSetClientData<UserFile>().Username}"); }

        public static void DisplayChatMap(string label, string message) { Printer.Message($"[Chat - {label}] > {message}"); }

        public static void DisplayAddSettlement(string value) { Printer.Message($"[Add settlement] > {value}"); }

        public static void DisplayRemoveSettlement(string value) { Printer.Message($"[Remove settlement] > {value}"); }

        public static void DisplayAddSite(string value) { Printer.Message($"[Add site] > {value}"); }

        public static void DisplayRemoveSite(string value) { Printer.Message($"[Remove site] > {value}"); }

        public static void DisplayAddFaction(string value) { Printer.Message($"[Add faction] > {value}"); }

        public static void DisplayRemoveFaction(string value) { Printer.Message($"[Remove faction] > {value}"); }

        public static void DisplayAddRoad(string value, string value2) { Printer.Message($"[Add road] > {value} - {value2}"); }

        public static void DisplayRemoveRoad(string value, string value2) { Printer.Message($"[Remove road] > {value} - {value2}"); }

        public static void DisplayServerBackup(string value) { Printer.Warning($"[Server backup] > {value}"); }

        public static void DisplayUserBackup(string value) { Printer.Message($"[User backup] > {value}"); }

        public static void DisplayResetPlayer(string value) { Printer.Message($"[Reset player] > {value}"); }

        public static void DisplayModBypass(string value) { Printer.Message($"[Mod bypass] > {value}"); }

        public static void DisplayModMismatch(string value) { Printer.Warning($"[Mod mismatch] > {value}"); }

        public static void DisplayVersionMismatch(ServerClient client) { Printer.Warning($"[Version mismatch] > {client.CurrentIP}"); }

        public static void DisplaySetScenario(string value) { Printer.Warning($"[Set scenario] > {value}"); }

        public static void DisplaySetStoryteller(string value) { Printer.Warning($"[Set storyteller] > {value}"); }

        public static void DisplaySetDifficulty(string value) { Printer.Warning($"[Set difficulty] > {value}"); }
    }
}
