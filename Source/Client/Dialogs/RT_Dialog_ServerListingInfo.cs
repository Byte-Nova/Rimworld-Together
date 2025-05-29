using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GameClient.Managers;
using GameClient.Misc;
using GameClient.TCP;
using HarmonyLib;
using RimWorld;
using Shared;
using UnityEngine;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Dialogs
{
    public class RT_Dialog_ServerListingInfo : Window
    {
        public override Vector2 InitialSize => new Vector2(600f, 250f);

        private static FieldInfo ModsConfigData;

        private static FieldInfo ModsConfigDataActiveMods;

        private ServerInfo ServerInfo { get; set; }

        public RT_Dialog_ServerListingInfo(ServerInfo info) 
        {
            this.ServerInfo = info;
            ModsConfigData = AccessTools.Field(typeof(ModsConfig), "data");
            ModsConfigDataActiveMods = AccessTools.Field(AccessTools.TypeByName("Verse.ModsConfig+ModsConfigData"), "activeMods");
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Vector2 titleSize = Text.CalcSize(ServerInfo._name);
            float centeredx = inRect.width / 2;
            Rect titleRect = new Rect(centeredx - titleSize.x / 2, inRect.y, titleSize.x, titleSize.y);
            Widgets.Label(titleRect, ServerInfo._name);

            Widgets.DrawLineHorizontal(0, titleSize.y + 3f, inRect.width);

            Text.Font = GameFont.Small;
            Rect descriptionRect = new Rect(inRect.x, titleSize.y + 6f, inRect.width / 3 * 2, inRect.height - 55f);
            Widgets.Label(descriptionRect, ServerInfo._description);

            Rect connectRect = new Rect(inRect.width - 110f, inRect.height - 55f, 100f, 45f);

            if (Widgets.ButtonText(connectRect, "Connect"))
            {
                MatchModlists();
            }
        }
        private void MatchModlists() 
        {
            var data = CheckInstalledMods();
            if (data.IsAllMissingOptional == false && (data.MissingMods.Any() || data.DownloadableMods.Any() || data.ModsToEnable.Any()))
            {
                ShowMissingModsDialog(data);
            }
            else
            {
                ConnectToServer();
            }
        }

        private void ShowMissingModsDialog(ModlistAnalyzer data) 
        {
            Printer.Warning($"Found {data.DownloadableMods.Count} mods to download and {data.MissingMods.Count} mods that cannot be downloaded.");
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Listing("Missing mods!",
                "Do you want to download/enable the missing mods automatically?",
                data.Display.ToArray(),
                () => ProcessMissingMods(data)
            ));
        }

        private void ProcessMissingMods(ModlistAnalyzer data) 
        {
            foreach (var value in data.DownloadableMods.ToList())
            {
                if (ServerBrowserManager.DownloadMod(value.Value))
                {
                    try
                    {
                        data.DownloadableMods.Remove(value.Key);
                        List<string> activeMods = (List<string>)ModsConfigDataActiveMods.GetValue(ModsConfigData.GetValue(null));
                        activeMods.Add(value.Key);
                        Printer.Warning($"Enabled mod {value.Key}", LogImportanceMode.Verbose);
                    }
                    catch (Exception ex)
                    {
                        Printer.Error($"Error while trying to activate mod {value.Key}. You will need to manually enable / download this mod.\n{ex}");
                    }
                }
                else 
                {
                    Printer.Warning($"Failed to download mod {value.Key} with steam id {value.Value}", LogImportanceMode.Verbose);
                }
            }
            if (data.DownloadableMods.Count > 0) 
            {
                ShowDownloadErrorDialog(data);
            }
            else 
            {
                RestartGameWithNewModlist(data);
            }
        }

        private void ShowDownloadErrorDialog(ModlistAnalyzer data) 
        {
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Listing("Error",
                "Something went wrong while downloading the following mods:",
                data.DownloadableMods.Keys.ToArray()));
        }

        private void RestartGameWithNewModlist(ModlistAnalyzer data) 
        {
            List<string> modsToEnable = new List<string>();
            modsToEnable.AddRange(ServerInfo._config.RequiredMods);
            modsToEnable.AddRange(ServerInfo._config.OptionalMods);
            foreach (string str in modsToEnable)
            {
                if (!ServerInfo._config.ForbiddenMods.Contains(str) && !data.MissingMods.Contains(str))
                {
                    ModsConfig.SetActive(str, true);
                    Printer.Warning($"Enabled mod {str}", LogImportanceMode.Verbose);
                }
            }
            ModsConfig.Save();
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("Success.", new string[] { "Game will now restart, give some time for steam to download the mods." },
            () =>
            {
                GenCommandLine.Restart();
            }));
        }

        private ModlistAnalyzer CheckInstalledMods() 
        {
            ModlistAnalyzer data = new ModlistAnalyzer();
            for (int i = 0; i < ServerInfo._config.UnsortedMods.Length; i++)
            {
                string mod = ServerInfo._config.UnsortedMods[i];
                var foundMod = ModLister.AllInstalledMods.FirstOrDefault(x => x.PackageId == mod);
                if (foundMod != null)
                {
                    if (!foundMod.Active)
                    {
                        data.Display.Add($"[Enableable]> {foundMod.Name}");
                        data.ModsToEnable.Add(mod);
                        if (data.IsAllMissingOptional && ServerInfo._config.RequiredMods.Contains(mod))
                        {
                            data.IsAllMissingOptional = false;
                        }
                    }
                    continue;
                }

                if (ServerInfo._config.AllModIds[i] != 0)
                {
                    data.DownloadableMods.Add(mod, ServerInfo._config.AllModIds[i]);
                    data.ModsToEnable.Add(mod);
                    data.Display.Add($"[Downloadable]> {mod}");
                    if (data.IsAllMissingOptional && ServerInfo._config.RequiredMods.Contains(mod))
                    {
                        data.IsAllMissingOptional = false;
                    }
                }
                else
                {
                    data.MissingMods.Add(mod);
                    data.ModsToEnable.Add(mod);
                    data.Display.Add($"[Unavailable]> {mod}");
                    if (data.IsAllMissingOptional && ServerInfo._config.RequiredMods.Contains(mod))
                    {
                        data.IsAllMissingOptional = false;
                    }
                }
            }
            return data;
        }

        private void ConnectToServer() 
        {
            Network.Ip = ServerInfo._ip;
            Network.Port = ServerInfo._port.ToString();
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Wait("Trying to connect to server"));
            Threader.GenerateThread(Threader.Mode.Start);
            Close();
        }

        private class ModlistAnalyzer
        {
            public Dictionary<string, ulong> DownloadableMods { get; set; } = new();
            public List<string> MissingMods { get; set; } = new();
            public List<string> Display { get; set; } = new();
            public List<string> ModsToEnable { get; set; } = new();
            public bool IsAllMissingOptional { get; set; } = true;
        }
    }
}
