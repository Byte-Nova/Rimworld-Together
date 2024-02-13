using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RimworldTogether.GameClient.CustomMapGeneration
{
    public static class RT_GetOrGenerateMapUtility
    {
        public static Map GetOrGenerateMap(int tile, IntVec3 size, WorldObjectDef suggestedMapParentDef)
        {
            Map map = Current.Game.FindMap(tile);
            if (map == null)
            {
                Log.Message($"No existing map found at tile {tile} \nSearching for mapParent (world object on planet)");
                MapParent mapParent = Find.WorldObjects.MapParentAt(tile);
                if (mapParent == null)
                {
                    Log.Message($"No mapParent found\nchecking for suggestedMapParentDef");
                    if (suggestedMapParentDef == null)
                    {
                        Log.Error("Tried to get or generate map at " + tile + ", but there isn't any MapParent world object here and map parent def argument is null.", false);
                        return null;
                    }
                    mapParent = (MapParent)WorldObjectMaker.MakeWorldObject(suggestedMapParentDef);
                    mapParent.Tile = tile;
                    Find.WorldObjects.Add(mapParent);
                }

                map = RT_MapGenerator.GenerateMap(size, mapParent, mapParent.MapGeneratorDef, mapParent.ExtraGenStepDefs, null);
            }
            return map;
        }

        public static Map GetOrGenerateMap(int tile, WorldObjectDef suggestedMapParentDef)
        {
            return GetOrGenerateMapUtility.GetOrGenerateMap(tile, Find.World.info.initialMapSize, suggestedMapParentDef);
        }
    }
}
