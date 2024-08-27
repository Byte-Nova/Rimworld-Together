using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;
using static Shared.CommonEnumerators;


namespace GameClient
{
    public static class SiteManager
    {
        public static SitePartDef[] siteDefs;

        public static string[] siteDefLabels;

        public static int[] siteRewardCount;

        public static ThingDef[] siteRewardDefNames;

        public static void SetSiteData(ServerGlobalData serverGlobalData)
        {
            siteRewardDefNames = new ThingDef[]
            {
                ThingDefOf.RawPotatoes,
                ThingDefOf.Steel,
                ThingDefOf.WoodLog,
                ThingDefOf.Silver,
                ThingDefOf.ComponentIndustrial,
                ThingDefOf.Chemfuel,
                ThingDefOf.MedicineHerbal,
                ThingDefOf.Cloth,
                ThingDefOf.MealSimple
            };

            siteRewardCount = new int[]
            {
                serverGlobalData.siteValues.FarmlandRewardCount,
                serverGlobalData.siteValues.QuarryRewardCount,
                serverGlobalData.siteValues.SawmillRewardCount,
                serverGlobalData.siteValues.BankRewardCount,
                serverGlobalData.siteValues.LaboratoryRewardCount,
                serverGlobalData.siteValues.RefineryRewardCount,
                serverGlobalData.siteValues.HerbalWorkshopRewardCount,
                serverGlobalData.siteValues.TextileFactoryRewardCount,
                serverGlobalData.siteValues.FoodProcessorRewardCount
            };

            PersonalSiteManager.SetSiteData(serverGlobalData);
            FactionSiteManager.SetSiteData(serverGlobalData);
        }

        public static void SetSiteDefs()
        {
            List<SitePartDef> defs = new List<SitePartDef>();
            foreach (SitePartDef def in DefDatabase<SitePartDef>.AllDefs)
            {
                if (def.defName == "RTFarmland") defs.Add(def);
                else if (def.defName == "RTQuarry") defs.Add(def);
                else if (def.defName == "RTSawmill") defs.Add(def);
                else if (def.defName == "RTBank") defs.Add(def);
                else if (def.defName == "RTLaboratory") defs.Add(def);
                else if (def.defName == "RTRefinery") defs.Add(def);
                else if (def.defName == "RTHerbalWorkshop") defs.Add(def);
                else if (def.defName == "RTTextileFactory") defs.Add(def);
                else if (def.defName == "RTFoodProcessor") defs.Add(def);
            }
            siteDefs = defs.ToArray();

            List<string> siteNames = new List<string>();
            foreach(SitePartDef def in siteDefs)
            {
                siteNames.Add(def.label);
            }
            siteDefLabels = siteNames.ToArray();
        }

        public static SitePartDef GetDefForNewSite(int siteTypeID, bool isFromFaction)
        {
            return siteDefs[siteTypeID];
        }

        public static void ParseSitePacket(Packet packet)
        {
            SiteData siteData = Serializer.ConvertBytesToObject<SiteData>(packet.contents);

            switch(siteData.siteStepMode)
            {
                case SiteStepMode.Accept:
                    OnSiteAccept();
                    break;

                case SiteStepMode.Build:
                    PlayerSiteManager.SpawnSingleSite(siteData);
                    break;

                case SiteStepMode.Destroy:
                    PlayerSiteManager.RemoveSingleSite(siteData);
                    break;

                case SiteStepMode.Info:
                    OnSimpleSiteOpen(siteData);
                    break;

                case SiteStepMode.Deposit:
                    //Nothing goes here
                    break;

                case SiteStepMode.Retrieve:
                    OnWorkerRetrieval(siteData);
                    break;

                case SiteStepMode.Reward:
                    ReceiveSitesRewards(siteData);
                    break;

                case SiteStepMode.WorkerError:
                    OnWorkerError();
                    break;
            }
        }

        private static void OnSiteAccept()
        {
            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_OK("RTSiteBuilt".Translate()));

            SaveManager.ForceSave();
        }

