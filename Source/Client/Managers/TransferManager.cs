using GameClient.Core.Configs;
using GameClient.Dialogs;
using GameClient.Misc;
using GameClient.Values;
using TCPNetwork.Packets;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Shared.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Verse;
using Verse.Sound;
using static TCPNetwork.Packets.TransferData;
using static Shared.CommonEnumerators;

namespace GameClient.Managers
{
    //Class that handles all the thing transfers between clients in the mod

    public static class TransferManager
    {
        [HandlesPacket(PacketHeader.TransferManager)]
        private static void ParsePacket(byte[] bytes)
        {
            TransferData data = Serializer.ConvertBytesToObject<TransferData>(bytes);

            Printer.Warning(data, LogImportanceMode.Extreme);

            switch (data._stepMode)
            {
                case TransferStepMode.TradeRequest:
                    ReceiveTransferRequest(data);
                    break;

                case TransferStepMode.TradeAccept:
                    RT_Dialog_Wait.Instance.Close();
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("MESSAGE", new string[] { "Transfer was a success!" }));
                    if (data._transferMode == TransferMode.Pod) LaunchDropPods();
                    FinishTransfer(true);
                    break;

                case TransferStepMode.TradeReject:
                    RT_Dialog_Wait.Instance.Close();
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "Player rejected the trade!" }));
                    RecoverTradeItems(TransferLocation.Caravan);
                    break;

                case TransferStepMode.TradeReRequest:
                    RT_Dialog_Wait.Instance.Close();
                    ReceiveReboundRequest(data);
                    break;

                case TransferStepMode.TradeReAccept:
                    RT_Dialog_Wait.Instance.Close();
                    GetTransferedItemsToSettlement(TransferManagerHelper.GetAllTransferedItems(SessionValues.IncomingManifest));
                    break;

                case TransferStepMode.TradeReReject:
                    RT_Dialog_Wait.Instance.Close();
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "Player rejected the trade!" }));
                    RecoverTradeItems(TransferLocation.Settlement);
                    break;

                case TransferStepMode.Recover:
                    RT_Dialog_Wait.Instance.Close();
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "Player is not currently available!" }));
                    RecoverTradeItems(TransferLocation.Caravan);
                    break;
            }
        }

        //Takes transferable items from desired location

        public static void TakeTransferItems(TransferLocation transferLocation)
        {
            if (TradeSession.deal.TryExecute(out bool actuallyTraded))
            {
                SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();

                if (transferLocation == TransferLocation.Caravan)
                {
                    TradeSession.playerNegotiator.GetCaravan().RecacheInventory();
                }
            }
        }

        //Takes transferable items from drop pods

        public static void TakeTransferItemsFromPods(IEnumerable<IThingHolder> pods)
        {
            SessionValues.OutgoingManifest._transferMode = TransferMode.Pod;

            foreach (IThingHolder pod in pods)
            {
                try
                {
                    ThingOwner directlyHeldThings = pod.GetDirectlyHeldThings();

                    for (int i = 0; i < directlyHeldThings.Count(); i++)
                    {
                        TransferManagerHelper.AddThingToTransferManifest(directlyHeldThings[i], directlyHeldThings[i].stackCount);
                    }
                }
                catch { continue; }
            }
        }

        //Sends a transfer request to the server

        public static void SendTransferRequestToServer(TransferLocation transferLocation)
        {
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Wait("Waiting for transfer response"));

            if (transferLocation == TransferLocation.Caravan)
            {
                SessionValues.ChosenCaravan = TradeSession.playerNegotiator.GetCaravan();

                SessionValues.OutgoingManifest._stepMode = TransferStepMode.TradeRequest;
                SessionValues.OutgoingManifest._fromTile = Find.AnyPlayerHomeMap.Tile;
                SessionValues.OutgoingManifest._toTile = TradeSession.playerNegotiator.Tile;

                ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.TransferManager, SessionValues.OutgoingManifest);
            }

            else if (transferLocation == TransferLocation.Settlement)
            {
                RT_Dialog_ItemListing.Instance.Close();

                SessionValues.OutgoingManifest._stepMode = TransferStepMode.TradeReRequest;
                SessionValues.OutgoingManifest._fromTile = Find.AnyPlayerHomeMap.Tile;
                SessionValues.OutgoingManifest._toTile = SessionValues.IncomingManifest._fromTile;

                ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.TransferManager, SessionValues.OutgoingManifest);
            }

            else if (transferLocation == TransferLocation.Pod)
            {
                SessionValues.OutgoingManifest._stepMode = TransferStepMode.TradeRequest;
                SessionValues.OutgoingManifest._fromTile = Find.AnyPlayerHomeMap.Tile;
                SessionValues.OutgoingManifest._toTile = SessionValues.ChosenSettlement.Tile;

                ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.TransferManager, SessionValues.OutgoingManifest);
            }
        }

        //Recovers transfered items when trade fails

        public static void RecoverTradeItems(TransferLocation transferLocation)
        {
            try
            {
                Thing[] toRecover = TransferManagerHelper.GetAllTransferedItems(SessionValues.OutgoingManifest);

                if (transferLocation == TransferLocation.Caravan) GetTransferedItemsToCaravan(toRecover, false);
                else if (transferLocation == TransferLocation.Settlement) GetTransferedItemsToSettlement(toRecover, false);
            }

            catch
            {
                Printer.Warning("Rethrowing transfer items, might be RimWorld's fault");

                Thread.Sleep(100);

                RecoverTradeItems(transferLocation);
            }
        }

        //Receives the transfered items into the settlement

        public static void GetTransferedItemsToSettlement(Thing[] things, bool success = true, bool customMap = true, bool invokeMessage = true)
        {
            Action r1 = delegate
            {
                Map map = null;
                if (customMap) map = Find.Maps.Find(x => x.Tile == SessionValues.IncomingManifest._toTile);
                else map = Find.AnyPlayerHomeMap;

                foreach (Thing thing in things)
                {
                    if (thing.def.CanHaveFaction) thing.SetFactionDirect(Faction.OfPlayer);
                    RimworldManager.PlaceThingIntoMap(thing, map, ThingPlaceMode.Near, true, success);
                }

                FinishTransfer(success);
            };

            if (invokeMessage)
            {
                if (success) RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("MESSAGE", new string[] { "Transfer was a success!" }, r1));
                else RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "Transfer was cancelled!" }, r1));
            }
            else r1.Invoke();
        }

        //Receives the transfered items into the caravan

        public static void GetTransferedItemsToCaravan(Thing[] things, bool success = true, bool invokeMessage = true)
        {
            Action r1 = delegate
            {
                foreach (Thing thing in things) RimworldManager.PlaceThingIntoCaravan(thing, SessionValues.ChosenCaravan);

                FinishTransfer(success);
            };

            if (invokeMessage)
            {
                if (success) RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "Transfer was a success!" }, r1));
                else RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "Transfer was cancelled!" }, r1));
            }
            else r1.Invoke();
        }

        //Finishes the transfer order

        public static void FinishTransfer(bool success)
        {
            ClientValues.ToggleTradeStep(ClientValues.TradeMode.None);

            if (success) SaveManager.ForceSave();

            SessionValues.IncomingManifest = new TransferData();
            SessionValues.OutgoingManifest = new TransferData();

            ClientValues.ToggleTransfer(false);
        }

        //Executes when receiving a transfer request

        public static void ReceiveTransferRequest(TransferData transferData)
        {
            try
            {
                SessionValues.IncomingManifest = transferData;

                if (!ClientValues.IsReadyToPlay || ClientValues.IsInTransfer || ModConfigGetter.RejectTransfersBool)
                {
                    RejectRequest(transferData._transferMode, false);
                }

                else
                {
                    Action r1 = delegate
                    {
                        RT_Dialog_ItemListing d1 = new RT_Dialog_ItemListing(TransferManagerHelper.GetAllTransferedItems(transferData), 
                            transferData._transferMode);

                        RT_Dialog_Base.PushNewDialog(d1);
                    };

                    string description = string.Empty;
                    if (transferData._transferMode == TransferMode.Trade) description = "You are receiving a trade request";
                    else description = "You are receiving a gift request";

                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("MESSAGE", new string[] { description }, r1));
                }
            }

            catch
            {
                Printer.Warning("Rethrowing transfer items, might be RimWorld's fault");

                Thread.Sleep(100);

                ReceiveTransferRequest(transferData);
            }
        }

        //Executes after receiving a rebound transfer request

        public static void ReceiveReboundRequest(TransferData transferData)
        {
            try
            {
                SessionValues.IncomingManifest = transferData;

                RT_Dialog_ItemListing d1 = new RT_Dialog_ItemListing(TransferManagerHelper.GetAllTransferedItems(transferData), TransferMode.Rebound);
                RT_Dialog_Base.PushNewDialog(d1);
            }

            catch
            {
                Printer.Warning("Rethrowing transfer items, might be RimWorld's fault");

                Thread.Sleep(100);

                ReceiveReboundRequest(transferData);
            }
        }

        //Executes when rejecting a transfer request

        public static void RejectRequest(TransferMode transferMode, bool finishTransfer = true)
        {
            if (transferMode == TransferMode.Gift)
            {
                //Nothing should happen here
            }

            else if (transferMode == TransferMode.Trade)
            {
                SessionValues.IncomingManifest._stepMode = TransferStepMode.TradeReject;

                ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.TransferManager, SessionValues.IncomingManifest);
            }

            else if (transferMode == TransferMode.Pod)
            {
                //Nothing should happen here
            }

            else if (transferMode == TransferMode.Rebound)
            {
                SessionValues.IncomingManifest._stepMode = TransferStepMode.TradeReReject;

                ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.TransferManager, SessionValues.IncomingManifest);

                RecoverTradeItems(TransferLocation.Caravan);
            }

            if (finishTransfer) FinishTransfer(false);
        }

        //Launchs the drop pods with the desired transfer request

        public static void LaunchDropPods()
        {
            foreach (IThingHolder holder in SessionValues.ChosenPods.ToArray())
            {
                holder.GetDirectlyHeldThings().ClearAndDestroyContents();
            }
        }
    }

    //Helper class of the TransferManager class

    public static class TransferManagerHelper
    {
        //Adds desired thing into transfer manifest

        public static void AddThingToTransferManifest(Thing thing, int thingCount)
        {
            if (ScriberH.CheckIfThingIsHuman(thing))
            {
                Pawn pawn = thing as Pawn;

                SessionValues.OutgoingManifest._humans.Add(ScribeManager.HumanToString(pawn));

                RimworldManager.RemovePawnFromGame(pawn);
            }

            else if (ScriberH.CheckIfThingIsAnimal(thing))
            {
                Pawn pawn = thing as Pawn;

                SessionValues.OutgoingManifest._animals.Add(ScribeManager.SerializeToString(pawn, ScribeManager.SerializableType.Thing));

                RimworldManager.RemovePawnFromGame(pawn);
            }

            else SessionValues.OutgoingManifest._things.Add(ScribeManager.SerializeToString(thing, ScribeManager.SerializableType.Thing, thingCount));
        }

        //Gets the transfer location in the desired map

        public static IntVec3 GetTransferLocationInMap(Map map)
        {
            Thing tradingSpot = map.listerThings.AllThings.Find(x => x.def.defName == "RTTransferSpot");
            if (tradingSpot != null) return tradingSpot.Position;
            else
            {
                RT_Dialog_Message d1 = new RT_Dialog_Message("MESSAGE", new string[] 
                { 
                    "You are missing a transfer spot!",
                    "Received things will appear in the center of the map",
                    "Build a trading spot to change the drop location!"
                });

                RT_Dialog_Base.PushNewDialog(d1);

                return new IntVec3(map.Center.x, map.Center.y, map.Center.z);
            }
        }

        //Gets all the transfered items from the transfer into usable objects

        public static Thing[] GetAllTransferedItems(TransferData transferData)
        {
            List<Thing> allTransferedItems = new List<Thing>();

            foreach (HumanFile file in transferData._humans)
            {
                allTransferedItems.Add(ScribeManager.StringtoHuman(file));
            }

            foreach (string data in transferData._animals)
            {
                allTransferedItems.Add((Pawn)ScribeManager.SerializeFromString<Pawn>(data));
            }

            foreach (string data in transferData._things)
            {
                allTransferedItems.Add((Thing)ScribeManager.SerializeFromString<Thing>(data));
            }

            return allTransferedItems.ToArray();
        }
    }
}
