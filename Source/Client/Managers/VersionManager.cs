using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GameClient.Core;
using GameClient.Core.Preferences;
using GameClient.Dialogs;
using GameClient.Misc;
using GameClient.TCP;
using Shared;
using Verse;

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
                    UserLoginHandler.UseLoginData();
                    break;
            }
        }

        public static void SendClientVersion()
        {
            VersionData data = new VersionData();
            data._version = CommonValues.ExecutableVersion;

            Network.Listener.EnqueuePacket(PacketHeader.VersionManager, data);
        }

        public static void PromptChangeVersion()
        {
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Inputs("Version selection", 
                new string[] { "Release number", "Password (optional)" }, 
                new bool[] { false, true }, ChangeVersion));
        }

        private static void ChangeVersion()
        {
            string downloadPath = Path.Combine(Master.AppdataTempVersionPath, "3005289691.zip");
            string extractPath = Path.Combine(Master.AppdataTempVersionPath, "3005289691");
            string uri = $"https://github.com/Byte-Nova/Rimworld-Together/releases/download/{RT_Dialog_Inputs.DialogInputResults[0]}/3005289691.zip";

            bool freezeGame = true;
            Task.Run(delegate
            {
                if (!DownloadVersion(uri, downloadPath)) { freezeGame = false; return; }
                else if (!UnzipVersion(downloadPath, extractPath)) { freezeGame = false; return; }
                else if (!InstallVersion(extractPath)) { freezeGame = false; return; }
                else if (!Cleanup(downloadPath)) { freezeGame = false; return; }
                freezeGame = false;
            });

            while (freezeGame) Thread.Sleep(1);

            RT_Dialog_Message dialog2 = new RT_Dialog_Message("MESSAGE", new string[] { "The game will restart to apply the new version" }, 
                GenCommandLine.Restart);

            RT_Dialog_Base.PushNewDialog(dialog2);
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

        private static bool UnzipVersion(string downloadPath, string extractPath)
        {
            try
            {
                if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);

                string appPath = Path.Combine(Master.ModAddonsPath, "7z", "7z.exe");
                CMDExecuter.StartCMDWindow($"\"\"{appPath}\" x \"{downloadPath}\" -p\"{RT_Dialog_Inputs.DialogInputResults[1]}\" -o\"{extractPath}\"");

                return true;
            }
            catch { return false; }
        }

        private static bool InstallVersion(string extractPath)
        {
            try
            {
                string modsDirectory = Directory.GetParent(Master.ModMainPath).ToString();
                string installDirectory = Path.Combine(modsDirectory, "3005289691");

                CMDExecuter.StartCMDWindow($"rmdir \"{installDirectory}\" /s /q");

                CMDExecuter.StartCMDWindow($"move \"{extractPath}\" \"{modsDirectory}\"");

                return true;
            }
            catch { return false; }
        }

        private static bool Cleanup(string toClean)
        {
            try { CMDExecuter.StartCMDWindow($"del \"{toClean}\""); return true; }
            catch { return false; }
        }
    }
}
