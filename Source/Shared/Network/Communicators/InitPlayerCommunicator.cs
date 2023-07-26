using System;
using MessagePack;

namespace RimworldTogether.Shared.Network
{
    [MessagePackObject(true)]
    public struct InitPlayerData
    {
        public int playerId;
        public Guid guid;
    }

    public class InitPlayerCommunicator : CommunicatorBase<Guid, InitPlayerData>
    {
        public override void AcceptTAndReply(Guid data, Action<InitPlayerData> reply, int clientId = -1)
        {
            var answer = MainNetworkingUnit.server.RegisterNewPlayer();
            reply(new InitPlayerData
            {
                playerId = answer,
                guid = data
            });
        }
    }
}