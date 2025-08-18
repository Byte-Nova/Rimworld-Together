using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using Shared;
using GameClient.Managers;
using GameClient.Misc;
using Shared.Files;

namespace GameClient.Dialogs
{
    public class RT_Dialog_SiteMenu_Info : RT_Dialog_Base
    {
        public override Vector2 InitialSize => new Vector2(450f, 250f);

        public SitePartDef SitePartDef { get; private set; }

        public SiteInfoFile ConfigFile { get; private set; }

        public Dictionary<ThingDef, int> CostThing { get; private set; } = new Dictionary<ThingDef, int>();

        public Dictionary<ThingDef, int> RewardThing { get; private set; } = new Dictionary<ThingDef, int>();

        private bool IsInvalid { get; set; }

        public static RT_Dialog_SiteMenu_Info Instance { get; private set; }

        public RT_Dialog_SiteMenu_Info(SitePartDef thingChosen) //Send chosen site over
        {
            SitePartDef = thingChosen;
            this.Title = thingChosen.label;
            ConfigFile = SiteManager.SiteValues.SiteInfoFiles.Where(f => f.DefName == thingChosen.defName).First();
            Instance = this;

            for (int i = 0; i < ConfigFile.DefNameCost.Length; i++)
            {
                ThingDef toAdd = DefDatabase<ThingDef>.GetNamedSilentFail(ConfigFile.DefNameCost[i]);
                if (toAdd != null) CostThing.Add(toAdd, ConfigFile.Cost[i]);
                else Printer.Warning($"{ConfigFile.DefNameCost[i]} could not be found and won't be added to the list. Double check the def exists.");
            }

            for (int i = 0; i < ConfigFile.Rewards.Length; i++)
            {
                ThingDef toAdd = DefDatabase<ThingDef>.GetNamedSilentFail(ConfigFile.Rewards[i].RewardDef);
                if (toAdd != null) RewardThing.Add(toAdd, ConfigFile.Rewards[i].RewardAmount);
                else Printer.Warning($"{ConfigFile.Rewards[i].RewardDef} could not be found and won't be added to the list. Double check the def exists.");
            }

            if (RewardThing.Keys.Count == 0)
            {
                Printer.Error($"Could not load any rewards for the sites. Please double check your configs to make sure they are valid");
                IsInvalid = true; // Apparently you can't "this.Close() in the constructor
            }

            if (CostThing.Keys.Count == 0)
            {
                Printer.Error($"Could not load any cost for the sites. Please double check your configs to make sure they are valid");
                IsInvalid = true; // Apparently you can't "this.Close() in the constructor
            }
        }

        public override void DoWindowContents(Rect mainRect)
        {
            if (IsInvalid)
            {
                RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "Site could not be loaded because of invalid configuration" }));
                Close();
            }

            Widgets.DrawLineHorizontal(mainRect.x, mainRect.y - 1, mainRect.width);
            Widgets.DrawLineHorizontal(mainRect.x, mainRect.yMax + 1, mainRect.width);

            if (Widgets.CloseButtonFor(mainRect)) Close();
            float centeredX = mainRect.width / 2;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(Title).x / 2, mainRect.y, Text.CalcSize(Title).x, Text.CalcSize(Title).y), Title);

            Rect leftColumn = new Rect(mainRect.x, mainRect.y + 30f, mainRect.width / 2, mainRect.height - 20f);
            Widgets.DrawTextureFitted(leftColumn, SitePartDef.ExpandingIconTexture, 1f); // Icon of the site

            Rect rightColumn = new Rect(mainRect.width / 2, mainRect.y + 30f, mainRect.width / 2, mainRect.height - 70f);
            float heightDesc = Text.CalcHeight(SitePartDef.description, rightColumn.width - 16f) / 2 + 9f;
            float height = 40f + CostThing.Count() * 25f + RewardThing.Count() * 25f + heightDesc;
            Rect viewRightColumn = new Rect(rightColumn.x, rightColumn.y, rightColumn.width - 16f, height);

            Widgets.BeginScrollView(rightColumn, ref ScrollPosition, viewRightColumn);
            Text.Font = GameFont.Small;
            float num = viewRightColumn.y;

            Widgets.Label(new Rect(viewRightColumn.x, num, viewRightColumn.width, heightDesc), SitePartDef.description); // Description of site
            num += heightDesc;
            Widgets.Label(new Rect(viewRightColumn.x, num, viewRightColumn.width, 20f), "Cost:");
            num += 20f;

            foreach (ThingDef thing in CostThing.Keys)
            {
                Widgets.Label(new Rect(viewRightColumn.x, num, viewRightColumn.width, 25), $"- {thing.label} {CostThing[thing].ToString()}");
                num += 25;
            }

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(viewRightColumn.x, num, viewRightColumn.width, 20f), $"Produces every {SiteManager.SiteValues.TimeIntervalMinutes} minutes:");
            num += 20f;

            foreach (ThingDef thing in RewardThing.Keys)
            {
                Widgets.Label(new Rect(viewRightColumn.x, num, viewRightColumn.width, 25), $"- {thing.label} {RewardThing[thing].ToString()} ");
                num += 25;
            }

            Widgets.EndScrollView();
            if (Widgets.ButtonText(new Rect(rightColumn.x + 5f, rightColumn.yMax, rightColumn.width - 10f, 40f), "Buy"))
            {
                SiteManager.RequestSiteBuild(ConfigFile);
                RT_Dialog_SiteMenu.Instance.Close();
                RT_Dialog_SiteMenu_Info.Instance.Close();
            }
        }
    }
}

