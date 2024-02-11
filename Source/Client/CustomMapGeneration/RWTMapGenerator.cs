using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Noise;
using RimworldTogether.GameClient.CustomMapGeneration;
using RimworldTogether.Shared.JSON;

namespace RimworldTogether.GameClient.CustomMapGeneration
{
    public static class RWTMapGenerator
    {
        public static Map mapBeingGenerated;

        private static Dictionary<string, object> data = new Dictionary<string, object>();

        private static IntVec3 playerStartSpotInt = IntVec3.Invalid;

        public static List<IntVec3> rootsToUnfog = new List<IntVec3>();

        private static List<GenStepWithParams> tmpGenSteps = new List<GenStepWithParams>();

        public const string ElevationName = "Elevation";

        public const string FertilityName = "Fertility";

        public const string CavesName = "Caves";

        public const string RectOfInterestName = "RectOfInterest";

        public static MapGenFloatGrid Elevation
        {
            get
            {
                return RWTMapGenerator.FloatGridNamed("Elevation");
            }
        }

        public static MapGenFloatGrid Fertility
        {
            get
            {
                return RWTMapGenerator.FloatGridNamed("Fertility");
            }
        }

        public static MapGenFloatGrid Caves
        {
            get
            {
                return RWTMapGenerator.FloatGridNamed("Caves");
            }
        }

        public static IntVec3 PlayerStartSpot
        {
            get
            {
                if (!RWTMapGenerator.playerStartSpotInt.IsValid)
                {
                    Log.Error("Accessing player start spot before setting it.", false);
                    return IntVec3.Zero;
                }
                return RWTMapGenerator.playerStartSpotInt;
            }
            set
            {
                RWTMapGenerator.playerStartSpotInt = value;
            }
        }

        public static Map GenerateMap(IntVec3 mapSize, MapParent parent, MapGeneratorDef mapGenerator, IEnumerable<GenStepWithParams> extraGenStepDefs = null, Action<Map> extraInitBeforeContentGen = null)
        {



            ProgramState programState = Current.ProgramState;
            Current.ProgramState = ProgramState.MapInitializing;
            RWTMapGenerator.playerStartSpotInt = IntVec3.Invalid;
            RWTMapGenerator.rootsToUnfog.Clear();
            RWTMapGenerator.data.Clear();
            RWTMapGenerator.mapBeingGenerated = null;
            DeepProfiler.Start("InitNewGeneratedMap");
            Rand.PushState();
            int seed = Gen.HashCombineInt(Find.World.info.Seed, parent.Tile);
            Rand.Seed = seed;
            Map result;
            try
            {
                if (parent != null && parent.HasMap)
                {
                    Log.Error("Tried to generate a new map and set " + parent + " as its parent, but this world object already has a map. One world object can't have more than 1 map.", false);
                    parent = null;
                }
                DeepProfiler.Start("Set up map");
                Map map = new Map();
                map.uniqueID = Find.UniqueIDsManager.GetNextMapID();
                RWTMapGenerator.mapBeingGenerated = map;
                map.info.Size = mapSize;
                map.info.parent = parent;
                map.ConstructComponents();
                DeepProfiler.End();
                Current.Game.AddMap(map);
                if (extraInitBeforeContentGen != null)
                {
                    extraInitBeforeContentGen(map);
                }
                if (mapGenerator == null)
                {
                    Log.Error("Attempted to generate map without generator; falling back on encounter map", false);
                    mapGenerator = MapGeneratorDefOf.Encounter;
                }
                IEnumerable<GenStepWithParams> enumerable = from x in mapGenerator.genSteps
                                                            select new GenStepWithParams(x, default(GenStepParams));
                if (extraGenStepDefs != null)
                {
                    enumerable = enumerable.Concat(extraGenStepDefs);
                }
                map.areaManager.AddStartingAreas();
                map.weatherDecider.StartInitialWeather();
                DeepProfiler.Start("Generate contents into map");
                Log.Message("Generate Contents into Map");
                RWTMapGenerator.GenerateContentsIntoMap(enumerable, map, seed);
                DeepProfiler.End();
                Log.Message("Post Map Generate");
                Find.Scenario.PostMapGenerate(map);
                DeepProfiler.Start("Finalize map init");
                Log.Message("Finalize map init");


                Log.Message($"Map{map.mapDrawer}");

               
                map.FinalizeInit();
                DeepProfiler.End();
                DeepProfiler.Start("MapComponent.MapGenerated()");
                MapComponentUtility.MapGenerated(map);
                DeepProfiler.End();
                if (parent != null)
                {
                    parent.PostMapGenerate();
                }
                result = map;
            }
            finally
            {
                DeepProfiler.End();
                RWTMapGenerator.mapBeingGenerated = null;
                Current.ProgramState = programState;
                Rand.PopState();
            }
            return result;
        }

