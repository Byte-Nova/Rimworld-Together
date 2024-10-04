using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class DifficultyManager
    {
        public static void ParsePacket(ServerClient client, Packet packet)
        {
            SetCustomDifficulty(client, Serializer.ConvertBytesToObject<DifficultyData>(packet.contents));
        }

        public static void SetCustomDifficulty(ServerClient client, DifficultyData difficultyData)
        {
            if (!client.userFile.IsAdmin) 
            {
                UserManager.BanPlayerFromName(client.userFile.Username);
                Logger.Warning($"Player {client.userFile.Username} attempted to set the custom difficulty while not being an admin");
            }
            
            else 
            {
                Master.difficultyValues = difficultyData._values;
                Main_.SaveValueFile(ServerFileMode.Difficulty, true);
                Logger.Warning($"[Set difficulty] > {client.userFile.Username}");
            }
        }
    }
}
