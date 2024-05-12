using HarmonyLib;
using RimWorld.Planet;
using Shared;
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
        public static WorldData worldData;

        public static List<WorldGenStepDef> RT_WorldGenSteps;

        public static IEnumerable<WorldGenStepDef> GenStepsInOrder => from x in DefDatabase<WorldGenStepDef>.AllDefs
                                                                      orderby x.order, x.index
                                                                      select x;

        //A dictionary of the custom classes of world gen steps
        public static Dictionary<string, WorldGenStep> worldGenStepDict = new()
        {
            { "Terrain" ,       new WorldGenStep_Terrain()},
            { "Components",     new WorldGenStep_Components()},
            { "Lakes",          new WorldGenStep_Lakes()},
            { "Rivers",         new WorldGenStep_Rivers()},
            { "AncientSites",   new WorldGenStep_AncientSites()},
            { "AncientRoads",   new WorldGenStep_AncientRoads()},
            { "Pollution",      new WorldGenStep_Pollution()},
            { "Factions",       new WorldGenStep_Factions()},
            { "Roads",          new WorldGenStep_Roads()},
            { "Features",       new WorldGenStep_Features() }

        };


        //For each loaded worldGenStepDef, if Rimworld Together has a custom class for that step,
        //replace the current step with the custom step.
        public static void initializeGenerationDefs()
        {
            RT_WorldGenSteps = GenStepsInOrder.ToList();

            foreach (WorldGenStepDef step in RT_WorldGenSteps)
            {
                if (worldGenStepDict.ContainsKey(step.defName))
                    step.worldGenStep = worldGenStepDict[step.defName];
            }

        }
    }
}
