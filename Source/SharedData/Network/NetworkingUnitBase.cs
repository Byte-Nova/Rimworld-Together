using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using NetMQ;

namespace RimworldTogether.Shared.Network
{
    public abstract class NetworkingUnitBase
    {
        protected Task receiveTask;
        protected NetMQPoller _poller;
        protected ConcurrentBag<Action> _queuedActions = new ConcurrentBag<Action>();
        public abstract void Send<T>(int type, T data, int topic = 0);
        protected abstract void ServerReceiveReady(object sender, NetMQSocketEventArgs e);

        public void ExecuteActions()
        {
            while (_queuedActions.TryTake(out var item))
            {
                item();
            }
        }

        public void SpawnExecuteActionsTask()
        {
            new Task(() =>
            {
                while (true)
                {
                    if (MainNetworkingUnit.IsClient) MainNetworkingUnit.client.ExecuteActions();
                    else MainNetworkingUnit.server.ExecuteActions();
                }
            }).Start();
        }
    }
}