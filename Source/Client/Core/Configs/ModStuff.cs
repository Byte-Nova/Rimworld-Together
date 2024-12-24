using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;

namespace GameClient
{
    public class ModStuff : Mod
    {
        //Variables

        private readonly ModConfigs modConfigs;

        public ModStuff(ModContentPack content) : base(content)
        {
            modConfigs = GetSettings<ModConfigs>();
        }

        public override string SettingsCategory() { return "RimWorld Together"; }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.Label("Running version: " + CommonValues.executableVersion);
            listingStandard.GapLine();
            listingStandard.Label("Multiplayer Parameters");
            listingStandard.CheckboxLabeled("[When Playing] Deny all incoming transfers", ref modConfigs.rejectTransfersBool, "Automatically denies transfers");
            listingStandard.CheckboxLabeled("[When Playing] Deny all incoming site rewards", ref modConfigs.rejectSiteRewardsBool, "Automatically site rewards");
            listingStandard.CheckboxLabeled("[When Playing] Mute incomming chat messages", ref modConfigs.muteChatSoundBool, "Mute chat messages");
            if (listingStandard.ButtonTextLabeled("[When Playing] Server sync interval", $"[{ClientValues.autosaveDays}] Day/s")) ShowAutosaveFloatMenu();

            listingStandard.GapLine();
            listingStandard.Label("Compatibility");
            if (listingStandard.ButtonTextLabeled("Convert save for server use", "Convert")) { ShowConvertFloatMenu(); }
            if (listingStandard.ButtonTextLabeled("Open saves folder", "Open")) StartProcess(Master.savesFolderPath);

            listingStandard.GapLine();
            listingStandard.Label("Experimental");
            if (listingStandard.ButtonTextLabeled("Verbose mode", $"{ClientValues.currentVerboseMode}")) ShowVerbosesaveFloatMenu();

            listingStandard.GapLine();
            listingStandard.Label("External Sources");
            if (listingStandard.ButtonTextLabeled("Check out the mod's wiki!", "Open")) StartProcess("https://github.com/Byte-Nova/Rimworld-Together/wiki");
            if (listingStandard.ButtonTextLabeled("Check out the mod's Github!", "Open")) StartProcess("https://github.com/Byte-Nova/Rimworld-Together");
            if (listingStandard.ButtonTextLabeled("Check out the mod's incompatibility list!", "Open")) StartProcess("https://github.com/Byte-Nova/Rimworld-Together/blob/development/IncompatibilityList.md");
            if (listingStandard.ButtonTextLabeled("Check out the mod's donation page!", "Open")) StartProcess("https://ko-fi.com/rimworldtogether");
            if (listingStandard.ButtonTextLabeled("Join the mod's Discord community!", "Open")) StartProcess("https://discord.gg/yUF2ec8Vt8");

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        private void ShowAutosaveFloatMenu()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            List<Tuple<string, float>> autosaveDays = new List<Tuple<string, float>>()
            {
                Tuple.Create("0.125 Days", 0.125f),
                Tuple.Create("0.25 Days", 0.25f),
                Tuple.Create("0.5 Days", 0.5f),
                Tuple.Create("1 Day", 1.0f),
                Tuple.Create("2 Days", 2.0f),
                Tuple.Create("3 Days", 3.0f),
                Tuple.Create("5 Days", 5.0f),
                Tuple.Create("7 Days", 7.0f),
                Tuple.Create("14 Days", 14.0f)
            };

            foreach (Tuple<string, float> tuple in autosaveDays)
            {
                FloatMenuOption item = new FloatMenuOption(tuple.Item1, delegate
                {
                    ClientValues.autosaveDays = tuple.Item2;
                    ClientValues.autosaveInternalTicks = Mathf.RoundToInt(tuple.Item2 * 60000f);

                    PreferenceManager.SaveClientPreferences();
                });

                list.Add(item);
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }

        private void ShowVerbosesaveFloatMenu()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            List<Tuple<string, ClientValues.VerboseMode>> autosaveDays = new List<Tuple<string, ClientValues.VerboseMode>>()
            {
                Tuple.Create("None", ClientValues.VerboseMode.None),
                Tuple.Create("Verbose", ClientValues.VerboseMode.Verbose),
                Tuple.Create("Extreme", ClientValues.VerboseMode.Extreme)
            };

            foreach (Tuple<string, ClientValues.VerboseMode> tuple in autosaveDays)
            {
                FloatMenuOption item = new FloatMenuOption(tuple.Item1, delegate
                {
                    ClientValues.currentVerboseMode = tuple.Item2;
                    PreferenceManager.SaveClientPreferences();
                });

                list.Add(item);
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }

        private void ShowConvertFloatMenu()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            foreach(string str in Directory.GetFiles(Master.savesFolderPath).Where(fetch => fetch.EndsWith(".rws")))
            {
                FloatMenuOption item = new FloatMenuOption(Path.GetFileNameWithoutExtension(str), delegate
                {
                    string toConvertPath = str;
                    string conversionPath = str.Replace(".rws", ".mpsave");

                    byte[] compressedBytes = GZip.Compress(File.ReadAllBytes(toConvertPath));
                    File.WriteAllBytes(conversionPath, compressedBytes);

                    RT_Dialog_OK d2 = new RT_Dialog_OK("Save was converted successfully");
                    DialogManager.PushNewDialog(d2);
                });

                list.Add(item);
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }

        private void StartProcess(string processPath)
        {
            try { System.Diagnostics.Process.Start(processPath); } 
            catch { Logger.Warning($"Failed to start process {processPath}"); }
        }
    }
}
