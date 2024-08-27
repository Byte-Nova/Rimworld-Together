using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using static Shared.CommonEnumerators;
using Verse;

namespace GameClient
{
    public static class FactionManager
    {
        public static void ParseFactionPacket(Packet packet)
        {
            PlayerFactionData data = Serializer.ConvertBytesToObject<PlayerFactionData>(packet.contents);

            switch (data.stepMode)
            {
                case FactionStepMode.Create:
                    OnCreateFaction();
                    break;

                case FactionStepMode.Delete:
                    OnDeleteFaction();
                    break;

                case FactionStepMode.NameInUse:
                    OnFactionNameInUse();
                    break;

                case FactionStepMode.NoPower:
                    OnFactionNoPower();
                    break;

                case FactionStepMode.AddMember:
                    OnFactionGetInvited(data);
                    break;

                case FactionStepMode.RemoveMember:
                    OnFactionGetKicked();
                    break;

                case FactionStepMode.AdminProtection:
                    OnFactionAdminProtection();
                    break;

                case FactionStepMode.MemberList:
                    OnFactionMemberList(data);
                    break;
            }
        }

        public static void OnFactionOpen()
        {
            Action r3 = delegate
            {
                DialogManager.PushNewDialog(new RT_Dialog_Wait("RTFactionMemberWait".Translate()));

                PlayerFactionData playerFactionData = new PlayerFactionData();
                playerFactionData.stepMode = FactionStepMode.MemberList;

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), playerFactionData);
                Network.listener.EnqueuePacket(packet);
            };

