using System;
using System.Collections.Generic;
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
            if (!CommandLineParamsManager.instantVisit)
            {
                NetworkCallbackHolder.GetType<VisitCallbackCommunicator>().RegisterReplyHandler((data, cb, __) =>
                {
                    GameLogger.Log($"Replying to visit request for {data.targetToRelayTo}");
                    cb(new WrappedData<bool>(true, data.targetToRelayTo));
                });
                return;
            }

            new Task(() =>
            {
                try
                {
                    while (!NetworkCallbackHolder.GetType<VisitCallbackCommunicator>().readToVisit)
                    {
                        Thread.Sleep(1000);
                        // GameLogger.Log("Waiting for visit");
                        NetworkCallbackHolder.GetType<VisitCallbackCommunicator>().SendWithReply(new WrappedData<EmptyData>(new EmptyData(), 2), reply => { NetworkCallbackHolder.GetType<VisitCallbackCommunicator>().readToVisit = reply.data; });
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
                    throw;
                }
            }).Start();

            GameLogger.Error($"{NetworkCallbackHolder.GetType<VisitCallbackCommunicator>().readToVisit}");
            
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