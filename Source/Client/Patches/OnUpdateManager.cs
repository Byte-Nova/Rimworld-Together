using System;
using HarmonyLib;
using RimworldTogether.Shared.Misc;
using RimworldTogether.Shared.Network;
using Verse;

namespace RimworldTogether.GameClient.Patches
{
    [HarmonyPatch(typeof(Root_Play), "Update")]
    public class OnUpdateManager
    {
        private static int updateDivider = 0;

        public static void Postfix()
        {
            MainNetworkingUnit.client?.ExecuteActions();
            if (--updateDivider > 0) return;
            updateDivider = 100;
            if (MainNetworkingUnit.IsClient && MainNetworkingUnit.client.playerId == 1)
            {
                var communicator = NetworkCallbackHolder.GetType<TestSession>();
                communicator.Send(new WrappedData<int>(DateTime.Now.Millisecond, 2));
                GameLogger.Log("Sent");
            }

            if (MainNetworkingUnit.IsClient && MainNetworkingUnit.client.playerId == 2)
            {
                var communicator = NetworkCallbackHolder.GetType<TestSession>();
                communicator.RegisterAcceptHandler((data => GameLogger.Warning($"{data.data}")));
            }
        }
    }
}