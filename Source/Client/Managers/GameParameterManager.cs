using GameClient.Dialogs;
using GameClient.Misc;
using GameClient.Values;
using RimWorld;
using Shared;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Verse;
using static Shared.CommonEnumerators;
using Shared.Files;
using TCPNetwork.Packets;

namespace GameClient.Managers
{

    public static class GameParameterManager
    {
        public static void SetFirstTimeSetup()
        {
            string title = "Server Enforcements";
            string description = "Chose what features to enforce";
            string[] keys = new string[] { "Scenario", "Storyteller", "Difficulty" };
            string[] values = new string[] { "Free", "Enforced" };

            RT_Dialog_Base.PushNewDialog(new RT_Dialog_ListingWithTuple(title, description, keys, values, null,
                GameParameterManager.SendFirstTimeSetup));
        }

        public static void SetValues(ServerGlobalData data)
        {
            SessionValues.ScenarioFile = data._scenarioValues;
            SessionValues.StorytellerFile = data._storytellerValues;
            SessionValues.DifficultyFile = data._difficultyValues;
        }

        public static void SetScenario(ScenarioValuesFile file)
        {
            if (!file.EnforceScenario) return;
            else
            {
                Scenario toFind = ScenarioLister.AllScenarios().FirstOrDefault(fetch => fetch.name == file.ScenarioName);
                if (toFind != null) Current.Game.Scenario = toFind;
                else Current.Game.Scenario = ScenarioLister.AllScenarios().ToArray()[0];
            }
        }

        public static void SetDifficulty(DifficultyValuesFile file, bool bypass = false)
        {
            if (!file.EnforceDifficulty && !bypass) return;
            else
            {
                Current.Game.storyteller.difficultyDef = DifficultyDefOf.Rough;
                Current.Game.storyteller.difficulty = (Difficulty)ScribeManager.SerializeFromString<Difficulty>(file.ScribeData);
            }
        }

        public static void SetStoryteller(StorytellerValuesFile file, bool bypassCheck = false)
        {
            if (!file.EnforceStoryteller && !bypassCheck) return;
            else
            {
                StorytellerDef storytellerDef = DefDatabase<StorytellerDef>.AllDefs.First(fetch => fetch.defName == file.StorytellerDefname);
                DifficultyDef difficultyDef = Current.Game.storyteller.difficultyDef == null ? DifficultyDefOf.Easy : Current.Game.storyteller.difficultyDef;
                Difficulty difficulty = Current.Game.storyteller.difficulty == null ? new Difficulty(difficultyDef) : Current.Game.storyteller.difficulty;

                Current.Game.storyteller = new Storyteller(storytellerDef, difficultyDef, difficulty);
            }
        }

        public static void SendCurrentScenario(bool isEnforced)
        {
            ScenarioValuesFile file = new ScenarioValuesFile();
            file.EnforceScenario = isEnforced;
            file.ScenarioName = Current.Game.Scenario.name;

            GameParameterData data = new GameParameterData();
            data._stepMode = GenStepMode.Scenario;
            data._scenario = file;

            ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.GameParameterManager, data);
        }

        public static void SendCurrentStoryteller(bool isEnforced)
        {
            StorytellerValuesFile file = new StorytellerValuesFile();
            file.EnforceStoryteller = isEnforced;
            file.StorytellerDefname = Current.Game.storyteller.def.defName;

            GameParameterData data = new GameParameterData();
            data._stepMode = GenStepMode.Storyteller;
            data._storyteller = file;

            ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.GameParameterManager, data);
        }

        public static void SendCurrentDifficulty(bool isEnforced)
        {
            DifficultyValuesFile file = new DifficultyValuesFile();
            file.EnforceDifficulty = isEnforced;
            file.ScribeData = ScribeManager.SerializeToString(Current.Game.storyteller.difficulty, ScribeManager.SerializableType.Other);

            GameParameterData data = new GameParameterData();
            data._stepMode = GenStepMode.Difficulty;
            data._difficulty = file;

            ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.GameParameterManager, data);
        }

        public static void SendCurrentModConfigs(bool isEnforced)
        {
            ModConfigData data = new ModConfigData();
            data._stepMode = ModConfigStepMode.Send;
            data._configFile = ModManagerH.SortModsIntoCategories(RT_Dialog_ListingWithTuple.DialogTupleListingResultString,
                RT_Dialog_ListingWithTuple.DialogTupleListingResultInt);

            List<string> modFileNames = new List<string>();
            List<string> modConfigs = new List<string>();
            foreach (string str in ModManagerH.GetAllModConfigs())
            {
                modFileNames.Add(Path.GetFileName(str));
                modConfigs.Add(File.ReadAllText(str));
            }

            data._configFile.ModFileNames = modFileNames.ToArray();
            data._configFile.ModConfigs = modConfigs.ToArray();
            data._configFile.EnforcedConfigs = isEnforced;

            ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.ModManager, data);
        }

        private static void SendFirstTimeSetup()
        {
            if (RT_Dialog_ListingWithTuple.DialogTupleListingResultInt[0] == 1) { GameParameterManager.SendCurrentScenario(true); }
            if (RT_Dialog_ListingWithTuple.DialogTupleListingResultInt[1] == 1) { GameParameterManager.SendCurrentStoryteller(true); }
            if (RT_Dialog_ListingWithTuple.DialogTupleListingResultInt[2] == 1) { GameParameterManager.SendCurrentDifficulty(true); }

            WorldManager.SendWorld();
            EventManager.SendExistingEventsToServer();
            SaveManager.ForceSave();

            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("MESSAGE", 
                new string[] { "Some configurations might require a reconnection to apply" }));
        }
    }
}
