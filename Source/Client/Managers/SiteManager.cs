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

        public static void SetSiteDetails(ServerOverallJSON serverOverallJSON)
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
                    int.Parse(serverOverallJSON.FarmlandRewardCount),
                    int.Parse(serverOverallJSON.QuarryRewardCount),
                    int.Parse(serverOverallJSON.SawmillRewardCount),
                    int.Parse(serverOverallJSON.BankRewardCount),
                    int.Parse(serverOverallJSON.LaboratoryRewardCount),
                    int.Parse(serverOverallJSON.RefineryRewardCount),
                    int.Parse(serverOverallJSON.HerbalWorkshopRewardCount),
                    int.Parse(serverOverallJSON.TextileFactoryRewardCount),
                    int.Parse(serverOverallJSON.FoodProcessorRewardCount)
                };
            }

            catch 
            {
                Log.Warning("Server didn't have site rewards set, defaulting to 0");

                siteRewardCount = new int[9]
                {
                    0, 0, 0, 0, 0, 0, 0, 0, 0
                };
            }

            PersonalSiteManager.SetSiteDetails(serverOverallJSON);
            FactionSiteManager.SetSiteDetails(serverOverallJSON);
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
            SiteDetailsJSON siteDetailsJSON = (SiteDetailsJSON)Serializer.ConvertBytesToObject(packet.contents);

            switch(int.Parse(siteDetailsJSON.siteStep))
            {
                case (int)CommonEnumerators.SiteStepMode.Accept:
                    OnSiteAccept();
                    break;

                case (int)CommonEnumerators.SiteStepMode.Build:
                    PlanetManager.SpawnSingleSite(siteDetailsJSON);
                    break;

                case (int)CommonEnumerators.SiteStepMode.Destroy:
                    PlanetManager.RemoveSingleSite(siteDetailsJSON);
                    break;

                case (int)CommonEnumerators.SiteStepMode.Info:
                    OnSimpleSiteOpen(siteDetailsJSON);
                    break;

                case (int)CommonEnumerators.SiteStepMode.Deposit:
                    //Nothing goes here
                    break;

                case (int)CommonEnumerators.SiteStepMode.Retrieve:
                    OnWorkerRetrieval(siteDetailsJSON);
                    break;

                case (int)CommonEnumerators.SiteStepMode.Reward:
                    ReceiveSitesRewards(siteDetailsJSON);
                    break;

                case (int)CommonEnumerators.SiteStepMode.WorkerError:
                    OnWorkerError();
                    break;
            }
        }

        private static void OnSiteAccept()
        {
            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_OK("RimworldTogether.SiteBuildSuccess".Translate()));

            SaveManager.ForceSave();
        }

        public static void OnSimpleSiteRequest()
        {
            DialogManager.PushNewDialog(new RT_Dialog_Wait("RimworldTogether.SiteInfoWaiting".Translate()));

            SiteDetailsJSON siteDetailsJSON = new SiteDetailsJSON();
            siteDetailsJSON.tile = ClientValues.chosenSite.Tile.ToString();
            siteDetailsJSON.siteStep = ((int)CommonEnumerators.SiteStepMode.Info).ToString();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteDetailsJSON);
            Network.listener.EnqueuePacket(packet);
        }

        public static void OnSimpleSiteOpen(SiteDetailsJSON siteDetailsJSON)
        {
            DialogManager.PopWaitDialog();

            if (siteDetailsJSON.workerData == null)
            {
                RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("RimworldTogether.SiteWorkerSend".Translate(), 
                    delegate { PrepareSendPawnScreen(); }, null);

                DialogManager.PushNewDialog(d1);
            }

            else
            {
                RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("RimworldTogether.SiteWorkerRetrive".Translate(),
                    delegate { RequestWorkerRetrieval(siteDetailsJSON); }, null);

                DialogManager.PushNewDialog(d1);
            }
        }

        private static void RequestWorkerRetrieval(SiteDetailsJSON siteDetailsJSON)
        {
            DialogManager.PushNewDialog(new RT_Dialog_Wait("RimworldTogether.SiteWorkerWait".Translate()));

            siteDetailsJSON.siteStep = ((int)CommonEnumerators.SiteStepMode.Retrieve).ToString();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteDetailsJSON);
            Network.listener.EnqueuePacket(packet);
        }

        private static void OnWorkerRetrieval(SiteDetailsJSON siteDetailsJSON)
        {
            DialogManager.PopWaitDialog();

            Action r1 = delegate
            {
                Pawn pawnToRetrieve = HumanScribeManager.StringToHuman((HumanDetailsJSON)Serializer.
                    ConvertBytesToObject(siteDetailsJSON.workerData));

                TransferManagerHelper.TransferPawnIntoCaravan(pawnToRetrieve);

                SaveManager.ForceSave();
            };

            DialogManager.PushNewDialog(new RT_Dialog_OK("RimworldTogether.SiteWorkerBack".Translate(), r1));
        }

        private static void PrepareSendPawnScreen()
        {
            List<Pawn> pawns = ClientValues.chosenCaravan.PawnsListForReading;
            List<string> pawnNames = new List<string>();
            foreach (Pawn pawn in pawns)
            {
                if (TransferManagerHelper.CheckIfThingIsHuman(pawn)) pawnNames.Add(pawn.Label);
            }

            RT_Dialog_ListingWithButton d1 = new RT_Dialog_ListingWithButton("RimworldTogether.PawnSelection".Translate(), "RimworldTogether.PawnSelect".Translate(), 
                pawnNames.ToArray(), SendPawnToSite);

            DialogManager.PushNewDialog(d1);
        }

        public static void SendPawnToSite()
        {
            List<Pawn> caravanPawns = ClientValues.chosenCaravan.PawnsListForReading;
            List<Pawn> caravanHumans = new List<Pawn>();
            foreach (Pawn pawn in caravanPawns)
            {
                if (TransferManagerHelper.CheckIfThingIsHuman(pawn)) caravanHumans.Add(pawn);
            }

            Pawn pawnToSend = caravanHumans[DialogManager.dialogListingWithButtonResult];
            ClientValues.chosenCaravan.RemovePawn(pawnToSend);

            SiteDetailsJSON siteDetailsJSON = new SiteDetailsJSON();
            siteDetailsJSON.tile = ClientValues.chosenSite.Tile.ToString();
            siteDetailsJSON.siteStep = ((int)CommonEnumerators.SiteStepMode.Deposit).ToString();
            siteDetailsJSON.workerData = Serializer.ConvertObjectToBytes(HumanScribeManager.HumanToString(pawnToSend));

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteDetailsJSON);
            Network.listener.EnqueuePacket(packet);

            if (caravanHumans.Count == 1) ClientValues.chosenCaravan.Destroy();

            SaveManager.ForceSave();
        }

        public static void RequestDestroySite()
        {
            Action r1 = delegate
            {
                SiteDetailsJSON siteDetailsJSON = new SiteDetailsJSON();
                siteDetailsJSON.tile = ClientValues.chosenSite.Tile.ToString();
                siteDetailsJSON.siteStep = ((int)CommonEnumerators.SiteStepMode.Destroy).ToString();

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteDetailsJSON);
                Network.listener.EnqueuePacket(packet);
            };

            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("RimworldTogether.SiteRemove".Translate(), r1, null);
            DialogManager.PushNewDialog(d1);
        }

        private static void OnWorkerError()
        {
            DialogManager.PushNewDialog(new RT_Dialog_Error("RimworldTogether.SiteHasWorker".Translate()));
        }

        private static void ReceiveSitesRewards(SiteDetailsJSON siteDetailsJSON)
        {
            if (ClientValues.isReadyToPlay && !ClientValues.autoRejectSiteRewards && RimworldManager.CheckIfPlayerHasMap())
            {
                Site[] sites = Find.WorldObjects.Sites.ToArray();
                List<Site> rewardedSites = new List<Site>();

                foreach (Site site in sites)
                {
                    if (siteDetailsJSON.sitesWithRewards.Contains(site.Tile.ToString())) rewardedSites.Add(site);
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
                        ItemDetailsJSON itemDetailsJSON = new ItemDetailsJSON();
                        itemDetailsJSON.defName = siteRewardDefNames[i].defName;
                        itemDetailsJSON.quantity = siteRewardCount[i];
                        itemDetailsJSON.quality = "null";
                        itemDetailsJSON.hitpoints = siteRewardDefNames[i].BaseMaxHitPoints;

                        if (siteRewardCount[i] > 0) thingsToGet.Add(ThingScribeManager.StringToItem(itemDetailsJSON));

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

        public static void SetSiteDetails(ServerOverallJSON serverOverallJSON)
        {
            try
            {
                sitePrices = new int[9]
                {
                    int.Parse(serverOverallJSON.PersonalFarmlandCost),
                    int.Parse(serverOverallJSON.PersonalQuarryCost),
                    int.Parse(serverOverallJSON.PersonalSawmillCost),
                    int.Parse(serverOverallJSON.PersonalBankCost),
                    int.Parse(serverOverallJSON.PersonalLaboratoryCost),
                    int.Parse(serverOverallJSON.PersonalRefineryCost),
                    int.Parse(serverOverallJSON.PersonalHerbalWorkshopCost),
                    int.Parse(serverOverallJSON.PersonalTextileFactoryCost),
                    int.Parse(serverOverallJSON.PersonalFoodProcessorCost)
                };
            }

            catch 
            {
                Log.Warning("Server didn't have personal site prices set, defaulting to 0");

                sitePrices = new int[9]
                {
                    0, 0, 0, 0, 0, 0, 0, 0, 0
                };
            }
        }

        public static void PushConfirmSiteDialog()
        {
            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("RimworldTogether.SiteWillCost".Translate(sitePrices[DialogManager.selectedScrollButton]), RequestSiteBuild, null);

            DialogManager.PushNewDialog(d1);
        }

        public static void RequestSiteBuild()
        {
            DialogManager.PopDialog(DialogManager.dialogScrollButtons);

            if (!RimworldManager.CheckIfHasEnoughSilverInCaravan(sitePrices[DialogManager.selectedScrollButton]))
            {
                DialogManager.PushNewDialog(new RT_Dialog_Error("RimworldTogether.SilverLack".Translate()));
            }

            else
            {
                TransferManagerHelper.RemoveThingFromCaravan(ThingDefOf.Silver, sitePrices[DialogManager.selectedScrollButton]);

                SiteDetailsJSON siteDetailsJSON = new SiteDetailsJSON();
                siteDetailsJSON.siteStep = ((int)CommonEnumerators.SiteStepMode.Build).ToString();
                siteDetailsJSON.tile = ClientValues.chosenCaravan.Tile.ToString();
                siteDetailsJSON.type = DialogManager.selectedScrollButton.ToString();
                siteDetailsJSON.isFromFaction = false;

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteDetailsJSON);
                Network.listener.EnqueuePacket(packet);

                DialogManager.PushNewDialog(new RT_Dialog_Wait("RimworldTogether.IdleConstruction".Translate()));
            }
        }
    }

    public static class FactionSiteManager
    {
        public static int[] sitePrices;

        public static void SetSiteDetails(ServerOverallJSON serverOverallJSON)
        {
            try
            {
                sitePrices = new int[9]
                {
                    int.Parse(serverOverallJSON.FactionFarmlandCost),
                    int.Parse(serverOverallJSON.FactionQuarryCost),
                    int.Parse(serverOverallJSON.FactionSawmillCost),
                    int.Parse(serverOverallJSON.FactionBankCost),
                    int.Parse(serverOverallJSON.FactionLaboratoryCost),
                    int.Parse(serverOverallJSON.FactionRefineryCost),
                    int.Parse(serverOverallJSON.FactionHerbalWorkshopCost),
                    int.Parse(serverOverallJSON.FactionTextileFactoryCost),
                    int.Parse(serverOverallJSON.FactionFoodProcessorCost)
                };
            }

            catch
            {
                Log.Warning("Server didn't have faction site prices set, defaulting to 0");

                sitePrices = new int[9]
                {
                    0, 0, 0, 0, 0, 0, 0, 0, 0
                };
            }
        }

        public static void PushConfirmSiteDialog()
        {
            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("RimworldTogether.SiteWillCost".Translate(sitePrices[DialogManager.selectedScrollButton]), RequestSiteBuild, null);

            DialogManager.PushNewDialog(d1);
        }

        public static void RequestSiteBuild()
        {
            DialogManager.PopDialog(DialogManager.dialogScrollButtons);

            if (!RimworldManager.CheckIfHasEnoughSilverInCaravan(sitePrices[DialogManager.selectedScrollButton]))
            {
                DialogManager.PushNewDialog(new RT_Dialog_Error("RimworldTogether.SilverLack".Translate()));
            }

            else
            {
                TransferManagerHelper.RemoveThingFromCaravan(ThingDefOf.Silver, sitePrices[DialogManager.selectedScrollButton]);

                SiteDetailsJSON siteDetailsJSON = new SiteDetailsJSON();
                siteDetailsJSON.siteStep = ((int)CommonEnumerators.SiteStepMode.Build).ToString();
                siteDetailsJSON.tile = ClientValues.chosenCaravan.Tile.ToString();
                siteDetailsJSON.type = DialogManager.selectedScrollButton.ToString();
                siteDetailsJSON.isFromFaction = true;

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteDetailsJSON);
                Network.listener.EnqueuePacket(packet);

                DialogManager.PushNewDialog(new RT_Dialog_Wait("RimworldTogether.IdleConstruction".Translate()));
            }
        }
    }
}
