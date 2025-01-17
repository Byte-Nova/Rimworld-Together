using GameClient.Core.Preferences;
using GameClient.Dialogs;
using GameClient.Managers;
using GameClient.Misc;
using GameClient.Values;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;
using System.Diagnostics;

namespace GameClient.Core.Configs
{
    public class ModTweaker : Mod
    {
        //Variables

        private readonly ModExposer modConfigs;

        public ModTweaker(ModContentPack content) : base(content)
        {
            modConfigs = GetSettings<ModExposer>();
        }

        public override string SettingsCategory() { return "RimWorld Together"; }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.GapLine();
            listingStandard.Label("Multiplayer Parameters");
            listingStandard.CheckboxLabeled("[When Playing] Deny all incoming transfers", ref modConfigs.rejectTransfersBool, "Automatically denies transfers");
            listingStandard.CheckboxLabeled("[When Playing] Deny all incoming site rewards", ref modConfigs.rejectSiteRewardsBool, "Automatically site rewards");
            listingStandard.CheckboxLabeled("[When Playing] Mute incomming chat messages", ref modConfigs.muteChatSoundBool, "Mute chat messages");
            if (listingStandard.ButtonTextLabeled("[When Playing] Server sync interval", $"[{ClientValues.autosaveDays}] Day/s")) ShowAutosaveFloatMenu();

            listingStandard.GapLine();
            listingStandard.Label("Debugging");
            if (listingStandard.ButtonTextLabeled("Verbosity mode", $"{ClientValues.currentVerboseMode}")) ShowVerboseFloatMenu();
            if (listingStandard.ButtonTextLabeled("Open logs folder", "Open")) StartProcess(Master.appdataPath);
            if (listingStandard.ButtonTextLabeled("Convert save for server use", "Convert")) { ShowConvertSaveFloatMenu(); }

            listingStandard.GapLine();
            listingStandard.Label("Tweaks");
            if (listingStandard.ButtonTextLabeled("Change mod version [Windows only]", "Change")) { VersionManager.PromptChangeVersion(); }

            GUI.color = Color.red;
            if (listingStandard.ButtonTextLabeled("Reset account [DANGEROUS]", "Reset")) { ShowResetAccountQuestion(); }
            GUI.color = Color.white;

            listingStandard.GapLine();
            listingStandard.Label("External Sources");
            if (listingStandard.ButtonTextLabeled("Check out the mod's wiki!", "Open")) StartProcess("https://github.com/Byte-Nova/Rimworld-Together/wiki");
            if (listingStandard.ButtonTextLabeled("Check out the mod's Github!", "Open")) StartProcess("https://github.com/Byte-Nova/Rimworld-Together");
            if (listingStandard.ButtonTextLabeled("Check out the mod's incompatibility list!", "Open")) StartProcess("https://github.com/Byte-Nova/Rimworld-Together/blob/development/IncompatibilityList.md");
            if (listingStandard.ButtonTextLabeled("Check out the mod's donation page!", "Open")) StartProcess("https://ko-fi.com/rimworldtogether");
            if (listingStandard.ButtonTextLabeled("Check out mod's Discord community!", "Open")) StartProcess("https://discord.gg/yUF2ec8Vt8");

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

                    PlayerPreferenceManager.SavePlayerPreferences();
                });

                list.Add(item);
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }

        private void ShowVerboseFloatMenu()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            List<Tuple<string, ClientValues.VerboseMode>> verboseModes = new List<Tuple<string, ClientValues.VerboseMode>>()
            {
                Tuple.Create("None", ClientValues.VerboseMode.None),
                Tuple.Create("Verbose", ClientValues.VerboseMode.Verbose),
                Tuple.Create("Extreme", ClientValues.VerboseMode.Extreme)
            };

            foreach (Tuple<string, ClientValues.VerboseMode> tuple in verboseModes)
            {
                FloatMenuOption item = new FloatMenuOption(tuple.Item1, delegate
                {
                    ClientValues.currentVerboseMode = tuple.Item2;
                    PlayerPreferenceManager.SavePlayerPreferences();
                });

                list.Add(item);
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }

        private void ShowConvertSaveFloatMenu()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            foreach (string str in Directory.GetFiles(Master.savesFolderPath).Where(fetch => fetch.EndsWith(".rws")))
            {
                FloatMenuOption item = new FloatMenuOption(Path.GetFileNameWithoutExtension(str), delegate
                {
                    string toConvertPath = str;
                    string conversionPath = str.Replace(".rws", ".mpsave");

                    byte[] compressedBytes = GZip.CompressBytes(File.ReadAllBytes(toConvertPath));
                    File.WriteAllBytes(conversionPath, compressedBytes);

                    RT_Dialog_OK d2 = new RT_Dialog_OK("Save was converted successfully");
                    DialogManager.PushNewDialog(d2);
                });

                list.Add(item);
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }

        private void ShowResetAccountQuestion()
        {
            RT_Dialog_YesNo dialog = new RT_Dialog_YesNo("Are you sure you want to RESET your ACCOUNT?",
                delegate
                {
                    UserLoginManager.DeleteLoginData();
                    DialogManager.PushNewDialog(new RT_Dialog_OK("Account has been reset"));
                });

            DialogManager.PushNewDialog(dialog);
        }

        private void StartProcess(string processPath)
        {
            try { Process.Start(processPath); }
            catch { Printer.Warning($"Failed to start process {processPath}"); }
        }
    }
}
