using System.Collections.Generic;
using System.Linq;
using GameClient.Dialogs;
using GameClient.Misc;
using GameClient.TCP;
using GameClient.Values;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;
using static Shared.CommonEnumerators;


namespace GameClient.Managers
{
    //Class that handles settlement and site player goodwills

    public static class GoodwillManager
    {
        [HandlesPacket(PacketHeader.GoodWillManager)]
        private static void ParsePacket(byte[] bytes)
        {
            FactionGoodwillData data = Serializer.ConvertBytesToObject<FactionGoodwillData>(bytes);

            Printer.Warning(data, LogImportanceMode.Extreme);

            ChangeStructureGoodwill(data);
            RT_Dialog_Wait.Instance.Close();
        }

        //Tries to request a goodwill change depending on the values given

        public static void TryRequestGoodwill(Goodwill type, GoodwillTarget target)
        {
            int tileToUse = 0;
            if (target == GoodwillTarget.Settlement) tileToUse = SessionValues.ChosenSettlement.Tile;
            else if (target == GoodwillTarget.Site) tileToUse = SessionValues.ChosenSite.Tile;

            Faction factionToUse = null;
            if (target == GoodwillTarget.Settlement) factionToUse = SessionValues.ChosenSettlement.Faction;
            else if (target == GoodwillTarget.Site) factionToUse = SessionValues.ChosenSite.Faction;

            if (type == Goodwill.Enemy)
            {
                if (factionToUse == ClientValues.EnemyPlayer)
                {
                    RT_Dialog_Message d1 = new RT_Dialog_Message("ERROR", new string[] { "Chosen settlement is already marked as enemy!" });
                    RT_Dialog_Base.PushNewDialog(d1);
                }
                else RequestChangeStructureGoodwill(tileToUse, Goodwill.Enemy);
            }

            else if (type == Goodwill.Neutral)
            {
                if (factionToUse == ClientValues.NeutralPlayer)
                {
                    RT_Dialog_Message d1 = new RT_Dialog_Message("ERROR", new string[] { "Chosen settlement is already marked as neutral!" });
                    RT_Dialog_Base.PushNewDialog(d1);
                }
                else RequestChangeStructureGoodwill(tileToUse, Goodwill.Neutral);
            }

            else if (type == Goodwill.Ally)
            {
                if (factionToUse == ClientValues.AllyPlayer)
                {
                    RT_Dialog_Message d1 = new RT_Dialog_Message("ERROR", new string[] { "Chosen settlement is already marked as ally!" });
                    RT_Dialog_Base.PushNewDialog(d1);
                }
                else RequestChangeStructureGoodwill(tileToUse, Goodwill.Ally);
            }
        }

        //Requests a structure goodwill change to the server

        public static void RequestChangeStructureGoodwill(int structureTile, Goodwill goodwill)
        {
            FactionGoodwillData factionGoodwillData = new FactionGoodwillData();
            factionGoodwillData._tile = structureTile;
            factionGoodwillData._goodwill = goodwill;

            Network.Listener.EnqueuePacket(PacketHeader.GoodWillManager, factionGoodwillData);

            RT_Dialog_Wait d1 = new RT_Dialog_Wait("Changing settlement goodwill");
            RT_Dialog_Base.PushNewDialog(d1);
        }

        //Changes a structure goodwill from a packet

        public static void ChangeStructureGoodwill(FactionGoodwillData data)
        {
            ChangeSettlementGoodwills(data);
            ChangeSiteGoodwills(data);
        }

        //Changes a settlement goodwill from a request

        private static void ChangeSettlementGoodwills(FactionGoodwillData factionGoodwillData)
        {
            List<Settlement> toChange = new List<Settlement>();
            foreach (int settlementTile in factionGoodwillData._settlementTiles)
            {
                Settlement settlement = Find.WorldObjects.Settlements.Find(x => x.Tile == settlementTile);
                if (settlement.Faction == Faction.OfPlayer) continue;
                else toChange.Add(settlement);
            }

            for (int i = 0; i < toChange.Count(); i++)
            {
                SettlementManager.PlayerSettlements.Remove(toChange[i]);
                Find.WorldObjects.Remove(toChange[i]);

                Settlement newSettlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                newSettlement.Tile = toChange[i].Tile;
                newSettlement.Name = toChange[i].Name;
                newSettlement.SetFaction(PlanetManagerHelper.GetPlayerFactionFromGoodwill(factionGoodwillData._settlementGoodwills[i]));

                SettlementManager.PlayerSettlements.Add(newSettlement);
                Find.WorldObjects.Add(newSettlement);
            }
        }

        //Changes a site goodwill from a request

        private static void ChangeSiteGoodwills(FactionGoodwillData factionGoodwillData)
        {
            List<Site> toChange = new List<Site>();
            foreach (int siteTile in factionGoodwillData._siteTiles) { toChange.Add(Find.WorldObjects.Sites.Find(x => x.Tile == siteTile)); }

            for (int i = 0; i < toChange.Count(); i++)
            {
                SiteManager.PlayerSites.Remove(toChange[i]);
                Find.WorldObjects.Remove(toChange[i]);

                Site newSite = SiteMaker.MakeSite(sitePart: toChange[i].MainSitePartDef,
                            tile: toChange[i].Tile,
                            threatPoints: 1000,
                            faction: PlanetManagerHelper.GetPlayerFactionFromGoodwill(factionGoodwillData._siteGoodwills[i]));

                SiteManager.PlayerSites.Add(newSite);
                Find.WorldObjects.Add(newSite);
            }
        }
    }
}
