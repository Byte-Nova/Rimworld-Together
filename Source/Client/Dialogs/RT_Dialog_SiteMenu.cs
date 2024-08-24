using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace GameClient.Dialogs
{
    public class RT_Dialog_SiteMenu : Window
    {
        public Vector2 initialSize = new Vector2(700f, 450);
        public override Vector2 InitialSize => initialSize;

        public string title = "Choose a site";

        private Vector2 scrollPosition = Vector2.zero;
        public override void DoWindowContents(Rect rect)
        {
            Widgets.DrawLineHorizontal(rect.x, rect.y - 1, rect.width);
            Widgets.DrawLineHorizontal(rect.x, rect.yMax + 1, rect.width);

            float centeredX = rect.width / 2;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(title).x / 2, rect.y, Text.CalcSize(title).x, Text.CalcSize(title).y), title);
            if(Widgets.CloseButtonFor(rect))Close();

            Rect mainRect = new Rect(0, 50f, rect.width, rect.height - 50f);
            float height = 6f + (float)DataTest.GetSiteData().Count() * 50f;
            Rect viewRect = new Rect(0f, 50f, mainRect.width - 16f, height);
            Widgets.BeginScrollView(mainRect, ref scrollPosition, viewRect);
            float num = 0;
            float num2 = scrollPosition.y - 30f;
            float num3 = scrollPosition.y + mainRect.height;
            int num4 = 0;

            for (int i = 0; i < DataTest.GetSiteData().Count(); i++) // Function to get the SiteData. Replace with current site values once logged in
            {
                if (num > num2 && num < num3)
                {
                    Rect inRect = new Rect(0f, num, viewRect.width, 50f);
                    DrawCustomRow(inRect, DataTest.GetSiteData()[i], num4);
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
            Widgets.DrawTextureFitted(textRect, thing.ExpandingIconTexture, 1f); // Icon of the site
            Widgets.Label(fixedRect, thing.description); // Description of the site
            if (Mouse.IsOver(highLightRect)) 
            {
                Widgets.DrawLineHorizontal(highLightRect.x, highLightRect.y, highLightRect.width);
                Widgets.DrawLineHorizontal(highLightRect.x, highLightRect.yMax, highLightRect.width);
                Widgets.DrawLineVertical(highLightRect.x, highLightRect.y, highLightRect.height);
                Widgets.DrawLineVertical(highLightRect.xMax - 1 , highLightRect.y, highLightRect.height);
            }
            if (Widgets.ButtonInvisible(highLightRect))Find.WindowStack.Add(new RT_Dialog_SiteMenu_Info(thing));
        }
    }

    public class RT_Dialog_SiteMenu_Info : Window 
    {
        public Vector2 initialSize = new Vector2(450f, 250f);
        public override Vector2 InitialSize => initialSize;

        public SitePartDef thing;

        public string title;

        private Vector2 scrollPosition = Vector2.zero;

        public RT_Dialog_SiteMenu_Info(SitePartDef thingChosen) //Send chosen site over
        {
            thing = thingChosen;
            title = thingChosen.label;
        }
        public override void DoWindowContents(Rect mainRect)
        {
            List<ThingDef> dummyCost = new List<ThingDef>() { DefDatabase<ThingDef>.GetNamed("WoodLog"), DefDatabase<ThingDef>.GetNamed("WoodLog"), DefDatabase<ThingDef>.GetNamed("WoodLog"), DefDatabase<ThingDef>.GetNamed("WoodLog"), DefDatabase<ThingDef>.GetNamed("WoodLog"), DefDatabase<ThingDef>.GetNamed("WoodLog"), DefDatabase<ThingDef>.GetNamed("WoodLog"), DefDatabase<ThingDef>.GetNamed("WoodLog") }; // Dummy list
            int amount = 27; // Dummy amount
            List<ThingDef> dummylist = new List<ThingDef>() { DefDatabase<ThingDef>.GetNamed("Steel"), DefDatabase<ThingDef>.GetNamed("WoodLog"), DefDatabase<ThingDef>.GetNamed("Steel") }; // Dummy list
            int time = 30; // Dummy time

            Widgets.DrawLineHorizontal(mainRect.x, mainRect.y - 1, mainRect.width);
            Widgets.DrawLineHorizontal(mainRect.x, mainRect.yMax + 1, mainRect.width);

            if (Widgets.CloseButtonFor(mainRect)) Close();
            float centeredX = mainRect.width / 2;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(title).x / 2, mainRect.y, Text.CalcSize(title).x, Text.CalcSize(title).y), title);

            Rect leftColumn = new Rect(mainRect.x, mainRect.y + 30f, mainRect.width / 2, mainRect.height - 20f);
            Widgets.DrawTextureFitted(leftColumn, thing.ExpandingIconTexture, 1f); // Icon of the site

            Rect rightColumn = new Rect(mainRect.width / 2, mainRect.y + 30f, mainRect.width / 2, mainRect.height - 70f);
            float heightDesc = Text.CalcHeight(thing.description, rightColumn.width - 16f) / 2;
            float height = 40f + (float)dummyCost.Count() * 15f + (float)dummylist.Count() * 15f + heightDesc;
            Rect viewRightColumn = new Rect(rightColumn.x, rightColumn.y, rightColumn.width - 16f, height);

            Widgets.BeginScrollView(rightColumn, ref scrollPosition, viewRightColumn);
            Text.Font = GameFont.Small;
            float num = viewRightColumn.y;

            Widgets.Label(new Rect(viewRightColumn.x, num, viewRightColumn.width, heightDesc), thing.description); // Description of site
            num += heightDesc;
            Logger.Warning(num.ToString());
            Widgets.Label(new Rect(viewRightColumn.x, num, viewRightColumn.width, 20f), ("Cost:"));
            Text.Font = GameFont.Tiny;
            num += 20f;

            foreach (ThingDef thing in dummyCost)
            {
                Widgets.Label(new Rect(viewRightColumn.x, num, viewRightColumn.width, 15f), $"- {thing.label} {amount.ToString()}");
                num += 15f;
            }
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(viewRightColumn.x, num, viewRightColumn.width, 20f), ($"Produces every {time.ToString()} minutes:"));
            num += 20f;
            Text.Font = GameFont.Tiny;
            foreach (ThingDef thing in dummylist)
            {
                Widgets.Label(new Rect(viewRightColumn.x, num, viewRightColumn.width, 15f), $"- {thing.label} {amount.ToString()} ");
                num += 15f;
            }
            Widgets.EndScrollView();
            if (Widgets.ButtonText(new Rect(rightColumn.x + 5f, rightColumn.yMax, rightColumn.width - 10f, 40f), "Buy")) ; //Call function here to place site
        }
    }

    public static class DataTest //This class is purely to get dummy data to fill the fields. Can be removed once real data can be acquired
    {
        public static List<SitePartDef> GetSiteData()
        {
            List<SitePartDef> defs = new List<SitePartDef>();
            foreach (SitePartDef def in DefDatabase<SitePartDef>.AllDefs)
            {
                if (def.defName == "RTFarmland") defs.Add(def);
                else if (def.defName == "RTQuarry") defs.Add(def);
                else if (def.defName == "RTSawmill") defs.Add(def);
                else if (def.defName == "RTBank") defs.Add(def);
                else if (def.defName == "RTLaboratory") defs.Add(def);
                else if (def.defName == "RTRefinery") defs.Add(def);
                else if (def.defName == "RTHerbalWorkshop") defs.Add(def);
                else if (def.defName == "RTTextileFactory") defs.Add(def);
                else if (def.defName == "RTFoodProcessor") defs.Add(def);
            }
            return defs;
        }
    }
}

