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
        ModConfigs modConfigs;

        public ModStuff(ModContentPack content) : base(content)
        {
            modConfigs = GetSettings<ModConfigs>();
        }

        public override string SettingsCategory() { return "Rimworld Together"; }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.Label("Running version: " + CommonValues.executableVersion);

            listingStandard.GapLine();
            listingStandard.Label("Multiplayer Parameters");
            listingStandard.CheckboxLabeled("[When Playing] Deny all incoming transfers", ref modConfigs.transferBool, "Automatically denies transfers");
            listingStandard.CheckboxLabeled("[When Playing] Deny all incoming site rewards", ref modConfigs.siteRewardsBool, "Automatically site rewards");
            if (listingStandard.ButtonTextLabeled("[When Playing] Server sync interval", $"[{ClientValues.autosaveDays}] Day/s"))
            {
                ShowAutosaveFloatMenu();
            }


            listingStandard.GapLine();
            listingStandard.Label("Misc");
            if (listingStandard.ButtonText("Convert save for server use"))
            {
                ShowConvertMenu();
            }

            listingStandard.GapLine();
            listingStandard.Label("Experimental");
            listingStandard.CheckboxLabeled("Use verbose logs", ref modConfigs.verboseBool, "Output more advanced info on the logs");
            if (listingStandard.ButtonTextLabeled("Open logs folder", "Open"))
            {
                try { System.Diagnostics.Process.Start(Master.mainPath); } catch { }
            }

            listingStandard.GapLine();
            listingStandard.Label("External Sources");
            if (listingStandard.ButtonTextLabeled("Check the mod's wiki!", "Open"))
            {
                try { System.Diagnostics.Process.Start("https://rimworld-together.fandom.com/wiki/Rimworld_Together_Wiki"); } catch { }
            }
            if (listingStandard.ButtonTextLabeled("Join the mod's Discord community!", "Open"))
            {
                try { System.Diagnostics.Process.Start("https://discord.gg/NCsArSaqBW"); } catch { }
            }
            if (listingStandard.ButtonTextLabeled("Check out the mod's Github!", "Open"))
            {
                try { System.Diagnostics.Process.Start("https://github.com/Byte-Nova/Rimworld-Together"); } catch { }
            }

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        private void ShowAutosaveFloatMenu()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            List<Tuple<string, int>> savedServers = new List<Tuple<string, int>>()
            {
                Tuple.Create("1 Day", 1),
                Tuple.Create("2 Days", 2),
                Tuple.Create("3 Days", 3),
                Tuple.Create("5 Days", 5),
                Tuple.Create("7 Days", 7),
                Tuple.Create("14 Days", 14)
            };

            foreach (Tuple<string, int> tuple in savedServers)
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

        private void ShowConvertMenu()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            foreach(string str in Directory.GetFiles(Master.savesPath).Where(fetch => fetch.EndsWith(".rws")))
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
    }
}
