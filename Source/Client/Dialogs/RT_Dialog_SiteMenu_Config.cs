using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using Shared;

namespace GameClient
{
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

            Rect rightColumn = new Rect(mainRect.width / 2, mainRect.y + 30f, mainRect.width / 2, mainRect.height - 20f);
            float heightDesc = Text.CalcHeight(sitePartDef.description, rightColumn.width - 16f) / 2 +9f;
            float height = 40f + (float)rewardThing.Count() * 25f + heightDesc;
            Rect viewRightColumn = new Rect(rightColumn.x, rightColumn.y, rightColumn.width - 16f, height);

            Widgets.BeginScrollView(rightColumn, ref scrollPosition, viewRightColumn);
            Text.Font = GameFont.Small;
            float num = viewRightColumn.y;

            Widgets.Label(new Rect(viewRightColumn.x, num, viewRightColumn.width, heightDesc), sitePartDef.description);
            num += heightDesc;

            Widgets.Label(new Rect(viewRightColumn.x, num, viewRightColumn.width, 20f), $"Produces every {SiteManager.siteValues.TimeIntervalMinutes.ToString()} minutes:");
            num += 20f;
            Text.Font = GameFont.Small;
            foreach (ThingDef thing in rewardThing.Keys)
            {
                Widgets.Label(new Rect(viewRightColumn.x, num, viewRightColumn.width, 25f), $"- {thing.label} {rewardThing[thing].ToString()} ");
                if (Widgets.ButtonText(new Rect(viewRightColumn.width + 210f, num, viewRightColumn.width - 210f, 25f), "Choose"))
                {
                    SiteManager.RequestSiteChangeConfig(configFile, thing.defName);
                    DialogManager.dialogSiteMenu.Close();
                    DialogManager.dialogSiteMenuConfig.Close();
                }
                num += 25;
            }
            Widgets.EndScrollView();
        }
    }
}

