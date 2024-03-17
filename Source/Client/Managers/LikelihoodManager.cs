using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;


namespace GameClient
{
    //Class that handles settlement and site player likelihoods

    public static class LikelihoodManager
    {
        //Tries to request a likelihood change depending on the values given

        public static void TryRequestLikelihood(CommonEnumerators.Likelihoods type, CommonEnumerators.LikelihoodTarget target)
        {
            int tileToUse = 0;
            if (target == CommonEnumerators.LikelihoodTarget.Settlement) tileToUse = ClientValues.chosenSettlement.Tile;
            else if (target == CommonEnumerators.LikelihoodTarget.Site) tileToUse = ClientValues.chosenSite.Tile;

            Faction factionToUse = null;
            if (target == CommonEnumerators.LikelihoodTarget.Settlement) factionToUse = ClientValues.chosenSettlement.Faction;
            else if (target == CommonEnumerators.LikelihoodTarget.Site) factionToUse = ClientValues.chosenSite.Faction;

            if (type == CommonEnumerators.Likelihoods.Enemy)
            {
                if (factionToUse == FactionValues.enemyPlayer)
                {
                    RT_Dialog_Error d1 = new RT_Dialog_Error("Chosen settlement is already marked as enemy!");
                    DialogManager.PushNewDialog(d1);
                }
                else RequestChangeStructureLikelihood(tileToUse, 0);
            }

            else if (type == CommonEnumerators.Likelihoods.Neutral)
            {
                if (factionToUse == FactionValues.neutralPlayer)
                {
                    RT_Dialog_Error d1 = new RT_Dialog_Error("Chosen settlement is already marked as neutral!");
                    DialogManager.PushNewDialog(d1);
                }
                else RequestChangeStructureLikelihood(tileToUse, 1);
            }

            else if (type == CommonEnumerators.Likelihoods.Ally)
            {
                if (factionToUse == FactionValues.allyPlayer)
                {
                    RT_Dialog_Error d1 = new RT_Dialog_Error("Chosen settlement is already marked as ally!");
                    DialogManager.PushNewDialog(d1);
                }
                else RequestChangeStructureLikelihood(tileToUse, 2);
            }
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
