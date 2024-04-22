using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    //Class that handles settlement and site player goodwills

    public static class GoodwillManager
    {

    public static Dictionary<Goodwills, Faction> goodwillToFaction = new()
        {
          {Goodwills.Enemy, FactionValues.enemyPlayer },
          {Goodwills.Neutral, FactionValues.neutralPlayer },
          {Goodwills.Ally, FactionValues.allyPlayer }
        };



        //Tries to request a likelihood change depending on the values given
        public static void TryRequestGoodwill(Goodwills goodwill, GoodwillTarget target)
        {

            int tileToUse = 0;
            if (target == GoodwillTarget.Settlement) tileToUse = ClientValues.chosenSettlement.Tile;
            else if (target == GoodwillTarget.Site) tileToUse = ClientValues.chosenSite.Tile;

            Faction factionToUse = null;
            if (target == GoodwillTarget.Settlement) factionToUse = ClientValues.chosenSettlement.Faction;
            else if (target == GoodwillTarget.Site) factionToUse = ClientValues.chosenSite.Faction;

            if(factionToUse == goodwillToFaction[goodwill])
             DialogManager.PushNewDialog(new RT_Dialog_Error($"Chosen settlement is already marked as {goodwill}!"));
            else
              RequestChangeStructureGoodwill(tileToUse, (int)goodwill);

        }

        //Requests a structure goodwill change to the server

        public static void RequestChangeStructureGoodwill(int structureTile, int value)
        {
            FactionGoodwillData factionGoodwillData = new FactionGoodwillData();
            factionGoodwillData.tile = structureTile.ToString();
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
            foreach (string settlementTile in factionGoodwillData.settlementTiles)
            {
                toChange.Add(Find.WorldObjects.Settlements.Find(x => x.Tile == int.Parse(settlementTile)));
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
            foreach (string siteTile in factionGoodwillData.siteTiles)
            {
                toChange.Add(Find.WorldObjects.Sites.Find(x => x.Tile == int.Parse(siteTile)));
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
