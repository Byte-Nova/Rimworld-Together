using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GameServer.Core;
using GameServer.Misc;
using Shared;

namespace GameServer.Managers
{
    public static class ModDownloadManager
    {
        private static readonly HttpClient client;
        private static readonly string pathForTempMod = Path.Combine(Master.tempPath ,"TempMod.mod");
        private static readonly string pathForTempDirectory = Path.Combine(Master.tempPath, "TempMod");


        static ModDownloadManager() 
        {
            HttpClientHandler handler = new HttpClientHandler() { AllowAutoRedirect = true };
            client = new HttpClient(handler);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
        }

        // We use this to find the latest Harmony release, since they like to change the numbers around
        public static async Task<string> GetDownloadUrlFromPartialUrl(string repoOwner, string repoName, string partialName) 
        {
            try
            {
                string apiUrl = $"https://api.github.com/repos/{repoOwner}/{repoName}/releases/latest";
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0"); // Avoids 403 or someshit

                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    HandleError403(response);
                    return string.Empty;
                }
                string responseJson = await client.GetStringAsync(apiUrl);
                using JsonDocument doc = JsonDocument.Parse(responseJson);
                JsonElement root = doc.RootElement;

                foreach (JsonElement asset in root.GetProperty("assets").EnumerateArray())
                {
                    string assetName = asset.GetProperty("name").GetString();
                    if (assetName.StartsWith(partialName))
                    {
                        string downloadUrl = asset.GetProperty("browser_download_url").GetString();
                        return downloadUrl;
                    }
                }
            }
            catch (Exception ex)
            {
                Printer.Error($"Exception while downloading from {repoName}\n{ex}");
            }
            return string.Empty;
        }
        // Harmony is required for almost any patches to work, so we fetch it automatically.
        public static async Task DownloadHarmony() 
        {
            try
            {
                await (Task.Delay(2000)); // This is to avoid an API error about rate limit, fun I know
                string url = await GetDownloadUrlFromPartialUrl("pardeike", "Harmony", "Harmony-Fat");
                if (url == string.Empty)
                {
                    throw new Exception();
                }
                await Download(url, true, true);
                ZipFile.ExtractToDirectory(pathForTempMod, pathForTempDirectory);
                string pathToCheck = Path.Combine(Master.compatibilityPatchesPath, "0Harmony.dll");
                if (File.Exists(pathToCheck))
                    File.Delete(pathToCheck);
                File.Copy(Path.Combine(pathForTempDirectory, "net8.0", "0Harmony.dll"), pathToCheck);
                File.Delete(pathForTempMod);
                Directory.Delete(pathForTempDirectory, true);
            }
            catch (Exception ex)
            {
                Printer.Error($"Failed to download Harmony...\n{ex}");

                if(File.Exists(pathForTempMod))
                    File.Delete(pathForTempMod);
                if (Directory.Exists(pathForTempDirectory))
                    Directory.Delete(pathForTempDirectory, true);
            }
        }
        // We use this method if we find a .dll by default
        public static async Task<Boolean> Download(string url, bool compressedDownload = false, bool wasHarmony = false)
        {
            try
            {
                Printer.Warning($"Downloading {url}");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    HandleError403(response);
                    return false;
                }
                response.EnsureSuccessStatusCode();

                await using (FileStream fileStream = new FileStream(pathForTempMod, FileMode.Create))
                {
                    await response.Content.CopyToAsync(fileStream);
                }
                if (!compressedDownload) 
                {
                    string pathToCheck = Path.Combine(Master.compatibilityPatchesPath, url.Split("/").Last());
                    if(File.Exists(pathToCheck))
                        File.Delete(pathToCheck);
                    File.Copy(pathForTempMod, pathToCheck);
                    File.Delete(pathForTempMod);
                }
                Printer.Warning($"Downloaded {url.Split("/").Last()}.");
                if (!wasHarmony)
                {
                    if (!File.Exists(Path.Combine(Master.compatibilityPatchesPath, "0Harmony.dll")))
                    {
                        Printer.Warning("Harmony was missing, downloading automatically...");
                        await ModDownloadManager.DownloadHarmony();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during download of {url}: {ex}");

                if (File.Exists(pathForTempMod))
                    File.Delete(pathForTempMod);
                if (Directory.Exists(pathForTempDirectory))
                    Directory.Delete(pathForTempDirectory, true);
            }
            return false;
        }
        // We use this method if we find a .zip file by default
        public async static Task<bool> DownloadAndDecompress(string url)
        {
            bool result = await Download(url, true);
            if (!result)
                return false;
            Printer.Warning("Decompressing...");
            try
            {
                ZipFile.ExtractToDirectory(pathForTempMod, pathForTempDirectory);
                string sourcePath1 = Directory.GetDirectories(pathForTempDirectory).Length == 0 ? Directory.GetFiles(Directory.GetDirectories(pathForTempDirectory).First()).First() : string.Empty;
                string sourcePath2 = Directory.GetFiles(pathForTempDirectory).FirstOrDefault() ?? string.Empty;
                if (sourcePath1 != string.Empty) 
                {
                    string pathToCheck = Master.compatibilityPatchesPath + Path.GetFileName(sourcePath1);
                    if (File.Exists(pathToCheck))
                        File.Delete(pathToCheck);
                    File.Copy(sourcePath1, pathToCheck);
                }
                if(sourcePath2 != string.Empty) 
                {
                    string pathToCheck = Master.compatibilityPatchesPath + Path.GetFileName(sourcePath2);
                    if (File.Exists(pathToCheck))
                        File.Delete(pathToCheck);
                    File.Copy(sourcePath2, pathToCheck);
                }
                Directory.Delete(pathForTempDirectory, true);
                File.Delete(pathForTempMod);
                Printer.Warning($"Decompressed {url}");
                if (!File.Exists(Path.Combine(Master.compatibilityPatchesPath, "0Harmony.dll")))
                {
                    Printer.Warning("Harmony was missing, downloading automatically...");
                    await ModDownloadManager.DownloadHarmony();
                }
                return true;
            }
            catch (Exception ex)
            {
                Printer.Error($"Error during decompression\n{ex}");

                if(File.Exists(pathForTempMod))
                    File.Delete(pathForTempMod);
                if(Directory.Exists(pathForTempDirectory))
                    Directory.Delete(pathForTempDirectory, true );
            }
            return false;
        }
        // We use this to fetch automatically a .dll or .zip if the user didn't specify one
        public static async Task<string> FetchLatestReleaseFile(string url)
        {
            HttpResponseMessage response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Printer.Error("Failed to fetch latest release information.");
                return string.Empty;
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();
            JsonDocument json = JsonDocument.Parse(jsonResponse);
            JsonElement root = json.RootElement;

            if (!root.TryGetProperty("assets", out JsonElement assets))
                return string.Empty;

            string fileUrl = assets.EnumerateArray()
                .Select(asset => asset.GetProperty("browser_download_url").GetString())
                .FirstOrDefault(url => url.EndsWith(".dll") || url.EndsWith(".zip"));

            return fileUrl;
        }

        private static void HandleError403(HttpResponseMessage response) 
        {

            string remainingRequests = response.Headers.Contains("X-RateLimit-Remaining")
    ? response.Headers.GetValues("X-RateLimit-Remaining").FirstOrDefault()
    : "Unknown";

            DateTimeOffset resetTime = response.Headers.Contains("X-RateLimit-Reset")
                ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(response.Headers.GetValues("X-RateLimit-Reset").First())).LocalDateTime
                : DateTimeOffset.Now;

            Printer.Error($"Rate limit exceeded. This most likely means you tried downloading mods too quickly. \nYou can try again at: {resetTime}\nRemember you can always download mods manually.");
            Printer.Error($"You had {remainingRequests} left", CommonEnumerators.LogImportanceMode.Verbose);
        }
    }
}
