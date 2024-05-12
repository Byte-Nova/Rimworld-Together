using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;


namespace GameClient
{
    //Class that handles settlement and site player goodwills

    public static class GoodwillManager
    {
        //Tries to request a goodwill change depending on the values given

        public static void TryRequestGoodwill(CommonEnumerators.Goodwills type, CommonEnumerators.GoodwillTarget target)
        {
            int tileToUse = 0;
            if (target == CommonEnumerators.GoodwillTarget.Settlement) tileToUse = ClientValues.chosenSettlement.Tile;
            else if (target == CommonEnumerators.GoodwillTarget.Site) tileToUse = ClientValues.chosenSite.Tile;

            Faction factionToUse = null;
            if (target == CommonEnumerators.GoodwillTarget.Settlement) factionToUse = ClientValues.chosenSettlement.Faction;
            else if (target == CommonEnumerators.GoodwillTarget.Site) factionToUse = ClientValues.chosenSite.Faction;

            if (type == CommonEnumerators.Goodwills.Enemy)
            {
                if (factionToUse == FactionValues.enemyPlayer)
                {
                    RT_Dialog_Error d1 = new RT_Dialog_Error("Chosen settlement is already marked as enemy!");
                    DialogManager.PushNewDialog(d1);
                }
                else RequestChangeStructureGoodwill(tileToUse, 0);
            }

            else if (type == CommonEnumerators.Goodwills.Neutral)
            {
                if (factionToUse == FactionValues.neutralPlayer)
                {
                    RT_Dialog_Error d1 = new RT_Dialog_Error("Chosen settlement is already marked as neutral!");
                    DialogManager.PushNewDialog(d1);
                }
                else RequestChangeStructureGoodwill(tileToUse, 1);
            }

            else if (type == CommonEnumerators.Goodwills.Ally)
            {
                if (factionToUse == FactionValues.allyPlayer)
                {
                    RT_Dialog_Error d1 = new RT_Dialog_Error("Chosen settlement is already marked as ally!");
                    DialogManager.PushNewDialog(d1);
                }
                else RequestChangeStructureGoodwill(tileToUse, 2);
            }
        }

        //Requests a structure goodwill change to the server

        public static void RequestChangeStructureGoodwill(int structureTile, int value)
        {
            FactionGoodwillData factionGoodwillData = new FactionGoodwillData();
            factionGoodwillData.tile = structureTile;
            factionGoodwillData.goodwill = value.ToString();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.GoodwillPacket), factionGoodwillData);
            Network.listener.EnqueuePacket(packet);

            RT_Dialog_Wait d1 = new RT_Dialog_Wait("Changing settlement goodwill");
            DialogManager.PushNewDialog(d1);
        }

        //Changes a structure goodwill from a packet

        public static void ChangeStructureGoodwill(Packet packet)
        {
            FactionGoodwillData factionGoodwillData = (FactionGoodwillData)Serializer.ConvertBytesToObject(packet.contents);
            ChangeSettlementGoodwills(factionGoodwillData);
            ChangeSiteGoodwills(factionGoodwillData);
        }

        //Changes a settlement goodwill from a request

        private static void ChangeSettlementGoodwills(FactionGoodwillData factionGoodwillData)
        {
            List<Settlement> toChange = new List<Settlement>();
            foreach (int settlementTile in factionGoodwillData.settlementTiles)
            {
                toChange.Add(Find.WorldObjects.Settlements.Find(x => x.Tile == settlementTile));
            }

            for (int i = 0; i < toChange.Count(); i++)
            {
                PlanetManager.playerSettlements.Remove(toChange[i]);
                Find.WorldObjects.Remove(toChange[i]);

                Settlement newSettlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                newSettlement.Tile = toChange[i].Tile;
                newSettlement.Name = toChange[i].Name;
                newSettlement.SetFaction(PlanetManagerHelper.GetPlayerFaction(int.Parse(factionGoodwillData.settlementGoodwills[i])));

                PlanetManager.playerSettlements.Add(newSettlement);
                Find.WorldObjects.Add(newSettlement);
            }
        }

        //Changes a site goodwill from a request

        private static void ChangeSiteGoodwills(FactionGoodwillData factionGoodwillData)
        {
            List<Site> toChange = new List<Site>();
            foreach (int siteTile in factionGoodwillData.siteTiles)
            {
                toChange.Add(Find.WorldObjects.Sites.Find(x => x.Tile == siteTile));
            }

            for (int i = 0; i < toChange.Count(); i++)
            {
                PlanetManager.playerSites.Remove(toChange[i]);
                Find.WorldObjects.Remove(toChange[i]);

                Site newSite = SiteMaker.MakeSite(sitePart: toChange[i].MainSitePartDef,
                            tile: toChange[i].Tile,
                            threatPoints: 1000,
                            faction: PlanetManagerHelper.GetPlayerFaction(int.Parse(factionGoodwillData.siteGoodwills[i])));

                PlanetManager.playerSites.Add(newSite);
                Find.WorldObjects.Add(newSite);
            }
        }
    }
}
