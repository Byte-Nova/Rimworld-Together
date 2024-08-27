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
}
