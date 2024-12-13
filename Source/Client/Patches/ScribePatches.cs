using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using HarmonyLib;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    [HarmonyPatch(typeof(ScribeSaver), nameof(ScribeSaver.InitSaving))]
    public static class PatchSaving
    {
        [HarmonyPrefix]
        public static bool DoPre(ScribeSaver __instance, ref XmlWriter ___writer, string documentElementName)
        {
            if (Network.state == ClientNetworkState.Disconnected) return true;
            else if (!ClientValues.isUsingScriber) return true;
            else
            {
                try
                {
                    Scribe.mode = LoadSaveMode.Saving;
                    
                    XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                    xmlWriterSettings.Indent = true;
                    xmlWriterSettings.IndentChars = "\t";

                    RTScriber.stringWriter = new StringWriter();
                    ___writer = XmlWriter.Create(RTScriber.stringWriter, xmlWriterSettings);
                    ___writer.WriteStartDocument();
                    __instance.EnterNode(documentElementName);
                }

                catch (Exception e)
                {
                    Logger.Error($"Exception while init save patched scribe: {e}");
                    __instance.ForceStop();
                    throw;
                }

                return false;
            }
        }
    }

    [HarmonyPatch(typeof(ScribeLoader), nameof(ScribeLoader.InitLoading))]
    public static class PatchLoading
    {
        [HarmonyPrefix]
        public static bool DoPre(ScribeLoader __instance, string filePath)
        {
            if (Network.state == ClientNetworkState.Disconnected) return true;
            else if (!ClientValues.isUsingScriber) return true;
            else
            {
                try
                {
                    using (StringReader input = new StringReader(filePath))
                    {
                        using XmlTextReader reader = new XmlTextReader(input);
                        XmlDocument xmlDocument = new XmlDocument();
                        xmlDocument.Load(reader);

                        __instance.curXmlParent = xmlDocument.DocumentElement;
                    }

                    Scribe.mode = LoadSaveMode.LoadingVars;
                }

                catch (Exception e)
                {
                    Logger.Error($"Exception while init load patched scribe: {e}");
                    __instance.ForceStop();
                    throw;
                }

                return false;
            }
        }
    }

    //TODO
    //Find a way to handle scribe errors better than this

    [HarmonyPatch(typeof(PawnTextureAtlas), nameof(PawnTextureAtlas.GC))]
    public static class PatchPawnAtlas
    {
        [HarmonyPrefix]
        public static bool DoPre(ref Dictionary<Pawn, PawnTextureAtlasFrameSet> ___frameAssignments, ref List<Pawn> ___tmpPawnsToFree, ref List<PawnTextureAtlasFrameSet> ___freeFrameSets)
        {
            if (Network.state == ClientNetworkState.Disconnected) return true;
            else
            {
                try
                {
                    foreach (Pawn key in ___frameAssignments.Keys)
                    {
                        if (!key.SpawnedOrAnyParentSpawned)
                        {
                            ___tmpPawnsToFree.Add(key);
                        }
                    }

                    foreach (Pawn item in ___tmpPawnsToFree)
                    {
                        ___freeFrameSets.Add(___frameAssignments[item]);
                        ___frameAssignments.Remove(item);
                    }
                }
                
                catch (Exception e) 
                { 
                    ___frameAssignments.Clear();
                    Logger.Error(e.ToString(), LogImportanceMode.Extreme); 
                }

                ___tmpPawnsToFree.Clear();

                return false;
            }
        }
    }

    [HarmonyPatch(typeof(DebugLoadIDsSavingErrorsChecker), nameof(DebugLoadIDsSavingErrorsChecker.RegisterDeepSaved))]
    public static class PatchRegisterDeepSaved
    {
        [HarmonyPrefix]
        public static bool DoPre()
        {
            if (Network.state == ClientNetworkState.Disconnected) return true;
            else return false;
        }
    }

    [HarmonyPatch(typeof(DebugLoadIDsSavingErrorsChecker), nameof(DebugLoadIDsSavingErrorsChecker.CheckForErrorsAndClear))]
    public static class PatchCheckForErrorsAndClear
    {
        [HarmonyPrefix]
        public static bool DoPre(DebugLoadIDsSavingErrorsChecker __instance)
        {
            if (Network.state == ClientNetworkState.Disconnected) return true;
            else
            {
                __instance.Clear();
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(LoadedObjectDirectory), nameof(LoadedObjectDirectory.RegisterLoaded))]
    public static class PatchRegisterLoaded
    {
        [HarmonyPrefix]
        public static bool DoPre(ILoadReferenceable reffable, ref Dictionary<string, ILoadReferenceable> ___allObjectsByLoadID, ref Dictionary<int, ILoadReferenceable> ___allThingsByThingID)
        {
            if (Network.state == ClientNetworkState.Disconnected) return true;
            else
            {
                try { ___allObjectsByLoadID.Add(reffable.GetUniqueLoadID(), reffable); }
                catch (Exception e) { Logger.Error(e.ToString(), LogImportanceMode.Extreme); }

                if (reffable is not Thing thing) return false;

                try { ___allThingsByThingID.Add(thing.thingIDNumber, reffable); }
                catch (Exception e) { Logger.Error(e.ToString(), LogImportanceMode.Extreme); }
                
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(Log), nameof(Log.Warning))]
    public static class PatchScribeWarning
    {
        [HarmonyPrefix]
        public static bool DoPre()
        {
            if (Network.state == ClientNetworkState.Disconnected) return true;
            else if (!ClientValues.isUsingScriber) return true;
            else return false;
        }
    }

    [HarmonyPatch(typeof(Log), nameof(Log.Error))]
    public static class PatchScribeError
    {
        [HarmonyPrefix]
        public static bool DoPre()
        {
            if (Network.state == ClientNetworkState.Disconnected) return true;
            else if (!ClientValues.isUsingScriber) return true;
            else return false;
        }
    }
}