using MessagePack;
using Shared;
using Shared.Misc;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using static Shared.CommonEnumerators;
using static TCPNetwork.Packets.SynchronousData;

namespace TCPNetwork.Packets
{
    public class SaveData
    {
        public SaveStepMode _stepMode { get; set; } = SaveStepMode.Send;

        public bool _forceDisconnect { get; set; } = false;

        public bool _forceUseSave { get; set; } = false;

        public byte[] _fileBytes { get; set; } = null;
    }
}
