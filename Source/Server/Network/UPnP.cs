﻿using Mono.Nat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Shared.CommonEnumerators;

namespace GameServer
{
    //Class that handles UPnP forwarding between the server and the router

    public class UPnP
    {
        //Useful variables
        public bool autoPortForwardSuccessful;

        public UPnP()
        {
            ConsoleManager.WriteToConsole($"[UPnP] > Attempting to forward port '{Network.port}'", LogMode.Warning);

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
                ConsoleManager.WriteToConsole("Could not enable UPnP - Possible causes:\n" +
                    "- the port is being used\n" +
                    "- the router has UPnP disabled\n" +
                    "- the router/modem does not have ports available",
                    LogMode.Error);
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
                if (!autoPortForwardSuccessful) ConsoleManager.WriteToConsole("successfully portforwarded the server", LogMode.Warning);
                autoPortForwardSuccessful = true;

                ConsoleManager.WriteToConsole("UPnP forward successful", LogMode.Warning);
            }
            catch (Exception e) { ConsoleManager.WriteToConsole(e.ToString(), LogMode.Error); }
        }
    }
}
