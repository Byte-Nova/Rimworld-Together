using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GameClient.Misc;
using RimWorld;
using Shared;
using Shared.Files;
using Shared.Misc;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Managers
{
    public static class ScribeManager
    {
        public static StringWriter StringWriter { get; set; } = new StringWriter();

        public static string ScribeTreeName { get; private set; } = "T";

        public static string ScribeNodeName { get; private set; } = "N";

        public enum SerializableType { Thing, Pawn, Other }

        public static string SerializeToString(object toSave, SerializableType type = SerializableType.Thing, int customCount = -1, string customID = null)
        {
            SessionHandler.IsUsingScriber = true;

            string scribeData = string.Empty;
            int originalCount = -1;
            string originalID = string.Empty;
            Thing objAsThing = null;
            Pawn objAsPawn = null;

            try
            {
                if (type == SerializableType.Thing)
                {
                    objAsThing = toSave as Thing;

                    originalCount = objAsThing.stackCount;
                    if (customCount != -1) objAsThing.stackCount = customCount;
                }

                else if (type == SerializableType.Pawn)
                {
                    objAsPawn = toSave as Pawn;

                    originalID = objAsPawn.ThingID;
                    if (customID != null) objAsPawn.ThingID = customID;
                }

                Scribe.saver.InitSaving("", ScribeTreeName);

                Scribe_Deep.Look(ref toSave, ScribeNodeName);

                Scribe.saver.FinalizeSaving();

                if (type == SerializableType.Thing)
                {
                    if (customCount != -1) objAsThing.stackCount = originalCount;
                }

                if (type == SerializableType.Pawn)
                {
                    if (customID != null) objAsPawn.ThingID = originalID;
                }

                scribeData = new Regex(@">\s*<").Replace(StringWriter.ToString(), "><");
            }
            catch (Exception e) { Printer.Error(e.ToString(), LogImportanceMode.Extreme); }

            SessionHandler.IsUsingScriber = false;

            return scribeData.ToString();
        }

        public static T SerializeFromString<T>(string scribeData, SerializableType type = SerializableType.Thing, bool enforceID = false)
        {
            SessionHandler.IsUsingScriber = true;

            object toLoad = null;

            try
            {
                Scribe.loader.InitLoading(scribeData);

                Scribe_Deep.Look(ref toLoad, ScribeNodeName);

                Scribe.loader.FinalizeLoading();

                if (type == SerializableType.Thing)
                {
                    Thing thing = toLoad as Thing;
                    if (!enforceID) thing.thingIDNumber = Find.UniqueIDsManager.GetNextThingID();
                }

                else if (type == SerializableType.Pawn)
                {
                    Pawn pawn = toLoad as Pawn;
                    if (pawn.def.CanHaveFaction) pawn.SetFactionDirect(Faction.OfPlayer);
                    if (!enforceID) pawn.thingIDNumber = Find.UniqueIDsManager.GetNextThingID();
                }
            }
            catch (Exception e) { Printer.Error(e.ToString(), LogImportanceMode.Extreme); }

            SessionHandler.IsUsingScriber = false;

            return (T)toLoad;
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