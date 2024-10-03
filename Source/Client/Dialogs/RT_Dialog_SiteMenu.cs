using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using Shared;

namespace GameClient.Dialogs
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
                if(isInConfigMode) Find.WindowStack.Add(new RT_Dialog_SiteMenu_Config(thing));
                else Find.WindowStack.Add(new RT_Dialog_SiteMenu_Info(thing));
        }
    }

    public class RT_Dialog_SiteMenu_Info : Window 
    {
        public Vector2 initialSize = new Vector2(450f, 250f);
        public override Vector2 InitialSize => initialSize;

        public SitePartDef sitePartDef;

        public SiteInfoFile configFile;

        public Dictionary<ThingDef,int> costThing = new Dictionary<ThingDef, int>();

        public Dictionary<ThingDef, int> rewardThing = new Dictionary<ThingDef, int>();

        public string title;

        private Vector2 scrollPosition = Vector2.zero;

        private bool invalid;

        public RT_Dialog_SiteMenu_Info(SitePartDef thingChosen) //Send chosen site over
        {
            sitePartDef = thingChosen;
            title = thingChosen.label;
            configFile = SiteManager.siteData.Where(f => f.DefName == thingChosen.defName).First();
            for (int i = 0; i < configFile.DefNameCost.Length; i++)
            {
                ThingDef toAdd = DefDatabase<ThingDef>.GetNamedSilentFail(configFile.DefNameCost[i]);
                if (toAdd != null) costThing.Add(toAdd, configFile.Cost[i]);
                else Logger.Warning($"{configFile.DefNameCost[i]} could not be found and won't be added to the list. Double check the def exists.");
            }
            for (int i = 0; i < configFile.Rewards.Length; i++) 
            {
                ThingDef toAdd = DefDatabase<ThingDef>.GetNamedSilentFail(configFile.Rewards[i].RewardDef);
                if (toAdd != null) rewardThing.Add(toAdd, configFile.Rewards[i].RewardAmount);
                else Logger.Warning($"{configFile.Rewards[i].RewardDef} could not be found and won't be added to the list. Double check the def exists.");
            }
            if (rewardThing.Keys.Count == 0)
            {
                Logger.Error($"Could not load any rewards for the sites. Please double check your configs to make sure they are valid");
                invalid = true; // Apparently you can't "this.Close() in the constructor
            }
            if (costThing.Keys.Count == 0)
            {
                Logger.Error($"Could not load any cost for the sites. Please double check your configs to make sure they are valid");
                invalid = true; // Apparently you can't "this.Close() in the constructor
            }
            DialogManager.dialogSiteMenuInfo = this;
        }

        public override void DoWindowContents(Rect mainRect)
        {
            if (invalid) 
            {
                DialogManager.PushNewDialog(new RT_Dialog_Error("Site could not be loaded because of invalid configuration"));
                this.Close();
            }
            Widgets.DrawLineHorizontal(mainRect.x, mainRect.y - 1, mainRect.width);
            Widgets.DrawLineHorizontal(mainRect.x, mainRect.yMax + 1, mainRect.width);

            if (Widgets.CloseButtonFor(mainRect)) Close();
            float centeredX = mainRect.width / 2;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(title).x / 2, mainRect.y, Text.CalcSize(title).x, Text.CalcSize(title).y), title);

            Rect leftColumn = new Rect(mainRect.x, mainRect.y + 30f, mainRect.width / 2, mainRect.height - 20f);
            Widgets.DrawTextureFitted(leftColumn, sitePartDef.ExpandingIconTexture, 1f); // Icon of the site

            Rect rightColumn = new Rect(mainRect.width / 2, mainRect.y + 30f, mainRect.width / 2, mainRect.height - 70f);
            float heightDesc = Text.CalcHeight(sitePartDef.description, rightColumn.width - 16f) / 2 + 9f;
            float height = 40f + ((float)costThing.Count() * 25f) + ((float)rewardThing.Count() * 25f) + heightDesc;
            Rect viewRightColumn = new Rect(rightColumn.x, rightColumn.y, rightColumn.width - 16f, height);

            Widgets.BeginScrollView(rightColumn, ref scrollPosition, viewRightColumn);
            Text.Font = GameFont.Small;
            float num = viewRightColumn.y;

            Widgets.Label(new Rect(viewRightColumn.x, num, viewRightColumn.width, heightDesc), sitePartDef.description); // Description of site
            num += heightDesc;
            Widgets.Label(new Rect(viewRightColumn.x, num, viewRightColumn.width, 20f), ("Cost:"));
            num += 20f;

            foreach (ThingDef thing in costThing.Keys)
            {
                Widgets.Label(new Rect(viewRightColumn.x, num, viewRightColumn.width, 25), $"- {thing.label} {costThing[thing].ToString()}");
                num += 25;
            }
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(viewRightColumn.x, num, viewRightColumn.width, 20f), ($"Produces every {SiteManager.interval.ToString()} minutes:"));
            num += 20f;
            foreach (ThingDef thing in rewardThing.Keys)
            {
                Widgets.Label(new Rect(viewRightColumn.x, num, viewRightColumn.width, 25), $"- {thing.label} {rewardThing[thing].ToString()} ");
                num += 25;
            }
            Widgets.EndScrollView();
            if (Widgets.ButtonText(new Rect(rightColumn.x + 5f, rightColumn.yMax, rightColumn.width - 10f, 40f), "Buy"))
            {
                SiteManager.RequestSiteBuild(configFile);
                DialogManager.dialogSiteMenu.Close();
                DialogManager.dialogSiteMenuInfo.Close();
            }
        }
    }

    public class RT_Dialog_SiteMenu_Config : Window
    {
        public Vector2 initialSize = new Vector2(600f, 250f);
        public override Vector2 InitialSize => initialSize;

        public SitePartDef sitePartDef;

        public SiteInfoFile configFile;

        public Dictionary<ThingDef, int> costThing = new Dictionary<ThingDef, int>();

        public Dictionary<ThingDef, int> rewardThing = new Dictionary<ThingDef, int>();

        public string title;

        private Vector2 scrollPosition = Vector2.zero;

        private bool invalid;

        public RT_Dialog_SiteMenu_Config(SitePartDef thingChosen) //Send chosen site over
        {
            sitePartDef = thingChosen;
            title = thingChosen.label;
            configFile = SiteManager.siteData.Where(f => f.DefName == thingChosen.defName).First();
            for (int i = 0; i < configFile.DefNameCost.Length; i++)
            {
                ThingDef toAdd = DefDatabase<ThingDef>.GetNamedSilentFail(configFile.DefNameCost[i]);
                if (toAdd != null) costThing.Add(toAdd, configFile.Cost[i]);
                else Logger.Warning($"{configFile.DefNameCost[i]} could not be found and won't be added to the list. Double check the def exists.");
            }
            for (int i = 0; i < configFile.Rewards.Length; i++)
            {
                ThingDef toAdd = DefDatabase<ThingDef>.GetNamedSilentFail(configFile.Rewards[i].RewardDef);
                if (toAdd != null) rewardThing.Add(toAdd, configFile.Rewards[i].RewardAmount);
                else Logger.Warning($"{configFile.Rewards[i].RewardDef} could not be found and won't be added to the list. Double check the def exists.");
            }
            if (rewardThing.Keys.Count == 0)
            {
                Logger.Error($"Could not load any rewards for the sites. Please double check your configs to make sure they are valid");
                invalid = true; // Apparently you can't "this.Close() in the constructor
            }
            if (costThing.Keys.Count == 0)
            {
                Logger.Error($"Could not load any cost for the sites. Please double check your configs to make sure they are valid");
                invalid = true; // Apparently you can't "this.Close() in the constructor
            }
            DialogManager.dialogSiteMenuConfig = this;
        }

        public override void DoWindowContents(Rect mainRect)
        {
            if (invalid)
            {
                DialogManager.PushNewDialog(new RT_Dialog_Error("Site could not be loaded because of invalid configuration"));
                this.Close();
            }
            Widgets.DrawLineHorizontal(mainRect.x, mainRect.y - 1, mainRect.width);
            Widgets.DrawLineHorizontal(mainRect.x, mainRect.yMax + 1, mainRect.width);

            if (Widgets.CloseButtonFor(mainRect)) Close();
            float centeredX = mainRect.width / 2;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(title).x / 2, mainRect.y, Text.CalcSize(title).x, Text.CalcSize(title).y), title);

            Rect leftColumn = new Rect(mainRect.x, mainRect.y + 30f, mainRect.width / 2, mainRect.height - 20f);
            Widgets.DrawTextureFitted(leftColumn, sitePartDef.ExpandingIconTexture, 1f);

            Rect rightColumn = new Rect(mainRect.width / 2, mainRect.y + 30f, mainRect.width / 2, mainRect.height);
            float heightDesc = Text.CalcHeight(sitePartDef.description, rightColumn.width - 16f) / 2 +9f;
            float height = 40f + (float)rewardThing.Count() * 25f + heightDesc;
            Rect viewRightColumn = new Rect(rightColumn.x, rightColumn.y, rightColumn.width - 16f, height);

            Widgets.BeginScrollView(rightColumn, ref scrollPosition, viewRightColumn);
            Text.Font = GameFont.Small;
            float num = viewRightColumn.y;

            Widgets.Label(new Rect(viewRightColumn.x, num, viewRightColumn.width, heightDesc), sitePartDef.description);
            num += heightDesc;

            Widgets.Label(new Rect(viewRightColumn.x, num, viewRightColumn.width, 20f), ($"Produces every {SiteManager.interval.ToString()} minutes:"));
            num += 20f;
            Text.Font = GameFont.Small;
            foreach (ThingDef thing in rewardThing.Keys)
            {
                Widgets.Label(new Rect(viewRightColumn.x, num, viewRightColumn.width, 25f), $"- {thing.label} {rewardThing[thing].ToString()} ");
                if (Widgets.ButtonText(new Rect(viewRightColumn.width + 210f, num, viewRightColumn.width - 210f, 25f), "Choose"))
                {
                    SiteManager.ChangeConfig(configFile, thing.defName);
                    DialogManager.dialogSiteMenu.Close();
                    DialogManager.dialogSiteMenuConfig.Close();
                }
                num += 25;
            }
            Widgets.EndScrollView();
        }
    }
}

