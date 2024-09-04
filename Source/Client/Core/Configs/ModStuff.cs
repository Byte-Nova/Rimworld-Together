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
            listingStandard.Label("RTMultiplayerParameters".Translate());
            listingStandard.CheckboxLabeled("RTDenyIncomingTransfers".Translate(), ref modConfigs.rejectTransfersBool, "RTDenyIncomingTransfersDesc".Translate());
            listingStandard.CheckboxLabeled("RTDenyIncomingSiteRewards".Translate(), ref modConfigs.rejectSiteRewardsBool, "RTDenyIncomingSiteRewardsDesc".Translate());
            listingStandard.CheckboxLabeled("RTMuteChat".Translate(), ref modConfigs.muteChatSoundBool, "RTMuteChatDesc".Translate());
            if (listingStandard.ButtonTextLabeled("RTServerSyncInterval".Translate(), "RTServerSyncIntervalVar".Translate(ClientValues.autosaveDays))) ShowAutosaveFloatMenu();

            listingStandard.GapLine();
            listingStandard.Label("RTCompatibility".Translate());
            if (listingStandard.ButtonTextLabeled("RTConvertSave".Translate(), "RTConvertSaveButton".Translate())) { ShowConvertFloatMenu(); }
            if (listingStandard.ButtonTextLabeled("RTOpenSaveFolder".Translate(), "RTConfigOpen".Translate())) StartProcess(Master.savesFolderPath);

            listingStandard.GapLine();
            listingStandard.Label("RTExperimental".Translate());
            listingStandard.CheckboxLabeled("RTUseVerboseLogs".Translate(), ref modConfigs.verboseBool, "RTUseVerboseLogsDesc".Translate());
            listingStandard.CheckboxLabeled("RTExtremeVerboseLogs".Translate(), ref modConfigs.extremeVerboseBool, "RTExtremeVerboseLogsDesc".Translate());

            listingStandard.GapLine();
            listingStandard.Label("RTExternalSources".Translate());
            if (listingStandard.ButtonTextLabeled("RTWikiOpen".Translate(), "RTConfigOpen".Translate())) StartProcess("https://rimworldtogether.github.io/Guide");
            if (listingStandard.ButtonTextLabeled("RTGithubOpen".Translate(), "RTConfigOpen".Translate())) StartProcess("https://github.com/RimworldTogether/Rimworld-Together");
            if (listingStandard.ButtonTextLabeled("RTIncompatibilityOpen".Translate(), "RTConfigOpen".Translate())) StartProcess("https://github.com/RimworldTogether/Rimworld-Together/blob/development/IncompatibilityList.md");
            if (listingStandard.ButtonTextLabeled("RTDiscordOpen".Translate(), "RTConfigOpen".Translate())) StartProcess("https://discord.gg/yUF2ec8Vt8");

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        private void ShowAutosaveFloatMenu()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            List<Tuple<string, float>> autosaveDays = new List<Tuple<string, float>>()
            {
                Tuple.Create((string)"RTDays0.125".Translate(), 0.125f),
                Tuple.Create((string)"RTDays0.25".Translate(), 0.25f),
                Tuple.Create((string)"RTDays0.5".Translate(), 0.5f),
                Tuple.Create((string)"RTDays1".Translate(), 1.0f),
                Tuple.Create((string)"RTDays2".Translate(), 2.0f),
                Tuple.Create((string)"RTDays3".Translate(), 3.0f),
                Tuple.Create((string)"RTDays5".Translate(), 5.0f),
                Tuple.Create((string)"RTDays7".Translate(), 7.0f),
                Tuple.Create((string)"RTDays14".Translate(), 14.0f)
            };

            foreach (Tuple<string, float> tuple in autosaveDays)
            {
                FloatMenuOption item = new FloatMenuOption(tuple.Item1, delegate
                {
                    ClientValues.autosaveDays = tuple.Item2;
                    ClientValues.autosaveInternalTicks = Mathf.RoundToInt(tuple.Item2 * 60000f);

                    PreferenceManager.SaveClientPreferences(ClientValues.autosaveDays.ToString());
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

                    RT_Dialog_OK d2 = new RT_Dialog_OK("RTConvertSaveSuccesful".Translate());
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
