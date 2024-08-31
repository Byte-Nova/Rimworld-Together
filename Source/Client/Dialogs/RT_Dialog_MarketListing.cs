using System;
using System.Linq;
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

        public override Vector2 InitialSize => new Vector2(400f, 400f);

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

        private readonly ThingData[] elements;

        private readonly Map settlementMap;

        public RT_Dialog_MarketListing(ThingData[] elements, Map settlementMap, Action actionClick = null, Action actionCancel = null)
        {
            DialogManager.dialogMarketListing = this;

            title = "RTGlobalMarket".Translate();
            description = "RTAvailableSilver".Translate(RimworldManager.GetSilverInMap(settlementMap));

            this.elements = elements;
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

            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMin, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "RTDialogNew".Translate()))
            {
                if (Find.WorldObjects.Settlements.Find(fetch => FactionValues.playerFactions.Contains(fetch.Faction)) == null)
                {
                    DialogManager.PushNewDialog(new RT_Dialog_Error("RTNoOneToTrade".Translate()));
                }
                else { MarketManager.RequestAddStock(); }

                DialogManager.dialogMarketListing = null;
                Close();
            }

            if (Widgets.ButtonText(new Rect(new Vector2(centeredX - buttonX / 2, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "RTDialogReload".Translate()))
            {
                MarketManager.RequestReloadStock();
            }

            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMax - buttonX, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "RTDialogClose".Translate())) 
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
                        Thing thing = ThingScribeManager.StringToItem(elements[i]);
                        if (thing.MarketValue > 0) DrawCustomRow(rect, thing, num4);
                        else continue;
                    }
                    catch { continue; }
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
            
            Widgets.Label(fixedRect, $"{toDisplay.Label.CapitalizeFirst()} > ${toDisplay.MarketValue}/u");
            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMax - selectButtonX, rect.yMax - selectButtonY), new Vector2(selectButtonX, selectButtonY)), "RTDialogSelect".Translate()))
            {
                DialogManager.dialogMarketListingResult = index;

                Action toDo = delegate
                {
                    if (int.Parse(DialogManager.dialog1ResultOne) <= 0)
                    {
                        DialogManager.PushNewDialog(new RT_Dialog_Error("RTTradeQuantityError".Translate()));
                    }

                    else if (toDisplay.stackCount < int.Parse(DialogManager.dialog1ResultOne))
                    {
                        DialogManager.PushNewDialog(new RT_Dialog_Error("RTTradeTooMuch".Translate()));
                    }

                    else
                    {
                        int requiredSilver = (int)(toDisplay.MarketValue * int.Parse(DialogManager.dialog1ResultOne));
                        if (RimworldManager.CheckIfHasEnoughSilverInMap(settlementMap, requiredSilver))
                        {
                            actionClick?.Invoke();
                            Close();
                        }
                        else DialogManager.PushNewDialog(new RT_Dialog_Error("RTNotEnoughSilver".Translate()));
                    }
                };
                DialogManager.PushNewDialog(new RT_Dialog_1Input("RTRequestQuantity".Translate(), "RTRequestQuantityDesc".Translate(), toDo, null));
            }
        }
    }
}
