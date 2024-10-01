using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Shared;
using UnityEngine;
using Verse;

namespace GameClient
{
    //Dialog window for the market functions

    public class RT_Dialog_MarketListing : Window
    {
        //UI

        public override Vector2 InitialSize => new Vector2(600f, 400f);

        private Vector2 scrollPosition = Vector2.zero;

        private readonly string title;

        private readonly string description;

        private readonly Action actionClick;

        private readonly Action actionCancel;

        private readonly float buttonX = 100f;

        private readonly float buttonY = 38f;

        private readonly float selectButtonX = 47f;

        private readonly float selectButtonY = 25f;

        //Variables

        private readonly Thing[] elements;

        private readonly Map settlementMap;

        public RT_Dialog_MarketListing(ThingDataFile[] elements, Map settlementMap, Action actionClick = null, Action actionCancel = null)
        {
            DialogManager.dialogMarketListing = this;

            title = "Global Market";
            description = $"Silver available for trade: {RimworldManager.GetSpecificThingCountInMap(ThingDefOf.Silver, settlementMap)}";
            List<Thing> things = new List<Thing>();
            foreach (var element in elements)
            {
                Logger.Warning(element.DefName);
                Thing thing = null;
                try { thing = ThingScribeManager.StringToItem(element); } catch{ continue; }
                if (thing != null)
                {
                    Logger.Warning(thing.def.defName);
                    things.Add(thing);
                }
                Logger.Error("-------------");
            }
            Logger.Error(things.Count().ToString());
            this.elements = things.ToArray();
            this.actionClick = actionClick;
            this.actionCancel = actionCancel;
            this.settlementMap = settlementMap;

            forcePause = true;
            absorbInputAroundWindow = true;
            soundAppear = SoundDefOf.CommsWindow_Open;
            
            closeOnAccept = false;
            closeOnCancel = true;
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

            Widgets.DrawLineHorizontal(rect.x, descriptionLineDif2, rect.width);

            FillMainRect(new Rect(0f, descriptionLineDif2 + 10f, rect.width, rect.height - buttonY - 85f));

            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMin, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "New"))
            {
                if (Find.WorldObjects.Settlements.Find(fetch => FactionValues.playerFactions.Contains(fetch.Faction)) == null)
                {
                    DialogManager.PushNewDialog(new RT_Dialog_Error("There's no one in the server to trade with!"));
                }
                else { MarketManager.RequestAddStock(); }

                DialogManager.dialogMarketListing = null;
                Close();
            }

