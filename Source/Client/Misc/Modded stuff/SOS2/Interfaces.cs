using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static Shared.CommonEnumerators;

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
        void ReceiveDataSettlement(PlayerShipData data);
        void ReceiveDataFile(SpaceSettlementFile data);
    }

    public interface IShipMovement 
    {
        void ReceiveData(MovementData data);
    }
    public interface IRemoveShip
    {
        void ReceiveData(PlayerShipData data);
    }

    public interface IRemoveShipFromTile 
    {
        void ReceiveData(int tile);
    }

    public interface IStartSOS2 
    {
        void ReceiveData();
    }

    public interface IChangeShipGoodwill 
    {
        void ReceiveData(int tile,Goodwill data);
    }
}
