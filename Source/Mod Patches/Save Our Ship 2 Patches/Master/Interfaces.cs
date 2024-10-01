using SaveOurShip2;
using System.Threading.Tasks;
using Verse;

namespace RT_SOS2Patches
{
    // Classes responsible for data transfer from GameClient
    public class IsSettlementShip : GameClient.SOS2.IisShip
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

    public class StartSOS2 : GameClient.SOS2.IStartSOS2
    {
        public void ReceiveData() 
        {
            Main.Start();
        }
    }
}
