using System.Linq;
using System.Threading.Tasks;
using GameClient.Managers;
using GameClient.Misc;
using TCPNetwork.Packets;
using Shared;
using UnityEngine;
using Verse;

namespace GameClient.Dialogs
{
    public class RT_Dialog_ServerListing : RT_Dialog_Base
    {
        public override Vector2 InitialSize => new Vector2(650f, 400f);

        public ServerInfo[] AllServers { get; private set; } = new ServerInfo[0];

        public static RT_Dialog_Base Instance { get; private set; }

        private bool IsLoading { get; set; } = false;

        private bool FailedToFetchServers { get; set; } = false;

        private float _loadingStartTime = -1f;

        public RT_Dialog_ServerListing()
        {
            IsLoading = false;
            FailedToFetchServers = false;

            Instance = this;
            this.Title = "Server Browser";
            this.Description = "This is a list of all publicly available servers!";

            closeOnAccept = false;
            closeOnCancel = true;
            doCloseX = true;

            _ = GetServersAsync();
        }

        public override bool OnCloseRequest()
        {
            // todo abort server serach when closing...
            if (IsLoading)
            {
                Printer.Warning("Cannot close the server browser while it is loading!");
                return false;
            }
            return base.OnCloseRequest();
        }

        private async Task<bool> GetServersAsync()
        {
            IsLoading = true;
            _loadingStartTime = -1f;  // initialize the loading animation
            try
            {
                var success = await Task.Run(GetServers);
                FailedToFetchServers = !success;
                return success;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool GetServers() 
        {
            ServerInfo[] servers = ServerBrowserManager.GetAllServersAvailable();

            if (servers == null) return false;
            else
            {
                AllServers = servers;

                Printer.Warning($"Found {servers.Count()} servers in the server browser", CommonEnumerators.LogImportanceMode.Verbose);

                foreach (ServerInfo server in servers)
                {
                    Printer.Warning($"Server found! {server._name}", CommonEnumerators.LogImportanceMode.Verbose);
                }

                return true;
            }
        }

        public override void DoWindowContents(Rect rect)
        {
            if (FailedToFetchServers) Close();
            
            if (IsLoading) DoWindowContentsLoading(rect);
            else DoWindowContentsServerList(rect);
        }

        private void DoWindowContentsLoading(Rect rect)
        {
            if (_loadingStartTime < 0f)
                _loadingStartTime = Time.realtimeSinceStartup;

            int seconds = Mathf.FloorToInt(Time.realtimeSinceStartup - _loadingStartTime);

            float lineHeight = 32f;
            float centeredX = rect.width / 2;
            float windowDescriptionDif = Text.CalcSize(base.Description).y + StandardMargin;
            float titleLineDif1 = windowDescriptionDif - Text.CalcSize(base.Description).y * 0.25f;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(base.Title).x / 2, rect.y, Text.CalcSize(base.Title).x, Text.CalcSize(base.Title).y), base.Title);

            Widgets.DrawLineHorizontal(rect.x, titleLineDif1, rect.width);

            Text.Font = GameFont.Medium;
            var labelLoading = "Loading servers...";
            Widgets.Label(new Rect(centeredX - Text.CalcSize(labelLoading).x / 2, windowDescriptionDif + lineHeight, Text.CalcSize(labelLoading).x, Text.CalcSize(labelLoading).y), labelLoading);

            // Draw mood loading indicator
            var space = 100f;
            float moodY = windowDescriptionDif + space; //Text.CalcSize(base.Description).y * 1.1f;

            Text.Font = GameFont.Medium;
            var labelMood = "Mood";
            Widgets.Label(new Rect(100f, moodY, Text.CalcSize(labelMood).x, Text.CalcSize(labelMood).y), labelMood);

            moodY += lineHeight;
            Text.Font = GameFont.Small;
            DrawLabelWithValue(rect, moodY, "Very low expectations", "24", Color.green);

            moodY += lineHeight;
            Text.Font = GameFont.Small;
            DrawLabelWithValue(rect, moodY, "Waiting for servers", (-seconds-1).ToString(), Color.red);
        }

