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
            Logger.WriteToConsole($"[UPnP] > Attempting to forward port '{Network.port}'", Logger.LogMode.Warning);

            NatUtility.DeviceFound += DeviceFound;

            TryToMapPort();
        }

        //Function that acts as a clock to check if UPnP was forwarded correctly
        public void TryToMapPort()
        {

            NatUtility.StartDiscovery();

            for(int i = 0; i < 20; i++)
            {
                Thread.Sleep(250);
                if (autoPortForwardSuccessful) break;
            }

            if (!autoPortForwardSuccessful)
            {
                Logger.WriteToConsole("Could not enable UPnP - Possible causes:\n" +
                    "- the port is being used\n" +
                    "- the router has UPnP disabled\n" +
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

                //This line can run multiple times if you are connected to multiple devices (Theres no reason for that, so only print it once)
                if (!autoPortForwardSuccessful) Logger.WriteToConsole("successfully portforwarded the server", Logger.LogMode.Warning);
                autoPortForwardSuccessful = true;

            }
            catch (Exception e) { Logger.WriteToConsole(e.ToString(), Logger.LogMode.Error); }
        }
    }
}
