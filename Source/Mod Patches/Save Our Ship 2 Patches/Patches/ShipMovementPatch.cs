using GameClient;
using HarmonyLib;
using RT_SOS2Patches.Master;
using SaveOurShip2;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
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
