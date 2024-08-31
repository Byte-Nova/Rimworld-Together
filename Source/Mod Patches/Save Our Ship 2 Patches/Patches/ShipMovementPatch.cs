using GameClient;
using GameClient.SOS2;
using HarmonyLib;
using SaveOurShip2;
using static Shared.CommonEnumerators;

namespace RT_SOS2Patches
{
    [HarmonyPatch(typeof(WorldObjectOrbitingShip), nameof(WorldObjectOrbitingShip.Tick))]
    public static class ShipMovementPatch
    {
        [HarmonyPostfix]
        public static void DoPost(WorldObjectOrbitingShip __instance)
        {
            if (Network.state == ClientNetworkState.Connected)
            {
                if (__instance.orbitalMove != 0)
                {
                    MovementManager.phi = __instance.Phi;
                    MovementManager.theta = __instance.Theta;
                    MovementManager.radius = __instance.Radius;
                    if(MovementManager.tile == -1) 
                    {
                        MovementManager.tile = __instance.Map.Tile;
                    }
                    MovementManager.shipMoved = true;
                }
            }
        }
    }
}
