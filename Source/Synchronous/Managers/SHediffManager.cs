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

namespace Synchronous.Managers
{
    public static class SHediffManager
    {
        public static void Ask(Hediff hediff, BodyPartRecord bodyPart, Pawn pawn, PlayerHediff.HediffMode mode, float tendQuality = -1)
        {
            PlayerHediff playerHediff = new PlayerHediff();
            playerHediff.Mode = mode;
            playerHediff.MapTile = pawn.Map.Tile;
            playerHediff.PawnID = pawn.ThingID;
            playerHediff.HediffDefname = hediff.def.defName;
            playerHediff.PartDefname = bodyPart != null ? bodyPart.def.defName : null;
            playerHediff.Severity = hediff.Severity;
            playerHediff.IsPermanent = hediff.IsPermanent();
            playerHediff.TendQuality = tendQuality;

            ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.SPlayerHediff, Serializer.ConvertObjectToBytes(playerHediff));
        }

        [HandlesPacket(PacketHeader.SPlayerHediff)]
        private static void Receive(byte[] bytes)
        {
            PlayerHediff playerHediff = Serializer.ConvertBytesToObject<PlayerHediff>(bytes);

            PatchHandler.ExecuteInBypass(delegate
            {
                if (playerHediff.Mode == PlayerHediff.HediffMode.Add) AddHediff(playerHediff);
                else if (playerHediff.Mode == PlayerHediff.HediffMode.Remove) RemoveHediff(playerHediff);
                else UpdateHediff(playerHediff);
            });
        }

        private static void AddHediff(PlayerHediff data)
        {
            Map map = Finder.GetMapFromTile(data.MapTile);
            Pawn pawn = Finder.GetPawnFromID(map, data.PawnID);
            BodyPartRecord part = Finder.GetBodyPartFromDefname(pawn, data.PartDefname);

            HediffDef hediffDef = DefDatabase<HediffDef>.AllDefs.First(fetch => fetch.defName == data.HediffDefname);
            Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn, part);
            hediff.Severity = data.Severity;

            if (data.IsPermanent)
            {
                HediffComp_GetsPermanent hediffComp = hediff.TryGetComp<HediffComp_GetsPermanent>();
                hediffComp.IsPermanent = true;
            }

            if (hediff != null) pawn.health.AddHediff(hediff, part);
        }

        private static void RemoveHediff(PlayerHediff data)
        {
            Map map = Finder.GetMapFromTile(data.MapTile);
            Pawn pawn = Finder.GetPawnFromID(map, data.PawnID);
            BodyPartRecord part = Finder.GetBodyPartFromDefname(pawn, data.PartDefname);
            Hediff hediff = Finder.GetHediffFromPart(pawn, part, data.HediffDefname, false);

            if (hediff != null) pawn.health.RemoveHediff(hediff);
        }

        private static void UpdateHediff(PlayerHediff data)
        {
            Map map = Finder.GetMapFromTile(data.MapTile);
            Pawn pawn = Finder.GetPawnFromID(map, data.PawnID);
            BodyPartRecord part = Finder.GetBodyPartFromDefname(pawn, data.PartDefname);
            Hediff hediff = Finder.GetHediffFromPart(pawn, part, data.HediffDefname, true);

            if (hediff != null)
            {
                hediff.Severity = data.Severity;

                HediffComp_TendDuration comp = hediff.TryGetComp<HediffComp_TendDuration>();
                comp.CompTended(data.TendQuality, -1);
            }
        }
    }
}
