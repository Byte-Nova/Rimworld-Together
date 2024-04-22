using Mono.Nat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    //Class that handles UPnP forwarding between the server and the router

    public class UPnP
    {
        //Useful variables
        public bool autoPortForwardSuccessful;

        public UPnP()
        {
            Logger.WriteToConsole($"Attempting to forward UPnP on port '{Network.port}'", Logger.LogMode.Warning);

            NatUtility.DeviceFound += DeviceFound;
            NatUtility.StartDiscovery();

            TryEnableUPnP();
        }

        //Function that acts as a clock to check if UPnP was forwarded correctly

        public void TryEnableUPnP()
        {
            Thread.Sleep(5000);

            NatUtility.StopDiscovery();

            if (!autoPortForwardSuccessful)
            {
                Logger.WriteToConsole("Could not enable UPnP - Possible causes:\n" +
                    "- the port is being used\n" +
                    "- the router has uPnP disabled\n" +
                    "- the router/modem does not have ports available",
                    Logger.LogMode.Error);
            }
        }

        //Trigger that executes whenever a device for UPnP was found

        private void DeviceFound(object sender, DeviceEventArgs args)
        {
            try
            {
                INatDevice device = args.Device;
                device.CreatePortMap(new Mapping(Protocol.Tcp, Network.port, Network.port));
                autoPortForwardSuccessful = true;

                Logger.WriteToConsole("UPnP forward successful", Logger.LogMode.Warning);
            }
            catch (Exception e) { Logger.WriteToConsole(e.ToString(), Logger.LogMode.Error); }
        }
    }
}
