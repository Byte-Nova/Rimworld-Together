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

        public static void SetValues(ServerGlobalData serverGlobalData)
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
                serverGlobalData._siteValues.FarmlandRewardCount,
                serverGlobalData._siteValues.QuarryRewardCount,
                serverGlobalData._siteValues.SawmillRewardCount,
                serverGlobalData._siteValues.BankRewardCount,
                serverGlobalData._siteValues.LaboratoryRewardCount,
                serverGlobalData._siteValues.RefineryRewardCount,
                serverGlobalData._siteValues.HerbalWorkshopRewardCount,
                serverGlobalData._siteValues.TextileFactoryRewardCount,
                serverGlobalData._siteValues.FoodProcessorRewardCount
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

            switch(siteData._stepMode)
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
            siteData._siteFile.Tile = SessionValues.chosenSite.Tile;
            siteData._stepMode = SiteStepMode.Info;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SitePacket), siteData);
            Network.listener.EnqueuePacket(packet);
        }

        public static void OnSimpleSiteOpen(SiteData siteData)
        {
            DialogManager.PopWaitDialog();

            if (siteData._siteFile.WorkerData == null)
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

            siteData._stepMode = SiteStepMode.Retrieve;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SitePacket), siteData);
            Network.listener.EnqueuePacket(packet);
        }

        private static void OnWorkerRetrieval(SiteData siteData)
        {
            DialogManager.PopWaitDialog();

            Action r1 = delegate
            {
                Pawn pawnToRetrieve = HumanScribeManager.StringToHuman(Serializer.ConvertBytesToObject<HumanDataFile>(siteData._siteFile.WorkerData));

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
            siteData._siteFile.Tile = SessionValues.chosenSite.Tile;
            siteData._stepMode = SiteStepMode.Deposit;
            siteData._siteFile.WorkerData = Serializer.ConvertObjectToBytes(HumanScribeManager.HumanToString(pawnToSend));

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
                siteData._siteFile.Tile = SessionValues.chosenSite.Tile;
                siteData._stepMode = SiteStepMode.Destroy;

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
                    if (siteData._sitesWithRewards.Contains(site.Tile)) rewardedSites.Add(site);
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
                        ThingDataFile thingData = new ThingDataFile();
                        thingData.DefName = siteRewardDefNames[i].defName;
                        thingData.Quantity = siteRewardCount[i];
                        thingData.Quality = 0;
                        thingData.Hitpoints = siteRewardDefNames[i].BaseMaxHitPoints;

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
                serverGlobalData._siteValues.PersonalFarmlandCost,
                serverGlobalData._siteValues.PersonalQuarryCost,
                serverGlobalData._siteValues.PersonalSawmillCost,
                serverGlobalData._siteValues.PersonalBankCost,
                serverGlobalData._siteValues.PersonalLaboratoryCost,
                serverGlobalData._siteValues.PersonalRefineryCost,
                serverGlobalData._siteValues.PersonalHerbalWorkshopCost,
                serverGlobalData._siteValues.PersonalTextileFactoryCost,
                serverGlobalData._siteValues.PersonalFoodProcessorCost
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
                siteData._stepMode = SiteStepMode.Build;
                siteData._siteFile.Tile = SessionValues.chosenCaravan.Tile;
                siteData._siteFile.Type = DialogManager.selectedScrollButton;

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
                serverGlobalData._siteValues.FactionFarmlandCost,
                serverGlobalData._siteValues.FactionQuarryCost,
                serverGlobalData._siteValues.FactionSawmillCost,
                serverGlobalData._siteValues.FactionBankCost,
                serverGlobalData._siteValues.FactionLaboratoryCost,
                serverGlobalData._siteValues.FactionRefineryCost ,
                serverGlobalData._siteValues.FactionHerbalWorkshopCost,
                serverGlobalData._siteValues.FactionTextileFactoryCost,
                serverGlobalData._siteValues.FactionFoodProcessorCost
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
                siteData._stepMode = SiteStepMode.Build;
                siteData._siteFile.Tile = SessionValues.chosenCaravan.Tile;
                siteData._siteFile.Type = DialogManager.selectedScrollButton;
                siteData._siteFile.FactionFile = new FactionFile();

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SitePacket), siteData);
                Network.listener.EnqueuePacket(packet);

                DialogManager.PushNewDialog(new RT_Dialog_Wait("RTSiteBuildingWait".Translate()));
            }
        }
    }
}
