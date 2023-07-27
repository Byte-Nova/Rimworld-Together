using System;
using System.Numerics;
using HarmonyLib;
using JsonDiffPatchDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RimworldTogether.Shared.Misc;
using RimworldTogether.Shared.Network;
using Verse;
using BigInteger = Mono.Math.BigInteger;

namespace RimworldTogether.GameClient.Patches
{
    [HarmonyPatch(typeof(Root_Play), "Update")]
    public class OnUpdateManager
    {
        private static int updateDivider = 0;
        public class Person
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        public static void Postfix()
        {
            MainNetworkingUnit.client?.ExecuteActions();
            if (--updateDivider > 0) return;
            updateDivider = 100;
            GameLogger.Log(MainNetworkingUnit.client.playerId.ToString());
            if (MainNetworkingUnit.IsClient && MainNetworkingUnit.client.playerId == 1)
            {
                try
                {
                    var communicator = NetworkCallbackHolder.GetType<TestSession>();
                    //random binary array
                    var arr = new byte[500];
                    new Random().NextBytes(arr);
                    var arr2 = new byte[500];
                    new Random().NextBytes(arr2);
                    var jdp = new JsonDiffPatch();
                    var left = JToken.Parse(@"{ ""key"": false }");
                    var right = JToken.Parse(@"{ ""key"": true }");
                    JToken patch = jdp.Diff(left, right);
                    
                    communicator.Send(new WrappedData<string>(patch.ToString(), 2));
                    GameLogger.Log("Sent");
                }
                catch (Exception e)
                {
                    GameLogger.Log(e.ToString());
                    throw;
                }
            }

            if (MainNetworkingUnit.IsClient && MainNetworkingUnit.client.playerId == 2)
            {
                // var communicator = NetworkCallbackHolder.GetType<TestSession>();
                // communicator.RegisterAcceptHandler((data => GameLogger.Warning($"{data.data}")));
            }
        }
    }
}