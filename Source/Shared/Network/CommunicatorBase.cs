using System;
using System.Collections.Generic;
using MessagePack;
using NetMQ.Sockets;

namespace RimworldTogether.Shared.Network
{
    public interface ICommunicatorBase
    {
    }

    class StaticVarHolderCommunicatorBase
    {
        public static int idCounter = 1;
    }

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

    public abstract class CommunicatorBase<T, TReply> : ICommunicatorBase
    {
        protected CommunicatorBase()
        {
            _id = StaticVarHolderCommunicatorBase.idCounter++;
            NetworkCallbackHolder.Callbacks[_id] = Accept;
            NetworkCallbackHolder.Callbacks[-_id] = AcceptReply;
        }

        private readonly int _id;
        private readonly Dictionary<int, Action<byte[], int>> _callbacks = new Dictionary<int, Action<byte[], int>>();
        private Action<byte[], int> _acceptHandler;
        private Action<T, Action<TReply>, int> _replyHandler;
        private int _replyId = 0;
        private int NextReplyId => _replyId++;
        public void RegisterAcceptHandler(Action<T> handler)
        {
            _acceptHandler = Typeless(handler);
        }

        public void RegisterReplyHandler(Action<T, Action<TReply>, int> handler)
        {
            _replyHandler = handler;
        }

        public virtual void Accept(byte[] data, int clientId = -1)
        {
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

            var message = MessagePackSerializer.Deserialize<T>(data);
            AcceptT(message, clientId);
        }

        public virtual void AcceptT(T data, int clientId = -1) => throw new NotImplementedException();
        public virtual void AcceptTAndReply(T data, Action<TReply> callback, int clientId = -1) => _replyHandler(data, callback, clientId);

        public void Reply(TReply data, int callbackId, int clientId = -1)
        {
            MainNetworkingUnit.Send(-_id, new ReplyData
            {
                callBackId = callbackId,
                data = MessagePackSerializer.Serialize(data)
            }, MainNetworkingUnit.client.playerId);
        }

        [MessagePackObject(true)]
        public struct ReplyData
        {
            public byte[] data;
            public T Data => MessagePackSerializer.Deserialize<T>(data);
            public int callBackId;
        }

        public void AcceptReply(byte[] data, int clientId = -1)
        {
            var message = MessagePackSerializer.Deserialize<ReplyData>(data);
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

        public void SendWithReply(T data, Action<TReply> reply)
        {
            var replyId = NextReplyId;
            _callbacks[replyId] = Typeless(reply);
            MainNetworkingUnit.Send(_id, new ReplyData { data = MessagePackSerializer.Serialize(data), callBackId = replyId });
        }

        public void SendWithReply(Action<TReply> reply)
        {
            if (typeof(T) != typeof(EmptyData)) throw new Exception("You need to specify data to send");
            var replyId = NextReplyId;
            _callbacks[replyId] = Typeless(reply);
            MainNetworkingUnit.Send(_id, new ReplyData { data = MessagePackSerializer.Serialize(new EmptyData()), callBackId = replyId });
        }

        public void Send(T data, int id = 0) => MainNetworkingUnit.Send(_id, data, id);
    }
}