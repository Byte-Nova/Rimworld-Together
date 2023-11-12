using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.GameClient.Values;
using Shared.Misc;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimworldTogether.GameClient.Dialogs
{
    public class RT_Dialog_TransferMenu : Window
    {
        public override Vector2 InitialSize => new Vector2(600f, 512f);

        public string title = "Transfer Menu";

        public string description = "Select the items you wish to transfer";

        private int startAcceptingInputAtFrame;

        private bool AcceptsInput => startAcceptingInputAtFrame <= Time.frameCount;

        private float buttonX = 100f;
        private float buttonY = 37f;

        private List<Tradeable> cachedTradeables;

        private Vector2 scrollPosition = Vector2.zero;

        private QuickSearchWidget quickSearchWidget = new QuickSearchWidget();

        private bool allowItems;

        private bool allowAnimals;

        private bool allowHumans;

        CommonEnumerators.TransferLocation transferLocation;

        private Pawn playerNegotiator;

        public override QuickSearchWidget CommonSearchWidget => quickSearchWidget;

        public RT_Dialog_TransferMenu(CommonEnumerators.TransferLocation transferLocation, bool allowItems = false, bool allowAnimals = false, 
            bool allowHumans = false)
        {
            DialogManager.dialogTransferMenu = this;
            this.transferLocation = transferLocation;
            this.allowItems = allowItems;
            this.allowAnimals = allowAnimals;
            this.allowHumans = allowHumans;

            ClientValues.ToggleTransfer(true);

            forcePause = true;
            absorbInputAroundWindow = true;

            soundAppear = SoundDefOf.CommsWindow_Open;
            //soundClose = SoundDefOf.CommsWindow_Close;

            closeOnAccept = false;
            closeOnCancel = false;

            PrepareWindow();
        }

        private void PrepareWindow()
        {
            GetNegotiator();

            GenerateTradeList();

            LoadAllAvailableTradeables();

            SetupSearchWidget();

            SetupTrade();
        }

        public override void DoWindowContents(Rect rect)
        {
            float windowDescriptionDif = Text.CalcSize(description).y + 8;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect((rect.width / 2) - Text.CalcSize(title).x / 2, rect.y, rect.width, Text.CalcSize(title).y), title);

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect((rect.width / 2) - Text.CalcSize(description).x / 2, windowDescriptionDif, rect.width, Text.CalcSize(description).y), description);
            Text.Font = GameFont.Medium;

            FillMainRect(new Rect(0f, 55f, rect.width, rect.height - buttonY - 65));

            if (Widgets.ButtonText(new Rect(new Vector2(rect.x, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "Accept"))
            {
                OnAccept();
            }

            if (Widgets.ButtonText(new Rect(new Vector2((rect.width / 2) - (buttonX / 2), rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "Reset"))
            {
                OnReset();
            }

            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMax - buttonX, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "Cancel"))
            {
                OnCancel();
            }
        }

        private void FillMainRect(Rect mainRect)
        {
            Widgets.DrawLineHorizontal(mainRect.x, mainRect.y - 1, mainRect.width);
            Widgets.DrawLineHorizontal(mainRect.x, mainRect.yMax + 1, mainRect.width);

            float height = 6f + (float)cachedTradeables.Count * 30f;
            Rect viewRect = new Rect(0f, 0f, mainRect.width - 16f, height);
            Widgets.BeginScrollView(mainRect, ref scrollPosition, viewRect);
            float num = 0;
            float num2 = scrollPosition.y - 30f;
            float num3 = scrollPosition.y + mainRect.height;
            int num4 = 0;

            for (int i = 0; i < cachedTradeables.Count; i++)
            {
                if (num > num2 && num < num3)
                {
                    Rect rect = new Rect(0f, num, viewRect.width, 30f);
                    DrawCustomRow(rect, cachedTradeables[i], num4);
                }

                num += 30f;
                num4++;
            }

            Widgets.EndScrollView();
        }

        private void OnAccept()
        {
            if (transferLocation == CommonEnumerators.TransferLocation.Caravan)
            {
                Action r1 = delegate
                {
                    ClientValues.outgoingManifest.transferMode = ((int)CommonEnumerators.TransferMode.Gift).ToString();
                    postChoosing();
                };

                Action r2 = delegate
                {
                    ClientValues.outgoingManifest.transferMode = ((int)CommonEnumerators.TransferMode.Trade).ToString();
                    postChoosing();
                };

                RT_Dialog_2Button d2 = new RT_Dialog_2Button("Transfer Type", "Please choose the transfer type to use",
                    "Gift", "Trade", r1, r2, null);

                RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("Are you sure you want to continue with the transfer?",
                    delegate { DialogManager.PushNewDialog(d2); }, null);

                DialogManager.PushNewDialog(d1);
            }

            else if (transferLocation == CommonEnumerators.TransferLocation.Settlement)
            {
                Action r1 = delegate
                {
                    ClientValues.outgoingManifest.transferMode = ((int)CommonEnumerators.TransferMode.Rebound).ToString();
                    DialogManager.PopDialog(DialogManager.dialogItemListing);
                    postChoosing();
                };

                RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("Are you sure you want to continue with the transfer?",
                    r1, null);

                DialogManager.PushNewDialog(d1);
            }

            void postChoosing()
            {
                TransferManager.TakeTransferItems(transferLocation);
                TransferManager.SendTransferRequestToServer(transferLocation);
                Close();
            }
        }

        private void OnCancel()
        {
            Action r1 = delegate
            {
                if (transferLocation == CommonEnumerators.TransferLocation.Settlement)
                {
                    TransferManager.RejectRequest(CommonEnumerators.TransferMode.Trade);
                }

                TransferManager.FinishTransfer(false);

                Close();
            };

            if (transferLocation == CommonEnumerators.TransferLocation.Settlement)
            {
                DialogManager.PushNewDialog(new RT_Dialog_YesNo("Are you sure you want to decline?",
                    r1, null));
            }
            else r1.Invoke();
        }

        private void OnReset()
        {
            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
            GenerateTradeList();
            LoadAllAvailableTradeables();
            TradeSession.deal.Reset();
        }

        private void GetNegotiator()
        {
            if (transferLocation == CommonEnumerators.TransferLocation.Caravan)
            {
                playerNegotiator = ClientValues.chosenCaravan.PawnsListForReading.Find(fetch => fetch.IsColonist && !fetch.skills.skills[10].PermanentlyDisabled);
            }

            else if (transferLocation == CommonEnumerators.TransferLocation.Settlement)
            {
                playerNegotiator = Find.AnyPlayerHomeMap.mapPawns.AllPawns.Find(fetch => fetch.IsColonist && !fetch.skills.skills[10].PermanentlyDisabled);
            }
        }

        private void SetupTrade()
        {
            if (transferLocation == CommonEnumerators.TransferLocation.Caravan)
            {
                TradeSession.SetupWith(ClientValues.chosenSettlement, playerNegotiator, true);
            }

            else if (transferLocation == CommonEnumerators.TransferLocation.Settlement)
            {
                TradeSession.SetupWith(Find.WorldObjects.SettlementAt(int.Parse(ClientValues.incomingManifest.fromTile)), 
                    playerNegotiator, true);
            }
        }

        private void DrawCustomRow(Rect rect, Tradeable trad, int index)
        {
            Text.Font = GameFont.Small;
            float width = rect.width;

            Widgets.DrawLightHighlight(rect);

            GUI.BeginGroup(rect);

            Rect rect5 = new Rect(width - 225, 0f, 240f, rect.height);
            bool flash = Time.time - Dialog_Trade.lastCurrencyFlashTime < 1f && trad.IsCurrency;
            TransferableUIUtility.DoCountAdjustInterface(rect5, trad, index, trad.GetMinimumToTransfer(), trad.GetMaximumToTransfer(), flash);

            width -= 225;

            int num2 = trad.CountHeldBy(Transactor.Colony);
            if (num2 != 0)
            {
                Rect rect6 = new Rect(width, 0f, 100f, rect.height);
                Rect rect7 = new Rect(rect6.x - 75f, 0f, 75f, rect.height);
                if (Mouse.IsOver(rect7)) Widgets.DrawHighlight(rect7);

                Rect rect8 = rect7;
                rect8.xMin += 5f;
                rect8.xMax -= 5f;
                Widgets.Label(rect8, num2.ToStringCached());
                TooltipHandler.TipRegionByKey(rect7, "ColonyCount");
            }

            width -= 90f;

            TransferableUIUtility.DoExtraIcons(trad, rect, ref width);

            Rect idRect = new Rect(0f, 0f, width, rect.height);
            TransferableUIUtility.DrawTransferableInfo(trad, idRect, Color.white);
            GenUI.ResetLabelAlign();
            GUI.EndGroup();
        }

        public void GenerateTradeList()
        {
            ClientValues.listToShowInTradesMenu = new List<Tradeable>();

            if (transferLocation == CommonEnumerators.TransferLocation.Caravan)
            {
                if (allowItems)
                {
                    List<Thing> caravanItems = CaravanInventoryUtility.AllInventoryItems(ClientValues.chosenCaravan);
                    foreach (Thing item in caravanItems)
                    {
                        Tradeable tradeable = new Tradeable();
                        tradeable.AddThing(item, Transactor.Colony);
                        ClientValues.listToShowInTradesMenu.Add(tradeable);
                    }
                }

                foreach (Pawn pawn in ClientValues.chosenCaravan.pawns)
                {
                    if (TransferManagerHelper.CheckIfThingIsHuman(pawn))
                    {
                        if (allowHumans)
                        {
                            if (pawn == playerNegotiator) continue;
                            else
                            {
                                Tradeable tradeable = new Tradeable();
                                tradeable.AddThing(pawn, Transactor.Colony);
                                ClientValues.listToShowInTradesMenu.Add(tradeable);
                            }
                        }
                    }

                    else if (TransferManagerHelper.CheckIfThingIsAnimal(pawn))
                    {
                        if (allowAnimals)
                        {
                            Tradeable tradeable = new Tradeable();
                            tradeable.AddThing(pawn, Transactor.Colony);
                            ClientValues.listToShowInTradesMenu.Add(tradeable);
                        }
                    }
                }
            }

            else if (transferLocation == CommonEnumerators.TransferLocation.Settlement)
            {
                Map map = Find.Maps.Find(x => x.Tile == int.Parse(ClientValues.incomingManifest.toTile));

                if (allowItems)
                {
                    Zone[] zones = map.zoneManager.AllZones.ToArray();
                    foreach (Zone zone in zones)
                    {
                        Thing[] items = zone.AllContainedThings.ToArray();
                        foreach (Thing item in items)
                        {
                            if (item.def.alwaysHaulable)
                            {
                                Tradeable tradeable = new Tradeable();
                                tradeable.AddThing(item, Transactor.Colony);
                                ClientValues.listToShowInTradesMenu.Add(tradeable);
                            }
                        }
                    }
                }

                Pawn[] pawnsInMap = map.mapPawns.PawnsInFaction(Faction.OfPlayer).ToArray();
                foreach (Pawn pawn in pawnsInMap)
                {
                    if (TransferManagerHelper.CheckIfThingIsHuman(pawn))
                    {
                        if (allowHumans)
                        {
                            if (pawn == playerNegotiator) continue;
                            else
                            {
                                Tradeable tradeable = new Tradeable();
                                tradeable.AddThing(pawn, Transactor.Colony);
                                ClientValues.listToShowInTradesMenu.Add(tradeable);
                            }
                        }
                    }

                    else if (TransferManagerHelper.CheckIfThingIsAnimal(pawn))
                    {
                        if (allowAnimals)
                        {
                            Tradeable tradeable = new Tradeable();
                            tradeable.AddThing(pawn, Transactor.Colony);
                            ClientValues.listToShowInTradesMenu.Add(tradeable);
                        }
                    }
                }
            }
        }

        public void LoadAllAvailableTradeables()
        {
            cachedTradeables = (from tr in ClientValues.listToShowInTradesMenu
                                where quickSearchWidget.filter.Matches(tr.Label)
                                orderby 0 descending
                                select tr)
                                .ThenBy((Tradeable tr) => tr.ThingDef.label)
                                .ThenBy((Tradeable tr) => tr.AnyThing.TryGetQuality(out QualityCategory qc) ? ((int)qc) : (-1))
                                .ThenBy((Tradeable tr) => tr.AnyThing.HitPoints)
                                .ToList();
            quickSearchWidget.noResultsMatched = !cachedTradeables.Any();
        }

        private void SetupSearchWidget()
        {
            commonSearchWidgetOffset.x = InitialSize.x - 50;
            commonSearchWidgetOffset.y = InitialSize.y - 50;
        }
    }
}
