using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GameClient
{
    public static class WorldGenerationData
    {
        public static WorldGenStepDef[] WorldSyncSteps;

        public static void initializeGenerationDefs()
        {
            List<WorldGenStepDef> WorldSyncStepsList = new List<WorldGenStepDef>();

            WorldSyncStepsList.Add(new WorldGenStepDef());
            WorldSyncStepsList[WorldSyncStepsList.Count - 1].worldGenStep = new WorldGenStep_Terrain();

            WorldSyncStepsList.Add(new WorldGenStepDef());
            WorldSyncStepsList[WorldSyncStepsList.Count-1].worldGenStep = new WorldGenStep_Components();

            WorldSyncStepsList.Add(new WorldGenStepDef());
            WorldSyncStepsList[WorldSyncStepsList.Count - 1].worldGenStep = new WorldGenStep_Lakes();

            WorldSyncStepsList.Add(new WorldGenStepDef());
            WorldSyncStepsList[WorldSyncStepsList.Count - 1].worldGenStep = new WorldGenStep_Rivers();

            WorldSyncStepsList.Add(new WorldGenStepDef());
            WorldSyncStepsList[WorldSyncStepsList.Count - 1].worldGenStep = new WorldGenStep_AncientSites();

            WorldSyncStepsList.Add(new WorldGenStepDef());
            WorldSyncStepsList[WorldSyncStepsList.Count - 1].worldGenStep = new WorldGenStep_AncientRoads();


            if (ModLister.BiotechInstalled)
            {
                WorldSyncStepsList.Add(new WorldGenStepDef());
                WorldSyncStepsList[WorldSyncStepsList.Count-1].worldGenStep = new WorldGenStep_Pollution();
            }

            WorldSyncStepsList.Add(new WorldGenStepDef());
            WorldSyncStepsList[WorldSyncStepsList.Count-1].worldGenStep = new WorldGenStep_Factions();
            WorldSyncStepsList.Add(new WorldGenStepDef());
            WorldSyncStepsList[WorldSyncStepsList.Count-1].worldGenStep = new WorldGenStep_Roads();
            WorldSyncStepsList.Add(new WorldGenStepDef());
            WorldSyncStepsList[WorldSyncStepsList.Count-1].worldGenStep = new WorldGenStep_Features();
            WorldSyncSteps = WorldSyncStepsList.ToArray();
        }
    }
}
