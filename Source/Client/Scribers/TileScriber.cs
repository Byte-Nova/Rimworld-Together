using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameClient.Misc;
using GameClient.Values;
using RimWorld.Planet;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Scribers
{
    // TODO
    // Find a way to make it usable

    public static class TileScriber
    {
        public static string TileToScribe(Tile toSave)
        {
            ClientValues.ToggleUsingScriber(true);

            try
            {
                Scribe.saver.InitSaving("", RTScriber.scribeTreeName);

                Scribe_Deep.Look(ref toSave, RTScriber.scribeNodeName);

                Scribe.saver.FinalizeSaving();
            }
            catch (Exception e) { Printer.Error(e.ToString(), LogImportanceMode.Verbose); };

            ClientValues.ToggleUsingScriber(false);

            return RTScriber.stringWriter.ToString();
        }

        public static Tile ScribeToTile(string scribeData)
        {
            ClientValues.ToggleUsingScriber(true);

            Tile toLoad = null;

            try
            {
                Scribe.loader.InitLoading(scribeData);

                Scribe_Deep.Look(ref toLoad, RTScriber.scribeNodeName);

                Scribe.loader.FinalizeLoading();
            }
            catch (Exception e) { Printer.Error(e.ToString(), LogImportanceMode.Verbose); };

            ClientValues.ToggleUsingScriber(false);

            return toLoad;
        }
    }
}
