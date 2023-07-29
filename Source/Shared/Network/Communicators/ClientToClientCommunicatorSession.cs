using System;
using MessagePack;
using RimworldTogether.Shared.Misc;

namespace RimworldTogether.Shared.Network
{
    [MessagePackObject(true)]
    public struct WrappedData<T>
    {
        public WrappedData(T data, int targetToRelayTo, int origin = -1)
        {
            this.data = data;
            this.targetToRelayTo = targetToRelayTo;
            if (origin == -1) this.origin = MainNetworkingUnit.client.playerId;
            else this.origin = origin;
        }

        public readonly T data;
        public readonly int targetToRelayTo;
        public readonly int origin;

        public WrappedData<T> FlipSenderOrigin()
        {
            return new WrappedData<T>(data, origin, targetToRelayTo);
        }
    }

    // Represents the session between two clients
    public abstract class ClientToClientCommunicatorSessionSend<TSend> : CommunicatorBase<WrappedData<TSend>>
    {
        //Simply relays the data from the client to the server and to the target client
        // The clients must register their own accept handlers
        public ClientToClientCommunicatorSessionSend()
        {
            if (!MainNetworkingUnit.IsClient)
            {
                RegisterAcceptHandler((data, origin) =>
                {
                    GameLogger.Debug.Log($"Relaying {data.data} from {origin} to {data.targetToRelayTo}");
                    Send(data, data.targetToRelayTo);
                });
            }
            else
                RegisterAcceptHandler((data, origin) => GameLogger.Warning($"Default client accept handler for {GetType().Name} data: {data.data} target(should be us): {data.targetToRelayTo} {origin} {MainNetworkingUnit.client.playerId}"));
        }
    }

    public abstract class ClientToClientCommunicatorSessionReply<TReply> : ClientToClientCommunicatorSessionReply<EmptyData, TReply>
    {
    }

    public abstract class ClientToClientCommunicatorSessionReply<TSend, TReply> : CommunicatorBase<WrappedData<TSend>, WrappedData<TReply>>
    {
        //Simply relays the data from the client to the server and to the target client
        // The clients must register their own accept handlers
        public ClientToClientCommunicatorSessionReply()
        {
            if (!MainNetworkingUnit.IsClient)
            {
                RegisterReplyHandler((data, callback, origin) =>
                {
                    GameLogger.Debug.Log($"Relaying reply request {data.data} from {origin} to {data.targetToRelayTo}");
                    SendWithReply(data, data =>
                    {
                        GameLogger.Debug.Log($"Relaying reply {data.data} from {origin} to {data.targetToRelayTo}");
                        callback(data);
                    }, data.targetToRelayTo);
                });
            }
            else
                RegisterReplyHandler((data, callback, origin) => GameLogger.Warning($"Attempted to call client reply handler for {GetType().Name} data: {data.data} target(should be us): {data.targetToRelayTo} {origin} {MainNetworkingUnit.client.playerId}"));
        }
    }

    public class TestSession : ClientToClientCommunicatorSessionSend<string>
    {
    }
}