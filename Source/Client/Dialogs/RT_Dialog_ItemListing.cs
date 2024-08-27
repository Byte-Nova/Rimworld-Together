using System;
using System.Linq;
using RimWorld;
using Shared;
using UnityEngine;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public class RT_Dialog_ItemListing : Window
    {
        public override Vector2 InitialSize => new Vector2(350f, 512f);

        public string title = "RTDialogItemListing".Translate();

        private int startAcceptingInputAtFrame;

        private Vector2 scrollPosition = Vector2.zero;

        private bool AcceptsInput => startAcceptingInputAtFrame <= Time.frameCount;

        private float buttonX = 100f;

        private float buttonY = 37f;

        private Thing[] listedThings;

        private TransferMode transferMode;

        public RT_Dialog_ItemListing(Thing[] listedThings, TransferMode transferMode)
        {
            DialogManager.dialogItemListing = this;
            this.listedThings = listedThings;
            this.transferMode = transferMode;

            ClientValues.ToggleTransfer(true);

            forcePause = true;
            absorbInputAroundWindow = true;

            soundAppear = SoundDefOf.CommsWindow_Open;
            

            closeOnAccept = false;
            closeOnCancel = false;
        }

        public override void DoWindowContents(Rect rect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect((rect.width / 2) - Text.CalcSize(title).x / 2, rect.y, rect.width, Text.CalcSize(title).y), title);

            FillMainRect(new Rect(0f, 35f, rect.width, rect.height - buttonY - 45));

            if (Widgets.ButtonText(new Rect(new Vector2(rect.x, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "RTDialogAccept".Translate()))
            {
                OnAccept();
            }

            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMax - buttonX, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "RTDialogCancel".Translate()))
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

            if (DeepScribeHelper.CheckIfThingIsHuman(thing))
            {
                Widgets.Label(fixedRect, $"[H] {itemName}");
            }

            else if (DeepScribeHelper.CheckIfThingIsAnimal(thing))
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
                if (transferMode == TransferMode.Gift)
                {
                    TransferManager.GetTransferedItemsToSettlement(listedThings);
                }

                else if (transferMode == TransferMode.Trade)
                {
                    if (RimworldManager.CheckIfSocialPawnInMap(Find.AnyPlayerHomeMap))
                    {
                        DialogManager.PushNewDialog(new RT_Dialog_TransferMenu(TransferLocation.Settlement, true, true, true));
                    }

                    else
                    {
                        DialogManager.PushNewDialog(new RT_Dialog_Error("RTTransferError".Translate()));
                        TransferManager.RejectRequest(transferMode);
                    }
                }

                else if (transferMode == TransferMode.Pod)
                {
                    TransferManager.GetTransferedItemsToSettlement(listedThings);
                }

                else if (transferMode == TransferMode.Rebound)
                {
                    SessionValues.incomingManifest.stepMode = TransferStepMode.TradeReAccept;

                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.TransferPacket), SessionValues.incomingManifest);
                    Network.listener.EnqueuePacket(packet);

                    TransferManager.GetTransferedItemsToCaravan(listedThings);
                }

                Close();
            };

            DialogManager.PushNewDialog(new RT_Dialog_YesNo("RTTransferAcceptSure".Translate(),
                r1, null));
        }

        private void OnReject()
        {
            Action r1 = delegate
            {
                TransferManager.RejectRequest(transferMode);

                Close();
            };

            DialogManager.PushNewDialog(new RT_Dialog_YesNo("RTTransferCancelSure".Translate(),
                r1, null));
        }
    }
}
