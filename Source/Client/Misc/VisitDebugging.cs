using System.Collections.Generic;
using System.Threading;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.GameClient.Misc;
using RimworldTogether.GameClient.Values;
using Verse;

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
        if (!CommandLineParamsManager.instantVisit) return;
        Thread.Sleep(5000);//Yeah not best solution but whatever for now
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