            if (Widgets.ButtonText(new Rect(new Vector2(centeredX - buttonX / 2, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "Reload"))
            {
                MarketManager.RequestReloadStock();
            }

            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMax - buttonX, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "Close")) 
            {
                DialogManager.dialogMarketListing = null;
                ClientValues.ToggleTransfer(false);
                
                if (actionCancel != null) actionCancel.Invoke();
                Close(); 
            }
        }

        private void FillMainRect(Rect mainRect)
        {
            float height = 6f + (float)elements.Count() * 30f;
            Rect viewRect = new Rect(0f, 0f, mainRect.width - 16f, height);
            Widgets.BeginScrollView(mainRect, ref scrollPosition, viewRect);
            float num = 0;
            float num2 = scrollPosition.y - 30f;
            float num3 = scrollPosition.y + mainRect.height;
            int num4 = 0;

            for (int i = 0; i < elements.Count(); i++)
            {
                if (num > num2 && num < num3)
                {
                    Rect rect = new Rect(0f, num, viewRect.width, 30f);

                    try
                    {
                        if (elements[i].MarketValue > 0) DrawCustomRow(rect, elements[i], num4);
                        else continue;
                    }
                    catch { Logger.Message(elements[i].def.defName); continue; }
                }

                num += 30f;
                num4++;
            }

            Widgets.EndScrollView();
        }

        private void DrawCustomRow(Rect rect, Thing toDisplay, int index)
        {
            Text.Font = GameFont.Small;
            Rect fixedRect = new Rect(new Vector2(rect.x, rect.y + 5f), new Vector2(rect.width - 16f, rect.height - 5f));
            if (index % 2 == 0) Widgets.DrawHighlight(fixedRect);

            string[] names = GetDisplayNames(toDisplay);
            string displayName = names[0];
            string displaySimple = names[1];
            Widgets.Label(fixedRect, $"{displayName.CapitalizeFirst()}");
            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMax - selectButtonX, rect.yMax - selectButtonY), new Vector2(selectButtonX, selectButtonY)), "Select"))
            {
                DialogManager.dialogMarketListingResult = index;

                Action toDo = delegate
                {
                    if (int.Parse(DialogManager.dialog1ResultOne) <= 0)
                    {
                        DialogManager.PushNewDialog(new RT_Dialog_Error("You are trying to request an invalid quantity!"));
                    }

                    else if (toDisplay.stackCount < int.Parse(DialogManager.dialog1ResultOne))
                    {
                        DialogManager.PushNewDialog(new RT_Dialog_Error("You are trying to request more than is available!"));
                    }

                    else
                    {
                        int requiredSilver = (int)(toDisplay.MarketValue * int.Parse(DialogManager.dialog1ResultOne));
                        if (RimworldManager.CheckIfHasEnoughSilverInMap(settlementMap, requiredSilver))
                        {
                            actionClick?.Invoke();
                            Close();
                        }
                        else DialogManager.PushNewDialog(new RT_Dialog_Error("You do not have enough silver!"));
                    }
                };
                DialogManager.PushNewDialog(new RT_Dialog_1Input($"{displaySimple} request", $"Type the quantity you want to request\nCost per unit:{toDisplay.MarketValue} | Amount available:{toDisplay.stackCount}", toDo, null));
            }
        }

        private string[] GetDisplayNames(Thing thing) 
        {
            string text = thing.LabelCapNoCount.CapitalizeFirst() + " ";
            string textSimple = thing.LabelCapNoCount.CapitalizeFirst();
            Type type;
            FieldInfo fieldInfo;
            ReadingOutcomeDoerGainResearch research;
            QualityCategory qc;
            switch (thing.def.defName)
            {
                case "TextBook":
                    Book book = (Book)thing;
                    text = book.def.defName + ": ";
                    book.BookComp.TryGetDoer<BookOutcomeDoerGainSkillExp>(out BookOutcomeDoerGainSkillExp xp);
                    if (xp != null)
                    {
                        foreach (KeyValuePair<SkillDef, float> pair in xp.Values) text += $"{pair.Key.defName}, ";
                    }
                    thing.TryGetQuality(out qc);
                    text += QualityUtility.GetLabelShort(qc);

                    textSimple = thing.def.defName + " ";
                    break;
                case "Schematic":
                    book = (Book)thing;
                    text = book.def.defName + ": ";
                    book.BookComp.TryGetDoer<ReadingOutcomeDoerGainResearch>(out research);
                    if (research != null)
                    {
                        type = research.GetType();
                        fieldInfo = type.GetField("values", BindingFlags.NonPublic | BindingFlags.Instance);
                        Dictionary<ResearchProjectDef, float> researchDict = (Dictionary<ResearchProjectDef, float>)fieldInfo.GetValue(research);
                        foreach (ResearchProjectDef key in researchDict.Keys) text += $"{key.defName}, ";
                    }
                    thing.TryGetQuality(out qc);
                    text += QualityUtility.GetLabelShort(qc);

                    textSimple = thing.def.defName + " ";
                    break;
                case "Novel":
                    book = (Book)thing;
                    text = book.def.defName + ": ";
                    type = book.GetType();
                    fieldInfo = type.GetField("joyFactor", BindingFlags.NonPublic | BindingFlags.Instance);
                    text += (float)fieldInfo.GetValue(book) * 100 + "% recreation, ";
                    thing.TryGetQuality(out qc);
                    text += QualityUtility.GetLabelShort(qc);

                    textSimple = thing.def.defName + " ";
                    break;
                case "Tome":
                    book = (Book)thing;
                    text = book.def.defName + ": ";
                    book.BookComp.TryGetDoer<ReadingOutcomeDoerGainResearch>(out research);
                    if (research != null)
                    {
                        type = research.GetType();
                        fieldInfo = type.GetField("values", BindingFlags.NonPublic | BindingFlags.Instance);
                        Dictionary<ResearchProjectDef, float> researchDict = (Dictionary<ResearchProjectDef, float>)fieldInfo.GetValue(research);
                        foreach (ResearchProjectDef key in researchDict.Keys) text += $"{key.defName}, ";

                        type = book.GetType();
                        fieldInfo = type.GetField("mentalBreakChancePerHour", BindingFlags.NonPublic | BindingFlags.Instance);
                        text += "mental break:"+ ((float)fieldInfo.GetValue(book) * 100).ToStringDecimalIfSmall() +"% ";
                        thing.TryGetQuality(out qc);
                        text += QualityUtility.GetLabel(qc);

                        textSimple = thing.def.defName + " ";
                    }
                    break;
                case "Genepack":
                    Genepack pack = (Genepack)thing;
                    text = pack.def.defName + ": ";
                    foreach (GeneDef gene in pack.GeneSet.GenesListForReading)
                    {
                        text += $"{gene.label}, ";
                    }
                    break;
            }
            return new string[] {text,textSimple};
        }
    }
}
