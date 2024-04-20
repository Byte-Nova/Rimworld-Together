using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    //Class that handles settlement and site player likelihoods

    public static class LikelihoodManager
    {

    public static Dictionary<Likelihoods, Faction> likelihoodToFaction = new()
        {
          {Likelihoods.Enemy, FactionValues.enemyPlayer },
          {Likelihoods.Neutral, FactionValues.neutralPlayer },
          {Likelihoods.Ally, FactionValues.allyPlayer }
        };



        //Tries to request a likelihood change depending on the values given
        public static void TryRequestLikelihood(Likelihoods likelihood, LikelihoodTarget target)
        {

            int tileToUse = 0;
            if (target == LikelihoodTarget.Settlement) tileToUse = ClientValues.chosenSettlement.Tile;
            else if (target == LikelihoodTarget.Site) tileToUse = ClientValues.chosenSite.Tile;

            Faction factionToUse = null;
            if (target == LikelihoodTarget.Settlement) factionToUse = ClientValues.chosenSettlement.Faction;
            else if (target == LikelihoodTarget.Site) factionToUse = ClientValues.chosenSite.Faction;

            if(factionToUse == likelihoodToFaction[likelihood])
             DialogManager.PushNewDialog(new RT_Dialog_Error("Chosen settlement is already marked as enemy!"));
            else
              RequestChangeStructureLikelihood(tileToUse, (int)likelihood);

        }

        //Requests a structure likelihood change to the server

        public static void RequestChangeStructureLikelihood(int structureTile, int value)
        {
            StructureLikelihoodJSON structureLikelihoodJSON = new StructureLikelihoodJSON();
            structureLikelihoodJSON.tile = structureTile.ToString();
            structureLikelihoodJSON.likelihood = value.ToString();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.LikelihoodPacket), structureLikelihoodJSON);
            Network.listener.EnqueuePacket(packet);

            RT_Dialog_Wait d1 = new RT_Dialog_Wait("Changing settlement likelihood");
            DialogManager.PushNewDialog(d1);
        }

        //Changes a structure likelihood from a packet

        public static void ChangeStructureLikelihood(Packet packet)
        {
            StructureLikelihoodJSON structureLikelihoodJSON = (StructureLikelihoodJSON)Serializer.ConvertBytesToObject(packet.contents);
            ChangeSettlementLikelihoods(structureLikelihoodJSON);
            ChangeSiteLikelihoods(structureLikelihoodJSON);
        }

        //Changes a settlement likelihood from a request

        private static void ChangeSettlementLikelihoods(StructureLikelihoodJSON structureLikelihoodJSON)
        {
            List<Settlement> toChange = new List<Settlement>();
            foreach (string settlementTile in structureLikelihoodJSON.settlementTiles)
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
                newSettlement.SetFaction(PlanetManagerHelper.GetPlayerFaction(int.Parse(structureLikelihoodJSON.settlementLikelihoods[i])));

                PlanetManager.playerSettlements.Add(newSettlement);
                Find.WorldObjects.Add(newSettlement);
            }
        }

        //Changes a site likelihood from a request

        private static void ChangeSiteLikelihoods(StructureLikelihoodJSON structureLikelihoodJSON)
        {
            List<Site> toChange = new List<Site>();
            foreach (string siteTile in structureLikelihoodJSON.siteTiles)
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
                            faction: PlanetManagerHelper.GetPlayerFaction(int.Parse(structureLikelihoodJSON.siteLikelihoods[i])));

                PlanetManager.playerSites.Add(newSite);
                Find.WorldObjects.Add(newSite);
            }
        }
    }
}
