using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public class SitesUI : WITab
    {
        private Vector2 scrollPosition;

        private static readonly Vector2 WinSize = new Vector2(432f, 540f);

        public override bool IsVisible => true;

        private string tabTitle;

        public SitesUI()
        {
            size = WinSize;
            labelKey = "Sites";
        }

        protected override void FillTab()
        {
            if (Network.state == ClientNetworkState.Connected)
            {
                tabTitle = $"Player Sites [{SiteManager.playerSites.Count()}]";

                float horizontalLineDif = Text.CalcSize(tabTitle).y + 3f + 10f;

                Rect outRect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
                Rect rect = new Rect(10f, 10f, outRect.width - 16f, Mathf.Max(0f, outRect.height));

                Text.Font = GameFont.Medium;
                Widgets.Label(rect, tabTitle);
                Widgets.DrawLineHorizontal(rect.x, horizontalLineDif, rect.width);
                GenerateList(new Rect(new Vector2(rect.x, rect.y + 30f), new Vector2(rect.width, rect.height - 30f)));
            }
        }

        private void GenerateList(Rect mainRect)
        {
            var orderedDictionary = SiteManager.playerSites.OrderBy(x => x.Label);

            float height = 6f + (float)orderedDictionary.Count() * 30f;
            Rect viewRect = new Rect(mainRect.x, mainRect.y, mainRect.width - 16f, height);

            Widgets.BeginScrollView(mainRect, ref scrollPosition, viewRect);

            float num = 0;
            float num2 = scrollPosition.y - 30f;
            float num3 = scrollPosition.y + mainRect.height;
            int num4 = 0;

            foreach (Site playerSite in orderedDictionary)
            {
                if (num > num2 && num < num3)
                {
                    Rect rect = new Rect(0f, mainRect.y + num, viewRect.width, 30f);
                    DrawCustomRow(rect, playerSite, num4);
                }

                num += 30f;
                num4++;
            }

            Widgets.EndScrollView();
        }

        private void DrawCustomRow(Rect rect, Site playerSite, int index)
        {
            Text.Font = GameFont.Small;

            if (index % 2 == 0) Widgets.DrawLightHighlight(rect);
            Rect fixedRect = new Rect(new Vector2(rect.x + 10f, rect.y + 5f), new Vector2(rect.width - 52f, rect.height));

            float buttonX = 47f;
            float buttonY = 30f;
            Widgets.Label(fixedRect, $"{playerSite.Label} - {playerSite.Tile}");
            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMax - buttonX, rect.y), new Vector2(buttonX, buttonY)), "Focus"))
            {
                foreach (Site site in Find.World.worldObjects.Sites)
                {
                    if (site.Tile == playerSite.Tile)
                    {
                        CameraJumper.TryJumpAndSelect(new GlobalTargetInfo(site));
                        break;
                    }
                }
            }
        }
    }
}
