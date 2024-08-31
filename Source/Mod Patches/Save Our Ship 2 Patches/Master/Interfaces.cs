using GameClient;
using RT_SOS2Patches.Master;
using SaveOurShip2;
using Shared;
using System.Threading.Tasks;
using Verse;

namespace RT_SOS2Patches
{
    // Classes responsible for data transfer from GameClient
    public class IsSettlementShip : GameClient.IisShip
    {
        public Task<bool> ReceiveDataAsync(Map data)
        {
            ShipMapComp comp = data.GetComponent<ShipMapComp>();
            if (comp.IsPlayerShipMap == true)
            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }
    }
    public class ClearAllSettlements : GameClient.IClearAllShipSettlement
    {
        public void ReceiveData()
        {
            Logger.Message("[SOS2]Clearing all SOS2 settlements");
            PlayerSpaceSettlementManager.ClearAllSettlements();
        }
    }

    public class SpawnShip : GameClient.ISpawnShip
    {
        public void ReceiveDataSettlement(SpaceSettlementData data) 
        {
            PlayerSpaceSettlementManager.SpawnSingleSettlement(data);
        }
        public void ReceiveDataFile(OnlineSpaceSettlementFile data) 
        {
            PlayerSpaceSettlementManager.AddSettlementFromFile(data);
        }
    }
    public class MoveShip : GameClient.IShipMovement
    {
        public void ReceiveData(MovementData data)
        {
            MovementManager.MoveShipFromTile(data);
        }
    }
    public class StartSOS2 : GameClient.IStartSOS2
    {
        public void ReceiveData() 
        {
            Main.Start();
        }
    }
}