        public static void OnSimpleSiteRequest()
        {
            DialogManager.PushNewDialog(new RT_Dialog_Wait("RTWaitSiteInfo".Translate()));

            SiteData siteData = new SiteData();
            siteData.siteFile.tile = SessionValues.chosenSite.Tile;
            siteData.siteStepMode = SiteStepMode.Info;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SitePacket), siteData);
            Network.listener.EnqueuePacket(packet);
        }

        public static void OnSimpleSiteOpen(SiteData siteData)
        {
            DialogManager.PopWaitDialog();

            if (siteData.siteFile.workerData == null)
            {
                RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("RTSiteNoCurrentWorker".Translate(), 
                    delegate { PrepareSendPawnScreen(); }, null);

                DialogManager.PushNewDialog(d1);
            }

            else
            {
                RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("RTSiteHasWorker".Translate(),
                    delegate { RequestWorkerRetrieval(siteData); }, null);

                DialogManager.PushNewDialog(d1);
            }
        }

        private static void RequestWorkerRetrieval(SiteData siteData)
        {
            DialogManager.PushNewDialog(new RT_Dialog_Wait("RTSiteWaitingWorker".Translate()));

            siteData.siteStepMode = SiteStepMode.Retrieve;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SitePacket), siteData);
            Network.listener.EnqueuePacket(packet);
        }

        private static void OnWorkerRetrieval(SiteData siteData)
        {
            DialogManager.PopWaitDialog();

            Action r1 = delegate
            {
                Pawn pawnToRetrieve = HumanScribeManager.StringToHuman(Serializer.ConvertBytesToObject<HumanData>(siteData.siteFile.workerData));

                RimworldManager.PlaceThingIntoCaravan(pawnToRetrieve, SessionValues.chosenCaravan);

                SaveManager.ForceSave();
            };

            DialogManager.PushNewDialog(new RT_Dialog_OK("RTSiteWorkerRecovered".Translate(), r1));
        }

        private static void PrepareSendPawnScreen()
        {
            List<Pawn> pawns = SessionValues.chosenCaravan.PawnsListForReading;
            List<string> pawnNames = new List<string>();
            foreach (Pawn pawn in pawns)
            {
                if (DeepScribeHelper.CheckIfThingIsHuman(pawn)) pawnNames.Add(pawn.Label);
            }

            RT_Dialog_ListingWithButton d1 = new RT_Dialog_ListingWithButton("RTSitePawnSelectionMenu".Translate(), "RTSitePawnSelectionMenuDesc".Translate(), 
                pawnNames.ToArray(), SendPawnToSite);

            DialogManager.PushNewDialog(d1);
        }

        public static void SendPawnToSite()
        {
            List<Pawn> caravanPawns = SessionValues.chosenCaravan.PawnsListForReading;
            List<Pawn> caravanHumans = new List<Pawn>();
            foreach (Pawn pawn in caravanPawns)
            {
                if (DeepScribeHelper.CheckIfThingIsHuman(pawn)) caravanHumans.Add(pawn);
            }

            Pawn pawnToSend = caravanHumans[DialogManager.dialogButtonListingResultInt];
            SessionValues.chosenCaravan.RemovePawn(pawnToSend);

            SiteData siteData = new SiteData();
            siteData.siteFile.tile = SessionValues.chosenSite.Tile;
            siteData.siteStepMode = SiteStepMode.Deposit;
            siteData.siteFile.workerData = Serializer.ConvertObjectToBytes(HumanScribeManager.HumanToString(pawnToSend));

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SitePacket), siteData);
            Network.listener.EnqueuePacket(packet);

            if (caravanHumans.Count == 1) SessionValues.chosenCaravan.Destroy();

            SaveManager.ForceSave();
        }

        public static void RequestDestroySite()
        {
            Action r1 = delegate
            {
                SiteData siteData = new SiteData();
                siteData.siteFile.tile = SessionValues.chosenSite.Tile;
                siteData.siteStepMode = SiteStepMode.Destroy;

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SitePacket), siteData);
                Network.listener.EnqueuePacket(packet);
            };

            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("RTSiteDestroySure".Translate(), r1, null);
            DialogManager.PushNewDialog(d1);
        }

        private static void OnWorkerError()
        {
            DialogManager.PushNewDialog(new RT_Dialog_Error("RTSiteWorkerInside".Translate()));
        }

        private static void ReceiveSitesRewards(SiteData siteData)
        {
            if (ClientValues.isReadyToPlay && !ClientValues.rejectSiteRewardsBool && RimworldManager.CheckIfPlayerHasMap())
            {
                Site[] sites = Find.WorldObjects.Sites.ToArray();
                List<Site> rewardedSites = new List<Site>();

                foreach (Site site in sites)
                {
                    if (siteData.sitesWithRewards.Contains(site.Tile)) rewardedSites.Add(site);
                }

                Thing[] rewards = GetSiteRewards(rewardedSites.ToArray());

                if (rewards.Count() > 0)
                {
                    TransferManager.GetTransferedItemsToSettlement(rewards, true, false, false);

                    RimworldManager.GenerateLetter("RTSiteRewards".Translate(), "RTSiteRewardsDesc".Translate(), LetterDefOf.PositiveEvent);
                }
            }
        }

        private static Thing[] GetSiteRewards(Site[] sites)
        {
            List<Thing> thingsToGet = new List<Thing>();
            foreach (Site site in sites)
            {
                for (int i = 0; i < siteDefs.Count(); i++)
                {
                    if (site.MainSitePartDef == siteDefs[i])
                    {
                        ThingData thingData = new ThingData();
                        thingData.defName = siteRewardDefNames[i].defName;
                        thingData.quantity = siteRewardCount[i];
                        thingData.quality = 0;
                        thingData.hitpoints = siteRewardDefNames[i].BaseMaxHitPoints;

                        if (siteRewardCount[i] > 0) thingsToGet.Add(ThingScribeManager.StringToItem(thingData));

                        break;
                    }
                }
            }

            return thingsToGet.ToArray();
        }
    }

    public static class PersonalSiteManager
    {
        public static int[] sitePrices;

        public static void SetSiteData(ServerGlobalData serverGlobalData)
        {
            sitePrices = new int[]
            {
                serverGlobalData.siteValues.PersonalFarmlandCost,
                serverGlobalData.siteValues.PersonalQuarryCost,
                serverGlobalData.siteValues.PersonalSawmillCost,
                serverGlobalData.siteValues.PersonalBankCost,
                serverGlobalData.siteValues.PersonalLaboratoryCost,
                serverGlobalData.siteValues.PersonalRefineryCost,
                serverGlobalData.siteValues.PersonalHerbalWorkshopCost,
                serverGlobalData.siteValues.PersonalTextileFactoryCost,
                serverGlobalData.siteValues.PersonalFoodProcessorCost
            };
        }

        public static void PushConfirmSiteDialog()
        {
            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("RTSiteCost".Translate(sitePrices[DialogManager.selectedScrollButton]), RequestSiteBuild, null);

            DialogManager.PushNewDialog(d1);
        }

        public static void RequestSiteBuild()
        {
            DialogManager.PopDialog(DialogManager.dialogScrollButtons);

            if (!RimworldManager.CheckIfHasEnoughSilverInCaravan(SessionValues.chosenCaravan, sitePrices[DialogManager.selectedScrollButton]))
            {
                DialogManager.PushNewDialog(new RT_Dialog_Error("RTNotEnoughSilver".Translate()));
            }

            else
            {
                RimworldManager.RemoveThingFromCaravan(ThingDefOf.Silver, sitePrices[DialogManager.selectedScrollButton], SessionValues.chosenCaravan);

                SiteData siteData = new SiteData();
                siteData.siteStepMode = SiteStepMode.Build;
                siteData.siteFile.tile = SessionValues.chosenCaravan.Tile;
                siteData.siteFile.type = DialogManager.selectedScrollButton;

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SitePacket), siteData);
                Network.listener.EnqueuePacket(packet);

                DialogManager.PushNewDialog(new RT_Dialog_Wait("RTSiteBuildingWait".Translate()));
            }
        }
    }

    public static class FactionSiteManager
    {
        public static int[] sitePrices;

        public static void SetSiteData(ServerGlobalData serverGlobalData)
        {
            sitePrices = new int[]
            {
                serverGlobalData.siteValues.FactionFarmlandCost,
                serverGlobalData.siteValues.FactionQuarryCost,
                serverGlobalData.siteValues.FactionSawmillCost,
                serverGlobalData.siteValues.FactionBankCost,
                serverGlobalData.siteValues.FactionLaboratoryCost,
                serverGlobalData.siteValues.FactionRefineryCost ,
                serverGlobalData.siteValues.FactionHerbalWorkshopCost,
                serverGlobalData.siteValues.FactionTextileFactoryCost,
                serverGlobalData.siteValues.FactionFoodProcessorCost
            };
        }

        public static void PushConfirmSiteDialog()
        {
            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("RTSiteCost".Translate(sitePrices[DialogManager.selectedScrollButton]), RequestSiteBuild, null);

            DialogManager.PushNewDialog(d1);
        }

        public static void RequestSiteBuild()
        {
            DialogManager.PopDialog(DialogManager.dialogScrollButtons);

            if (!RimworldManager.CheckIfHasEnoughSilverInCaravan(SessionValues.chosenCaravan, sitePrices[DialogManager.selectedScrollButton]))
            {
                DialogManager.PushNewDialog(new RT_Dialog_Error("RTNotEnoughSilver".Translate()));
            }

            else
            {
                RimworldManager.RemoveThingFromCaravan(ThingDefOf.Silver, sitePrices[DialogManager.selectedScrollButton], SessionValues.chosenCaravan);

                SiteData siteData = new SiteData();
                siteData.siteStepMode = SiteStepMode.Build;
                siteData.siteFile.tile = SessionValues.chosenCaravan.Tile;
                siteData.siteFile.type = DialogManager.selectedScrollButton;
                siteData.siteFile.factionFile = new FactionFile();

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SitePacket), siteData);
                Network.listener.EnqueuePacket(packet);

                DialogManager.PushNewDialog(new RT_Dialog_Wait("RTSiteBuildingWait".Translate()));
            }
        }
    }
}
