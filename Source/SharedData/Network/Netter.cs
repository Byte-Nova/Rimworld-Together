using System;

namespace RimworldTogether.Shared.Network
{
    public class Netter
    {
        public static void A()
        {
            Console.WriteLine("NetMQ successfully installed!");
            MainNetworkingUnit.isClient = true;
            MainNetworkingUnit.client.Connect();
        }
    }
}