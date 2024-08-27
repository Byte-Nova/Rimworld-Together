using GameClient;
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
            Logger.Warning(data.Parent.Label + " SOS");
            ShipMapComp comp = data.GetComponent<ShipMapComp>();
            if (comp != null)
            {
                Logger.Warning("True");
                PlayerSpaceSettlementHelper.SendSettlementToServer(data);
                return Task.FromResult(true);
            }
            else
            {
                Logger.Warning("False");
                return Task.FromResult(false);
            }
        }
    }
    public class ClearAllSettlements : GameClient.IClearAllShipSettlement
    {
        public void ReceiveData()
        {
            Logger.Message("Clearing all SOS2 settlements");
            PlayerSpaceSettlementManager.ClearAllSettlements();
        }
    }
}
