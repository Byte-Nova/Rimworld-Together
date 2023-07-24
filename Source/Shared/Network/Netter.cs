using System;
using NetMQ.Sockets;

namespace RimworldTogether.Shared.Network
{
    public class Netter
    {
        public static void A()
        {
            Console.WriteLine("NetMQ successfully installed!");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            MainNetworkingUnit.isClient = true;
            MainNetworkingUnit.client.Connect();
        }
    }
}