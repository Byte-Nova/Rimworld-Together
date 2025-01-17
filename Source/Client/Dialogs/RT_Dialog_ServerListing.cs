using System.Linq;
using GameClient.Core.Preferences;
using GameClient.Files;
using GameClient.Managers;
using GameClient.Misc;
using GameClient.TCP;
using RimWorld;
using UnityEngine;
using Verse;

namespace GameClient.Dialogs
{
    public class RT_Dialog_ServerListing : Window
    {
        public override Vector2 InitialSize => new Vector2(650f, 400f);

        public readonly string title = "Recent servers";

        public readonly string description = "This list shows the servers you last joined";

        private Vector2 scrollPosition = Vector2.zero;

        private readonly Vector2 button = new Vector2(150f, 38f);

        private readonly Vector2 selectButton = new Vector2(47f, 25f);

        private readonly Vector2 deleteButton = new Vector2(47f, 25f);

        public RecentServersFile recentServers => RecentServersManager.LoadRecentServers();

        public RT_Dialog_ServerListing()
        {
            DialogManager.dialogServerListing = this;

            forcePause = true;
            absorbInputAroundWindow = true;
            soundAppear = SoundDefOf.CommsWindow_Open;

            closeOnAccept = false;
            closeOnCancel = false;
        }

        public override void DoWindowContents(Rect rect)
        {
            float centeredX = rect.width / 2;

            float windowDescriptionDif = Text.CalcSize(description).y + StandardMargin;
            float descriptionLineDif1 = windowDescriptionDif - Text.CalcSize(description).y * 0.25f;
            float descriptionLineDif2 = windowDescriptionDif + Text.CalcSize(description).y * 1.1f;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(title).x / 2, rect.y, Text.CalcSize(title).x, Text.CalcSize(title).y), title);

            Widgets.DrawLineHorizontal(rect.x, descriptionLineDif1, rect.width);
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(description).x / 2, windowDescriptionDif, Text.CalcSize(description).x, Text.CalcSize(description).y), description);
            Text.Font = GameFont.Medium;
            Widgets.DrawLineHorizontal(rect.x, descriptionLineDif2, rect.width);

            FillMainRect(new Rect(0f, descriptionLineDif2 + 10f, rect.width, rect.height - button.y - 85f));

            Text.Font = GameFont.Small;
            if (Widgets.ButtonText(new Rect(new Vector2(centeredX - button.x / 2, rect.yMax - button.y), button), "Close")) Close();
        }

        private void FillMainRect(Rect mainRect)
        {
            float height = 6f + recentServers.ServerAddresses.Count() * 30f;
            Rect viewRect = new Rect(0f, 0f, mainRect.width - 16f, height);
            Widgets.BeginScrollView(mainRect, ref scrollPosition, viewRect);
            float num = 0;
            float num2 = scrollPosition.y - 30f;
            float num3 = scrollPosition.y + mainRect.height;
            int num4 = 0;

            for (int i = 0; i < recentServers.ServerAddresses.Count(); i++)
            {
                if (num > num2 && num < num3)
                {
                    Rect rect = new Rect(0f, num, viewRect.width, 30f);
                    DrawCustomRow(rect, recentServers.ServerNames[i], recentServers.ServerAddresses[i], num4);
                }

                num += 30f;
                num4++;
            }

            Widgets.EndScrollView();
        }

        private void DrawCustomRow(Rect rect, string serverName, string serverAddress, int index)
        {
            Text.Font = GameFont.Small;
            Rect fixedRect = new Rect(new Vector2(rect.x, rect.y + 5f), new Vector2(rect.width - 16f, rect.height - 5f));
            if (index % 2 == 0) Widgets.DrawHighlight(fixedRect);

            Widgets.Label(fixedRect, $"{serverName} - {serverAddress}");
            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMax - selectButton.x - deleteButton.x - 5f, rect.yMax - selectButton.y), selectButton), "Select"))
            {
                Network.ip = serverAddress.Split(':')[0];
                Network.port = serverAddress.Split(':')[1];

                DialogManager.PushNewDialog(new RT_Dialog_Wait("Trying to connect to server"));
                Threader.GenerateThread(Threader.Mode.Start);
                Close();
            }

            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMax - deleteButton.x, rect.yMax - deleteButton.y), deleteButton), "Delete"))
            {
                DialogManager.dialogServerListingIndex = index;

                RecentServersManager.RemoveServerFromList(serverName, serverAddress);

                ResetWindow();
            }
        }

        private void ResetWindow() { DialogManager.PushNewDialog(new RT_Dialog_ServerListing()); }
    }
}
