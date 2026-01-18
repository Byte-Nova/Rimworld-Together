using GameClient;
using GameClient.Hooks.TCPNetwork;
using GameClient.Misc;
using Shared;
using Synchronous.Misc;
using Synchronous.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Synchronous.Managers
{
    public static class SMentalStateManager
    {
        // We need a reference to the latest mental state so the game doesn't freak out while waiting for the server response

        public static MentalState LatestMentalState { get; set; } = null;

        public static void Ask(Pawn pawn, byte value, PlayerMentalState.MentalMode mode)
        {
            PlayerMentalState playerMentalState = new PlayerMentalState();
            playerMentalState.MapTile = pawn.Map.Tile;
            playerMentalState.PawnID = pawn.ThingID;
            playerMentalState.MentalStateByte = value;
            playerMentalState.Mode = mode;

            ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.SPlayerMentalState, playerMentalState);
        }

        [HandlesPacket(PacketHeader.SPlayerMentalState)]
        private static void Receive(byte[] bytes)
        {
            PlayerMentalState data = Serializer.ConvertBytesToObject<PlayerMentalState>(bytes);

            PatchHandler.ExecuteInBypass(delegate
            {
                if (data.Mode == PlayerMentalState.MentalMode.Add) AddMentalState(data);
                else RemoveMentalState(data);
            });
        }

        private static void AddMentalState(PlayerMentalState data)
        {
            Map map = Finder.GetMapFromTile(data.MapTile);
            Pawn pawn = Finder.GetPawnFromID(map, data.PawnID);
            MentalStateDef def = Finder.GetMentalStateDefFromByte(data.MentalStateByte);

            if (def != null) pawn.mindState.mentalStateHandler.TryStartMentalState(def);
        }

        private static void RemoveMentalState(PlayerMentalState data)
        {
            Map map = Finder.GetMapFromTile(data.MapTile);
            Pawn pawn = Finder.GetPawnFromID(map, data.PawnID);
            MentalStateDef def = Finder.GetMentalStateDefFromByte(data.MentalStateByte);

            if (def != null)
            {
                if (pawn.MentalState != null)
                {
                    pawn.MentalState.RecoverFromState();
                }
            }
        }
    }
}
