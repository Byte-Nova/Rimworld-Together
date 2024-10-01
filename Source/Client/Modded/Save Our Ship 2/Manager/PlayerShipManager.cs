using RimWorld;
using RimWorld.Planet;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.SOS2
{
    public static class PlayerShipManager
    {
        public static List<WorldObjectFakeOrbitingShip> spacePlayerSettlement = new List<WorldObjectFakeOrbitingShip>();

        public static void AddSettlementFromFile(SpaceSettlementFile settlementFile)
        {
            try
            {
                WorldObjectFakeOrbitingShip ship = SetGoodWillShip(settlementFile.Goodwill);
                ship.Tile = settlementFile.Tile;
                ship.name = $"{settlementFile.Owner}'s ship";
                ship.SetFaction(PlanetManagerHelper.GetPlayerFactionFromGoodwill(settlementFile.Goodwill));
                ship.phi = settlementFile.phi;
                ship.theta = settlementFile.theta;
                ship.radius = settlementFile.radius;
                ship.OrbitSet();

                ship.altitude = 1000;

                spacePlayerSettlement.Add(ship);
                Find.WorldObjects.Add(ship);
            }
            catch (Exception e) { GameClient.Logger.Error($"[SOS2]Failed to build ship at {settlementFile.Tile}. Reason: {e}"); }
        }
        public static void ClearAllSettlements()
        {
            spacePlayerSettlement.Clear();

            WorldObject[] ships = Find.WorldObjects.AllWorldObjects.Where(worldObject => worldObject.def.defName == "RT_Ship " || worldObject.def.defName == "RT_ShipEnemy" || worldObject.def.defName == "RT_ShipNeutral").ToArray();
            foreach (WorldObject ship in ships) Find.WorldObjects.Remove(ship);
        }

        public static void SpawnSingleSettlement(PlayerShipData data)
        {
            if (ClientValues.isReadyToPlay)
            {
                try
                {
                    WorldObjectFakeOrbitingShip ship = SetGoodWillShip(data._settlementData.Goodwill);
                    ship.Tile = data._settlementData.Tile;
                    ship.name = $"{data._settlementData.Owner}'s ship";
                    ship.SetFaction(PlanetManagerHelper.GetPlayerFactionFromGoodwill(data._settlementData.Goodwill));
                    ship.phi = data._phi;
                    ship.theta = data._theta;
                    ship.radius = data._radius;
                    ship.OrbitSet();

                    ship.altitude = 1000;

                    spacePlayerSettlement.Add(ship);
                    Find.WorldObjects.Add(ship);
                }
                catch (Exception e) { GameClient.Logger.Error($"[SOS2]Failed to spawn ship at {data._settlementData.Tile}. Reason: {e}"); }
            }
        }

        public static void ChangeGoodwill(int tile, Goodwill goodwill, WorldObjectFakeOrbitingShip oldship = null)
        {
            if (oldship == null)
            {
                oldship = (WorldObjectFakeOrbitingShip)PlayerSettlementManager.GetWorldObjectFromTile(tile);
            }
            Logger.Warning(oldship.Faction.Name);
            PlayerShipManager.spacePlayerSettlement.Remove(oldship);
            Find.WorldObjects.Remove(oldship);

            WorldObjectFakeOrbitingShip ship = SetGoodWillShip(goodwill);
            ship.Tile = oldship.Tile;
            ship.name = $"{oldship.name}";
            ship.phi = oldship.phi;
            ship.theta = oldship.theta;
            ship.radius = oldship.radius;
            ship.OrbitSet();
            ship.altitude = 1000;

            spacePlayerSettlement.Add(ship);
            Find.WorldObjects.Add(ship);
        }

        public static WorldObjectFakeOrbitingShip SetGoodWillShip(Goodwill goodwill)
        {
            WorldObjectFakeOrbitingShip ship;
            switch (goodwill)
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
            ship.SetFaction(PlanetManagerHelper.GetPlayerFactionFromGoodwill(goodwill));
            return ship;
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
}
