using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public class RT_Dialog_TransferMenu : Window
    {
        //UI

        public override Vector2 InitialSize => new Vector2(600f, 512f);

        private Vector2 scrollPosition = Vector2.zero;

        public readonly string title = "Transfer Menu";

        public readonly string description = "Select the items you wish to transfer";

        private readonly float buttonX = 100f;

        private readonly float buttonY = 37f;

        private readonly int startAcceptingInputAtFrame;

        private bool AcceptsInput => startAcceptingInputAtFrame <= Time.frameCount;

        //Variables

        private readonly TransferLocation transferLocation;

        private List<Tradeable> cachedTradeables;

        private Pawn playerNegotiator;

        private readonly bool allowItems;

        private readonly bool allowAnimals;

        private readonly bool allowHumans;

        private readonly bool allowFreeThings;

        public RT_Dialog_TransferMenu(TransferLocation transferLocation, bool allowItems = false, bool allowAnimals = false, bool allowHumans = false, bool allowFreeThings = true)
        {
            DialogManager.dialogTransferMenu = this;
            this.transferLocation = transferLocation;
            this.allowItems = allowItems;
            this.allowAnimals = allowAnimals;
            this.allowHumans = allowHumans;
            this.allowFreeThings = allowFreeThings;

            ClientValues.ToggleTransfer(true);

            forcePause = true;
            absorbInputAroundWindow = true;
            soundAppear = SoundDefOf.CommsWindow_Open;
            
            closeOnAccept = false;
            closeOnCancel = false;

            PrepareWindow();
        }

        private void PrepareWindow()
        {
            GetNegotiator();

            GenerateTradeList();

            LoadAllAvailableTradeables();

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

            if (Widgets.ButtonText(new Rect(new Vector2(rect.x, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "Accept")) OnAccept();
            if (Widgets.ButtonText(new Rect(new Vector2((rect.width / 2) - (buttonX / 2), rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "Reset")) OnReset();
            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMax - buttonX, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "Cancel")) OnCancel();
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
            if (transferLocation == TransferLocation.Caravan)
            {
                Action r1 = delegate
                {
                    SessionValues.outgoingManifest._transferMode = TransferMode.Gift;
                    postChoosing();
                };

                Action r2 = delegate
                {
                    SessionValues.outgoingManifest._transferMode = TransferMode.Trade;
                    postChoosing();
                };

                RT_Dialog_2Button d2 = new RT_Dialog_2Button("Transfer Type", "Please choose the transfer type to use",
                    "Gift", "Trade", r1, r2, null);

                RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("Are you sure you want to continue with the transfer?",
                    delegate { DialogManager.PushNewDialog(d2); }, null);

                DialogManager.PushNewDialog(d1);
            }

            else if (transferLocation == TransferLocation.Settlement)
            {
                Action r1 = delegate
                {
                    SessionValues.outgoingManifest._transferMode = TransferMode.Rebound;
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
                if (transferLocation == TransferLocation.Settlement)
                {
                    TransferManager.RejectRequest(TransferMode.Trade);
                }

                TransferManager.FinishTransfer(false);

                Close();
            };

            if (transferLocation == TransferLocation.Settlement)
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
            if (transferLocation == TransferLocation.Caravan)
            {
                playerNegotiator = SessionValues.chosenCaravan.PawnsListForReading.Find(fetch => fetch.IsColonist && !fetch.skills.skills[10].PermanentlyDisabled);
            }

            else if (transferLocation == TransferLocation.Settlement)
            {
                playerNegotiator = Find.AnyPlayerHomeMap.mapPawns.AllPawns.Find(fetch => fetch.IsColonist && !fetch.skills.skills[10].PermanentlyDisabled);
            }
        }

        private void SetupTrade()
        {
            if (transferLocation == TransferLocation.Caravan)
            {
                TradeSession.SetupWith(SessionValues.chosenSettlement, playerNegotiator, true);
            }

            else if (transferLocation == TransferLocation.Settlement)
            {
                TradeSession.SetupWith(Find.WorldObjects.SettlementAt(SessionValues.incomingManifest._fromTile), 
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
            SessionValues.listToShowInTradesMenu = new List<Tradeable>();

            if (transferLocation == TransferLocation.Caravan)
            {
                List<Thing> caravanItems = CaravanInventoryUtility.AllInventoryItems(SessionValues.chosenCaravan);

                if (allowItems)
                {
                    foreach (Thing thing in caravanItems)
                    {
                        if (thing.MarketValue == 0 && !allowFreeThings) continue;
                        else
                        {
                            Tradeable tradeable = new Tradeable();
                            tradeable.AddThing(thing, Transactor.Colony);
                            SessionValues.listToShowInTradesMenu.Add(tradeable);
                        }
                    }
                }

                if (allowHumans || allowAnimals)
                {
                    foreach (Pawn pawn in SessionValues.chosenCaravan.pawns)
                    {
                        if (ScriberHelper.CheckIfThingIsHuman(pawn))
                        {
                            if (allowHumans)
                            {
                                if (pawn == playerNegotiator) continue;
                                else
                                {
                                    Tradeable tradeable = new Tradeable();
                                    tradeable.AddThing(pawn, Transactor.Colony);
                                    SessionValues.listToShowInTradesMenu.Add(tradeable);
                                }
                            }
                        }

                        else if (ScriberHelper.CheckIfThingIsAnimal(pawn))
                        {
                            if (allowAnimals)
                            {
                                Tradeable tradeable = new Tradeable();
                                tradeable.AddThing(pawn, Transactor.Colony);
                                SessionValues.listToShowInTradesMenu.Add(tradeable);
                            }
                        }
                    }
                }
            }

            else if (transferLocation == TransferLocation.Settlement)
            {
                Map map = Find.Maps.Find(x => x.Tile == SessionValues.incomingManifest._toTile);

                List<Pawn> pawnsInMap = map.mapPawns.PawnsInFaction(Faction.OfPlayer).ToList();
                pawnsInMap.AddRange(map.mapPawns.PrisonersOfColony);

                Thing[] thingsInMap = RimworldManager.GetAllThingsInMap(map);

                if (allowItems)
                {
                    foreach(Thing thing in thingsInMap)
                    {
                        if (thing.MarketValue == 0 && !allowFreeThings) continue;
                        {
                            Tradeable tradeable = new Tradeable();
                            tradeable.AddThing(thing, Transactor.Colony);
                            SessionValues.listToShowInTradesMenu.Add(tradeable);
                        }
                    }
                }

                if (allowHumans || allowAnimals)
                {
                    foreach (Pawn pawn in pawnsInMap)
                    {
                        if (ScriberHelper.CheckIfThingIsAnimal(pawn))
                        {
                            if (allowAnimals)
                            {
                                Tradeable tradeable = new Tradeable();
                                tradeable.AddThing(pawn, Transactor.Colony);
                                SessionValues.listToShowInTradesMenu.Add(tradeable);
                            }
                        }

                        else
                        {
                            if (allowHumans)
                            {
                                if (pawn == playerNegotiator) continue;
                                else
                                {
                                    Tradeable tradeable = new Tradeable();
                                    tradeable.AddThing(pawn, Transactor.Colony);
                                    SessionValues.listToShowInTradesMenu.Add(tradeable);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void LoadAllAvailableTradeables()
        {
            cachedTradeables = (from tr in SessionValues.listToShowInTradesMenu 
                orderby 0 descending select tr)
                .ThenBy((Tradeable tr) => tr.ThingDef.label)
                .ThenBy((Tradeable tr) => tr.AnyThing.TryGetQuality(out QualityCategory qc) ? ((int)qc) : (-1))
                .ThenBy((Tradeable tr) => tr.AnyThing.HitPoints)
                .ToList();
        }
    }
}
