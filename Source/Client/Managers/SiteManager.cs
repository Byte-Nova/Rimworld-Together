﻿using System;
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

        public static string[] siteRewardDefNames;

        public static void SetSiteDetails(ServerOverallJSON serverOverallJSON)
        {
            siteRewardDefNames = new string[]
            {
                ThingDefOf.RawPotatoes.defName,
                ThingDefOf.Steel.defName,
                ThingDefOf.WoodLog.defName,
                ThingDefOf.Silver.defName,
                ThingDefOf.ComponentIndustrial.defName,
                ThingDefOf.Chemfuel.defName,
                ThingDefOf.MedicineHerbal.defName,
                ThingDefOf.Cloth.defName,
                ThingDefOf.MealSimple.defName
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
                Logs.Warning("Server didn't have site rewards set, defaulting to 0");

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
            DialogManager.PopDialog();
            DialogManager.PushNewDialog(new RT_Dialog_OK("The desired site has been built!", DialogManager.clearStack));

            SaveManager.ForceSave();
        }

        public static void OnSimpleSiteRequest()
        {
            DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for site information"));

            SiteDetailsJSON siteDetailsJSON = new SiteDetailsJSON();
            siteDetailsJSON.tile = ClientValues.chosenSite.Tile.ToString();
            siteDetailsJSON.siteStep = ((int)CommonEnumerators.SiteStepMode.Info).ToString();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteDetailsJSON);
            Network.listener.EnqueuePacket(packet);
        }

        public static void OnSimpleSiteOpen(SiteDetailsJSON siteDetailsJSON)
        {
            DialogManager.PopDialog();

            if (siteDetailsJSON.workerData == null)
            {
                RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("There is no current worker on this site, send?", 
                    delegate { PrepareSendPawnScreen(); }, null);

                DialogManager.PushNewDialog(d1);
            }

            else
            {
                RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("You have a worker on this site, retrieve?",
                    delegate { DialogManager.clearStack(); RequestWorkerRetrieval(siteDetailsJSON); }, null);

                DialogManager.PushNewDialog(d1);
            }
        }

        private static void RequestWorkerRetrieval(SiteDetailsJSON siteDetailsJSON)
        {
            DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for site worker"));

            siteDetailsJSON.siteStep = ((int)CommonEnumerators.SiteStepMode.Retrieve).ToString();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteDetailsJSON);
            Network.listener.EnqueuePacket(packet);
        }

        private static void OnWorkerRetrieval(SiteDetailsJSON siteDetailsJSON)
        {
            DialogManager.PopDialog();

            Action r1 = delegate
            {
                DialogManager.clearStack();
                Pawn pawnToRetrieve = HumanScribeManager.StringToHuman((HumanDetailsJSON)Serializer.
                    ConvertBytesToObject(siteDetailsJSON.workerData));

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
                if (TransferManagerHelper.CheckIfThingIsHuman(pawn)) pawnNames.Add(pawn.Label);
            }

            RT_Dialog_ListingWithButton d1 = new RT_Dialog_ListingWithButton("Pawn Selection", "Select the pawn you wish to send", 
                pawnNames.ToArray(), 
                delegate { 
                    DialogManager.setInputReserve(); 
                    DialogManager.clearStack(); 
                    SendPawnToSite(); 
                });

            DialogManager.PushNewDialog(d1);
        }

        public static void SendPawnToSite()
        {
            DialogManager.PopDialog();
            List<Pawn> caravanPawns = ClientValues.chosenCaravan.PawnsListForReading;
            List<Pawn> caravanHumans = new List<Pawn>();
            foreach (Pawn pawn in caravanPawns)
            {
                if (TransferManagerHelper.CheckIfThingIsHuman(pawn)) caravanHumans.Add(pawn);
            }

            Pawn pawnToSend = caravanHumans[(int)DialogManager.inputReserve[0]];
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
                DialogManager.clearStack();
            };

            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("Are you sure you want to destroy this site?", r1, null);
            DialogManager.PushNewDialog(d1);
        }

        private static void OnWorkerError()
        {
            DialogManager.PushNewDialog(new RT_Dialog_Error("The site has a worker inside!"));
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
                        itemDetailsJSON.defName = siteRewardDefNames[i];
                        itemDetailsJSON.quantity = siteRewardCount[i];
                        itemDetailsJSON.quality = "null";

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
                Logs.Warning("Server didn't have personal site prices set, defaulting to 0");

                sitePrices = new int[9]
                {
                    0, 0, 0, 0, 0, 0, 0, 0, 0
                };
            }
        }

        public static void PushConfirmSiteDialog()
        {
            DialogManager.setInputReserve();
            
            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo($"This site will cost you {sitePrices[(int)DialogManager.inputReserve[0]]} " +
                $"silver, continue?", RequestSiteBuild, null);

            DialogManager.PushNewDialog(d1);
        }

        public static void RequestSiteBuild()
        {

            if (!RimworldManager.CheckIfHasEnoughSilverInCaravan(sitePrices[(int)DialogManager.inputReserve[0]]))
            {
                DialogManager.PushNewDialog(new RT_Dialog_Error("You do not have enough silver!", DialogManager.clearStack));
            }
            else
            {

                TransferManagerHelper.RemoveThingFromCaravan(ThingDefOf.Silver, sitePrices[(int)DialogManager.inputReserve[0]]);

                SiteDetailsJSON siteDetailsJSON = new SiteDetailsJSON();
                siteDetailsJSON.siteStep = ((int)CommonEnumerators.SiteStepMode.Build).ToString();
                siteDetailsJSON.tile = ClientValues.chosenCaravan.Tile.ToString();
                siteDetailsJSON.type = ((int)DialogManager.inputReserve[0]).ToString();
                siteDetailsJSON.isFromFaction = false;

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteDetailsJSON);
                Network.listener.EnqueuePacket(packet);

                DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for building"));
            }

            Logs.Message("point 4");
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
                Logs.Warning("Server didn't have faction site prices set, defaulting to 0");

                sitePrices = new int[9]
                {
                    0, 0, 0, 0, 0, 0, 0, 0, 0
                };
            }
        }

        public static void PushConfirmSiteDialog()
        {
            DialogManager.setInputReserve();

            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo($"This site will cost you {sitePrices[(int)DialogManager.inputReserve[0]]} " +
                $"silver, continue?", RequestSiteBuild, null);

            DialogManager.PushNewDialog(d1);
        }

        public static void RequestSiteBuild()
        {
            DialogManager.PopDialog();

            if (!RimworldManager.CheckIfHasEnoughSilverInCaravan(sitePrices[(int)DialogManager.inputReserve[0]]))
            {
                DialogManager.PushNewDialog(new RT_Dialog_Error("You do not have enough silver!"));
            }

            else
            {
                TransferManagerHelper.RemoveThingFromCaravan(ThingDefOf.Silver, sitePrices[(int)DialogManager.inputReserve[0]]);

                SiteDetailsJSON siteDetailsJSON = new SiteDetailsJSON();
                siteDetailsJSON.siteStep = ((int)CommonEnumerators.SiteStepMode.Build).ToString();
                siteDetailsJSON.tile = ClientValues.chosenCaravan.Tile.ToString();
                siteDetailsJSON.type = ((int)DialogManager.inputCache[0]).ToString();
                siteDetailsJSON.isFromFaction = true;

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteDetailsJSON);
                Network.listener.EnqueuePacket(packet);

                DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for building"));
            }
        }
    }
}
