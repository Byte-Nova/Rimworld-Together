using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;
using Verse.Sound;
using static Shared.CommonEnumerators;

namespace GameClient
{
    //Class that handles all the thing transfers between clients in the mod

    public static class TransferManager
    {
        //Parses the packet into useful orders

        public static void ParseTransferPacket(Packet packet)
        {
            TransferData transferData = (TransferData)Serializer.ConvertBytesToObject(packet.contents);

            switch (transferData.transferStepMode)
            {
                case TransferStepMode.TradeRequest:
                    ReceiveTransferRequest(transferData);
                    break;

                case TransferStepMode.TradeAccept:
                    DialogManager.PopWaitDialog();
                    DialogManager.PushNewDialog(new RT_Dialog_OK("Transfer was a success!"));
                    if (transferData.transferMode == TransferMode.Pod) LaunchDropPods();
                    FinishTransfer(true);
                    break;

                case TransferStepMode.TradeReject:
                    DialogManager.PopWaitDialog();
                    DialogManager.PushNewDialog(new RT_Dialog_Error("Player rejected the trade!"));
                    RecoverTradeItems(TransferLocation.Caravan);
                    break;

                case TransferStepMode.TradeReRequest:
                    DialogManager.PopWaitDialog();
                    ReceiveReboundRequest(transferData);
                    break;

                case TransferStepMode.TradeReAccept:
                    DialogManager.PopWaitDialog();
                    GetTransferedItemsToSettlement(TransferManagerHelper.GetAllTransferedItems(ClientValues.incomingManifest));
                    break;

                case TransferStepMode.TradeReReject:
                    DialogManager.PopWaitDialog();
                    DialogManager.PushNewDialog(new RT_Dialog_Error("Player rejected the trade!"));
                    RecoverTradeItems(TransferLocation.Settlement);
                    break;

                case TransferStepMode.Recover:
                    DialogManager.PopWaitDialog();
                    DialogManager.PushNewDialog(new RT_Dialog_Error("Player is not currently available!"));
                    RecoverTradeItems(TransferLocation.Caravan);
                    break;
            }
        }

        //Takes transferable items from desired location

        public static void TakeTransferItems(TransferLocation transferLocation)
        {
            ClientValues.outgoingManifest.fromTile = Find.AnyPlayerHomeMap.Tile;

            if (transferLocation == TransferLocation.Caravan)
            {
                ClientValues.outgoingManifest.toTile = ClientValues.chosenSettlement.Tile;
            }

            else if (transferLocation == TransferLocation.Settlement)
            {
                ClientValues.outgoingManifest.toTile = ClientValues.incomingManifest.fromTile;
            }

            if (TradeSession.deal.TryExecute(out bool actuallyTraded))
            {
                SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();

                if (transferLocation == TransferLocation.Caravan)
                {
                    TradeSession.playerNegotiator.GetCaravan().RecacheImmobilizedNow();
                }
            }
        }

        //Takes transferable items from drop pods

        public static void TakeTransferItemsFromPods(CompLaunchable representative)
        {
            ClientValues.outgoingManifest.transferMode = TransferMode.Pod;
            ClientValues.outgoingManifest.fromTile = Find.AnyPlayerHomeMap.Tile;
            ClientValues.outgoingManifest.toTile = ClientValues.chosenSettlement.Tile;

            foreach (CompTransporter pod in representative.TransportersInGroup)
            {
                ThingOwner directlyHeldThings = pod.GetDirectlyHeldThings();

                for(int i = 0; i < directlyHeldThings.Count(); i++)
                {
                    TransferManagerHelper.AddThingToTransferManifest(directlyHeldThings[i], directlyHeldThings[i].stackCount);
                }
            }
        }

        //Sends a transfer request to the server

        public static void SendTransferRequestToServer(TransferLocation transferLocation)
        {
            DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for transfer response"));

            if (transferLocation == TransferLocation.Caravan)
            {
                ClientValues.outgoingManifest.transferStepMode = TransferStepMode.TradeRequest;

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), ClientValues.outgoingManifest);
                Network.listener.EnqueuePacket(packet);
            }

