using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameClient.Managers;
using GameClient.Misc;
using GameClient.Values;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Scribers
{
    public static class RTScriber
    {
        public static StringWriter stringWriter;

        public static readonly string scribeTreeName = "Tree";

        public static readonly string scribeNodeName = "Node";

        public static string ThingToScribe(Thing toSave, int customCount = -1)
        {
            ClientValues.ToggleUsingScriber(true);

            try
            {
                int originalCount = toSave.stackCount;
                if (customCount != -1) toSave.stackCount = customCount;

                Scribe.saver.InitSaving("", scribeTreeName);

                Scribe_Deep.Look(ref toSave, scribeNodeName);

                Scribe.saver.FinalizeSaving();

                if (customCount != -1) toSave.stackCount = originalCount;
            }
            catch (Exception e) { Printer.Error(e.ToString(), LogImportanceMode.Verbose); };

            ClientValues.ToggleUsingScriber(false);

            return stringWriter.ToString();
        }

        public static Thing ScribeToThing(string scribeData, bool hasCustomID)
        {
            ClientValues.ToggleUsingScriber(true);

            Thing toLoad = null;

            try
            {
                Scribe.loader.InitLoading(scribeData);

                Scribe_Deep.Look(ref toLoad, scribeNodeName);

                Scribe.loader.FinalizeLoading();

                if (!hasCustomID) toLoad.thingIDNumber = Find.UniqueIDsManager.GetNextThingID();
            }
            catch (Exception e) { Printer.Error(e.ToString(), LogImportanceMode.Verbose); };

            ClientValues.ToggleUsingScriber(false);

            return toLoad;
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

        public static bool CheckIfThingIsCorpse(Thing thing)
        {
            try
            {
                Corpse corpse = thing as Corpse;
                if (corpse != null) return true;
                else return false;
            }
            catch { return false; }
        }
    }
}