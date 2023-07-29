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

        public void ExecuteActions()
        {
            try
            {
                while (_queuedActions.TryTake(out var item))
                {
                    item();
                }
            }
            catch (Exception e)
            {
                GameLogger.Error(e.ToString());
            }
        }

        public void SpawnExecuteActionsTask()
        {
            new Task(() =>
            {
                while (true)
                    if (MainNetworkingUnit.IsClient) MainNetworkingUnit.client.ExecuteActions();
                    else MainNetworkingUnit.server.ExecuteActions();
            }).Start();
        }
    }
}