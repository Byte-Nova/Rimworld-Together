using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GameClient
{
    public static class SOS2SendData
    {
        public static async Task<bool> IsMapShip(Map data) 
        {
            Type receiverType = Master.SOS2.GetTypes().FirstOrDefault(t => typeof(IisShip).IsAssignableFrom(t));
            if (receiverType != null)
            {
                object receiverInstance = Activator.CreateInstance(receiverType);
                var methodInfo = receiverType.GetMethod("ReceiveDataAsync");
                Logger.Warning("test");
                bool resultTask = await (Task<bool>)methodInfo.Invoke(receiverInstance, new object[] { data });
                Logger.Warning(resultTask.ToString());
                return resultTask;
            } 
            else 
            {
                Logger.Error("Could not find type for ReceiveDataAsync in RT_SOS2Patches for interface IisShip. This should never happen");
                return false;
            }
        }

        public static void ClearAllShips() 
        {
            Type receiverType = Master.SOS2.GetTypes().FirstOrDefault(t => typeof(IClearAllShipSettlement).IsAssignableFrom(t));
            if (receiverType != null)
            {
                object receiverInstance = Activator.CreateInstance(receiverType);
                var methodInfo = receiverType.GetMethod("ReceiveData");
                methodInfo.Invoke(receiverInstance, new object[] { });
            }
            else
            {
                Logger.Error("Could not find type for ReceiveData in RT_SOS2Patches for interface IClearAllShipSettlement. This should never happen");
            }
        }
    }
}
