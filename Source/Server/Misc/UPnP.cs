using Mono.Nat;

namespace GameServer.Misc
{
    //Class that handles UPnP forwarding between the server and the router

    public class UPnP
    {
        //Useful variables

        public bool AutoPortForwardSuccessful;

        public UPnP()
        {
            Printer.Warning($"[UPnP] > Attempting to forward port '{ServerNetwork.Port}'");

            NatUtility.DeviceFound += DeviceFound;

            TryToMapPort();
        }

        //Function that acts as a clock to check if UPnP was forwarded correctly

        public void TryToMapPort()
        {
            NatUtility.StartDiscovery();

            for (int i = 0; i < 20; i++)
            {
                Thread.Sleep(250);
                if (AutoPortForwardSuccessful) break;
            }

            if (!AutoPortForwardSuccessful)
            {
                Printer.Error("Could not enable UPnP - Possible causes:\n" +
                    "- the port is being used\n" +
                    "- the router has UPnP disabled\n" +
                    "- the router/modem does not have ports available");
            }
        }

        //Trigger that executes whenever a device for UPnP was found

        private void DeviceFound(object sender, DeviceEventArgs args)
        {
            try
            {
                INatDevice device = args.Device;
                device.CreatePortMap(new Mapping(Protocol.Tcp, int.Parse(ServerNetwork.Port), int.Parse(ServerNetwork.Port)));

                //This line can run multiple times if you are connected to multiple devices (Theres no reason for that, so only print it once)
                if (!AutoPortForwardSuccessful) Printer.Warning("successfully portforwarded the server");
                AutoPortForwardSuccessful = true;

                Printer.Warning("UPnP forward successful");
            }
            catch (Exception e) { Printer.Error(e.ToString()); }
        }
    }
}
