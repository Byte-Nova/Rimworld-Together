using GameClient;
using GameClient.Core;
using GameClient.Hooks.TCPNetwork;
using GameClient.Misc;
using RimWorld;
using Shared;
using Shared.Misc;
using Synchronous.Misc;
using Synchronous.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Synchronous.Managers
{
    public static class SDraftManager
    {
        private static List<PlayerDraft> PlayerDrafts { get; set; } = new List<PlayerDraft>();

        [OnSessionStart]
        private static void Initialize() { PlayerDrafts = new List<PlayerDraft>(); }

        [OnUpdate]
        private static void Check()
        {
            if (PlayerDrafts.Count > 0)
            {
                ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.SPlayerDraft, Serializer.ConvertObjectToBytes(PlayerDrafts));

                PlayerDrafts.Clear();
            }
        }

        public static void Ask(Pawn pawn, bool mode) 
        {
            PlayerDraft draft = new PlayerDraft();
            draft.MapTile = pawn.Map.Tile;
            draft.PawnID = pawn.ThingID;
            draft.DraftValue = mode;

            PlayerDrafts.Add(draft); 
        }

        [HandlesPacket(PacketHeader.SPlayerDraft)]
        private static void Receive(byte[] bytes)
        {
            PlayerDraft[] drafts = Serializer.ConvertBytesToObject<PlayerDraft[]>(bytes);

            PatchHandler.ExecuteInBypass(delegate
            {
                foreach (PlayerDraft playerDraft in drafts)
                {
                    Map map = Finder.GetMapFromTile(playerDraft.MapTile);
                    Pawn pawn = Finder.GetPawnFromID(map, playerDraft.PawnID);

                    pawn.drafter ??= new Pawn_DraftController(pawn);
                    pawn.drafter.Drafted = playerDraft.DraftValue;
                }
            });
        }
    }
}
