using RimworldTogether.Shared.JSON;

namespace RimworldTogether.GameClient.Planet
{
    public static class PlanetBuilderHelper
    {
        public static string[] tempSettlementTiles;

        public static string[] tempSettlementOwners;

        public static string[] tempSettlementLikelihoods;

        public static string[] tempSiteTiles;

        public static string[] tempSiteOwners;

        public static string[] tempSiteLikelihoods;

        public static string[] tempSiteTypes;

        public static bool[] tempSiteIsFromFactions;

        public static void SetWorldFeatures(ServerOverallJSON serverOverallJSON)
        {
            tempSettlementTiles = serverOverallJSON.settlementTiles.ToArray();
            tempSettlementOwners = serverOverallJSON.settlementOwners.ToArray();
            tempSettlementLikelihoods = serverOverallJSON.settlementLikelihoods.ToArray();

            tempSiteTiles = serverOverallJSON.siteTiles.ToArray();
            tempSiteOwners = serverOverallJSON.siteOwners.ToArray();
            tempSiteLikelihoods = serverOverallJSON.siteLikelihoods.ToArray();
            tempSiteTypes = serverOverallJSON.siteTypes.ToArray();
            tempSiteIsFromFactions = serverOverallJSON.isFromFactions.ToArray();
        }
    }
}
