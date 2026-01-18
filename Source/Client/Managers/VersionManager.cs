using GameClient.Core;
using GameClient.Dialogs;
using GameClient.Hooks.TCPNetwork;
using GameClient.Misc;
using Shared;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TCPNetwork.Packets;
using UnityEngine;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Managers
{
    public static class VersionManager
    {
        [HandlesPacket(PacketHeader.VersionManager)]
        private static void ParsePacket(byte[] bytes)
        {
            VersionData data = Serializer.ConvertBytesToObject<VersionData>(bytes);

            switch (data._step)
            {
                case VersionData.VersionStep.Ask:
                    SendClientVersion();
                    break;

                case VersionData.VersionStep.Pass:
                    LoginManager.UseLoginData();
                    break;
            }
        }

        public static void SendClientVersion()
        {
            VersionData data = new VersionData();
            data._version = CommonValues.ExecutableVersion;

            ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.VersionManager, data);
        }

        public static void PromptChangeVersion()
        {
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Inputs("Version selection", 
                new string[] { "Release number", "Password (optional)" }, 
                new bool[] { false, true }, ChangeVersion));
        }

        private static void ChangeVersion()
        {
            string downloadPath = Path.Combine(Master.AppdataVersionPath, "3005289691.zip");
            string uri = $"https://github.com/RimWorld-Together/Rimworld-Together/releases/download/{RT_Dialog_Inputs.DialogInputResults[0]}/3005289691.zip";

            if (!DownloadVersion(uri, downloadPath))
            {
                RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR",
                    new string[] { "Failed to download specified version! Please retry" }));
            }

            else
            {
                Action toDo = delegate
                {
                    string scriptPath = Path.Combine(Master.ModScriptsPath, "VersionUpdater.bat");
                    string copyPath = Path.Combine(Master.AppdataTempPath, "VersionUpdater.bat");
                    string modPath = Path.Combine(Master.AppdataTempPath, "ModPath.txt");

                    File.Copy(scriptPath, copyPath);
                    File.WriteAllText(modPath, Master.ModMainPath);

                    ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", $"/c {$"\"\"{copyPath}\""}");
                    processInfo.UseShellExecute = false;
                    Process.Start(processInfo);

                    Application.Quit();
                };

                RT_Dialog_Message dialog = new RT_Dialog_Message("MESSAGE", new string[] { "The game will close to apply the new version" },
                    toDo);

                RT_Dialog_Base.PushNewDialog(dialog);
            }
        }

        private static bool DownloadVersion(string uri, string downloadPath)
        {
            try
            {
                if (File.Exists(downloadPath)) File.Delete(downloadPath);

                using WebClient webClient = new WebClient();
                webClient.DownloadFile(new Uri(uri), downloadPath);

                return true;
            }
            catch { return false; }
        }
    }
}
