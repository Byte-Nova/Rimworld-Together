using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Shared;
using static Shared.CommonEnumerators;
using GameClient;
using SaveOurShip2;
namespace RT_SOS2Patches
{
    public static class PlayerShipManagerHelper
    {
        public static void SendSettlementToServer(Map map)
        {
            ShipMapComp comp = map.GetComponent<ShipMapComp>();
            WorldObjectOrbitingShip orbitShip = comp.mapParent;
            PlayerShipData spaceSiteData = new PlayerShipData();

            spaceSiteData._settlementData.isShip = true;
            spaceSiteData._settlementData.Tile = map.Tile;
            spaceSiteData._stepMode = SettlementStepMode.Add;
            spaceSiteData._theta = orbitShip.Theta;
            spaceSiteData._radius = orbitShip.Radius;
            orbitShip.Phi = UnityEngine.Random.Range(-70f, 70f);
            spaceSiteData._phi = orbitShip.Phi;
            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SpaceSettlementPacket), spaceSiteData);
            Network.listener.EnqueuePacket(packet);

            SaveManager.ForceSave();
        }
    }
}
