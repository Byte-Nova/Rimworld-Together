using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using Shared;

namespace GameClient
{
    public class RT_Dialog_SiteMenu : Window
    {
        public List<SiteInfoFile> SiteInfoFileList = new List<SiteInfoFile>();

        public Vector2 initialSize = new Vector2(700f, 450);
        
        public override Vector2 InitialSize => initialSize;

        public string title = "Choose a site";

        private Vector2 scrollPosition = Vector2.zero;

        private bool isInConfigMode;

        public RT_Dialog_SiteMenu(bool configMode) 
        {
            isInConfigMode = configMode;
            DialogManager.dialogSiteMenu = this;
        }

        public override void DoWindowContents(Rect rect)
        {
            Widgets.DrawLineHorizontal(rect.x, rect.y - 1, rect.width);
            Widgets.DrawLineHorizontal(rect.x, rect.yMax + 1, rect.width);

            float centeredX = rect.width / 2;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(title).x / 2, rect.y, Text.CalcSize(title).x, Text.CalcSize(title).y), title);
            if(Widgets.CloseButtonFor(rect))Close();

            Rect mainRect = new Rect(0, 50f, rect.width, rect.height - 50f);
            float height = 6f + (float)SiteManager.siteDefs.Count() * 50f;
            Rect viewRect = new Rect(0f, 50f, mainRect.width - 16f, height);
            Widgets.BeginScrollView(mainRect, ref scrollPosition, viewRect);
            float num = 50;
            float num2 = scrollPosition.y - 30f;
            float num3 = scrollPosition.y + mainRect.height;
            int num4 = 0;

            for (int i = 0; i < SiteManager.siteDefs.Length; i++)
            {
                if (num > num2 && num < num3)
                {
                    Rect inRect = new Rect(0f, num, viewRect.width, 50f);
                    DrawCustomRow(inRect, SiteManager.siteDefs[i], num4);
                }

                num += 50f;
                num4++;
            }

            Widgets.EndScrollView();
        }

        private void DrawCustomRow(Rect rect, SitePartDef thing, int index)
        {
            Text.Font = GameFont.Small;
            Rect highLightRect = new Rect(new Vector2(rect.x, rect.y), new Vector2(rect.width - 16f, 50f));
            Rect fixedRect = new Rect(new Vector2(highLightRect.x + 75, highLightRect.y), new Vector2(highLightRect.width - 75f, 55f));
            Rect textRect = new Rect(new Vector2(rect.x, rect.y), new Vector2(50f, 50f));

            if (index % 2 == 0) Widgets.DrawHighlight(highLightRect);
            Widgets.DrawTextureFitted(textRect, thing.ExpandingIconTexture, 1f);
            Widgets.Label(fixedRect, thing.description);

            if (Mouse.IsOver(highLightRect)) 
            {
                Widgets.DrawLineHorizontal(highLightRect.x, highLightRect.y, highLightRect.width);
                Widgets.DrawLineHorizontal(highLightRect.x, highLightRect.yMax, highLightRect.width);
                Widgets.DrawLineVertical(highLightRect.x, highLightRect.y, highLightRect.height);
                Widgets.DrawLineVertical(highLightRect.xMax - 1 , highLightRect.y, highLightRect.height);
            }

            if (Widgets.ButtonInvisible(highLightRect))
            {
                if (isInConfigMode) Find.WindowStack.Add(new RT_Dialog_SiteMenu_Config(thing));
                else Find.WindowStack.Add(new RT_Dialog_SiteMenu_Info(thing));
            }
        }
    }
}

