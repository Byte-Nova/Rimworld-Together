using GameClient.Patches;
using RimWorld.Planet;
using Shared;
using System.Collections.Generic;
using Verse;
namespace GameClient.Managers
{

    public static class PollutionManager
    {
        [HandlesPacket(PacketHeader.PollutionManager)]
        private static void ParsePacket(byte[] bytes)
        {
            if (ModsConfig.BiotechActive)
            {
                PollutionData data = Serializer.ConvertBytesToObject<PollutionData>(bytes);
                AddPollutedTileOrganic(data._pollutionData);
            }
        }

        public static void AddPollutedTiles(PollutionDetails[] details, bool forceRefresh)
        {
            if (details == null) return;

            foreach (PollutionDetails detail in details)
            {
                AddPollutedTileSimple(detail, forceRefresh);
            }

            //If we don't want to force refresh we wait for all and then refresh the layer
            if (!forceRefresh) PollutionManagerHelper.ForcePollutionLayerRefresh();
        }

        public static void AddPollutedTileOrganic(PollutionDetails details)
        {
            PollutionPatch.PatchAddPollution.addedByServer = true;
            WorldPollutionUtility.PolluteWorldAtTile(details.Tile, details.Quantity);
        }

        public static void AddPollutedTileSimple(PollutionDetails details, bool forceRefresh)
        {
            Tile toPollute = Find.WorldGrid.tiles[details.Tile];
            toPollute.pollution = details.Quantity;

            if (forceRefresh) PollutionManagerHelper.ForcePollutionLayerRefresh();
        }

        public static void ClearAllPollution()
        {
            foreach (Tile tile in Find.WorldGrid.tiles)
            {
                if (tile.pollution != 0) tile.pollution = 0;
            }

            PollutionManagerHelper.ForcePollutionLayerRefresh();
        }
    }

    public static class PollutionManagerHelper
    {
        public static PollutionDetails[] tempPollutionDetails;

        public static void SetValues(ServerGlobalData serverGlobalData)
        {
            tempPollutionDetails = serverGlobalData._pollutedTiles;
        }

        public static PollutionDetails[] GetPlanetPollutedTiles()
        {
            List<PollutionDetails> toGet = new List<PollutionDetails>();
            foreach (Tile tile in Find.WorldGrid.tiles)
            {
                if (tile.pollution != 0)
                {
                    PollutionDetails details = new PollutionDetails();
                    details.Tile = Find.WorldGrid.tiles.IndexOf(tile);
                    details.Quantity = tile.pollution;

                    toGet.Add(details);
                }
            }

            return toGet.ToArray();
        }

        public static void ForcePollutionLayerRefresh()
        {
            Find.World.renderer.SetDirty<WorldLayer_Pollution>();
            Find.World.renderer.RegenerateLayersIfDirtyInLongEvent();
        }
    }
}
