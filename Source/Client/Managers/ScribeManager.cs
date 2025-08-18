using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GameClient.Misc;
using GameClient.Values;
using RimWorld;
using Shared;
using Shared.Files;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Managers
{
    public static class ScribeManager
    {
        public static StringWriter StringWriter { get; set; } = new StringWriter();

        public static string ScribeTreeName { get; private set; } = "T";

        public static string ScribeNodeName { get; private set; } = "N";

        public enum SerializableType { Thing, Other }

        public static string SerializeToString(object toSave, SerializableType type, int customCount = -1)
        {
            ClientValues.ToggleUsingScriber(true);

            string scribeData = string.Empty;
            int originalCount = -1;
            Thing objAsThing = null;

            try
            {
                if (type == SerializableType.Thing)
                {
                    objAsThing = toSave as Thing;
                    originalCount = objAsThing.stackCount;
                    if (customCount != -1) objAsThing.stackCount = customCount;
                }

                Scribe.saver.InitSaving("", ScribeTreeName);

                Scribe_Deep.Look(ref toSave, ScribeNodeName);

                Scribe.saver.FinalizeSaving();

                if (type == SerializableType.Thing)
                {
                    if (customCount != -1) objAsThing.stackCount = originalCount;
                }

                scribeData = new Regex(@">\s*<").Replace(StringWriter.ToString(), "><");
            }
            catch (Exception e) { Printer.Error(e.ToString(), LogImportanceMode.Verbose); }

            ClientValues.ToggleUsingScriber(false);

            return scribeData.ToString();
        }

        public static T SerializeFromString<T>(string scribeData, SerializableType type = SerializableType.Thing)
        {
            ClientValues.ToggleUsingScriber(true);

            object toLoad = null;

            try
            {
                Scribe.loader.InitLoading(scribeData);

                Scribe_Deep.Look(ref toLoad, ScribeNodeName);

                Scribe.loader.FinalizeLoading();

                if (type == SerializableType.Thing)
                {
                    Thing thing = toLoad as Thing;
                    thing.thingIDNumber = Find.UniqueIDsManager.GetNextThingID();
                }
            }
            catch (Exception e) { Printer.Error(e.ToString(), LogImportanceMode.Verbose); }

            ClientValues.ToggleUsingScriber(false);

            return (T)toLoad;
        }

        //At some point merge these 2 below into the top functions, beware of ideology

        public static HumanFile HumanToString(Pawn human)
        {
            HumanFile humanFile = new HumanFile();

            humanFile.ScribeData = ScribeManager.SerializeToString(human, SerializableType.Thing);

            if (ModsConfig.IdeologyActive)
            {
                bool isPlayerIdeo = human.Ideo.initialPlayerIdeo;
                human.Ideo.initialPlayerIdeo = false;
                humanFile.IdeologyData = SerializeToString(human.Ideo, SerializableType.Other);
                human.Ideo.initialPlayerIdeo = isPlayerIdeo;
            }

            return humanFile;
        }

        public static Pawn StringtoHuman(HumanFile file)
        {
            Pawn pawn = (Pawn)ScribeManager.SerializeFromString<Pawn>(file.ScribeData);

            if (ModsConfig.IdeologyActive)
            {
                Ideo ideo = (Ideo)ScribeManager.SerializeFromString<Ideo>(file.IdeologyData);

                Ideo match = Find.IdeoManager.IdeosListForReading.FirstOrDefault(i => 
                    i.id == ideo.id && i.name == ideo.name && i.description == ideo.description);

                pawn.ideo.SetIdeo(match ?? ideo);
            }

            return (pawn);
        }
    }

    public static class ScriberH
    {
        public static bool CheckIfThingIsHuman(Thing thing)
        {
            try
            {
                if (thing.def.defName == "Human") return true;
                else return false;
            }
            catch { return false; }
        }

        public static bool CheckIfThingIsAnimal(Thing thing)
        {
            try
            {
                PawnKindDef animal = DefDatabase<PawnKindDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == thing.def.defName);
                if (animal != null) return true;
                else return false;
            }
            catch { return false; }
        }
    }
}