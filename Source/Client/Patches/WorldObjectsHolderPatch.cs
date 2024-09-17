using HarmonyLib;
using RimWorld.Planet;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public static class WorldObjectsHolderPatch
    {
        [HarmonyPatch(typeof(WorldObjectsHolder), nameof(WorldObjectsHolder.Add))]
        public static class SettlePatch
        {
            [HarmonyPostfix]
            public static void ModifyPost(WorldObject o)
            {
                if (ClientValues.isReadyToPlay) WorldObjectManager.NewWorldObjectAdded(o);
            }
        }
        [HarmonyPatch(typeof(WorldObjectsHolder), nameof(WorldObjectsHolder.Remove))]
        public static class DeletePatch
        {
            [HarmonyPostfix]
            public static void ModifyPost(WorldObject o)
            {
                if (ClientValues.isReadyToPlay) WorldObjectManager.WorldObjectRemoved(o);
            }
        }
    }
}
