using System;
using System.Linq;
using Verse;
using RimWorld.Planet;
using RimWorld;
using UnityEngine.Networking.Types;
using Shared;
using System.Collections.Generic;
using UnityEngine;

namespace GameClient
{
	public class WorldGenStep_Factions : WorldGenStep
	{

        private const int MaxPreferredFactionNameLength = 20;

        private static readonly FloatRange SettlementsPer100kTiles = new FloatRange(75f, 85f);

        public override int SeedPart
		{
			get
			{
				return 777998381;
			}
		}

		public override void GenerateFresh(string seed)
		{
            //If this is the first generation, run the vanilla Factions generations step code
            if (WorldGeneratorManager.firstGeneration)
            {
                FactionGenerator.GenerateFactionsIntoWorld(Current.CreatingWorld.info.factions);

                return;
            }
            Log.Message("Adding Factions");

            //Add Factions to the faction manager using their FactionDefs
			List<FactionDef> factions = Current.CreatingWorld.info.factions;
            if (factions != null)
            {
                foreach (FactionDef faction2 in factions)
                {
                    AddFactionToManager(faction2);
                }
            }
            Log.Message("adding settlements");

            //Get list of all factions
            IEnumerable<Faction> factionList = Find.World.factionManager.AllFactionsListForReading.Where((Faction x) => !x.def.isPlayer && !x.Hidden && !x.temporary);

            //Deserialize FactionData Dictionary
            IEnumerable<FactionData> factionDatas = WorldGeneratorManager.cachedWorldData.factions.Values.ToList().ConvertAll(x => (FactionData)Serializer.ConvertBytesToObject(x));

            Log.Message(factionDatas.ToList().Count());

            //For each faction, add all the settlements to the world
            if (factionList.Any())
            {
                Log.Message("There are some");
                List<SettlementData> settlementDatas = WorldGeneratorManager.cachedWorldData.SettlementDatas.ConvertAll(x => (SettlementData)Serializer.ConvertBytesToObject(x));
                Log.Message(settlementDatas.Count());

                Log.Message("faction list:");
                foreach (Faction faction in factionList)
                {
                    Log.Message(faction.def.defName);
                }

                foreach (SettlementData settlementData in settlementDatas)
                {
                    Log.Message(settlementData.settlementName);
                    Log.Message(settlementData.owner);
                    Faction faction = factionList.FirstOrDefault(fetch => fetch.def.defName == settlementData.owner);

                    Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                    if (faction == null) Log.Message("No faction");
                    settlement.SetFaction(faction);
                    if(settlement.Faction.color == null) Log.Message("not exist");
                    if (settlement.Faction.def.settlementTexturePath == null) Log.Message("No settlement texture");
                    settlement.Tile = settlementData.tile;
                    settlement.Name = settlementData.settlementName;
                    Find.WorldObjects.Add(settlement);
                }
            }
            Find.IdeoManager.SortIdeos();

        }

		public override void GenerateWithoutWorldData(string seed)
		{
		}

        private static void AddFactionToManager(FactionDef facDef)
        {
            if (facDef.fixedIdeo)
            {
                IdeoGenerationParms ideoGenerationParms = new IdeoGenerationParms(facDef, forceNoExpansionIdeo: false, null, null, name: facDef.ideoName, styles: facDef.styles, deities: facDef.deityPresets, hidden: facDef.hiddenIdeo, description: facDef.ideoDescription, forcedMemes: facDef.forcedMemes, classicExtra: false, forceNoWeaponPreference: false, forNewFluidIdeo: false, fixedIdeo: true, requiredPreceptsOnly: facDef.requiredPreceptsOnly);
                Find.FactionManager.Add(NewGeneratedFaction(new FactionGeneratorParms(facDef, ideoGenerationParms)));
            }
            else
            {
                Find.FactionManager.Add(NewGeneratedFaction(new FactionGeneratorParms(facDef)));
            }
        }

        public static Faction NewGeneratedFaction(FactionGeneratorParms parms)
        {
            FactionDef factionDef = parms.factionDef;
            parms.ideoGenerationParms.forFaction = factionDef;
            Faction faction = new Faction();
            faction.def = factionDef;
            faction.loadID = Find.UniqueIDsManager.GetNextFactionID();
            faction.colorFromSpectrum = NewRandomColorFromSpectrum(faction);
            faction.hidden = parms.hidden;
            if (factionDef.humanlikeFaction)
            {
                faction.ideos = new FactionIdeosTracker(faction);
                if (!faction.IsPlayer || !ModsConfig.IdeologyActive || !Find.GameInitData.startedFromEntry)
                {
                    faction.ideos.ChooseOrGenerateIdeo(parms.ideoGenerationParms);
                }
            }
            if (!factionDef.isPlayer)
            {
                if (factionDef.fixedName != null)
                {
                    faction.Name = factionDef.fixedName;
                }
                else
                {
                    string text = "";
                    for (int i = 0; i < 10; i++)
                    {
                        string text2 = NameGenerator.GenerateName(faction.def.factionNameMaker, Find.FactionManager.AllFactionsVisible.Select((Faction fac) => fac.Name));
                        if (text2.Length <= 20)
                        {
                            text = text2;
                        }
                    }
                    if (text.NullOrEmpty())
                    {
                        text = NameGenerator.GenerateName(faction.def.factionNameMaker, Find.FactionManager.AllFactionsVisible.Select((Faction fac) => fac.Name));
                    }
                    faction.Name = text;
                }
            }
            foreach (Faction item in Find.FactionManager.AllFactionsListForReading)
            {
                faction.TryMakeInitialRelationsWith(item);
            }
            if (!faction.Hidden && !factionDef.isPlayer)
            {
                Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                settlement.SetFaction(faction);
                settlement.Tile = TileFinder.RandomSettlementTileFor(faction);
                settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement);
                Find.WorldObjects.Add(settlement);
            }
            faction.TryGenerateNewLeader();
            return faction;
        }

        public static Faction NewGeneratedFactionWithRelations(FactionDef facDef, List<FactionRelation> relations, bool hidden = false)
        {
            return NewGeneratedFactionWithRelations(new FactionGeneratorParms(facDef, default(IdeoGenerationParms), hidden), relations);
        }

        public static Faction NewGeneratedFactionWithRelations(FactionGeneratorParms parms, List<FactionRelation> relations)
        {
            Faction faction = NewGeneratedFaction(parms);
            for (int i = 0; i < relations.Count; i++)
            {
                faction.SetRelation(relations[i]);
            }
            return faction;
        }

        public static float NewRandomColorFromSpectrum(Faction faction)
        {
            float num = -1f;
            float result = 0f;
            for (int i = 0; i < 20; i++)
            {
                float value = Rand.Value;
                float num2 = 1f;
                List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
                for (int j = 0; j < allFactionsListForReading.Count; j++)
                {
                    Faction faction2 = allFactionsListForReading[j];
                    if (faction2.def == faction.def)
                    {
                        float num3 = Mathf.Abs(value - faction2.colorFromSpectrum);
                        if (num3 < num2)
                        {
                            num2 = num3;
                        }
                    }
                }
                if (num2 > num)
                {
                    num = num2;
                    result = value;
                }
            }
            return result;
        }
    }
}