        private void DrawLabelWithValue(Rect rect, float y, string label, string value, Color valueColor)
        {
            float margin = 20f;
            float lineHeight = 32f;

            Vector2 labelSize = Text.CalcSize(label);
            Vector2 valueSize = Text.CalcSize(value);

            Widgets.Label(new Rect(rect.x + margin, y, labelSize.x, lineHeight), label);

            Color oldColor = GUI.color;
            GUI.color = valueColor;
            Widgets.Label(new Rect(rect.x + rect.width/2 - margin - valueSize.x, y, valueSize.x, lineHeight), value);
            GUI.color = oldColor;
        }

        private void DoWindowContentsServerList(Rect rect)
        {
            float centeredX = rect.width / 2;
            float windowDescriptionDif = Text.CalcSize(base.Description).y + StandardMargin;
            float descriptionLineDif1 = windowDescriptionDif - Text.CalcSize(base.Description).y * 0.25f;
            float descriptionLineDif2 = windowDescriptionDif + Text.CalcSize(base.Description).y * 1.1f;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(base.Title).x / 2, rect.y, Text.CalcSize(base.Title).x, Text.CalcSize(base.Title).y), base.Title);

            Widgets.DrawLineHorizontal(rect.x, descriptionLineDif1, rect.width);

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(base.Description).x / 2, windowDescriptionDif, Text.CalcSize(base.Description).x, Text.CalcSize(base.Description).y), base.Description);

            Text.Font = GameFont.Medium;
            FillMainRect(new Rect(0f, descriptionLineDif2 + 10f, rect.width, rect.height - DefaultButtonSize.y - 85f));

            Text.Font = GameFont.Small;
            if (Widgets.ButtonText(new Rect(new Vector2(centeredX - DefaultButtonSize.x / 2, rect.yMax - DefaultButtonSize.y), DefaultButtonSize), "Close")) Close();
        }

        private void FillMainRect(Rect mainRect)
        {
            float height = 6f + AllServers.Length * 30f;
            Rect viewRect = new Rect(0f, 0f, mainRect.width - 16f, height);
            Widgets.BeginScrollView(mainRect, ref base.ScrollPosition, viewRect);
            float num = 0;
            float num2 = base.ScrollPosition.y - 30f;
            float num3 = base.ScrollPosition.y + mainRect.height;
            int num4 = 0;
            AllServers = AllServers.ToList().OrderByDescending(x => x._currentPlayerCount).ToArray();
            for (int i = 0; i < AllServers.Length; i++)
            {
                if (num > num2 && num < num3)
                {
                    Rect rect = new Rect(0f, num, viewRect.width, 30f);
                    DrawCustomRow(rect, AllServers[i], num4);
                }

                num += 30f;
                num4++;
            }

            Widgets.EndScrollView();
        }

        private void DrawCustomRow(Rect rect, ServerInfo server, int index)
        {
            Text.Font = GameFont.Small;
            Rect fixedRect = new Rect(new Vector2(rect.x, rect.y + 5f), new Vector2(rect.width - 16f, rect.height - 5f));
            if (index % 2 == 0) Widgets.DrawHighlight(fixedRect);

            Widgets.Label(fixedRect, $"{server._name} - {server._ip} - {server._currentPlayerCount} / {server._maximumPlayerCount}");
            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMax - SmallerButtonSize.x - 5f, rect.yMax - TinyButtonSize.y), new Vector2(SmallerButtonSize.x, TinyButtonSize.y)), "Select"))
            {
                RT_Dialog_Base.PushNewDialog(new RT_Dialog_ServerListingInfo(server));
            }
        }

        private void ResetWindow() { RT_Dialog_Base.PushNewDialog(new RT_Dialog_ServerListing()); }
    }
}
