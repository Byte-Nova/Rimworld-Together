using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.GameClient.Values;
using RimworldTogether.Shared.Misc;
using RimworldTogether.Shared.Network;
using Verse;

namespace RimworldTogether.GameClient.Misc
{
    [HarmonyPatch(typeof(Map), "FinalizeLoading")]
    public static class MapFinalizeLoadingPatch
    {
        public static Caravan SpawnCaravanAt(int tile, Faction faction)
        {
            // create list of pawns
            List<Pawn> pawns = new List<Pawn>();

            // Create three pawns
            for (int i = 0; i < 3; i++)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, faction);
                pawns.Add(pawn);
            }

            // Make the caravan
            Caravan caravan = CaravanMaker.MakeCaravan(pawns, faction, tile, true);
            return caravan;
        }

        public static void Postfix()
        {
            foreach (System.Type allType in GenTypes.AllTypes)
            foreach (MethodInfo method in allType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                DebugActionAttribute customAttribute;
                if (method.TryGetAttribute(out customAttribute))
                    GameLogger.Log($"{method} {customAttribute}");
                if (method.TryGetAttribute(out DebugActionYielderAttribute _))
                {
                    foreach (DebugActionNode child in (IEnumerable<DebugActionNode>)method.Invoke(null, null))
                        GameLogger.Log($"{child}");
                }
            }

            if (!CommandLineParamsManager.instantVisit)
            {
                NetworkCallbackHolder.GetType<VisitCallbackCommunicator>().RegisterReplyHandler((data, cb, __) =>
                {
                    GameLogger.Debug.Log($"Replying to visit request for {data.targetToRelayTo}");
                    cb(new(true, data.targetToRelayTo));
                });
                return;
            }

            new Task(async () =>
            {
                try
                {
                    while (!NetworkCallbackHolder.GetType<VisitCallbackCommunicator>().readToVisit)
                    {
                        Thread.Sleep(100);
                        var id = await NetworkCallbackHolder.GetType<PlayerNameToIdCommunicator>().SendWithReplyAsync("B");
                        if (!id.HasValue || id.Value.data == -1) continue;
                        var reply = await NetworkCallbackHolder.GetType<VisitCallbackCommunicator>().SendWithReplyAsync(new(new(), id.Value.data));
                        if (!reply.HasValue) continue;
                        GameLogger.Log(reply.Value.data.data.ToString());
                        NetworkCallbackHolder.GetType<VisitCallbackCommunicator>().readToVisit = reply.Value.data.data;
                    }

                    var goal = "B's settlement";
                    var goalSettlement = FindSettlementByName(goal);
                    if (goalSettlement != null)
                    {
                        Log.Message($"Found settlement {goalSettlement.Name} at {goalSettlement.Tile}");
                        var tile = goalSettlement.Tile;
                        ClientValues.chosenSettlement = goalSettlement;
                        ClientValues.chosenCaravan = SpawnCaravanAt(tile, Faction.OfPlayer);
                        VisitManager.RequestVisit();
                    }
                    else
                    {
                        Log.Error($"No settlement found with name {goal}");
                    }
                }
                catch (Exception e)
                {
                    GameLogger.Error(e.ToString());
                }
            }).Start();
        }

        public static Settlement FindSettlementByName(string name)
        {
            foreach (Settlement settlement in Find.WorldObjects.Settlements)
            {
                if (settlement.Name == name)
                {
                    return settlement;
                }
            }

            return null; // return null if no settlement is found with the given name
        }
    }
}