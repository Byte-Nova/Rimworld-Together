using System.Threading.Tasks;
using MessagePack;
using NetMQ;
using NetMQ.Sockets;

namespace RimworldTogether.Shared.Network
{
    public class NetworkingUnitServer : NetworkingUnitBase
    {
        private PublisherSocket _publisherSocket;
        private PullSocket _subscriberSocket;
        private int _nextPlayerId = 1;
        public void Listen()
        {
            _publisherSocket = new PublisherSocket();
            _publisherSocket.Bind($"tcp://localhost:{MainNetworkingUnit.startPort}");
            _subscriberSocket = new PullSocket();
            _subscriberSocket.Bind($"tcp://localhost:{MainNetworkingUnit.startPort + 1}");
            _poller = new NetMQPoller() { _subscriberSocket };
            _subscriberSocket.ReceiveReady += ServerReceiveReady;
            receiveTask = Task.Run(_poller.Run);
        }

        public int RegisterNewPlayer()
        {
            return _nextPlayerId++;
        }

        protected override void ServerReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            var messageTopicReceived = _subscriberSocket.ReceiveFrameString();
            var topic = int.Parse(messageTopicReceived);
            var data = e.Socket.ReceiveFrameBytes();
            var networkType = MessagePackSerializer.Deserialize<MessagePackNetworkType>(data);
            _queuedActions.Add(() => NetworkCallbackHolder.Callbacks[networkType.type](networkType.data, topic));
        }

        public override void Send<T>(int type, T data, int topic = 0)
        {
            var srsData = MessagePackSerializer.Serialize(data);
            var messagePackNetworkType = new MessagePackNetworkType(type, srsData);
            var serializedData = MessagePackSerializer.Serialize(messagePackNetworkType);
            _publisherSocket.SendMoreFrame($"{topic}").SendFrame(serializedData);
        }
    }
}