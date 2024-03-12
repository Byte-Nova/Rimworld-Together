﻿using System;
using System.Linq;
using RimWorld;
using Shared;
using UnityEngine;
using Verse;

namespace GameClient
{
    public class RT_Dialog_ItemListing : Window
    {
        public override Vector2 InitialSize => new Vector2(350f, 512f);

        public string title = "Item Listing";

        private int startAcceptingInputAtFrame;

        private Vector2 scrollPosition = Vector2.zero;

        private bool AcceptsInput => startAcceptingInputAtFrame <= Time.frameCount;

        private float buttonX = 100f;

        private float buttonY = 37f;

        private Thing[] listedThings;

        private CommonEnumerators.TransferMode transferMode;

        public RT_Dialog_ItemListing(Thing[] listedThings, CommonEnumerators.TransferMode transferMode)
        {
            DialogManager.dialogItemListing = this;
            this.listedThings = listedThings;
            this.transferMode = transferMode;

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
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect((rect.width / 2) - Text.CalcSize(title).x / 2, rect.y, rect.width, Text.CalcSize(title).y), title);

            FillMainRect(new Rect(0f, 35f, rect.width, rect.height - buttonY - 45));

            if (Widgets.ButtonText(new Rect(new Vector2(rect.x, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "RimworldTogether.Accept".Translate()))
            {
                OnAccept();
            }

            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMax - buttonX, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "RimworldTogether.Cancel".Translate()))
            {
                OnReject();
            }
        }

        private void FillMainRect(Rect mainRect)
        {
            Widgets.DrawLineHorizontal(mainRect.x, mainRect.y - 1, mainRect.width);
            Widgets.DrawLineHorizontal(mainRect.x, mainRect.yMax + 1, mainRect.width);

            float height = 6f + (float)listedThings.Count() * 30f;
            Rect viewRect = new Rect(0f, 0f, mainRect.width - 16f, height);
            Widgets.BeginScrollView(mainRect, ref scrollPosition, viewRect);
            float num = 0;
            float num2 = scrollPosition.y - 30f;
            float num3 = scrollPosition.y + mainRect.height;
            int num4 = 0;

            for (int i = 0; i < listedThings.Count(); i++)
            {
                if (num > num2 && num < num3)
                {
                    Rect rect = new Rect(0f, num, viewRect.width, 30f);
                    DrawCustomRow(rect, listedThings[i], num4);
                }

                num += 30f;
                num4++;
            }

            Widgets.EndScrollView();
        }

        private void DrawCustomRow(Rect rect, Thing thing, int index)
        {
            Text.Font = GameFont.Small;
            Rect fixedRect = new Rect(new Vector2(rect.x, rect.y + 5f), new Vector2(rect.width - 16f, rect.height - 5f));
            if (index % 2 == 0) Widgets.DrawHighlight(fixedRect);

            string itemName = thing.LabelShort;
            if (itemName.Length > 1) itemName = char.ToUpper(itemName[0]) + itemName.Substring(1);
            else itemName = itemName.ToUpper();

            if (TransferManagerHelper.CheckIfThingIsHuman(thing))
            {
                Widgets.Label(fixedRect, $"[H] {itemName}");
            }

            else if (TransferManagerHelper.CheckIfThingIsAnimal(thing))
            {
                Widgets.Label(fixedRect, $"[A] {itemName}");
            }

            else
            {
                Widgets.Label(fixedRect, $"[I] {itemName} (x{thing.stackCount}) ({thing.HitPoints} HP)");
            }
        }

        private void OnAccept()
        {
            Action r1 = delegate
            {
                if (transferMode == CommonEnumerators.TransferMode.Gift)
                {
                    TransferManager.GetTransferedItemsToSettlement(listedThings);
                }

                else if (transferMode == CommonEnumerators.TransferMode.Trade)
                {
                    if (RimworldManager.CheckForAnySocialPawn(CommonEnumerators.SearchLocation.Settlement))
                    {
                        DialogManager.PushNewDialog(new RT_Dialog_TransferMenu(CommonEnumerators.TransferLocation.Settlement, true, true, true));
                    }

                    else
                    {
                        DialogManager.PushNewDialog(new RT_Dialog_Error("RimworldTogether.ImpossibleTrade".Translate()));
                        TransferManager.RejectRequest(transferMode);
                    }
                }

                else if (transferMode == CommonEnumerators.TransferMode.Pod)
                {
                    TransferManager.GetTransferedItemsToSettlement(listedThings);
                }

                else if (transferMode == CommonEnumerators.TransferMode.Rebound)
                {
                    ClientValues.incomingManifest.transferStepMode = ((int)CommonEnumerators.TransferStepMode.TradeReAccept).ToString();

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), ClientValues.incomingManifest);
                    Network.listener.EnqueuePacket(packet);

                    TransferManager.GetTransferedItemsToCaravan(listedThings);
                }

                Close();
            };

            DialogManager.PushNewDialog(new RT_Dialog_YesNo("RimworldTogether.AcceptConfirmed".Translate(),
                r1, null));
        }

        private void OnReject()
        {
            Action r1 = delegate
            {
                TransferManager.RejectRequest(transferMode);

                Close();
            };

            DialogManager.PushNewDialog(new RT_Dialog_YesNo("RimworldTogether.AcceptDeclined".Translate(),
                r1, null));
        }
    }
}
