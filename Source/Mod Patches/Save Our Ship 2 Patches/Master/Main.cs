using HarmonyLib;
using System.Reflection;
using GameClient;
using SaveOurShip2;
namespace RT_SOS2Patches
{
    public static class Main
    {
        private static readonly string patchID = "RT_SOS2Patches";
        public static int shipTile;
        public static void Start() 
        {
            LoadHarmonyPatches();
        }

        public static void GetShipTile() 
        {
            if (ShipInteriorMod2.FindPlayerShipMap() == null)
            {
                shipTile = -1;
            }
            else
            {
                shipTile = ShipInteriorMod2.FindPlayerShipMap().Tile;
            }
        }
        public static void LoadHarmonyPatches() 
        {
            Harmony harmony = new Harmony(patchID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

}
