using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GameClient
{
    public interface IisShip 
    {
        Task<bool> ReceiveDataAsync(Map data);
    }
    public interface IClearAllShipSettlement
    {
        void ReceiveData();
    }
    public interface ISpawnShip
    {
        void ReceiveDataSettlement(SpaceSettlementData data);
        void ReceiveDataFile(OnlineSpaceSettlementFile data);
    }

    public interface IShipMovement 
    {
        void ReceiveData(MovementData data);
    }
    public interface IRemoveShip
    {
        void ReceiveData(SpaceSettlementData data);
    }

    public interface IStartSOS2 
    {
        void ReceiveData();
    }
    public interface IRemoveShipFromTile
    {
        void ReceiveData(int i);
    }
}
