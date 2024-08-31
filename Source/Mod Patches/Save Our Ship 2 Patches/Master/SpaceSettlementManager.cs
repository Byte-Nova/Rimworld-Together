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
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using UnityEngine;
namespace RT_SOS2Patches
{
    public static class PlayerSpaceSettlementManager
    {
        public static List<WorldObjectFakeOrbitingShip> spacePlayerSettlement = new List<WorldObjectFakeOrbitingShip>();

        public static void AddSettlementFromFile(OnlineSpaceSettlementFile settlementFile)
        {
                try
                {
                WorldObjectFakeOrbitingShip ship;
                switch (settlementFile.goodwill)
                {
                    default:
                        ship = (WorldObjectFakeOrbitingShip)WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("RT_ShipNeutral"));
                        break;
                    case Goodwill.Enemy:
                        ship = (WorldObjectFakeOrbitingShip)WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("RT_ShipEnemy"));
                        break;
                    case Goodwill.Ally:
                        ship = (WorldObjectFakeOrbitingShip)WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("RT_Ship"));
                        break;
                    case Goodwill.Faction:
                        ship = (WorldObjectFakeOrbitingShip)WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("RT_Ship"));
                        break;
                }
                ship.Tile = settlementFile.tile;
                ship.name = $"{settlementFile.owner}'s ship";
                ship.SetFaction(PlanetManagerHelper.GetPlayerFactionFromGoodwill(settlementFile.goodwill));
                GameClient.Logger.Warning("Test");
                ship.phi = settlementFile.phi;
                ship.theta = settlementFile.theta;
                ship.radius = settlementFile.radius;
                ship.OrbitSet();

                ship.altitude = 1000;

                spacePlayerSettlement.Add(ship);
                Find.WorldObjects.Add(ship);
            }
            catch (Exception e) { GameClient.Logger.Error($"[SOS2]Failed to build ship at {settlementFile.tile}. Reason: {e}"); }
        }

        public static void ClearAllSettlements()
        {
            spacePlayerSettlement.Clear();

            WorldObject[] ships = Find.WorldObjects.AllWorldObjects.Where(worldObject => worldObject.def.defName == "RT_Ship " || worldObject.def.defName == "RT_ShipEnemy" || worldObject.def.defName == "RT_ShipNeutral").ToArray();
            foreach (WorldObject ship in ships) Find.WorldObjects.Remove(ship);
        }

        public static void SpawnSingleSettlement(SpaceSettlementData data)
        {
            if (ClientValues.isReadyToPlay)
            {
                try
                {
                    WorldObjectFakeOrbitingShip ship;
                    switch (data.settlementData.goodwill)
                    {
                        default:
                            ship = (WorldObjectFakeOrbitingShip)WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("RT_ShipNeutral"));
                            break;
                        case Goodwill.Enemy:
                            ship = (WorldObjectFakeOrbitingShip)WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("RT_ShipEnemy"));
                            break;
                        case Goodwill.Ally:
                            ship = (WorldObjectFakeOrbitingShip)WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("RT_Ship"));
                            break;
                        case Goodwill.Faction:
                            ship = (WorldObjectFakeOrbitingShip)WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("RT_Ship"));
                            break;
                    }
                    ship.Tile = data.settlementData.tile;
                    ship.name = $"{data.settlementData.owner}'s ship";
                    ship.SetFaction(PlanetManagerHelper.GetPlayerFactionFromGoodwill(data.settlementData.goodwill));
                    GameClient.Logger.Warning("Test");
                    ship.phi = data.phi;
                    ship.theta = data.theta;
                    ship.radius = data.radius;
                    ship.OrbitSet();

                    ship.altitude = 1000;

                    spacePlayerSettlement.Add(ship);
                    Find.WorldObjects.Add(ship);
                }
                catch (Exception e) { GameClient.Logger.Error($"[SOS2]Failed to spawn ship at {data.settlementData.tile}. Reason: {e}"); }
            }
        }
        public static void RemoveFromTile(int tile)
        {
            try
            {
                WorldObject toGet = Find.WorldObjects.AllWorldObjects.Where(x => x.Tile == tile).FirstOrDefault();
                WorldObjectFakeOrbitingShip settlement = spacePlayerSettlement.Find(x => x.Tile == toGet.Tile);
                if (settlement != null)
                {
                    spacePlayerSettlement.Remove(settlement);
                }
            }
            catch (Exception e) { GameClient.Logger.Error($"[SOS2]Failed to remove ship at {tile}. Reason: {e}"); }
        }
    }

    public static class PlayerSpaceSettlementHelper
    {
        public static void SendSettlementToServer(Map map) 
        {
            ShipMapComp comp = map.GetComponent<ShipMapComp>();
            WorldObjectOrbitingShip orbitShip = comp.mapParent;
            SpaceSettlementData spaceSiteData = new SpaceSettlementData();

            spaceSiteData.settlementData.isShip = true;
            spaceSiteData.settlementData.tile = map.Tile;
            spaceSiteData.stepMode = SettlementStepMode.Add;
            spaceSiteData.theta = orbitShip.Theta;
            spaceSiteData.radius = orbitShip.Radius;
            orbitShip.Phi = UnityEngine.Random.Range(-20f, 20f);
            spaceSiteData.phi = orbitShip.Phi;
            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SpaceSettlementPacket), spaceSiteData);
            Network.listener.EnqueuePacket(packet);

            SaveManager.ForceSave();
        }
    }
}
