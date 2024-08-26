using HarmonyLib;
using System;
using Verse;
using Shared;
using static Shared.CommonEnumerators;
using GameClient;
using SaveOurShip2;
namespace RT_SOS2Patches
{
    public static class GeneratePlayerShipMapPost
    {
        public static void DoPost(Map __result)
        {
            if (Network.state == NetworkState.Connected)
            {
                if (__result != null)
                {
                    ClientValues.ManageDevOptions();
                    CustomDifficultyManager.EnforceCustomDifficulty();

                    ShipMapComp comp = __result.GetComponent<ShipMapComp>();
                    WorldObjectOrbitingShip orbitShip = comp.mapParent;
                    SpaceSiteData spaceSiteData = new SpaceSiteData();

                    spaceSiteData.isShip = true;
                    spaceSiteData.name = comp.mapParent.Name;
                    spaceSiteData.tile = __result.Tile;
                    spaceSiteData.settlementStepMode = SettlementStepMode.Add;
                    spaceSiteData.phi = orbitShip.Phi;
                    spaceSiteData.theta = orbitShip.Theta;
                    spaceSiteData.radius = orbitShip.Radius;

                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SettlementPacket), spaceSiteData);
                    Network.listener.EnqueuePacket(packet);

                    SaveManager.ForceSave();

                    if (ClientValues.isGeneratingFreshWorld)
                    {
                        WorldGeneratorManager.SendWorldToServer();
                        ClientValues.ToggleGenerateWorld(false);
                    }
                }
            }
        }
    }
}
