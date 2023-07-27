using System;
using System.Threading.Tasks;
using MessagePack;
using NetMQ;
using NetMQ.Sockets;

namespace RimworldTogether.Shared.Network
{
    public class NetworkingUnitClient : NetworkingUnitBase
    {
        private SubscriberSocket _subscriberSocket;
        private PushSocket _publisherSocket;
        public int playerId;
        public Guid guid;

        public void Connect(string address = "localhost", int port = MainNetworkingUnit.startPort)
        {
            if(MainNetworkingUnit.server != null) throw new Exception("Attempted to connect to server while server is running");
            MainNetworkingUnit.IsClient = true;
            guid = Guid.NewGuid();
            Console.WriteLine($"Connectting to server with guid {guid}");
            _subscriberSocket = new SubscriberSocket();
            _subscriberSocket.Connect($"tcp://{address}:{port}");
            _subscriberSocket.Subscribe("0");
            _poller = new NetMQPoller() { _subscriberSocket };
            _subscriberSocket.ReceiveReady += ServerReceiveReady;
            receiveTask = Task.Run(_poller.Run);

            _publisherSocket = new PushSocket();
            _publisherSocket.Connect($"tcp://{address}:{port + 1}");
            NetworkCallbackHolder.GetType<InitPlayerCommunicator>().SendWithReply(guid, item =>
            {
                if (guid != item.guid) return;
                playerId = item.playerId;
                _subscriberSocket.Subscribe($"{playerId}");
                Console.WriteLine($"Connected {guid} with player id {playerId}");
            });
        }

        protected override void ServerReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            var messageTopicReceived = _subscriberSocket.ReceiveFrameString(); //The first frame is the topic, but we don't need it
            var data = e.Socket.ReceiveFrameBytes();
            var networkType = MessagePackSerializer.Deserialize<MessagePackNetworkType>(data);
            _queuedActions.Add(() => NetworkCallbackHolder.Callbacks[networkType.type](networkType.data, -1));
        }

        public override void Send<T>(int type, T data, int topic = 0)
        {
            if (receiveTask.Exception != null)
            {
                throw receiveTask.Exception;
            }

            var srsData = MessagePackSerializer.Serialize(data);
            var messagePackNetworkType = new MessagePackNetworkType(type, srsData);
            var msg = new Msg();
            var serializedData = MessagePackSerializer.Serialize(messagePackNetworkType);
            msg.InitGC(serializedData, serializedData.Length);
            _publisherSocket.SendMoreFrame($"{topic}").SendFrame(serializedData);
        }
    }
}