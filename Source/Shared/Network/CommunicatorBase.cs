using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessagePack;
using RimworldTogether.Shared.Misc;

namespace RimworldTogether.Shared.Network
{
    public interface ICommunicatorBase
    {
    }

    class StaticVarHolderCommunicatorBase
    {
        public static int idCounter = 1;
    }

    //Because we cant use null as a generic type param
    [MessagePackObject(true)]
    public struct EmptyData
    {
    }

    // Second type param is useless but it's needed to make it work
    public abstract class CommunicatorBase<T> : CommunicatorBase<T, EmptyData>
    {
    }

    public class CommunicatorBaseReplyOnly<T> : CommunicatorBase<EmptyData, T>
    {
    }

    public abstract class CommunicatorBase<TSend, TReply> : ICommunicatorBase
    {
        protected CommunicatorBase()
        {
            _id = StaticVarHolderCommunicatorBase.idCounter++;
            NetworkCallbackHolder.Callbacks[_id] = Accept;
            NetworkCallbackHolder.Callbacks[-_id] = AcceptReply;
        }

        private readonly int _id;
        private readonly Dictionary<int, Action<byte[], int>> _callbacks = new();
        private readonly Dictionary<int, TaskCompletionSource<ValueTuple<byte[], int>>> _tcs = new();
        private Action<byte[], int> _acceptHandler;
        private Action<TSend, Action<TReply>, int> _replyHandler;
        private int _replyId = 0;
        private int NextReplyId => _replyId++;

        public void RegisterAcceptHandler(Action<TSend> handler)
        {
            _acceptHandler = Typeless(handler);
        }

        public void RegisterAcceptHandler(Action<TSend, int> handler)
        {
            _acceptHandler = Typeless(handler);
        }

        public void RegisterReplyHandler(Action<TSend, Action<TReply>, int> handler)
        {
            GameLogger.Debug.Log($"Registering reply handler for {GetType().Name} {handler}");
            _replyHandler = handler;
        }

        public virtual void Accept(byte[] data, int clientId = -1)
        {
            var id = MainNetworkingUnit.client != null ? MainNetworkingUnit.client.playerId : 0;
            GameLogger.Debug.Log($"Accepting data {data} for type {GetType().Name} from {clientId} as {id}");
            if (_acceptHandler != null && _replyHandler != null) throw new Exception($"You cant use both accept and reply handlers! for type {GetType().Name}");
            if (_acceptHandler != null)
            {
                _acceptHandler(data, clientId);
                return;
            }

            if (typeof(TReply) != typeof(EmptyData))
            {
                var d = MessagePackSerializer.Deserialize<ReplyData>(data);
                AcceptTAndReply(d.Data, val => { Reply(val, d.callBackId, clientId); }, clientId);
                return;
            }

            var message = MessagePackSerializer.Deserialize<TSend>(data);
            AcceptT(message, clientId);
        }

        public virtual void AcceptT(TSend data, int clientId = -1) => throw new NotImplementedException();
        public virtual void AcceptTAndReply(TSend data, Action<TReply> callback, int clientId = -1) => _replyHandler(data, callback, clientId);

        public void Reply(TReply data, int callbackId, int clientId = -1)
        {
            MainNetworkingUnit.Send(-_id, new ReplyData
            {
                callBackId = callbackId,
                data = MessagePackSerializer.Serialize(data)
            }, clientId);
        }

        [MessagePackObject(true)]
        public struct ReplyData
        {
            public byte[] data;
            [IgnoreMember] public TSend Data => MessagePackSerializer.Deserialize<TSend>(data);
            public int callBackId;
        }

        public void AcceptReply(byte[] data, int clientId = -1)
        {
            var message = MessagePackSerializer.Deserialize<ReplyData>(data);
            GameLogger.Debug.Log($"Accapting reply data {data} for type {GetType().Name} from {clientId} with callbackId {MessagePackSerializer.Deserialize<ReplyData>(data).callBackId}");
            _callbacks[message.callBackId](message.data, clientId);
        }

        public static Action<byte[], int> Typeless<TA>(Action<TA, int> func)
        {
            return (data, id) => func(MessagePackSerializer.Deserialize<TA>(data), id);
        }

        public static Action<byte[], int> Typeless<TA>(Action<TA> func)
        {
            return (data, id) => func(MessagePackSerializer.Deserialize<TA>(data));
        }

        public void SendWithReply(TSend data, Action<TReply> reply, int target = 0)
        {
            var replyId = NextReplyId;
            _callbacks[replyId] = Typeless(reply);
            MainNetworkingUnit.Send(_id, new ReplyData { data = MessagePackSerializer.Serialize(data), callBackId = replyId }, target);
        }

        public struct ReplyResult
        {
            public ReplyResult(TReply data, int origin)
            {
                this.data = data;
                this.origin = origin;
            }

            public TReply data;
            public int origin;
        }

        public async Task<ReplyResult?> SendWithReplyAsync(TSend data, int target = 0, int timeoutMilliseconds = 500)
        {
            var replyId = NextReplyId;
            var tcs = new TaskCompletionSource<ReplyResult>();
            _callbacks[replyId] = (data, id) => tcs.SetResult(new(MessagePackSerializer.Deserialize<TReply>(data), id));
            MainNetworkingUnit.Send(_id, new ReplyData { data = MessagePackSerializer.Serialize(data), callBackId = replyId }, target);
            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(timeoutMilliseconds));

            // If the timeout task completed first, throw an exception.
            if (completedTask == tcs.Task) return await tcs.Task;
            return null;
        }

        public void SendWithReply(Action<TReply> reply, int target = 0)
        {
            if (typeof(TSend) != typeof(EmptyData)) throw new Exception("You need to specify data to send");
            var replyId = NextReplyId;
            _callbacks[replyId] = Typeless(reply);
            MainNetworkingUnit.Send(_id, new ReplyData { data = MessagePackSerializer.Serialize(new EmptyData()), callBackId = replyId }, target);
        }

        public void Send(TSend data, int id = 0) => MainNetworkingUnit.Send(_id, data, id);
    }
}