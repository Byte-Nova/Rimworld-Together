using GameClient.Dialogs;
using GameClient.Hooks.TCPNetwork;
using GameClient.Managers;
using GameClient.Misc;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Shared.Files.Maps;
using Shared.Files.Synchronous;
using Shared.Misc;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPNetwork.Packets;
using Verse;
using Verse.Noise;
using static Shared.CommonEnumerators;

namespace GameClient.Hooks.Synchronous
{
    public static class SynchronousManager
    {
        private enum SynchronousSide { Host, Guest }

        [HandlesPacket(PacketHeader.SynchronousManager)]
        private static void ParsePacket(byte[] bytes)
        {
            SynchronousData data = Serializer.ConvertBytesToObject<SynchronousData>(bytes);

            switch (data._stepMode)
            {
                case SynchronousData.StepMode.Ask:
                    OnAsk(data);
                    break;

                case SynchronousData.StepMode.Accept:
                    OnAccept(data);
                    break;

                case SynchronousData.StepMode.Reject:
                    OnReject(data);
                    break;

                case SynchronousData.StepMode.Start:
                    StartSession(SynchronousSide.Host);
                    break;
            }
        }

        public static void Ask(int tile)
        {
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Wait());

            SynchronousData data = new SynchronousData();
            data._stepMode = SynchronousData.StepMode.Ask;
            data._toTile = tile;
            data._party = GetPawnParty(SynchronousSide.Guest);

            ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.SynchronousManager, data);
        }

        private static void OnAsk(SynchronousData data)
        {
            RT_Dialog_Wait.Instance.Close();

            Action actionYes = delegate
            {
                RT_Dialog_Base.PushNewDialog(new RT_Dialog_Wait());

                SetMap(SynchronousSide.Host, null);

                MapManager.SendMapToServer(SessionHandler.SynchronousMap);

                SynchronousData _ = new SynchronousData();
                _._stepMode = SynchronousData.StepMode.Accept;
                _._fromTile = data._toTile;
                _._toTile = data._fromTile;
                _._party = GetPawnParty(SynchronousSide.Host);

                SpawnOtherPawns(SynchronousSide.Host, data._party.Pawns.ToArray());

                ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.SynchronousManager, _);
            };

            Action actionNo = delegate
            {
                SynchronousData _ = new SynchronousData();
                _._stepMode = SynchronousData.StepMode.Reject;
                _._fromTile = data._toTile;
                _._toTile = data._fromTile;

                ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.SynchronousManager, _);
            };

            string description = $"Player {data._fromTile} wants to synchronize, accept?";
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_YesNo(description, actionYes, actionNo));
        }

        private static void OnAccept(SynchronousData data)
        {
            RT_Dialog_Wait.Instance.Close();

            SetMap(SynchronousSide.Guest, data);

            EnterMap(SynchronousSide.Guest);

            SpawnOtherPawns(SynchronousSide.Guest, data._party.Pawns.ToArray());

            StartSession(SynchronousSide.Guest);
        }

        private static void OnReject(SynchronousData data)
        {
            RT_Dialog_Wait.Instance.Close();

            string[] description = new string[] { "Reject!" };
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message(null, description));
        }

        private static void StartSession(SynchronousSide side)
        {
            if (side == SynchronousSide.Host)
            {
                SessionHandler.IsSynchronousHost = true;
                MainThreadHandler.Instance.DoOnSynchronousStartMethods();
                RT_Dialog_Wait.Instance.Close();
            }

            else
            {
                SynchronousData data = new SynchronousData();
                data._stepMode = SynchronousData.StepMode.Start;
                ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.SynchronousManager, data);

                MainThreadHandler.Instance.DoOnSynchronousStartMethods();
            }
        }

        private static void EndSession()
        {
            MainThreadHandler.Instance.DoOnSynchronousEndMethods();
            SessionHandler.IsSynchronousHost = false;
        }

        private static void SetMap(SynchronousSide side, SynchronousData data)
        {
            if (side == SynchronousSide.Host) SessionHandler.SynchronousMap = Find.AnyPlayerHomeMap;
            else
            {
                MapFile file = Serializer.ConvertBytesToObject<MapFile>(data._contents);
                SessionHandler.SynchronousMap = MapSaveLoader.StringToMap(file, true, true, false, false, false, true);
            }
        }

        private static void EnterMap(SynchronousSide side)
        {
            if (side == SynchronousSide.Host) return;
            else
            {
                CaravanEnterMapUtility.Enter(SessionHandler.ChosenCaravan, SessionHandler.SynchronousMap, CaravanEnterMode.Edge,
                    CaravanDropInventoryMode.DoNotDrop, draftColonists: false);

                CameraJumper.TryJump(SessionHandler.SynchronousMap.Center, SessionHandler.SynchronousMap, CameraJumper.MovementMode.Pan);
            }
        }

        private static void SpawnOtherPawns(SynchronousSide side, string[] pawnData)
        {
            foreach (string str in pawnData)
            {
                Pawn pawn = ScribeManager.SerializeFromString<Pawn>(str, ScribeManager.SerializableType.Pawn, true);

                RimworldManager.PlaceThingIntoMap(pawn, SessionHandler.SynchronousMap, 
                    side == SynchronousSide.Host ? SessionHandler.SynchronousMap.Center : pawn.PositionHeld);

                pawn.SetFactionDirect(SessionHandler.NeutralFaction);
            }
        }

        private static PartyFile GetPawnParty(SynchronousSide side)
        {
            PartyFile file = new PartyFile();

            if (side == SynchronousSide.Host) file.Pawns = RimworldManager.GetMapPawnsIntoString(SessionHandler.SynchronousMap, true, true);
            else file.Pawns = RimworldManager.GetCaravanPawnsIntoString(SessionHandler.ChosenCaravan, true);

            return file;
        }
    }
}