        public static void GenerateContentsIntoMap(IEnumerable<GenStepWithParams> genStepDefs, Map map, int seed)
        {
            RWTMapGenerator.data.Clear();
            Rand.PushState();
            try
            {
                Rand.Seed = seed;
                RockNoises.Init(map);
                RWTMapGenerator.tmpGenSteps.Clear();
                RWTMapGenerator.tmpGenSteps.AddRange(from x in genStepDefs orderby x.def.order, x.def.index select x);

                if (mapBeingGenerated == null) { mapBeingGenerated = map; }

                //The classes listed in tmpGenSteps rely on MapGenerator to work.
                //Instead of copying all the GenStep classes, we are just going to give MapGenerator the necessary varaibles
                //To allow RWTMapGenerator to work.
                MapGenerator.mapBeingGenerated = mapBeingGenerated;
                int[] stepsToUse = {0,1,3,4,15};
                if(map == null)
                {
                    Log.Message("Map is null during gen steps");
                }else { Log.Message("Map is currently not null"); }


                for (int j = 0; j < stepsToUse.Length; j++)
                {
                    int i = stepsToUse[j];
                    DeepProfiler.Start("GenStep - " + RWTMapGenerator.tmpGenSteps[i].def);
                    try
                    {
                        Rand.Seed = Gen.HashCombineInt(seed, RWTMapGenerator.GetSeedPart(RWTMapGenerator.tmpGenSteps, i));
                        RWTMapGenerator.tmpGenSteps[i].def.genStep.Generate(map, RWTMapGenerator.tmpGenSteps[i].parms);
                        Log.Message($"Generated step {i}");
                    }
                    catch (Exception arg)
                    {
                        Log.Error("Error in GenStep: " + arg, false);
                    }
                    finally
                    {
                        DeepProfiler.End();
                    }
                }

                //add in the custom data
                DataToMap.addEverythingToMap(map);
                //add fog
                StepToRun(18, map);
            }
            finally
            {
                Rand.PopState();
                RockNoises.Reset();
                RWTMapGenerator.data.Clear();
            }
        }

        public static T GetVar<T>(string name)
        {
            object obj;
            if (RWTMapGenerator.data.TryGetValue(name, out obj))
            {
                return (T)((object)obj);
            }
            return default(T);
        }

        public static bool TryGetVar<T>(string name, out T var)
        {
            object obj;
            if (RWTMapGenerator.data.TryGetValue(name, out obj))
            {
                var = (T)((object)obj);
                return true;
            }
            var = default(T);
            return false;
        }

        public static void SetVar<T>(string name, T var)
        {
            RWTMapGenerator.data[name] = var;
        }

        public static MapGenFloatGrid FloatGridNamed(string name)
        {
            MapGenFloatGrid var = RWTMapGenerator.GetVar<MapGenFloatGrid>(name);
            if (var != null)
            {
                return var;
            }
            MapGenFloatGrid mapGenFloatGrid = new MapGenFloatGrid(RWTMapGenerator.mapBeingGenerated);
            RWTMapGenerator.SetVar<MapGenFloatGrid>(name, mapGenFloatGrid);
            return mapGenFloatGrid;
        }

        private static int GetSeedPart(List<GenStepWithParams> genSteps, int index)
        {
            int seedPart = genSteps[index].def.genStep.SeedPart;
            int num = 0;
            for (int i = 0; i < index; i++)
            {
                if (RWTMapGenerator.tmpGenSteps[i].def.genStep.SeedPart == seedPart)
                {
                    num++;
                }
            }
            return seedPart + num;
        }

        public static void StepToRun(int stepNum, Map map)
        {
            int seed = map.ConstantRandSeed;
            DeepProfiler.Start("GenStep - " + RWTMapGenerator.tmpGenSteps[stepNum].def);
            try
            {
                Rand.Seed = Gen.HashCombineInt(seed, RWTMapGenerator.GetSeedPart(RWTMapGenerator.tmpGenSteps, stepNum));
                RWTMapGenerator.tmpGenSteps[stepNum].def.genStep.Generate(map, RWTMapGenerator.tmpGenSteps[stepNum].parms);
                Log.Message($"Generated step {stepNum}");
            }
            catch (Exception arg)
            {
                Log.Error("Error in GenStep: " + arg, false);
            }
            finally
            {
                DeepProfiler.End();
            }
        }
    }

}
