namespace RimworldTogether.Shared.Network
{
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
    public class ClientToClientCommunicatorSession<TSend, TReply> : CommunicatorBase<WrappedData<TSend>, WrappedData<TReply>>
    {
        public ClientToClientCommunicatorSession()
        {
            if (!MainNetworkingUnit.IsClient)
                InitForServer();
        }

        //Simply relays the data from the client to the server and to the target client
        public void InitForServer()
        {
            RegisterAcceptHandler((data, origin) => Send(data, data.targetToRelayTo));
            RegisterReplyHandler((data, callback, origin) => SendWithReply(data, callback, data.targetToRelayTo));
        }
    }
}