using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using Shared;

namespace GameClient
{
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
            configFile = SiteManager.siteValues.SiteInfoFiles.Where(f => f.DefName == thingChosen.defName).First();

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
            Widgets.Label(new Rect(viewRightColumn.x, num, viewRightColumn.width, 20f), $"Produces every {SiteManager.siteValues.TimeIntervalMinutes} minutes:");
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
}

