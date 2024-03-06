using RimWorld;
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
            listingStandard.Label("Compatibility");
            if (listingStandard.ButtonTextLabeled("Convert save for server use", "Convert")) { ShowConvertMenu(); }
            if (listingStandard.ButtonTextLabeled("Open saves folder", "Open")) StartProcess(Master.savesFolderPath);
            if (listingStandard.ButtonTextLabeled("[When Playing] Get server world file", "Get")) { GenerateWorldFile(); }
            if (listingStandard.ButtonTextLabeled("Open server worlds folder", "Open")) StartProcess(Master.worldSavesFolderPath);

            listingStandard.GapLine();
            listingStandard.Label("Experimental");
            listingStandard.CheckboxLabeled("Use verbose logs", ref modConfigs.verboseBool, "Output more advanced info on the logs");
            if (listingStandard.ButtonTextLabeled("Open logs folder", "Open")) StartProcess(Master.mainPath);

            listingStandard.GapLine();
            listingStandard.Label("External Sources");
            if (listingStandard.ButtonTextLabeled("Check the mod's wiki!", "Open")) StartProcess("https://rimworld-together.fandom.com/wiki/Rimworld_Together_Wiki");
            if (listingStandard.ButtonTextLabeled("Join the mod's Discord community!", "Open")) StartProcess("https://discord.gg/NCsArSaqBW");
            if (listingStandard.ButtonTextLabeled("Check out the mod's Github!", "Open")) StartProcess("https://github.com/Byte-Nova/Rimworld-Together");

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

        private void GenerateWorldFile()
        {
            if (Network.isConnectedToServer)
            {
                WorldValuesFile worldValuesFile = new WorldValuesFile();

                worldValuesFile.seedString = Find.World.info.seedString;
                worldValuesFile.persistentRandomValue = Find.World.info.persistentRandomValue;
                worldValuesFile.planetCoverage = Find.World.info.planetCoverage.ToString();
                worldValuesFile.rainfall = ((int)Find.World.info.overallRainfall).ToString();
                worldValuesFile.temperature = ((int)Find.World.info.overallTemperature).ToString(); ;
                worldValuesFile.population = ((int)Find.World.info.overallPopulation).ToString();
                worldValuesFile.pollution = Find.World.info.pollution.ToString();

                foreach (Faction faction in Find.World.factionManager.AllFactions)
                {
                    if (faction.def == Faction.OfPlayer.def) continue;
                    else worldValuesFile.factions.Add(faction.def.defName);
                }

                WorldDetailsJSON worldDetailsJSON = new WorldDetailsJSON();
                XmlParser.GetWorldXmlData(worldDetailsJSON);

                worldValuesFile.tileBiomeDeflate = worldDetailsJSON.tileBiomeDeflate;
                worldValuesFile.tileElevationDeflate = worldDetailsJSON.tileElevationDeflate;
                worldValuesFile.tileHillinessDeflate = worldDetailsJSON.tileHillinessDeflate;
                worldValuesFile.tileTemperatureDeflate = worldDetailsJSON.tileTemperatureDeflate;
                worldValuesFile.tileRainfallDeflate = worldDetailsJSON.tileRainfallDeflate;
                worldValuesFile.tileSwampinessDeflate = worldDetailsJSON.tileSwampinessDeflate;
                worldValuesFile.tileFeatureDeflate = worldDetailsJSON.tileFeatureDeflate;
                worldValuesFile.tilePollutionDeflate = worldDetailsJSON.tilePollutionDeflate;
                worldValuesFile.tileRoadOriginsDeflate = worldDetailsJSON.tileRoadOriginsDeflate;
                worldValuesFile.tileRoadAdjacencyDeflate = worldDetailsJSON.tileRoadAdjacencyDeflate;
                worldValuesFile.tileRoadDefDeflate = worldDetailsJSON.tileRoadDefDeflate;
                worldValuesFile.tileRiverOriginsDeflate = worldDetailsJSON.tileRiverOriginsDeflate;
                worldValuesFile.tileRiverAdjacencyDeflate = worldDetailsJSON.tileRiverAdjacencyDeflate;
                worldValuesFile.tileRiverDefDeflate = worldDetailsJSON.tileRiverDefDeflate;

                Serializer.SerializeToFile(Path.Combine(Master.worldSavesFolderPath, "WorldValues.json"), worldValuesFile);

                DialogManager.PushNewDialog(new RT_Dialog_OK("World file was saved correctly!"));
            }
        }

        private void StartProcess(string processPath)
        {
            try { System.Diagnostics.Process.Start(processPath); } 
            catch { Log.Warning($"Failed to start process {processPath}"); }
        }
    }
}
