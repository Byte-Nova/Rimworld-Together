using Shared;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.SOS2
{
    public static class SOS2SendData
    {
        private static Assembly sos2Assembly;
        public static async Task<bool> IsMapShip(Map data) 
        {
                Type receiverType = sos2Assembly.GetTypes().FirstOrDefault(t => typeof(IisShip).IsAssignableFrom(t));
                if (receiverType != null)
                {
                    if (ClientValues.verboseBool) Logger.Message("[SOS2]Checking if current map is a ship");
                    object receiverInstance = Activator.CreateInstance(receiverType);
                    var methodInfo = receiverType.GetMethod("ReceiveDataAsync");
                    bool resultTask = await (Task<bool>)methodInfo.Invoke(receiverInstance, new object[] { data });
                    return resultTask;
                }
                else
                {
                    Logger.Error("Could not find type for ReceiveDataAsync in RT_SOS2Patches for interface IisShip. This should never happen");
                    return false;
                }
        }

        public static void StartSOS2()
        {
            sos2Assembly = Master.loadedPatches["SOS2Patch"];
            Type receiverType = sos2Assembly.GetTypes().FirstOrDefault(t => typeof(IStartSOS2).IsAssignableFrom(t));
            if (receiverType != null)
            {
                object receiverInstance = Activator.CreateInstance(receiverType);
                var methodInfo = receiverType.GetMethod("ReceiveData");
                methodInfo.Invoke(receiverInstance, new object[] { });
            }
            else
            {
                Logger.Error("Could not find type for ReceiveData in RT_SOS2Patches for interface IShipMovement. This should never happen");
            }
        }
    }
}
