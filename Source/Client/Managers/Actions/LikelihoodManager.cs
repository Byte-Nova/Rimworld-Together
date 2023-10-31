using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimworldTogether.GameClient.Dialogs;
using RimworldTogether.GameClient.Misc;
using RimworldTogether.GameClient.Planet;
using RimworldTogether.GameClient.Values;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Misc;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using Verse;

namespace RimworldTogether.GameClient.Managers.Actions
{
    public static class LikelihoodManager
    {
        public enum Likelihoods { Enemy, Neutral, Ally, Faction, Personal }

        public enum LikelihoodTarget { Settlement, Site }

        public static void TryRequestLikelihood(Likelihoods type, LikelihoodTarget target)
        {
            int tileToUse = 0;
            if (target == LikelihoodTarget.Settlement) tileToUse = ClientValues.chosenSettlement.Tile;
            else if (target == LikelihoodTarget.Site) tileToUse = ClientValues.chosenSite.Tile;

            Faction factionToUse = null;
            if (target == LikelihoodTarget.Settlement) factionToUse = ClientValues.chosenSettlement.Faction;
            else if (target == LikelihoodTarget.Site) factionToUse = ClientValues.chosenSite.Faction;

            if (type == Likelihoods.Enemy)
            {
                if (factionToUse == PlanetFactions.enemyPlayer)
                {
                    RT_Dialog_Error d1 = new RT_Dialog_Error("Chosen settlement is already marked as enemy!");
                    DialogManager.PushNewDialog(d1);
                }
                else RequestChangeStructureLikelihood(tileToUse, 0);
            }

            else if (type == Likelihoods.Neutral)
            {
                if (factionToUse == PlanetFactions.neutralPlayer)
                {
                    RT_Dialog_Error d1 = new RT_Dialog_Error("Chosen settlement is already marked as neutral!");
                    DialogManager.PushNewDialog(d1);
                }
                else RequestChangeStructureLikelihood(tileToUse, 1);
            }

            else if (type == Likelihoods.Ally)
            {
                if (factionToUse == PlanetFactions.allyPlayer)
                {
                    RT_Dialog_Error d1 = new RT_Dialog_Error("Chosen settlement is already marked as ally!");
                    DialogManager.PushNewDialog(d1);
                }
                else RequestChangeStructureLikelihood(tileToUse, 2);
            }
        }

        public static void ChangeStructureLikelihood(Packet packet)
        {
            StructureLikelihoodJSON structureLikelihoodJSON = (StructureLikelihoodJSON)ObjectConverter.ConvertBytesToObject(packet.contents);
            ChangeSettlementLikelihoods(structureLikelihoodJSON);
            ChangeSiteLikelihoods(structureLikelihoodJSON);
        }

        private static void ChangeSettlementLikelihoods(StructureLikelihoodJSON structureLikelihoodJSON)
        {
            Action toDo = delegate
            {
                List<Settlement> toChange = new List<Settlement>();
                foreach (string settlementTile in structureLikelihoodJSON.settlementTiles)
                {
                    toChange.Add(Find.WorldObjects.Settlements.Find(x => x.Tile == int.Parse(settlementTile)));
                }

                for(int i = 0; i < toChange.Count(); i++)
                {
                    PlanetBuilder.playerSettlements.Remove(toChange[i]);
                    Find.WorldObjects.Remove(toChange[i]);

                    Settlement newSettlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                    newSettlement.Tile = toChange[i].Tile;
                    newSettlement.Name = toChange[i].Name;
                    newSettlement.SetFaction(PlanetBuilder.GetPlayerFaction(int.Parse(structureLikelihoodJSON.settlementLikelihoods[i])));

                    PlanetBuilder.playerSettlements.Add(newSettlement);
                    Find.WorldObjects.Add(newSettlement);
                }
            };
            toDo.Invoke();
        }

        private static void ChangeSiteLikelihoods(StructureLikelihoodJSON structureLikelihoodJSON)
        {
            Action toDo = delegate
            {
                List<Site> toChange = new List<Site>();
                foreach (string siteTile in structureLikelihoodJSON.siteTiles)
                {
                    toChange.Add(Find.WorldObjects.Sites.Find(x => x.Tile == int.Parse(siteTile)));
                }

                for (int i = 0; i < toChange.Count(); i++)
                {
                    PlanetBuilder.playerSites.Remove(toChange[i]);
                    Find.WorldObjects.Remove(toChange[i]);

                    Site newSite = SiteMaker.MakeSite(sitePart: toChange[i].MainSitePartDef,
                                tile: toChange[i].Tile,
                                threatPoints: 1000,
                                faction: PlanetBuilder.GetPlayerFaction(int.Parse(structureLikelihoodJSON.siteLikelihoods[i])));

                    PlanetBuilder.playerSites.Add(newSite);
                    Find.WorldObjects.Add(newSite);
                }
            };
            toDo.Invoke();
        }

        public static void RequestChangeStructureLikelihood(int structureTile, int value)
        {
            RT_Dialog_Wait d1 = new RT_Dialog_Wait("Changing settlement likelihood");
            DialogManager.PushNewDialog(d1);

            StructureLikelihoodJSON structureLikelihoodJSON = new StructureLikelihoodJSON();
            structureLikelihoodJSON.tile = structureTile.ToString();
            structureLikelihoodJSON.likelihood = value.ToString();

            Packet packet = Packet.CreatePacketFromJSON("LikelihoodPacket", structureLikelihoodJSON);
            Network.Network.serverListener.SendData(packet);
        }
    }
}
