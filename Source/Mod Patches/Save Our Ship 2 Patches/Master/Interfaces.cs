using GameClient;
using RT_SOS2Patches.Master;
using SaveOurShip2;
using Shared;
using System.Threading.Tasks;
using Verse;
using static Shared.CommonEnumerators;

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
            PlayerShipManager.ClearAllSettlements();
        }
    }

    public class SpawnShip : GameClient.ISpawnShip
    {
        public void ReceiveDataSettlement(PlayerShipData data) 
        {
            PlayerShipManager.SpawnSingleSettlement(data);
        }
        public void ReceiveDataFile(SpaceSettlementFile data) 
        {
            PlayerShipManager.AddSettlementFromFile(data);
        }
    }

    public class RemoveShipFromTile : GameClient.IRemoveShipFromTile 
    {
        public void ReceiveData(int data)
        {
            PlayerShipManager.RemoveFromTile(data);
        }
    }

    public class RemoveShip : GameClient.IRemoveShip
    {
        public void ReceiveData(PlayerShipData data)
        {
            //Todo
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
    public class ChangeGoodwillShip : GameClient.IChangeShipGoodwill
    {
        public void ReceiveData(int tile,Goodwill data)
        {
            PlayerShipManager.ChangeGoodwill(tile, data);
        }
    }
}
