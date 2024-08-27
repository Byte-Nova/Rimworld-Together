using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Shared;
using static Shared.CommonEnumerators;
using GameClient;
using System.Threading.Tasks;
using SaveOurShip2;
namespace RT_SOS2Patches
{
    public static class PlayerSpaceSettlementManager
    {
        public static List<WorldObjectOrbitingShip> spacePlayerSettlement = new List<WorldObjectOrbitingShip>();

        public static void HandleData(SpaceSettlementData data)
        {
            switch (data.stepMode)
            {
                case SettlementStepMode.Add:
                    SpawnSingleSettlement(data);
                    break;

                case SettlementStepMode.Remove:
                    RemoveSingleSettlement(data);
                    break;
            }
        }

        public static void AddSettlements(OnlineSettlementFile[] settlements)
        {
            if (settlements == null) return;

            for (int i = 0; i < PlayerSettlementManagerHelper.tempSettlements.Count(); i++)
            {
                OnlineSettlementFile settlementFile = PlayerSettlementManagerHelper.tempSettlements[i];

                try
                {
                    WorldObjectOrbitingShip ship = (WorldObjectOrbitingShip)WorldObjectMaker.MakeWorldObject(ResourceBank.WorldObjectDefOf.ShipOrbiting);
                    ship.Tile = settlementFile.tile;
                    ship.Name = $"{settlementFile.owner}'s settlement";
                    ship.SetFaction(PlanetManagerHelper.GetPlayerFactionFromGoodwill(settlementFile.goodwill));

                    spacePlayerSettlement.Add(ship);
                    Find.WorldObjects.Add(ship);
                }
                catch (Exception e) { Logger.Error($"Failed to build settlement at {settlementFile.tile}. Reason: {e}"); }
            }
        }

        public static void ClearAllSettlements()
        {
            spacePlayerSettlement.Clear();

            WorldObject[] ships = Find.WorldObjects.AllWorldObjects.Where(worldObject => worldObject.def.defName == ResourceBank.WorldObjectDefOf.ShipOrbiting.defName).ToArray();
            foreach (WorldObject ship in ships) Find.WorldObjects.Remove(ship);
        }

        public static void SpawnSingleSettlement(SpaceSettlementData data)
        {
            if (ClientValues.isReadyToPlay)
            {
                try
                {
                    WorldObjectOrbitingShip ship = (WorldObjectOrbitingShip)WorldObjectMaker.MakeWorldObject(ResourceBank.WorldObjectDefOf.ShipOrbiting);
                    ship.Tile = data.settlementData.tile;
                    ship.Name = $"{data.settlementData.owner}'s settlement";
                    ship.SetFaction(PlanetManagerHelper.GetPlayerFactionFromGoodwill(data.settlementData.goodwill));
                    SetShipPosition(ship, data);

                    spacePlayerSettlement.Add(ship);
                    Find.WorldObjects.Add(ship);
                }
                catch (Exception e) { Logger.Error($"Failed to spawn settlement at {data.settlementData.tile}. Reason: {e}"); }
            }
        }

        public static void RemoveSingleSettlement(SpaceSettlementData data)
        {
            if (ClientValues.isReadyToPlay)
            {
                try
                {
                    WorldObjectOrbitingShip toGet = spacePlayerSettlement.Find(x => x.Tile == data.settlementData.tile);

                    spacePlayerSettlement.Remove(toGet);
                    Find.WorldObjects.Remove(toGet);
                }
                catch (Exception e) { Logger.Error($"Failed to remove settlement at {data.settlementData.tile}. Reason: {e}"); }
            }
        }

        public static void SetShipPosition(WorldObjectOrbitingShip ship, SpaceSettlementData data) 
        {
            ship.Radius = data.radius;
            ship.Theta = data.theta;
            ship.Phi = data.phi;
        }
    }

    public static class PlayerSpaceSettlementHelper
    {
        public static OnlineSettlementFile[] tempSettlements;

        public static void SetValues(ServerGlobalData serverGlobalData)
        {
            tempSettlements = serverGlobalData.playerSettlements;
        }
        public static void SendSettlementToServer(Map map) 
        {
            ShipMapComp comp = map.GetComponent<ShipMapComp>();
            WorldObjectOrbitingShip orbitShip = comp.mapParent;
            SpaceSettlementData spaceSiteData = new SpaceSettlementData();

            spaceSiteData.settlementData.isShip = true;
            spaceSiteData.settlementData.tile = map.Tile;
            spaceSiteData.stepMode = SettlementStepMode.Add;
            spaceSiteData.phi = orbitShip.Phi;
            spaceSiteData.theta = orbitShip.Theta;
            spaceSiteData.radius = orbitShip.Radius;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SpaceSettlementPacket), spaceSiteData);
            Network.listener.EnqueuePacket(packet);

            SaveManager.ForceSave();
        }
    }
}
