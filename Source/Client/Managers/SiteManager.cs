using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;


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

            try
            {
                siteRewardCount = new int[9]
                {
                    int.Parse(serverGlobalData.FarmlandRewardCount),
                    int.Parse(serverGlobalData.QuarryRewardCount),
                    int.Parse(serverGlobalData.SawmillRewardCount),
                    int.Parse(serverGlobalData.BankRewardCount),
                    int.Parse(serverGlobalData.LaboratoryRewardCount),
                    int.Parse(serverGlobalData.RefineryRewardCount),
                    int.Parse(serverGlobalData.HerbalWorkshopRewardCount),
                    int.Parse(serverGlobalData.TextileFactoryRewardCount),
                    int.Parse(serverGlobalData.FoodProcessorRewardCount)
                };
            }

            catch 
            {
                Logger.Warning("Server didn't have site rewards set, defaulting to 0");

                siteRewardCount = new int[9]
                {
                    0, 0, 0, 0, 0, 0, 0, 0, 0
                };
            }

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
            SiteData siteData = (SiteData)Serializer.ConvertBytesToObject(packet.contents);

            switch(int.Parse(siteData.siteStep))
            {
                case (int)CommonEnumerators.SiteStepMode.Accept:
                    OnSiteAccept();
                    break;

                case (int)CommonEnumerators.SiteStepMode.Build:
                    PlanetManager.SpawnSingleSite(siteData);
                    break;

                case (int)CommonEnumerators.SiteStepMode.Destroy:
                    PlanetManager.RemoveSingleSite(siteData);
                    break;

                case (int)CommonEnumerators.SiteStepMode.Info:
                    OnSimpleSiteOpen(siteData);
                    break;

                case (int)CommonEnumerators.SiteStepMode.Deposit:
                    //Nothing goes here
                    break;

                case (int)CommonEnumerators.SiteStepMode.Retrieve:
                    OnWorkerRetrieval(siteData);
                    break;

                case (int)CommonEnumerators.SiteStepMode.Reward:
                    ReceiveSitesRewards(siteData);
                    break;

                case (int)CommonEnumerators.SiteStepMode.WorkerError:
                    OnWorkerError();
                    break;
            }
        }

        private static void OnSiteAccept()
        {
            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_OK("The desired site has been built!"));

            SaveManager.ForceSave();
        }

        public static void OnSimpleSiteRequest()
        {
            DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for site information"));

            SiteData siteData = new SiteData();
            siteData.tile = ClientValues.chosenSite.Tile;
            siteData.siteStep = ((int)CommonEnumerators.SiteStepMode.Info).ToString();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteData);
            Network.listener.EnqueuePacket(packet);
        }

        public static void OnSimpleSiteOpen(SiteData siteData)
        {
            DialogManager.PopWaitDialog();

            if (siteData.workerData == null)
            {
                RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("There is no current worker on this site, send?", 
                    delegate { PrepareSendPawnScreen(); }, null);

                DialogManager.PushNewDialog(d1);
            }

            else
            {
                RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("You have a worker on this site, retrieve?",
                    delegate { RequestWorkerRetrieval(siteData); }, null);

                DialogManager.PushNewDialog(d1);
            }
        }

        private static void RequestWorkerRetrieval(SiteData siteData)
        {
            DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for site worker"));

            siteData.siteStep = ((int)CommonEnumerators.SiteStepMode.Retrieve).ToString();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteData);
            Network.listener.EnqueuePacket(packet);
        }

        private static void OnWorkerRetrieval(SiteData siteData)
        {
            DialogManager.PopWaitDialog();

            Action r1 = delegate
            {
                Pawn pawnToRetrieve = HumanScribeManager.StringToHuman((HumanData)Serializer.
                    ConvertBytesToObject(siteData.workerData));

                TransferManagerHelper.TransferPawnIntoCaravan(pawnToRetrieve);

                SaveManager.ForceSave();
            };

            DialogManager.PushNewDialog(new RT_Dialog_OK("Worker have been recovered", r1));
        }

        private static void PrepareSendPawnScreen()
        {
            List<Pawn> pawns = ClientValues.chosenCaravan.PawnsListForReading;
            List<string> pawnNames = new List<string>();
            foreach (Pawn pawn in pawns)
            {
                if (DeepScribeHelper.CheckIfThingIsHuman(pawn)) pawnNames.Add(pawn.Label);
            }

            RT_Dialog_ListingWithButton d1 = new RT_Dialog_ListingWithButton("Pawn Selection", "Select the pawn you wish to send", 
                pawnNames.ToArray(), SendPawnToSite);

            DialogManager.PushNewDialog(d1);
        }

        public static void SendPawnToSite()
        {
            List<Pawn> caravanPawns = ClientValues.chosenCaravan.PawnsListForReading;
            List<Pawn> caravanHumans = new List<Pawn>();
            foreach (Pawn pawn in caravanPawns)
            {
                if (DeepScribeHelper.CheckIfThingIsHuman(pawn)) caravanHumans.Add(pawn);
            }

            Pawn pawnToSend = caravanHumans[DialogManager.dialogListingWithButtonResult];
            ClientValues.chosenCaravan.RemovePawn(pawnToSend);

            SiteData siteData = new SiteData();
            siteData.tile = ClientValues.chosenSite.Tile;
            siteData.siteStep = ((int)CommonEnumerators.SiteStepMode.Deposit).ToString();
            siteData.workerData = Serializer.ConvertObjectToBytes(HumanScribeManager.HumanToString(pawnToSend));

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteData);
            Network.listener.EnqueuePacket(packet);

            if (caravanHumans.Count == 1) ClientValues.chosenCaravan.Destroy();

            SaveManager.ForceSave();
        }

        public static void RequestDestroySite()
        {
            Action r1 = delegate
            {
                SiteData siteData = new SiteData();
                siteData.tile = ClientValues.chosenSite.Tile;
                siteData.siteStep = ((int)CommonEnumerators.SiteStepMode.Destroy).ToString();

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteData);
                Network.listener.EnqueuePacket(packet);
            };

            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("Are you sure you want to destroy this site?", r1, null);
            DialogManager.PushNewDialog(d1);
        }

        private static void OnWorkerError()
        {
            DialogManager.PushNewDialog(new RT_Dialog_Error("The site has a worker inside!"));
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

                    RimworldManager.GenerateLetter("Site Rewards", "You have received site rewards!", LetterDefOf.PositiveEvent);
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
                        ItemData itemData = new ItemData();
                        itemData.defName = siteRewardDefNames[i].defName;
                        itemData.quantity = siteRewardCount[i];
                        itemData.quality = "null";
                        itemData.hitpoints = siteRewardDefNames[i].BaseMaxHitPoints;

                        if (siteRewardCount[i] > 0) thingsToGet.Add(ThingScribeManager.StringToItem(itemData));

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
            try
            {
                sitePrices = new int[9]
                {
                    int.Parse(serverGlobalData.PersonalFarmlandCost),
                    int.Parse(serverGlobalData.PersonalQuarryCost),
                    int.Parse(serverGlobalData.PersonalSawmillCost),
                    int.Parse(serverGlobalData.PersonalBankCost),
                    int.Parse(serverGlobalData.PersonalLaboratoryCost),
                    int.Parse(serverGlobalData.PersonalRefineryCost),
                    int.Parse(serverGlobalData.PersonalHerbalWorkshopCost),
                    int.Parse(serverGlobalData.PersonalTextileFactoryCost),
                    int.Parse(serverGlobalData.PersonalFoodProcessorCost)
                };
            }

            catch 
            {
                Logger.Warning("Server didn't have personal site prices set, defaulting to 0");

                sitePrices = new int[9]
                {
                    0, 0, 0, 0, 0, 0, 0, 0, 0
                };
            }
        }

        public static void PushConfirmSiteDialog()
        {
            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo($"This site will cost you {sitePrices[DialogManager.selectedScrollButton]} " +
                $"silver, continue?", RequestSiteBuild, null);

            DialogManager.PushNewDialog(d1);
        }

        public static void RequestSiteBuild()
        {
            DialogManager.PopDialog(DialogManager.dialogScrollButtons);

            if (!RimworldManager.CheckIfHasEnoughSilverInCaravan(sitePrices[DialogManager.selectedScrollButton]))
            {
                DialogManager.PushNewDialog(new RT_Dialog_Error("You do not have enough silver!"));
            }

            else
            {
                TransferManagerHelper.RemoveThingFromCaravan(ThingDefOf.Silver, sitePrices[DialogManager.selectedScrollButton]);

                SiteData siteData = new SiteData();
                siteData.siteStep = ((int)CommonEnumerators.SiteStepMode.Build).ToString();
                siteData.tile = ClientValues.chosenCaravan.Tile;
                siteData.type = DialogManager.selectedScrollButton.ToString();
                siteData.isFromFaction = false;

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteData);
                Network.listener.EnqueuePacket(packet);

                DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for building"));
            }
        }
    }

    public static class FactionSiteManager
    {
        public static int[] sitePrices;

        public static void SetSiteData(ServerGlobalData serverGlobalData)
        {
            try
            {
                sitePrices = new int[9]
                {
                    int.Parse(serverGlobalData.FactionFarmlandCost),
                    int.Parse(serverGlobalData.FactionQuarryCost),
                    int.Parse(serverGlobalData.FactionSawmillCost),
                    int.Parse(serverGlobalData.FactionBankCost),
                    int.Parse(serverGlobalData.FactionLaboratoryCost),
                    int.Parse(serverGlobalData.FactionRefineryCost),
                    int.Parse(serverGlobalData.FactionHerbalWorkshopCost),
                    int.Parse(serverGlobalData.FactionTextileFactoryCost),
                    int.Parse(serverGlobalData.FactionFoodProcessorCost)
                };
            }

            catch
            {
                Logger.Warning("Server didn't have faction site prices set, defaulting to 0");

                sitePrices = new int[9]
                {
                    0, 0, 0, 0, 0, 0, 0, 0, 0
                };
            }
        }

        public static void PushConfirmSiteDialog()
        {
            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo($"This site will cost you {sitePrices[DialogManager.selectedScrollButton]} " +
                $"silver, continue?", RequestSiteBuild, null);

            DialogManager.PushNewDialog(d1);
        }

        public static void RequestSiteBuild()
        {
            DialogManager.PopDialog(DialogManager.dialogScrollButtons);

            if (!RimworldManager.CheckIfHasEnoughSilverInCaravan(sitePrices[DialogManager.selectedScrollButton]))
            {
                DialogManager.PushNewDialog(new RT_Dialog_Error("You do not have enough silver!"));
            }

            else
            {
                TransferManagerHelper.RemoveThingFromCaravan(ThingDefOf.Silver, sitePrices[DialogManager.selectedScrollButton]);

                SiteData siteData = new SiteData();
                siteData.siteStep = ((int)CommonEnumerators.SiteStepMode.Build).ToString();
                siteData.tile = ClientValues.chosenCaravan.Tile;
                siteData.type = DialogManager.selectedScrollButton.ToString();
                siteData.isFromFaction = true;

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteData);
                Network.listener.EnqueuePacket(packet);

                DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for building"));
            }
        }
    }
}
