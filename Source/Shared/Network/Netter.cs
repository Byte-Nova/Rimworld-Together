using System;
using NetMQ.Sockets;

namespace RimworldTogether.Shared.Network
{
    public class Netter
    {
        public static void A()
        {
            using (var socket = new RequestSocket())
            {
                Console.WriteLine("NetMQ successfully installed!");
            }
        }
    }
}