//
//  Thanks to .tommi. for letting us use his custom trade window from the Phinix mod
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using RimWorld;
using UnityEngine;
using Verse;
/*
namespace GameClient
{
    public class RT_Trading_Window : Window
    {

        Thing testItem = new Thing();
        private const float SCROLLBAR_WIDTH = 16f;

        private const float DEFAULT_SPACING = 10f;

        private const float OFFER_WINDOW_WIDTH = 400f;
        private const float OFFER_WINDOW_TITLE_HEIGHT = 20f;
        private const float OFFER_WINDOW_ROW_HEIGHT = 30f;
        private const float OFFER_WINDOW_CHECKBOX_HEIGHT = 25f;

        private const float SORT_HEIGHT = 30f;

        private const float SEARCH_TEXT_FIELD_WIDTH = 135f;

        private const float TRADE_BUTTON_HEIGHT = 30f;

        private const float TITLE_HEIGHT = 30f;

        public override Vector2 InitialSize => new Vector2(1000f, 750f);

        private readonly Regex itemCountInputRegex = new Regex("\\d*");
        private readonly Texture2D tradeArrows = ContentFinder<Texture2D>.Get("tradeArrows");

        private Vector2 ourOfferScrollPos = Vector2.zero;
        private Vector2 theirOfferScrollPos = Vector2.zero;
        private Vector2 availableItemsScrollPos = Vector2.zero;

        bool tradeUpdated;

        public RT_Trading_Window()
        {
            testItem.def
            this.doCloseX = true;
            this.closeOnAccept = false;
            this.closeOnCancel = false;
            this.closeOnClickedOutside = false;
            this.forcePause = true;
            this.draggable = true;
        }

        public override void PreOpen()
        {
            base.PreOpen();


            // Select things from all maps that are player homes
            IEnumerable<Map> homeMaps = Find.Maps.Where(map => map.IsPlayerHome);
            IEnumerable<Thing> things;
            if (Client.Instance.AllItemsTradable)
            {
                // Get *everything*
                things = homeMaps.SelectMany(map => map.listerThings.AllThings);
            }
            else
            {
                // From each map, select all haul destinations, then everything stored there
                IEnumerable<SlotGroup> haulDestinations = homeMaps.SelectMany(map => map.haulDestinationManager.AllGroups);
                things = haulDestinations.SelectMany(haulDestination => haulDestination.HeldThings);
            }

            // Group all items and cache them for later
            availableItems = StackedThings.GroupThings(things.Where(thing => thing.def.category == ThingCategory.Item && !thing.def.IsCorpse));
            filteredAvailableItems = availableItems;

            // Pre-fill offer caches as well
            ourOfferCache = StackedThings.GroupThings(trade.ItemsOnOffer.Select(TradingThingConverter.ConvertThingFromProtoOrUnknown));
            theirOfferCache = StackedThings.GroupThings(trade.OtherPartyItemsOnOffer.Select(TradingThingConverter.ConvertThingFromProtoOrUnknown));
        }

        public override void Close(bool doCloseSound = true)
        {
            base.Close(doCloseSound);

            // Unsubscribe from events
            Client.Instance.OnTradeCompleted -= OnTradeFinished;
            Client.Instance.OnTradeCancelled -= OnTradeFinished;
        }

        public override void DoWindowContents(Rect inRect)
        {
            // Update trade if requested
            if (tradeUpdated)
            {
                
                // Copy the new trade details into place

                // Refresh trade caches
                    
                // Reset the update flag

            }

            // Build layout rects
            Rect titleRect = inRect.TopPartPixels(TITLE_HEIGHT);

            Rect offerHalfRect = new Rect(inRect.xMin, titleRect.yMax + DEFAULT_SPACING, inRect.width, (inRect.height - titleRect.height - DEFAULT_SPACING) / 2 - DEFAULT_SPACING / 2);
            Rect ourOfferRect = offerHalfRect.LeftPartPixels(OFFER_WINDOW_WIDTH);
            Rect theirOfferRect = offerHalfRect.RightPartPixels(OFFER_WINDOW_WIDTH);

            Rect centreColumnRect = new Rect(ourOfferRect.xMax + DEFAULT_SPACING, offerHalfRect.yMin, (theirOfferRect.xMin - DEFAULT_SPACING) - (ourOfferRect.xMax + DEFAULT_SPACING), offerHalfRect.height);
            Rect cancelButtonRect = centreColumnRect.BottomPartPixels(TRADE_BUTTON_HEIGHT);
            Rect resetButtonRect = new Rect(centreColumnRect.xMin, cancelButtonRect.yMin - (TRADE_BUTTON_HEIGHT + DEFAULT_SPACING * 2), centreColumnRect.width, TRADE_BUTTON_HEIGHT);
            Rect updateButtonRect = new Rect(centreColumnRect.xMin, resetButtonRect.yMin - (TRADE_BUTTON_HEIGHT + DEFAULT_SPACING), centreColumnRect.width, TRADE_BUTTON_HEIGHT);
            Rect tradeArrowsRect = centreColumnRect.TopPartPixels(centreColumnRect.height - (cancelButtonRect.yMax - updateButtonRect.yMin) - DEFAULT_SPACING);

            Rect searchFieldRect = new Rect(inRect.xMax - SEARCH_TEXT_FIELD_WIDTH, offerHalfRect.yMax + DEFAULT_SPACING, SEARCH_TEXT_FIELD_WIDTH, SORT_HEIGHT);
            Rect searchLabelRect = TranslatedBy(searchFieldRect ,- (SEARCH_TEXT_FIELD_WIDTH + DEFAULT_SPACING));
            Rect availableItemsRect = new Rect(inRect.xMin, searchFieldRect.yMax + DEFAULT_SPACING, inRect.width, inRect.yMax - searchFieldRect.yMax - DEFAULT_SPACING);

            // Save the current text settings
            GameFont previousFont = Text.Font;
            TextAnchor previousAnchor = Text.Anchor;

            // Title
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.LabelFit(titleRect, "Trade Tile");

            // Restore the text settings
            Text.Font = previousFont;
            Text.Anchor = previousAnchor;

            // Trade arrows
            Widgets.DrawTextureFitted(tradeArrowsRect, tradeArrows, 1f);

            // Our offer
            drawOffer(inRect: ourOfferRect,
                title: "Phinix_trade_ourOfferLabel".Translate(),
                itemStacks: ourOfferCache,
                scrollPos: ref ourOfferScrollPos,
                accepted: ref ourOfferAccepted,
                acceptedLabel: ("Phinix_trade_confirmOurTradeCheckbox" + (trade.Accepted ? "Checked" : "Unchecked")).Translate(),
                checkboxInteractive: true
            );

            //if ourOfferAccepted does not equal trade.Accepted
                // Update our accepted state

            // Their offer
            bool theirOfferAccepted = trade.OtherPartyAccepted;
            drawOffer(inRect: theirOfferRect,
                title: "Phinix_trade_theirOfferLabel".Translate(),
                itemStacks: theirOfferCache,
                scrollPos: ref theirOfferScrollPos,
                accepted: ref theirOfferAccepted,
                acceptedLabel: ("Phinix_trade_confirmTheirTradeCheckbox" + (trade.OtherPartyAccepted ? "Checked" : "Unchecked")).Translate(TextHelper.StripRichText(trade.OtherPartyDisplayName)),
                checkboxInteractive: false
            );

            // Update button
            if (Widgets.ButtonText(updateButtonRect, "Phinix_trade_updateButton".Translate()))
            {
                try
                {
                    // Create a new token
                    string token = Guid.NewGuid().ToString();
                    List <Thing> selectedThings = new List<Thing>();

                    // Collect all our things and despawn them all
                    foreach (StackedThings itemStack in availableItems)
                    {
                        // Pop the selected things from the stack
                        Thing[] things = itemStack.PopSelected().ToArray();

                        // Despawn each spawned thing
                        foreach (Thing thing in things)
                        {
                            if (thing.Spawned) thing.DeSpawn();
                        }

                        // Add them to the selected things list
                        selectedThings.AddRange(things);
                    }

                    //begin lock for pending items
                    lock (pendingItemStacksLock)
                    {
                        // Add the items to the pending dictionary
                        pendingItemStacks.Add(token, new PendingThings
                        {
                            Things = selectedThings.ToArray(),
                            Timestamp = DateTime.UtcNow
                        });
                    }
                    Log.Message("Added items to pending");


                    // Get the items we have on offer and splice in the selected items
                    

                    // Send an update to the server
                    
                    Log.Message("Sent update");
                }
                catch (Exception e)
                {
                    Log.Message(e.ToString());
                }
            }

            // Reset button
            if (Widgets.ButtonText(resetButtonRect, "Phinix_trade_resetButton".Translate()))
            {
                // Convert and drop our items in pods
                Client.Instance.DropPods(trade.ItemsOnOffer.Select(TradingThingConverter.ConvertThingFromProto));


                // Reset all selected counts to zero
                foreach (StackedThings stack in availableItems)
                {
                    stack.Selected = 0;
                }

                // Update trade items
                Client.Instance.UpdateTradeItems(trade.TradeId, Array.Empty<ProtoThing>());
            }

            // Save GUI colour
            Color previousColour = UnityEngine.GUI.color;

            // Cancel button
            UnityEngine.GUI.color = Color.red;
            if (Widgets.ButtonText(cancelButtonRect, "Phinix_trade_cancelButton".Translate()))
            {
                new Thread(() => Client.Instance.CancelTrade(trade.TradeId)).Start();
            }

            // Restore GUI colour
            UnityEngine.GUI.color = previousColour;

            // Search label
            TextAnchor textAnchor = Text.Anchor;
            GameFont fontSize = Text.Font;
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(searchLabelRect, "Phinix_trade_searchLabel".Translate());
            Text.Anchor = textAnchor;
            Text.Font = fontSize;

            // Search field
            string oldSearchText = searchText;
            searchText = Widgets.TextField(searchFieldRect, searchText);
            if (searchText != oldSearchText)
            {
                // Repopulate filtered item list with the new search if necessary
                filteredAvailableItems = availableItems.Where(stack => stack.Label.ToLower().Contains(searchText.ToLower())).ToList();
            }

            // Available items
            if (true//!filteredAvailableItems.Any())
            {
                // Draw a placeholder when nothing is present
                Widgets.DrawMenuSection(availableItemsRect);
                Widgets.NoneLabelCenteredVertically(availableItemsRect,"No items available");
            }
            else
            {
                drawItemStackList(availableItemsRect, filteredAvailableItems, ref availableItemsScrollPos, true);
            }
        }

        /// <summary>
        /// Event handler for the <see cref="Client.OnTradeCompleted"/> and <see cref="Client.OnTradeCancelled"/> events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnTradeFinished()
        {
            DialogManager.PopDialog();
        }

        /// <summary>
        /// Draws an offer containing a title, list of items on offer, and toggle-able checkbox with whether it's been accepted.
        /// </summary>
        /// <param name="inRect">Container to draw within</param>
        /// <param name="title">Title text</param>
        /// <param name="itemStacks">Item stacks on offer</param>
        /// <param name="scrollPos">Item list scroll position</param>
        /// <param name="accepted">Offer accepted state</param>
        /// <param name="acceptedLabel">Accepted state label</param>
        /// <param name="checkboxInteractive">Whether to draw the accepted state checkbox in a more distinctly interactive style</param>
        private void drawOffer(Rect inRect, string title, Int32 itemStacks, ref Vector2 scrollPos, ref bool accepted, string acceptedLabel, bool checkboxInteractive)
        {
            Rect titleRect = inRect.TopPartPixels(OFFER_WINDOW_TITLE_HEIGHT);
            Rect acceptedStateRect = new Rect(inRect.xMin, inRect.yMax - OFFER_WINDOW_CHECKBOX_HEIGHT - 2.5f, inRect.width, OFFER_WINDOW_CHECKBOX_HEIGHT);
            Rect acceptedStateLabelRect = acceptedStateRect.LeftPartPixels(acceptedStateRect.width - OFFER_WINDOW_CHECKBOX_HEIGHT);
            Rect checkboxRect = new Rect(acceptedStateRect.xMax - OFFER_WINDOW_CHECKBOX_HEIGHT, acceptedStateRect.yMin + ((acceptedStateRect.height - OFFER_WINDOW_CHECKBOX_HEIGHT) / 2), OFFER_WINDOW_CHECKBOX_HEIGHT, OFFER_WINDOW_CHECKBOX_HEIGHT);
            Rect itemListRect = new Rect(inRect.xMin, titleRect.yMax, inRect.width, acceptedStateRect.yMin - DEFAULT_SPACING - titleRect.yMax);

            // Save the current text settings
            TextAnchor textAnchor = Text.Anchor;
            GameFont fontSize = Text.Font;

            // Title
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.LabelFit(titleRect, title);

            // Restore the text settings
            Text.Anchor = textAnchor;
            Text.Font = fontSize;

            // Accepted state
            // Show a work tab-style checkbox texture if interactive
            if (checkboxInteractive)
            {
                textAnchor = Text.Anchor;
                fontSize = Text.Font;
                Text.Anchor = TextAnchor.MiddleLeft;

                Widgets.LabelFit(acceptedStateLabelRect, acceptedLabel);
                Widgets.DrawOptionBackground(checkboxRect, false);
                if (accepted) UnityEngine.GUI.DrawTexture(checkboxRect, WidgetsWork.WorkBoxCheckTex);
                if (Widgets.ButtonInvisible(acceptedStateRect, true)) accepted = !accepted;

                Text.Anchor = textAnchor;
                Text.Font = fontSize;
            }
            else
            {
                Widgets.CheckboxLabeled(acceptedStateRect, acceptedLabel, ref accepted);
            }

            // Items on offer
            drawItemStackList(itemListRect, itemStacks, ref scrollPos, false);
        }

        /// <summary>
        /// Draws a list of item stacks.
        /// </summary>
        /// <param name="inRect">Container to draw within</param>
        /// <param name="stacks">List of item stacks to draw</param>
        /// <param name="scrollPos">Scroll position</param>
        /// <param name="interactive">Whether to draw interactive buttons and quantity fields</param>
        private void drawItemStackList(Rect inRect, List<StackedThings> stacks, ref Vector2 scrollPos, bool interactive = false)
        {
            float ICON_WIDTH = 30f;
            float ROW_HEIGHT = ICON_WIDTH;
            float BUTTON_WIDTH = 40f;
            float QUANTITY_FIELD_WIDTH = 70f;
            float AVAILABLE_COUNT_WIDTH = 70f;
            float RIGHT_PADDING = 5f;

            TextAnchor textAnchor;
            GameFont fontSize;

            // Set up the content rect and start scrolling
            bool scrollbarsPresent = ROW_HEIGHT * stacks.Count > inRect.height;
            int nonEmptyStacks = stacks.Count(stack => stack.Count != 0);
            Rect contentRect = new Rect(inRect.xMin, inRect.yMin, scrollbarsPresent ? inRect.width - SCROLLBAR_WIDTH : inRect.width, ROW_HEIGHT * nonEmptyStacks);
            bool scrollRequired = contentRect.height > inRect.height;
            if (scrollRequired) Widgets.BeginScrollView(inRect, ref scrollPos, contentRect);

            bool alternateBackground = false;
            float currentY = contentRect.yMin;
            foreach (StackedThings stack in stacks)
            {
                // Don't deal with empty stacks
                if (stack.Things.Count == 0) continue;

                Rect rowRect = new Rect(contentRect.xMin, currentY, contentRect.width, ROW_HEIGHT);
                Rect iconRect = rowRect.LeftPartPixels(ICON_WIDTH);

                // Background
                if (alternateBackground) Widgets.DrawHighlight(rowRect);

                // Icon
                Widgets.ThingIcon(iconRect, stack.ThingDef, stack.StuffDef, stack.StyleDef, 0.9f);

                Rect itemNameRect;
                if (interactive)
                {
                    float buttonAreaWidth = ((BUTTON_WIDTH * 6) + QUANTITY_FIELD_WIDTH + AVAILABLE_COUNT_WIDTH + (DEFAULT_SPACING * 3));
                    Rect buttonAreaRect = new Rect(rowRect.xMax - (RIGHT_PADDING + buttonAreaWidth), rowRect.yMin, buttonAreaWidth, rowRect.height);
                    Rect quantityButton1Rect = new Rect(buttonAreaRect.xMin, buttonAreaRect.yMin, BUTTON_WIDTH, buttonAreaRect.height);
                    Rect quantityButton2Rect = quantityButton1Rect.TranslatedBy(BUTTON_WIDTH);
                    Rect quantityButton3Rect = quantityButton2Rect.TranslatedBy(BUTTON_WIDTH);
                    Rect quantityFieldRect = new Rect(quantityButton3Rect.xMax + DEFAULT_SPACING, buttonAreaRect.yMin, QUANTITY_FIELD_WIDTH, buttonAreaRect.height);
                    Rect availableCountRect = new Rect(quantityFieldRect.xMax + DEFAULT_SPACING, buttonAreaRect.yMin, AVAILABLE_COUNT_WIDTH, buttonAreaRect.height);
                    Rect quantityButton4Rect = new Rect(availableCountRect.xMax + DEFAULT_SPACING, buttonAreaRect.yMin, BUTTON_WIDTH, buttonAreaRect.height);
                    Rect quantityButton5Rect = quantityButton4Rect.TranslatedBy(BUTTON_WIDTH);
                    Rect quantityButton6Rect = quantityButton5Rect.TranslatedBy(BUTTON_WIDTH);

                    itemNameRect = new Rect(iconRect.xMax + DEFAULT_SPACING, rowRect.yMin, buttonAreaRect.xMin - iconRect.xMax - (DEFAULT_SPACING * 2), rowRect.height);

                    // -100 button
                    if (Widgets.ButtonText(quantityButton1Rect, "-100")) stack.Selected = Clamp(stack.Selected - 100, 0, stack.Count);

                    // -10 button
                    if (Widgets.ButtonText(quantityButton2Rect, "-10")) stack.Selected = Clamp(stack.Selected - 10, 0, stack.Count);

                    // -1 button
                    if (Widgets.ButtonText(quantityButton3Rect, "-1")) stack.Selected = Clamp(stack.Selected - 1, 0, stack.Count);

                    // +1 button
                    if (Widgets.ButtonText(quantityButton4Rect, "+1")) stack.Selected = Clamp(stack.Selected + 1, 0, stack.Count);

                    // +10 button
                    if (Widgets.ButtonText(quantityButton5Rect, "+10")) stack.Selected = Clamp(stack.Selected + 10, 0, stack.Count);

                    // +100 button
                    if (Widgets.ButtonText(quantityButton6Rect, "+100")) stack.Selected = Clamp(stack.Selected + 100, 0, stack.Count);

                    // Quantity text field
                    string buf = stack.Selected == 0 ? "" : stack.Selected.ToString();
                    buf = Widgets.TextField(quantityFieldRect, buf, 100, itemCountInputRegex);
                    stack.Selected = string.IsNullOrEmpty(buf) ? 0 : Clamp(int.Parse(buf), 0, stack.Count);

                    // Available count
                    TextAnchor textAnchor = Text.Anchor;
                    GameFont fontSize = Text.Font;
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(availableCountRect, $"/ {stack.Count}");
                    Text.Anchor = textAnchor;
                    Text.Font = fontSize;
                }
                else
                {
                    Rect itemCountRect = new Rect(rowRect.xMax - QUANTITY_FIELD_WIDTH - RIGHT_PADDING, rowRect.yMin, QUANTITY_FIELD_WIDTH, rowRect.height);
                    itemNameRect = new Rect(iconRect.xMax + DEFAULT_SPACING, rowRect.yMin, itemCountRect.xMin - iconRect.xMax - DEFAULT_SPACING, rowRect.height);

                    // Item count
                    textAnchor = Text.Anchor;
                    fontSize = Text.Font;
                    Text.Anchor = TextAnchor.MiddleRight;
                    Widgets.Label(itemCountRect, stack.Count.ToStringSI());
                    Text.Anchor = textAnchor;
                    Text.Font = fontSize;
                }

                // Item name
                textAnchor = Text.Anchor;
                fontSize = Text.Font;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.LabelFit(itemNameRect, "Label");
                Text.Anchor = textAnchor;
                Text.Font = fontSize;

                // Toggle alternate background colour
                alternateBackground = !alternateBackground;

                currentY += ROW_HEIGHT;
            }

            if (scrollRequired) Widgets.EndScrollView();
        }

        public static Rect TranslatedBy(Rect rect, float xOffset = 0f, float yOffset = 0f)
        {
            Vector2 newPosition = new Vector2(rect.x + xOffset, rect.y + yOffset);
            return new Rect(newPosition, rect.size);
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }

}
*/
