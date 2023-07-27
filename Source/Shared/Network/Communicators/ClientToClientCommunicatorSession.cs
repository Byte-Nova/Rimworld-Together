using System;
using MessagePack;
using RimworldTogether.Shared.Misc;

namespace RimworldTogether.Shared.Network
{
    [MessagePackObject(true)]
    public struct WrappedData<T>
    {
        public WrappedData(T data, int targetToRelayTo)
        {
            this.data = data;
            this.targetToRelayTo = targetToRelayTo;
        }

        public readonly T data;
        public readonly int targetToRelayTo;
    }

    // Represents the session between two clients
    public abstract class ClientToClientCommunicatorSession<TSend, TReply> : CommunicatorBase<WrappedData<TSend>, WrappedData<TReply>>
    {
        //Simply relays the data from the client to the server and to the target client
        // The clients must register their own accept handlers
        public ClientToClientCommunicatorSession()
        {
            if (!MainNetworkingUnit.IsClient)
            {
                RegisterAcceptHandler((data, origin) => Send(data, data.targetToRelayTo));
                RegisterReplyHandler((data, callback, origin) => SendWithReply(data, callback, data.targetToRelayTo));
            }
            else
                RegisterAcceptHandler((data, origin) => GameLogger.Warning($"Default handler for ${GetType().Name} data: {data.data} target(should be us): ${data.targetToRelayTo} ${origin}"));
        }
    }

    public class TestSession : ClientToClientCommunicatorSession<string, EmptyData>
    {
    }
}