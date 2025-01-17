using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using GameClient.Core;
using GameClient.Dialogs;
using GameClient.Misc;
using Verse;

namespace GameClient.Managers
{
    public static class VersionManager
    {
        public static void PromptChangeVersion()
        {
            RT_Dialog_OK dialog2 = new RT_Dialog_OK("The game will restart to apply the new version", GenCommandLine.Restart);
            DialogManager.PushNewDialog(new RT_Dialog_2Input("Version selection", "Release number", "Password (optional)",
                delegate
                {
                    string downloadPath = Path.Combine(Master.appdataTempVersionPath, "3005289691.zip");
                    string extractPath = Path.Combine(Master.appdataTempVersionPath, "3005289691");
                    string uri = $"https://github.com/Byte-Nova/Rimworld-Together/releases/download/{DialogManager.dialog2ResultOne}/3005289691.zip";

                    if (!DownloadVersion(uri, downloadPath))
                    {
                        DialogManager.PushNewDialog(new RT_Dialog_OK("Version failed to download, check and try again"));
                        return;
                    }

                    else if (!UnzipVersion(downloadPath, extractPath))
                    {
                        DialogManager.PushNewDialog(new RT_Dialog_OK("Version failed to decompress, check logs for more information"));
                        return;
                    }

                    else if (!InstallVersion(extractPath))
                    {
                        DialogManager.PushNewDialog(new RT_Dialog_OK("Version failed to install, check logs for more information"));
                        return;
                    }

                    else if (!Cleanup(downloadPath))
                    {
                        DialogManager.PushNewDialog(new RT_Dialog_OK("Installer failed the cleanup, check logs for more information"));
                        return;
                    }

                    DialogManager.PushNewDialog(dialog2);
                }, null, false, true
            ));
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

                string appPath = Path.Combine(Master.modAddonsPath, "7z", "7z.exe");
                StartCMDWindow($"\"\"{appPath}\" x \"{downloadPath}\" -p\"{DialogManager.dialog2ResultTwo}\" -o\"{extractPath}\"");

                return true;
            }
            catch { return false; }
        }

        private static bool InstallVersion(string extractPath)
        {
            try
            {
                string modsDirectory = Directory.GetParent(Master.modMainPath).ToString();
                string installDirectory = Path.Combine(modsDirectory, "3005289691");

                StartCMDWindow($"rmdir \"{installDirectory}\" /s /q");

                StartCMDWindow($"move \"{extractPath}\" \"{modsDirectory}\"");

                return true;
            }
            catch { return false; }
        }

        private static bool Cleanup(string toClean)
        {
            try { StartCMDWindow($"del \"{toClean}\""); return true; }
            catch { return false; }
        }

        private static void StartCMDWindow(string command)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", $"/c {command}");
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;

            Process process = Process.Start(processInfo);
            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => Printer.Error(e.Data);
            process.BeginErrorReadLine();

            process.WaitForExit();
            process.Close();
        }
    }
}
