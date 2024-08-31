using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class DifficultyManager
    {
        public static void ParseDifficultyPacket(ServerClient client, Packet packet)
        {
            SetCustomDifficulty(client, Serializer.ConvertBytesToObject<DifficultyData>(packet.contents));
        }

        public static void SetCustomDifficulty(ServerClient client, DifficultyData difficultyData)
        {
            if (!client.userFile.IsAdmin)
            {
                ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Username} attempted to set the custom difficulty while not being an admin");
            }
            
            else 
            {
                Master.difficultyValues = difficultyData._values;

                Logger.Warning($"[Set difficulty] > {client.userFile.Username}");

                Main_.SaveValueFile(ServerFileMode.Difficulty);
            }
        }
    }
}
