using RimWorld.Planet;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public static class SOS2SendData
    {
        public static async Task<bool> IsMapShip(Map data) 
        {
            Type receiverType = Master.SOS2.GetTypes().FirstOrDefault(t => typeof(IisShip).IsAssignableFrom(t));
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

        public static void ClearAllShips() 
        {
            if (ClientValues.verboseBool) Logger.Message("[SOS2]Clearing all dummy ships");
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

        public static void AddShipSettlement(PlayerShipData data)
        {
            if (ClientValues.verboseBool) Logger.Message("[SOS2]Adding new dummy ship from data");
            Type receiverType = Master.SOS2.GetTypes().FirstOrDefault(t => typeof(ISpawnShip).IsAssignableFrom(t));
            if (receiverType != null)
            {
                Logger.Warning($"Ship came with {data.settlementData.Owner} owner");
                object receiverInstance = Activator.CreateInstance(receiverType);
                var methodInfo = receiverType.GetMethod("ReceiveDataSettlement");
                methodInfo.Invoke(receiverInstance, new object[] {data});
            }
            else
            {
                Logger.Error("Could not find type for ReceiveData in RT_SOS2Patches for interface ISpawnShip. This should never happen");
            }
        }
        public static void AddShipSettlement(SpaceSettlementFile data)
        {
            if (ClientValues.verboseBool) Logger.Message("[SOS2]Adding new dummy ship from file");
            Type receiverType = Master.SOS2.GetTypes().FirstOrDefault(t => typeof(ISpawnShip).IsAssignableFrom(t));
            if (receiverType != null)
            {
                object receiverInstance = Activator.CreateInstance(receiverType);
                var methodInfo = receiverType.GetMethod("ReceiveDataFile");
                methodInfo.Invoke(receiverInstance, new object[] { data });
            }
            else
            {
                Logger.Error("Could not find type for ReceiveData in RT_SOS2Patches for interface ISpawnShip. This should never happen");
            }
        }

        public static void RemoveShip(PlayerShipData data) 
        {
            if (ClientValues.verboseBool) Logger.Message("[SOS2]Removing dummy ship");
            Type receiverType = Master.SOS2.GetTypes().FirstOrDefault(t => typeof(IRemoveShip).IsAssignableFrom(t));
            if (receiverType != null)
            {
                object receiverInstance = Activator.CreateInstance(receiverType);
                var methodInfo = receiverType.GetMethod("ReceiveData");
                methodInfo.Invoke(receiverInstance, new object[] { data });
            }
            else
            {
                Logger.Error("Could not find type for ReceiveData in RT_SOS2Patches for interface IRemoveShip. This should never happen");
            }
        }

        public static void RemoveShip(int tile) 
        {
            if (ClientValues.verboseBool) Logger.Message($"[SOS2]Removing dummy ship on tile {tile}");
            Type receiverType = Master.SOS2.GetTypes().FirstOrDefault(t => typeof(IRemoveShipFromTile).IsAssignableFrom(t));
            if (receiverType != null)
            {
                object receiverInstance = Activator.CreateInstance(receiverType);
                var methodInfo = receiverType.GetMethod("ReceiveData");
                methodInfo.Invoke(receiverInstance, new object[] { tile });
            }
            else
            {
                Logger.Error("Could not find type for ReceiveData in RT_SOS2Patches for interface IRemoveShipFromTile. This should never happen");
            }
        }
        public static void MakeShipMove(Packet packet)
        {
            if (ClientValues.verboseBool) Logger.Message("[SOS2]Moving a ship");
            MovementData data = Serializer.ConvertBytesToObject<MovementData>(packet.contents);
            Type receiverType = Master.SOS2.GetTypes().FirstOrDefault(t => typeof(IShipMovement).IsAssignableFrom(t));
            if (receiverType != null)
            {
                object receiverInstance = Activator.CreateInstance(receiverType);
                var methodInfo = receiverType.GetMethod("ReceiveData");
                methodInfo.Invoke(receiverInstance, new object[] { data });
            }
            else
            {
                Logger.Error("Could not find type for ReceiveData in RT_SOS2Patches for interface IShipMovement. This should never happen");
            }
        }
        public static void StartSOS2()
        {
            if (ClientValues.verboseBool) Logger.Message("[SOS2]Starting SOS2");
            Type receiverType = Master.SOS2.GetTypes().FirstOrDefault(t => typeof(IStartSOS2).IsAssignableFrom(t));
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

        public static void ChangeGoodWillOfShip(Goodwill data, int tile) 
        {
            if (ClientValues.verboseBool) Logger.Message($"[SOS2]Changing goodwill of {tile}");
            Type receiverType = Master.SOS2.GetTypes().FirstOrDefault(t => typeof(IChangeShipGoodwill).IsAssignableFrom(t));
            if (receiverType != null)
            {
                try
                {
                    object receiverInstance = Activator.CreateInstance(receiverType);
                    var methodInfo = receiverType.GetMethod("ReceiveData");
                    methodInfo.Invoke(receiverInstance, new object[] { tile, data });
                } catch (Exception e) {Logger.Error(e.ToString()); }
            }
            else
            {
                Logger.Error("Could not find type for ReceiveData in RT_SOS2Patches for interface IShipMovement. This should never happen");
            }
        }
    }
}
