using HarmonyLib;
using Shared;
namespace RT_SOS2Patches
{
    [HarmonyPatch(typeof(GameClient.PacketHandler), nameof(GameClient.PacketHandler.SpaceSettlementPacket))]
    public static class NetWorkPatch
    {
        [HarmonyPostfix]
        public static void DoPost(SpaceSettlementData __result)
        {
            PlayerSpaceSettlementManager.HandleData(__result);
        }
    }
}