            else if (transferLocation == TransferLocation.Settlement)
            {
                ClientValues.outgoingManifest.transferStepMode = TransferStepMode.TradeReRequest;

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), ClientValues.outgoingManifest);
                Network.listener.EnqueuePacket(packet);
            }

            else if (transferLocation == TransferLocation.Pod)
            {
                ClientValues.outgoingManifest.transferStepMode = TransferStepMode.TradeRequest;

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), ClientValues.outgoingManifest);
                Network.listener.EnqueuePacket(packet);
            }

            else if (transferLocation == TransferLocation.World)
            {
                MarketData marketData = new MarketData();
                marketData.marketStepMode = MarketStepMode.Add;
                marketData.transferThingBytes = ClientValues.outgoingManifest.itemDatas;

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.MarketPacket), marketData);
                Network.listener.EnqueuePacket(packet);
            }
        }

        //Recovers transfered items when trade fails

        public static void RecoverTradeItems(TransferLocation transferLocation)
        {
            try
            {
                Thing[] toRecover = TransferManagerHelper.GetAllTransferedItems(ClientValues.outgoingManifest);

                if (transferLocation == TransferLocation.Caravan)
                {
                    GetTransferedItemsToCaravan(toRecover, false);
                }

                else if (transferLocation == TransferLocation.Settlement)
                {
                    GetTransferedItemsToSettlement(toRecover, false);
                }

                else if (transferLocation == TransferLocation.Pod)
                {
                    //Do nothing
                }
            }

            catch
            {
                Logger.Warning("Rethrowing transfer items, might be Rimworld's fault");

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
                if (customMap) map = Find.Maps.Find(x => x.Tile == ClientValues.incomingManifest.toTile);
                else map = Find.AnyPlayerHomeMap;

                IntVec3 location = TransferManagerHelper.GetTransferLocationInMap(map);

                foreach (Thing thing in things)
                {
                    if (thing is Pawn) GenSpawn.Spawn(thing, location, map, Rot4.Random);
                    else GenPlace.TryPlaceThing(thing, location, map, ThingPlaceMode.Near);
                }

                FinishTransfer(success);
            };

            if (invokeMessage)
            {
                if (success) DialogManager.PushNewDialog(new RT_Dialog_OK("Transfer was a success!", r1));
                else DialogManager.PushNewDialog(new RT_Dialog_Error("Transfer was cancelled!", r1));
            }
            else r1.Invoke();
        }

        //Receives the transfered items into the caravan

        public static void GetTransferedItemsToCaravan(Thing[] things, bool success = true, bool invokeMessage = true)
        {
            Action r1 = delegate
            {
                foreach (Thing thing in things)
                {
                    if (DeepScribeHelper.CheckIfThingIsHuman(thing))
                    {
                        TransferManagerHelper.TransferPawnIntoCaravan(thing as Pawn);
                    }

                    else if (DeepScribeHelper.CheckIfThingIsAnimal(thing))
                    {
                        TransferManagerHelper.TransferPawnIntoCaravan(thing as Pawn);
                    }

                    else TransferManagerHelper.TransferItemIntoCaravan(thing);
                }

                FinishTransfer(success);
            };

            if (invokeMessage)
            {
                if (success) DialogManager.PushNewDialog(new RT_Dialog_OK("Transfer was a success!", r1));
                else DialogManager.PushNewDialog(new RT_Dialog_Error("Transfer was cancelled!", r1));
            }
            else r1.Invoke();
        }

        //Finishes the transfer order

        public static void FinishTransfer(bool success)
        {
            if (success) SaveManager.ForceSave();

            ClientValues.incomingManifest = new TransferData();
            ClientValues.outgoingManifest = new TransferData();
            ClientValues.ToggleTransfer(false);
        }

        //Executes when receiving a transfer request

        public static void ReceiveTransferRequest(TransferData transferData)
        {
            try
            {
                ClientValues.incomingManifest = transferData;

                if (!ClientValues.isReadyToPlay || ClientValues.isInTransfer || ClientValues.rejectTransferBool)
                {
                    RejectRequest(transferData.transferMode, false);
                }

                else
                {
                    Action r1 = delegate
                    {
                        if (transferData.transferMode == TransferMode.Gift)
                        {
                            RT_Dialog_ItemListing d1 = new RT_Dialog_ItemListing(TransferManagerHelper.GetAllTransferedItems(transferData), TransferMode.Gift);
                            DialogManager.PushNewDialog(d1);
                        }

                        else if (transferData.transferMode == TransferMode.Trade)
                        {
                            RT_Dialog_ItemListing d1 = new RT_Dialog_ItemListing(TransferManagerHelper.GetAllTransferedItems(transferData), TransferMode.Trade);
                            DialogManager.PushNewDialog(d1);
                        }

                        else if (transferData.transferMode == TransferMode.Pod)
                        {
                            RT_Dialog_ItemListing d1 = new RT_Dialog_ItemListing(TransferManagerHelper.GetAllTransferedItems(transferData), TransferMode.Pod);
                            DialogManager.PushNewDialog(d1);
                        }
                    };

                    if (transferData.transferMode == TransferMode.Gift)
                    {
                        DialogManager.PushNewDialog(new RT_Dialog_OK("You are receiving a gift request", r1));
                    }

                    else if (transferData.transferMode == TransferMode.Trade)
                    {
                        DialogManager.PushNewDialog(new RT_Dialog_OK("You are receiving a trade request", r1));
                    }

                    else if (transferData.transferMode == TransferMode.Pod)
                    {
                        DialogManager.PushNewDialog(new RT_Dialog_OK("You are receiving a gift request", r1));
                    }
                }
            }

            catch
            {
                Logger.Warning("Rethrowing transfer items, might be Rimworld's fault");

                Thread.Sleep(100);

                ReceiveTransferRequest(transferData);
            }        
        }

        //Executes after receiving a rebound transfer request

        public static void ReceiveReboundRequest(TransferData transferData)
        {
            try
            {
                ClientValues.incomingManifest = transferData;

                RT_Dialog_ItemListing d1 = new RT_Dialog_ItemListing(TransferManagerHelper.GetAllTransferedItems(transferData), TransferMode.Rebound);
                DialogManager.PushNewDialog(d1);
            }

            catch
            {
                Logger.Warning("Rethrowing transfer items, might be Rimworld's fault");

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
                ClientValues.incomingManifest.transferStepMode = TransferStepMode.TradeReject;

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), ClientValues.incomingManifest);
                Network.listener.EnqueuePacket(packet);
            }

            else if (transferMode == TransferMode.Pod)
            {
                //Nothing should happen here
            }

            else if (transferMode == TransferMode.Rebound)
            {
                ClientValues.incomingManifest.transferStepMode = TransferStepMode.TradeReReject;

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), ClientValues.incomingManifest);
                Network.listener.EnqueuePacket(packet);

                RecoverTradeItems(TransferLocation.Caravan);
            }

            if (finishTransfer) FinishTransfer(false);
        }

        //Launchs the drop pods with the desired transfer request

        public static void LaunchDropPods()
        {
            ClientValues.chosendPods.TryLaunch(ClientValues.chosenSettlement.Tile, 
                new TransportPodsArrivalAction_GiveGift(ClientValues.chosenSettlement));
        }
    }

    //Helper class of the TransferManager class

    public static class TransferManagerHelper
    {
        //Adds desired thing into transfer manifest

        public static void AddThingToTransferManifest(Thing thing, int thingCount)
        {
            if (DeepScribeHelper.CheckIfThingIsHuman(thing))
            {
                Pawn pawn = thing as Pawn;

                ClientValues.outgoingManifest.humanDatas.Add(Serializer.ConvertObjectToBytes
                    (HumanScribeManager.HumanToString(pawn, false)));

                if (Find.WorldPawns.AllPawnsAliveOrDead.Contains(pawn))
                {
                    Find.WorldPawns.RemovePawn(pawn);
                }
            }

            else if (DeepScribeHelper.CheckIfThingIsAnimal(thing))
            {
                Pawn pawn = thing as Pawn;

                ClientValues.outgoingManifest.animalDatas.Add(Serializer.ConvertObjectToBytes
                    (AnimalScribeManager.AnimalToString(pawn)));

                if (Find.WorldPawns.AllPawnsAliveOrDead.Contains(pawn))
                {
                    Find.WorldPawns.RemovePawn(pawn);
                }
            }

            else
            {
                ClientValues.outgoingManifest.itemDatas.Add(Serializer.ConvertObjectToBytes
                    (ThingScribeManager.ItemToString(thing, thingCount)));
            }
        }

        //Gets the transfer location in the desired map

        public static IntVec3 GetTransferLocationInMap(Map map)
        {
            Thing tradingSpot = map.listerThings.AllThings.Find(x => x.def.defName == "RTTransferSpot");
            if (tradingSpot != null) return tradingSpot.Position;
            else
            {
                RT_Dialog_OK_Loop d1 = new RT_Dialog_OK_Loop(new string[] { "You are missing a transfer spot!",
                    "Received items will appear in the center of the map",
                    "Build a trading spot to change the drop location!"});

                DialogManager.PushNewDialog(d1);

                return new IntVec3(map.Center.x, map.Center.y, map.Center.z);
            }
        }

        //Gets all the transfered items from the transfer into usable objects

        public static Thing[] GetAllTransferedItems(TransferData transferData)
        {
            List<Thing> allTransferedItems = new List<Thing>();

            foreach (Pawn pawn in HumanScribeManager.GetHumansFromString(transferData)) allTransferedItems.Add(pawn);

            foreach (Pawn animal in AnimalScribeManager.GetAnimalsFromString(transferData)) allTransferedItems.Add(animal);

            foreach (Thing thing in ThingScribeManager.GetItemsFromString(transferData)) allTransferedItems.Add(thing);

            return allTransferedItems.ToArray();
        }

        //Transfers a pawn into the caravan

        public static void TransferPawnIntoCaravan(Pawn pawnToTransfer)
        {
            if (!Find.WorldPawns.AllPawnsAliveOrDead.Contains(pawnToTransfer))
            {
                Find.WorldPawns.PassToWorld(pawnToTransfer);
            }

            pawnToTransfer.SetFaction(Faction.OfPlayer);
            ClientValues.chosenCaravan.AddPawn(pawnToTransfer, false);
        }

        //Transfers an item into the caravan

        public static void TransferItemIntoCaravan(Thing thingToTransfer)
        {
            if (thingToTransfer.stackCount == 0) return;

            ClientValues.chosenCaravan.AddPawnOrItem(thingToTransfer, false);
        }

        //Removes an item from the caravan

        public static void RemoveThingFromCaravan(ThingDef thingDef, int requiredQuantity)
        {
            if (requiredQuantity == 0) return;

            List<Thing> caravanQuantity = CaravanInventoryUtility.AllInventoryItems(ClientValues.chosenCaravan)
                .FindAll(x => x.def == thingDef);

            int takenQuantity = 0;
            foreach (Thing thing in caravanQuantity)
            {
                if (takenQuantity + thing.stackCount >= requiredQuantity)
                {
                    thing.holdingOwner.Take(thing, requiredQuantity - takenQuantity);
                    break;
                }

                else if (takenQuantity + thing.stackCount < requiredQuantity)
                {
                    thing.holdingOwner.Take(thing, thing.stackCount);
                    takenQuantity += thing.stackCount;
                }
            }
        }


        //Removes an item from the settlement

        public static void RemoveThingFromSettlement(Map map, ThingDef thingDef, int requiredQuantity)
        {
            if (requiredQuantity == 0) return;

            List<Thing> thingInMap = new List<Thing>();
            foreach (Zone zone in map.zoneManager.AllZones)
            {
                foreach (Thing thing in zone.AllContainedThings.Where(fetch => fetch.def.category == ThingCategory.Item))
                {
                    if (thing.def == thingDef && !thing.Position.Fogged(map))
                    {
                        thingInMap.Add(thing);
                    }
                }
            }

            int takenQuantity = 0;
            foreach (Thing thing in thingInMap)
            {
                if (takenQuantity + thing.stackCount >= requiredQuantity)
                {
                    thing.stackCount -= requiredQuantity - takenQuantity;
                    if (thing.stackCount <= 0) thing.Destroy();
                    break;
                }

                else if (takenQuantity + thing.stackCount < requiredQuantity)
                {
                    thing.Destroy();
                    takenQuantity += thing.stackCount;
                }
            }
        }
    }
}
