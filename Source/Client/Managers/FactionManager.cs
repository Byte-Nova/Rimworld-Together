using Shared;
using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public static class FactionManager
    {
        public static void ParseFactionPacket(Packet packet)
        {
            PlayerFactionData data = Serializer.ConvertBytesToObject<PlayerFactionData>(packet.contents);

            switch (data._stepMode)
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
                DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for member list"));

                PlayerFactionData playerFactionData = new PlayerFactionData();
                playerFactionData._stepMode = FactionStepMode.MemberList;

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), playerFactionData);
                Network.listener.EnqueuePacket(packet);
            };

            Action r2 = delegate
            {
                PlayerFactionData playerFactionData = new PlayerFactionData();
                playerFactionData._stepMode = FactionStepMode.RemoveMember;
                playerFactionData._dataInt = SessionValues.chosenSettlement.Tile;

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), playerFactionData);
                Network.listener.EnqueuePacket(packet);
            };

            Action r1 = delegate
            {
                DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for faction deletion"));

                PlayerFactionData playerFactionData = new PlayerFactionData();
                playerFactionData._stepMode = FactionStepMode.Delete;

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), playerFactionData);
                Network.listener.EnqueuePacket(packet);
            };

            RT_Dialog_YesNo d3 = new RT_Dialog_YesNo("Are you sure you want to LEAVE your faction?", r2, null);

            RT_Dialog_YesNo d2 = new RT_Dialog_YesNo("Are you sure you want to DELETE your faction?", r1, null);

            RT_Dialog_3Button d1 = new RT_Dialog_3Button("Faction Management", "Manage your faction from here",
                "Members", "Delete", "Leave",
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
                    DialogManager.PushNewDialog(new RT_Dialog_Error("Faction name is invalid! Please try again!"));
                }

                else
                {
                    DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for faction creation"));

                    PlayerFactionData playerFactionData = new PlayerFactionData();
                    playerFactionData._stepMode = FactionStepMode.Create;
                    playerFactionData._factionFile.name = DialogManager.dialog1ResultOne;

                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), playerFactionData);
                    Network.listener.EnqueuePacket(packet);
                }
            };
            RT_Dialog_1Input d2 = new RT_Dialog_1Input("New Faction Name", "Input the name of your new faction", r2, null);

            Action r1 = delegate { DialogManager.PushNewDialog(d2); };
            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("You are not a member of any faction! Create one?", r1, null);

            DialogManager.PushNewDialog(d1);
        }

        public static void OnFactionOpenOnMember()
        {
            Action r1 = delegate
            {
                PlayerFactionData playerFactionData = new PlayerFactionData();
                playerFactionData._stepMode = FactionStepMode.Promote;
                playerFactionData._dataInt = SessionValues.chosenSettlement.Tile;

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), playerFactionData);
                Network.listener.EnqueuePacket(packet);
            };

            Action r2 = delegate
            {
                PlayerFactionData playerFactionData = new PlayerFactionData();
                playerFactionData._stepMode = FactionStepMode.Demote;
                playerFactionData._dataInt = SessionValues.chosenSettlement.Tile;

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), playerFactionData);
                Network.listener.EnqueuePacket(packet);
            };

            Action r3 = delegate
            {
                PlayerFactionData playerFactionData = new PlayerFactionData();
                playerFactionData._stepMode = FactionStepMode.RemoveMember;
                playerFactionData._dataInt = SessionValues.chosenSettlement.Tile;

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), playerFactionData);
                Network.listener.EnqueuePacket(packet);
            };

            RT_Dialog_YesNo d5 = new RT_Dialog_YesNo("Are you sure you want to demote this player?", 
                r2,
                delegate { DialogManager.PushNewDialog(DialogManager.previousDialog); });

            RT_Dialog_YesNo d4 = new RT_Dialog_YesNo("Are you sure you want to promote this player?", 
                r1,
                delegate { DialogManager.PushNewDialog(DialogManager.previousDialog); });

            RT_Dialog_YesNo d3 = new RT_Dialog_YesNo("Are you sure you want to kick this player?", 
                r3,
                delegate { DialogManager.PushNewDialog(DialogManager.previousDialog); });

            RT_Dialog_2Button d2 = new RT_Dialog_2Button("Power Management Menu", "Choose what you want to manage",
                "Promote", "Demote",
                delegate { DialogManager.PushNewDialog(d4); },
                delegate { DialogManager.PushNewDialog(d5); },
                null);

            RT_Dialog_2Button d1 = new RT_Dialog_2Button("Management Menu", "Choose what you want to manage", 
                "Powers", "Kick", 
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
                playerFactionData._stepMode = FactionStepMode.AddMember;
                playerFactionData._dataInt = SessionValues.chosenSettlement.Tile;

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), playerFactionData);
                Network.listener.EnqueuePacket(packet);
            };

            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("Do you want to invite this player to your faction?", r1, null);
            DialogManager.PushNewDialog(d1);
        }

        private static void OnCreateFaction()
        {
            ServerValues.hasFaction = true;

            string[] messages = new string[]
            {
                "Your faction has been created!",
                "You can now access its menu through the same button"
            };

            DialogManager.PopWaitDialog();
            RT_Dialog_OK_Loop d1 = new RT_Dialog_OK_Loop(messages);
            DialogManager.PushNewDialog(d1);
        }

        private static void OnDeleteFaction()
        {
            ServerValues.hasFaction = false;

            if (!ClientValues.isInTransfer) DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_Error("Your faction has been deleted!"));
        }

        private static void OnFactionNameInUse()
        {
            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_Error("That faction name is already in use!"));
        }

        private static void OnFactionNoPower()
        {
            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_Error("You don't have enough power for this action!"));
        }

        private static void OnFactionGetInvited(PlayerFactionData factionManifest)
        {
            Action r1 = delegate
            {
                ServerValues.hasFaction = true;

                factionManifest._stepMode = FactionStepMode.AcceptInvite;

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), factionManifest);
                Network.listener.EnqueuePacket(packet);
            };

            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo($"Invited to {factionManifest._factionFile.name}, accept?", r1, null);
            DialogManager.PushNewDialog(d1);
        }

        private static void OnFactionGetKicked()
        {
            ServerValues.hasFaction = false;

            DialogManager.PushNewDialog(new RT_Dialog_OK("You have been kicked from your faction!"));
        }

        private static void OnFactionAdminProtection()
        {
            DialogManager.PushNewDialog(new RT_Dialog_Error("You can't do this action as a faction admin!"));
        }

        private static void OnFactionMemberList(PlayerFactionData factionManifest)
        {
            DialogManager.PopWaitDialog();

            List<string> toDisplay = new List<string>();
            for (int i = 0; i < factionManifest._factionFile.currentMembers.Count; i++)
            {
                toDisplay.Add($"{factionManifest._factionFile.currentMembers[i]} " +
                    $"- {(FactionRanks)factionManifest._factionFile.currentRanks[i]}");
            }

            RT_Dialog_Listing d1 = new RT_Dialog_Listing("Faction Members", 
                "All faction members are depicted here", toDisplay.ToArray());

            DialogManager.PushNewDialog(d1);
        }
    }
}
