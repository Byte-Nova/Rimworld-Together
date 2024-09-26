using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient
{
    public static class FactionManagerPatch
    {
        [HarmonyPatch(typeof(RimWorld.FactionManager), nameof(RimWorld.FactionManager.Add))]
        public static class AddedPatch
        {
            [HarmonyPostfix]
            public static void ModifyPost(Faction faction)
            {
                if(ClientValues.isReadyToPlay) if(ServerValues.isAdmin) NPCFactionManager.QueueFactionToServer(faction);
            }
        }
    }
}
