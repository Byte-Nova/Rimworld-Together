using System.Collections.Concurrent;
using NetMQ;
using RimworldTogether.Shared.Misc;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace RimworldTogether.Shared.Network
{
    public abstract class NetworkingUnitBase
    {
        protected Task receiveTask;
        protected NetMQPoller _poller;
        protected ConcurrentBag<Action> _queuedActions = new ConcurrentBag<Action>();
        public abstract void Send<T>(int type, T data, int topic = 0);
        protected abstract void ServerReceiveReady(object sender, NetMQSocketEventArgs e);

        private void ExecuteActions()
        {
            while (_queuedActions.TryTake(out var item))
            {
                item();
            }

            Thread.Sleep(100);
            if (MainNetworkingUnit.IsClient) GameLogger.Warning($"{MainNetworkingUnit.client.playerId} ${MainNetworkingUnit.IsClient} ${DateTime.Now.Millisecond}");
            if (MainNetworkingUnit.IsClient && MainNetworkingUnit.client.playerId == 1)
            {
                var communicator = NetworkCallbackHolder.GetType<TestSession>();
                communicator.InitForClient();
                communicator.Send(new WrappedData<int>(DateTime.Now.Millisecond, 2));
                GameLogger.Log("Sent");
            }

            if (MainNetworkingUnit.IsClient && MainNetworkingUnit.client.playerId == 2)
            {
                var communicator = NetworkCallbackHolder.GetType<TestSession>();
                communicator.RegisterAcceptHandler((data => GameLogger.Warning($"{data.data}")));
            }
        }

        public void SpawnExecuteActionsTask()
        {
            new Task(() =>
            {
                try
                {
                    while (true)
                    {
                        if (MainNetworkingUnit.IsClient) MainNetworkingUnit.client.ExecuteActions();
                        else MainNetworkingUnit.server.ExecuteActions();
                    }
                }
                catch (Exception e)
                {
                    GameLogger.Error(e.ToString());
                    throw;
                }
            }).Start();
        }
    }
}