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

        public static SiteInfoFile[] siteData;

        public static float interval;
        public static void SetValues(ServerGlobalData serverGlobalData)
        {
            siteData = serverGlobalData._siteValues.SiteInfoFiles;
            foreach(SiteInfoFile site in serverGlobalData._siteValues.SiteInfoFiles) 
            {
                if(site.overrideDescription != "") 
                {
                    siteDefs.Where(S => S.defName == site.DefName).FirstOrDefault().description = site.overrideDescription;
                }
                if (site.overrideName != "")
                {
                    siteDefs.Where(S => S.defName == site.DefName).FirstOrDefault().label = site.overrideName;
                }
            }
            interval = serverGlobalData._siteValues.TimeIntervalMinute;
        }

        public static void SetSiteDefs()
        {
            List<SitePartDef> defs = new List<SitePartDef>();
            defs.Add(DefDatabase<SitePartDef>.GetNamed("RTFarmland"));
            defs.Add(DefDatabase<SitePartDef>.GetNamed("RTQuarry"));
            defs.Add(DefDatabase<SitePartDef>.GetNamed("RTSawmill"));
            defs.Add(DefDatabase<SitePartDef>.GetNamed("RTBank"));
            defs.Add(DefDatabase<SitePartDef>.GetNamed("RTLaboratory"));
            defs.Add(DefDatabase<SitePartDef>.GetNamed("RTRefinery"));
            defs.Add(DefDatabase<SitePartDef>.GetNamed("RTHerbalWorkshop"));
            defs.Add(DefDatabase<SitePartDef>.GetNamed("RTTextileFactory"));
            defs.Add(DefDatabase<SitePartDef>.GetNamed("RTFoodProcessor"));
            siteDefs = defs.ToArray();
        }

        public static SitePartDef GetDefForNewSite(string siteDef)
        {
            return siteDefs.Where(S => S.defName == siteDef).First();
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
                    SpawnSingleSite(siteData._siteFile);
                    break;

                case SiteStepMode.Destroy:
                    RemoveSingleSite(siteData._siteFile);
                    break;
            }
        }
        private static void OnSiteAccept()
        {
            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_OK("The desired site has been built!"));

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

            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("Are you sure you want to destroy this site?", r1, null);
            DialogManager.PushNewDialog(d1);
        }

        public static List<Site> playerSites = new List<Site>();

        public static void AddSites(SiteIdendity[] sites)
        {
            foreach (SiteIdendity toAdd in sites)
            {
                SpawnSingleSite(toAdd);
            }
        }

        public static void ClearAllSites()
        {
            Site[] sites = Find.WorldObjects.Sites.Where(fetch => FactionValues.playerFactions.Contains(fetch.Faction) ||
                fetch.Faction == Faction.OfPlayer).ToArray();

            foreach (Site toRemove in sites)
            {
                SiteIdendity siteFile = new SiteIdendity();
                siteFile.Tile = toRemove.Tile;
                RemoveSingleSite(siteFile);
            }
        }

        public static void SpawnSingleSite(SiteIdendity toAdd)
        {
            if (Find.WorldObjects.Sites.FirstOrDefault(fetch => fetch.Tile == toAdd.Tile) != null) return;
            else
            {
                try
                {
                    SitePartDef siteDef = SiteManager.GetDefForNewSite(toAdd.Type.DefName);
                    Site site = SiteMaker.MakeSite(sitePart: siteDef,
                        tile: toAdd.Tile,
                        threatPoints: 1000,
                        faction: PlanetManagerHelper.GetPlayerFactionFromGoodwill(toAdd.Goodwill));
                    
                    playerSites.Add(site);
                    Find.WorldObjects.Add(site);
                }
                catch (Exception e) { Logger.Error($"Failed to spawn site at {toAdd.Tile}. Reason: {e}"); }
            }
        }

        public static void RemoveSingleSite(SiteIdendity toRemove)
        {
            try
            {
                Site toGet = Find.WorldObjects.Sites.Find(fetch => fetch.Tile == toRemove.Tile);
                if (!RimworldManager.CheckIfMapHasPlayerPawns(toGet.Map))
                {
                    if (playerSites.Contains(toGet)) playerSites.Remove(toGet);
                    Find.WorldObjects.Remove(toGet);
                }
                else Logger.Warning($"Ignored removal of site at {toGet.Tile} because player was inside");
            }
            catch (Exception e) { Logger.Error($"Failed to remove site at {toRemove.Tile}. Reason: {e}"); }
        }

        public static void RequestSiteBuild(SiteInfoFile configFile)
        {
            bool shouldCancel = false;
            for (int i = 0;i < configFile.DefNameCost.Length;i++) {
                if (!RimworldManager.CheckIfHasEnoughItemInCaravan(SessionValues.chosenCaravan, configFile.DefNameCost[i], configFile.Cost[i]))
                {
                    shouldCancel = true;
                    DialogManager.PushNewDialog(new RT_Dialog_Error("You do not have enough silver!"));
                }
            }
            if(!shouldCancel)
            {
                for (int i = 0; i < configFile.DefNameCost.Length; i++) 
                    RimworldManager.RemoveThingFromCaravan(SessionValues.chosenCaravan, DefDatabase<ThingDef>.GetNamed(configFile.DefNameCost[i]), configFile.Cost[i]);

                SiteData siteData = new SiteData();
                siteData._stepMode = SiteStepMode.Build;
                siteData._siteFile.Tile = SessionValues.chosenCaravan.Tile;
                siteData._siteFile.Type.DefName = configFile.DefName;
                if (ServerValues.hasFaction) siteData._siteFile.FactionFile = new FactionFile();
                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SitePacket), siteData);
                Network.listener.EnqueuePacket(packet);

                DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for building"));
            }
        }

        public static void ChangeConfig(SiteInfoFile config, string reward) 
        {
            SiteData siteData = new SiteData();
            siteData._stepMode = SiteStepMode.Config;
            siteData._siteFile.ChosenReward = reward;
            siteData._siteFile.Type = config;
            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SitePacket), siteData);
            Network.listener.EnqueuePacket(packet);
        }
    }
}

public static class PlayerSiteManagerHelper
{
    public static SiteIdendity[] tempSites;

    public static void SetValues(ServerGlobalData serverGlobalData)
    {
        tempSites = serverGlobalData._playerSites;
    }
}