            Action r2 = delegate
            {
                PlayerFactionData playerFactionData = new PlayerFactionData();
                playerFactionData.stepMode = FactionStepMode.RemoveMember;
                playerFactionData.manifestDataInt = SessionValues.chosenSettlement.Tile;

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), playerFactionData);
                Network.listener.EnqueuePacket(packet);
            };

            Action r1 = delegate
            {
                DialogManager.PushNewDialog(new RT_Dialog_Wait("RTFactionDelete".Translate()));

                PlayerFactionData playerFactionData = new PlayerFactionData();
                playerFactionData.stepMode = FactionStepMode.Delete;

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), playerFactionData);
                Network.listener.EnqueuePacket(packet);
            };

            RT_Dialog_YesNo d3 = new RT_Dialog_YesNo("RTFactionLeaveSure".Translate(), r2, null);

            RT_Dialog_YesNo d2 = new RT_Dialog_YesNo("RTFactionDeleteSure".Translate(), r1, null);

            RT_Dialog_3Button d1 = new RT_Dialog_3Button("RTFactionManagement".Translate(), "RTFactionManagementDesc".Translate(),
                "RTFactionMembers".Translate(), "RTFactionDelete".Translate(), "RTFactionLeave".Translate(),
                delegate { r3(); },
                delegate { DialogManager.PushNewDialog(d2); },
                delegate { DialogManager.PushNewDialog(d3); },
                null);

            DialogManager.PushNewDialog(d1);
        }

        public static void OnNoFactionOpen()
        {
            Action r2 = delegate
            {
                if (string.IsNullOrWhiteSpace(DialogManager.dialog1ResultOne) || DialogManager.dialog1ResultOne.Length > 32)
                {
                    DialogManager.PushNewDialog(new RT_Dialog_Error("RTFactionNameInvalid".Translate()));
                }

                else
                {
                    DialogManager.PushNewDialog(new RT_Dialog_Wait("RTFactionWaitCreate".Translate()));

                    PlayerFactionData playerFactionData = new PlayerFactionData();
                    playerFactionData.stepMode = FactionStepMode.Create;
                    playerFactionData.manifestDataString = DialogManager.dialog1ResultOne;

                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), playerFactionData);
                    Network.listener.EnqueuePacket(packet);
                }
            };
            RT_Dialog_1Input d2 = new RT_Dialog_1Input("RTFactionCreateNewName".Translate(), "RTFactionCreateNewNameDesc".Translate(), r2, null);

            Action r1 = delegate { DialogManager.PushNewDialog(d2); };
            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("RTFactionNotAMember".Translate(), r1, null);

            DialogManager.PushNewDialog(d1);
        }

        public static void OnFactionOpenOnMember()
        {
            Action r1 = delegate
            {
                PlayerFactionData playerFactionData = new PlayerFactionData();
                playerFactionData.stepMode = FactionStepMode.Promote;
                playerFactionData.manifestDataInt = SessionValues.chosenSettlement.Tile;

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), playerFactionData);
                Network.listener.EnqueuePacket(packet);
            };

            Action r2 = delegate
            {
                PlayerFactionData playerFactionData = new PlayerFactionData();
                playerFactionData.stepMode = FactionStepMode.Demote;
                playerFactionData.manifestDataInt = SessionValues.chosenSettlement.Tile;

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), playerFactionData);
                Network.listener.EnqueuePacket(packet);
            };

            Action r3 = delegate
            {
                PlayerFactionData playerFactionData = new PlayerFactionData();
                playerFactionData.stepMode = FactionStepMode.RemoveMember;
                playerFactionData.manifestDataInt = SessionValues.chosenSettlement.Tile;

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), playerFactionData);
                Network.listener.EnqueuePacket(packet);
            };

            RT_Dialog_YesNo d5 = new RT_Dialog_YesNo("RTFactionDemoteSure".Translate(), 
                r2,
                delegate { DialogManager.PushNewDialog(DialogManager.previousDialog); });

            RT_Dialog_YesNo d4 = new RT_Dialog_YesNo("RTFactionPromoteSure".Translate(), 
                r1,
                delegate { DialogManager.PushNewDialog(DialogManager.previousDialog); });

            RT_Dialog_YesNo d3 = new RT_Dialog_YesNo("RTFactionKickSure.Translate", 
                r3,
                delegate { DialogManager.PushNewDialog(DialogManager.previousDialog); });

            RT_Dialog_2Button d2 = new RT_Dialog_2Button("RTFactionPowerManagement".Translate(), "RTFactionPowerManagementDesc".Translate(),
                "RTFactionPromote".Translate(), "RTFactionDemote".Translate(),
                delegate { DialogManager.PushNewDialog(d4); },
                delegate { DialogManager.PushNewDialog(d5); },
                null);

            RT_Dialog_2Button d1 = new RT_Dialog_2Button("RTFactionMemberManagement".Translate(), "RTFactionMemberManagementDesc".Translate(), 
                "RTFactionPowers".Translate(), "RTFactionKick".Translate(), 
                delegate { DialogManager.PushNewDialog(d2); }, 
                delegate { DialogManager.PushNewDialog(d3); }, 
                null);

            DialogManager.PushNewDialog(d1);
        }

        public static void OnFactionOpenOnNonMember()
        {
            Action r1 = delegate
            {
                PlayerFactionData playerFactionData = new PlayerFactionData();
                playerFactionData.stepMode = FactionStepMode.AddMember;
                playerFactionData.manifestDataInt = SessionValues.chosenSettlement.Tile;

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), playerFactionData);
                Network.listener.EnqueuePacket(packet);
            };

            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("RTFactionInviteSure".Translate(), r1, null);
            DialogManager.PushNewDialog(d1);
        }

        private static void OnCreateFaction()
        {
            ServerValues.hasFaction = true;

            string[] messages = new string[]
            {
                "RTFactionCreated".Translate(),
                "RTFactionCreatedDesc".Translate()
            };

            DialogManager.PopWaitDialog();
            RT_Dialog_OK_Loop d1 = new RT_Dialog_OK_Loop(messages);
            DialogManager.PushNewDialog(d1);
        }

        private static void OnDeleteFaction()
        {
            ServerValues.hasFaction = false;

            if (!ClientValues.isInTransfer) DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_Error("RTFactionDeleted".Translate()));
        }

        private static void OnFactionNameInUse()
        {
            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_Error("RTFactionNameAlreadyUsed".Translate()));
        }

        private static void OnFactionNoPower()
        {
            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_Error("RTFactionNoPower".Translate()));
        }

        private static void OnFactionGetInvited(PlayerFactionData factionManifest)
        {
            Action r1 = delegate
            {
                ServerValues.hasFaction = true;

                factionManifest.stepMode = FactionStepMode.AcceptInvite;

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), factionManifest);
                Network.listener.EnqueuePacket(packet);
            };

            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("RTFactionInvitedTo".Translate(factionManifest.manifestDataString), r1, null);
            DialogManager.PushNewDialog(d1);
        }

        private static void OnFactionGetKicked()
        {
            ServerValues.hasFaction = false;

            DialogManager.PushNewDialog(new RT_Dialog_OK("RTFactionYouKicked".Translate()));
        }

        private static void OnFactionAdminProtection()
        {
            DialogManager.PushNewDialog(new RT_Dialog_Error("RTFactionAdminProtection".Translate()));
        }

        private static void OnFactionMemberList(PlayerFactionData factionManifest)
        {
            DialogManager.PopWaitDialog();

            List<string> unraveledDatas = new List<string>();
            for (int i = 0; i < factionManifest.manifestComplexData.Count(); i++)
            {
                unraveledDatas.Add($"{factionManifest.manifestComplexData[i]} " +
                    $"- {(FactionRanks)int.Parse(factionManifest.manifestSecondaryComplexData[i])}");
            }

            RT_Dialog_Listing d1 = new RT_Dialog_Listing("RTFactionMemberMenu".Translate(), 
                "RTFactionMemberMenuDesc".Translate(), unraveledDatas.ToArray());

            DialogManager.PushNewDialog(d1);
        }
    }
}
