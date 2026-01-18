using GameClient.Dialogs;
using GameClient.Misc;
using RimWorld;
using Shared;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Verse;
using static Shared.CommonEnumerators;
using TCPNetwork.Packets;
using Shared.Files.Configs;
using GameClient.Hooks.TCPNetwork;

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
            SessionHandler.CurrentScenario = data._scenarioValues;
            SessionHandler.CurrentStoryteller = data._storytellerValues;
            SessionHandler.CurrentDifficulty = data._difficultyValues;
        }

        public static void SetScenario(ScenarioConfigFile file)
        {
            if (!file.IsEnforced) return;
            else
            {
                Scenario toFind = ScenarioLister.AllScenarios().FirstOrDefault(fetch => fetch.name == file.Name);
                if (toFind != null) Current.Game.Scenario = toFind;
                else Current.Game.Scenario = ScenarioLister.AllScenarios().ToArray()[0];
            }
        }

        public static void SetDifficulty(DifficultyConfigFile file, bool bypass = false)
        {
            if (!file.IsEnforced && !bypass) return;
            else
            {
                Current.Game.storyteller.difficultyDef = DifficultyDefOf.Rough;
                Current.Game.storyteller.difficulty = (Difficulty)ScribeManager.SerializeFromString<Difficulty>(file.ScribeData);
            }
        }

        public static void SetStoryteller(StorytellerConfigFile file, bool bypassCheck = false)
        {
            if (!file.IsEnforced && !bypassCheck) return;
            else
            {
                StorytellerDef storytellerDef = DefDatabase<StorytellerDef>.AllDefs.First(fetch => fetch.defName == file.DefName);
                DifficultyDef difficultyDef = Current.Game.storyteller.difficultyDef == null ? DifficultyDefOf.Easy : Current.Game.storyteller.difficultyDef;
                Difficulty difficulty = Current.Game.storyteller.difficulty == null ? new Difficulty(difficultyDef) : Current.Game.storyteller.difficulty;

                Current.Game.storyteller = new Storyteller(storytellerDef, difficultyDef, difficulty);
            }
        }

        public static void SendCurrentScenario(bool isEnforced)
        {
            ScenarioConfigFile file = new ScenarioConfigFile();
            file.Name = Current.Game.Scenario.name;
            file.IsEnforced = isEnforced;

            GameParameterData data = new GameParameterData();
            data._stepMode = GenStepMode.Scenario;
            data._bytes = Serializer.ConvertObjectToBytes(file);

            ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.GameParameterManager, data);
        }

        public static void SendCurrentStoryteller(bool isEnforced)
        {
            StorytellerConfigFile file = new StorytellerConfigFile();
            file.DefName = Current.Game.storyteller.def.defName;
            file.IsEnforced = isEnforced;

            GameParameterData data = new GameParameterData();
            data._stepMode = GenStepMode.Storyteller;
            data._bytes = Serializer.ConvertObjectToBytes(file);

            ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.GameParameterManager, data);
        }

        public static void SendCurrentDifficulty(bool isEnforced)
        {
            DifficultyConfigFile file = new DifficultyConfigFile();
            file.IsEnforced = isEnforced;
            file.ScribeData = ScribeManager.SerializeToString(Current.Game.storyteller.difficulty, 
                ScribeManager.SerializableType.Other);

            GameParameterData data = new GameParameterData();
            data._stepMode = GenStepMode.Difficulty;
            data._bytes = Serializer.ConvertObjectToBytes(file);

            ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.GameParameterManager, data);
        }

        public static void SendCurrentModConfigs(bool isEnforced)
        {
            ModConfigData data = new ModConfigData();
            data._stepMode = ModConfigStepMode.Send;
            data._configFile.IsEnforced = isEnforced;
            data._configFile = ModManagerH.SortModsIntoCategories(RT_Dialog_ListingWithTuple.DialogTupleListingResultString,
                RT_Dialog_ListingWithTuple.DialogTupleListingResultInt);

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
                new string[] { "Some configurations might require a server restart to apply" }));
        }
    }
}
