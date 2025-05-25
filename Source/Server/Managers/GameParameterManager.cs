using GameServer.Core;
using GameServer.Misc;
using GameServer.TCP;
using Shared;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{

    public static class GameParameterManager
    {
        [HandlesPacket(PacketHeader.GameParameterManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes)
        {
            GameParameterData data = Serializer.ConvertBytesToObject<GameParameterData>(bytes);

            Printer.Warning(data, LogImportanceMode.Extreme);

            switch (data._stepMode)
            {
                case GenStepMode.Scenario:
                    SetScenario(client, data._scenario);
                    break;

                case GenStepMode.Storyteller:
                    SetStoryteller(client, data._storyteller);
                    break;

                case GenStepMode.Difficulty:
                    SetDifficulty(client, data._difficulty);
                    break;
            }
        }

        private static void SetScenario(ServerClient client, ScenarioValuesFile file)
        {
            if (!client.UserFile.IsAdmin && Master.WorldValues != null)
            {
                UserManager.BanPlayerFromName(client.UserFile.Uid);
                Printer.Warning($"Player {client.UserFile.Uid} attempted to set the scenario while not being an admin");
            }

            else
            {
                Master.ScenarioValues = file;
                Main_.SaveValueFile(ServerFileMode.Scenario, true);
                InformationDisplayer.DisplaySetScenario(client.UserFile.Uid);
            }
        }

        private static void SetStoryteller(ServerClient client, StorytellerValuesFile file)
        {
            if (!client.UserFile.IsAdmin && Master.WorldValues != null)
            {
                UserManager.BanPlayerFromName(client.UserFile.Uid);
                Printer.Warning($"Player {client.UserFile.Uid} attempted to set the storyteller while not being an admin");
            }

            else
            {
                Master.StorytellerValues = file;
                Main_.SaveValueFile(ServerFileMode.Storyteller, true);
                InformationDisplayer.DisplaySetStoryteller(client.UserFile.Uid);
            }
        }

        private static void SetDifficulty(ServerClient client, DifficultyValuesFile file)
        {
            if (!client.UserFile.IsAdmin && Master.WorldValues != null)
            {
                UserManager.BanPlayerFromName(client.UserFile.Uid);
                Printer.Warning($"Player {client.UserFile.Uid} attempted to set the difficulty while not being an admin");
            }

            else
            {
                Master.DifficultyValues = file;
                Main_.SaveValueFile(ServerFileMode.Difficulty, true);
                InformationDisplayer.DisplaySetDifficulty(client.UserFile.Uid);
            }
        }
    }
}
