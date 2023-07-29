using System;
using MessagePack;

namespace RimworldTogether.Shared.Network
{
    [MessagePackObject(true)]
    public struct InitPlayerDataReply
    {
        public int playerId;
        public Guid guid;
    }
    
    [MessagePackObject(true)]
    public struct InitPlayerSendData
    {
        public Guid guid;
        public string playerName;
    }

    public class InitPlayerCommunicator : CommunicatorBase<InitPlayerSendData, InitPlayerDataReply>
    {
        public override void AcceptTAndReply(InitPlayerSendData data, Action<InitPlayerDataReply> reply, int clientId = -1)
        {
            var answer = MainNetworkingUnit.server.RegisterNewPlayer(data.playerName);
            reply(new()
            {
                playerId = answer,
                guid = data.guid
            });
        }
    }
}