using System;
using System.Linq;
using RimWorld;
using Shared;
using UnityEngine;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
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

        private readonly MarketType marketType;

        private readonly ItemData[] elements;

        public RT_Dialog_MarketListing(MarketType marketType, ItemData[] elements, Action actionClick = null, Action actionCancel = null)
        {
            DialogManager.dialogMarketListing = this;
            this.marketType = marketType;

            if (marketType == MarketType.Global)
            {
                title = "Global Market";
                description = "Trade with the rest of the world remotely";
            }

            else if (marketType == MarketType.Faction)
            {
                title = "Faction Market";
                description = "Trade with your faction members remotely";
            }

            this.elements = elements;
            this.actionClick = actionClick;
            this.actionCancel = actionCancel;

            ClientValues.ToggleTransfer(true);

            forcePause = true;
            absorbInputAroundWindow = true;
            soundAppear = SoundDefOf.CommsWindow_Open;
            //soundClose = SoundDefOf.CommsWindow_Close;
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

            FillMainRect(new Rect(0f, descriptionLineDif2 + 10f, rect.width, rect.height - buttonY - 85f));

            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMin, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "New"))
            {
                ClientValues.chosenSettlement = Find.WorldObjects.Settlements.First(fetch => fetch.Faction == Faction.OfPlayer);
                ClientValues.chosenCaravan = Find.WorldObjects.Caravans.First(fetch => fetch.Faction == Faction.OfPlayer);
                RT_Dialog_TransferMenu d1 = new RT_Dialog_TransferMenu(TransferLocation.World, true, true, true);
                DialogManager.PushNewDialog(d1);
            }

            if (Widgets.ButtonText(new Rect(new Vector2(centeredX - buttonX / 2, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "Reload"))
            {
                MarketData marketData = new MarketData();
                marketData.marketStepMode = MarketStepMode.Reload;
                marketData.marketType = marketType;

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.MarketPacket), marketData);
                Network.listener.EnqueuePacket(packet);
            }

            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMax - buttonX, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "Close"))
            {
                ClientValues.ToggleTransfer(false);
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

                    Thing thing = ThingScribeManager.StringToItem(elements[i]);
                    DrawCustomRow(rect, thing, num4);
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

            Widgets.Label(fixedRect, $"{toDisplay.Label} > ${toDisplay.MarketValue}");
            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMax - selectButtonX, rect.yMax - selectButtonY), new Vector2(selectButtonX, selectButtonY)), "Select"))
            {
                DialogManager.dialogMarketListingResult = index;
                actionClick?.Invoke();
                Close();
            }
        }
    }
}
