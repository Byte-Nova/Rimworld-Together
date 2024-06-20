using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient
{
    public static class AidManager
    {
        public static void ParsePacket(Packet packet)
        {
            AidData data = (AidData)Serializer.ConvertBytesToObject(packet.contents);

            switch (data.stepMode)
            {

            }
        }
    }
}
