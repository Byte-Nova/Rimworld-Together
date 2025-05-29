using GameClient.Dialogs;
using GameClient.TCP;
using GameClient.Values;
using Shared;
using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace GameClient.Managers
{

    public static class GuildManager
    {
        [HandlesPacket(PacketHeader.GuildManager)]
        private static void ParsePacket(byte[] bytes)
        {
            PlayerGuildData data = Serializer.ConvertBytesToObject<PlayerGuildData>(bytes);

            switch (data._stepMode)
            {
                case GuildStepMode.Create:
                    OnCreateFaction();
                    break;

                case GuildStepMode.Delete:
                    OnDeleteFaction();
                    break;

                case GuildStepMode.NameInUse:
                    OnFactionNameInUse();
                    break;

                case GuildStepMode.NoPower:
                    OnFactionNoPower();
                    break;

                case GuildStepMode.AddMember:
                    OnFactionGetInvited(data);
                    break;

                case GuildStepMode.RemoveMember:
                    OnFactionGetKicked();
                    break;

                case GuildStepMode.AdminProtection:
                    OnFactionAdminProtection();
                    break;

                case GuildStepMode.MemberList:
                    OnFactionMemberList(data);
                    break;
            }
        }

        public static void OnFactionOpen()
        {
            Action r3 = delegate
            {
                RT_Dialog_Base.PushNewDialog(new RT_Dialog_Wait("Waiting for member list"));

                PlayerGuildData playerFactionData = new PlayerGuildData();
                playerFactionData._stepMode = GuildStepMode.MemberList;

                Network.Listener.EnqueuePacket(PacketHeader.GuildManager, playerFactionData);
            };

            Action r2 = delegate
            {
                PlayerGuildData playerFactionData = new PlayerGuildData();
                playerFactionData._stepMode = GuildStepMode.RemoveMember;
                playerFactionData._dataInt = SessionValues.ChosenSettlement.Tile;

                Network.Listener.EnqueuePacket(PacketHeader.GuildManager, playerFactionData);
            };

            Action r1 = delegate
            {
                RT_Dialog_Base.PushNewDialog(new RT_Dialog_Wait("Waiting for faction deletion"));

                PlayerGuildData playerFactionData = new PlayerGuildData();
                playerFactionData._stepMode = GuildStepMode.Delete;

                Network.Listener.EnqueuePacket(PacketHeader.GuildManager, playerFactionData);
            };

            RT_Dialog_YesNo d3 = new RT_Dialog_YesNo("Are you sure you want to LEAVE your faction?", r2, null);

            RT_Dialog_YesNo d2 = new RT_Dialog_YesNo("Are you sure you want to DELETE your faction?", r1, null);

            RT_Dialog_Buttons d1 = new RT_Dialog_Buttons("Faction Management", "Manage your faction from here",
                new string[] { "Members", "Delete", "Leave" },
                new Action[] { delegate { r3(); }, delegate { RT_Dialog_Base.PushNewDialog(d2); }, delegate { RT_Dialog_Base.PushNewDialog(d3); } },
                null);

            RT_Dialog_Base.PushNewDialog(d1);
        }

        public static void OnNoFactionOpen()
        {
            Action r2 = delegate
            {
                if (string.IsNullOrWhiteSpace(RT_Dialog_Inputs.DialogInputResults[0]) || RT_Dialog_Inputs.DialogInputResults[0].Length > 32)
                {
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "Faction name is invalid! Please try again!" }));
                }

                else
                {
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Wait("Waiting for faction creation"));

                    PlayerGuildData playerFactionData = new PlayerGuildData();
                    playerFactionData._stepMode = GuildStepMode.Create;
                    playerFactionData._file.Name = RT_Dialog_Inputs.DialogInputResults[0];

                    Network.Listener.EnqueuePacket(PacketHeader.GuildManager, playerFactionData);
                }
            };
            RT_Dialog_Inputs d2 = new RT_Dialog_Inputs("New Faction Name", new string[] { "Input the name of your new faction" }, new bool[] { false }, r2);

            Action r1 = delegate { RT_Dialog_Base.PushNewDialog(d2); };
            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("You are not a member of any faction! Create one?", r1, null);

            RT_Dialog_Base.PushNewDialog(d1);
        }

        public static void OnFactionOpenOnMember()
        {
            Action r5 = delegate
            {
                PlayerGuildData playerFactionData = new PlayerGuildData();
                playerFactionData._stepMode = GuildStepMode.Demote;
                playerFactionData._dataInt = SessionValues.ChosenSettlement.Tile;

                Network.Listener.EnqueuePacket(PacketHeader.GuildManager, playerFactionData);
            };

            Action r4 = delegate
            {
                PlayerGuildData playerFactionData = new PlayerGuildData();
                playerFactionData._stepMode = GuildStepMode.Promote;
                playerFactionData._dataInt = SessionValues.ChosenSettlement.Tile;

                Network.Listener.EnqueuePacket(PacketHeader.GuildManager, playerFactionData);
            };

            Action r3 = delegate
            {
                PlayerGuildData playerFactionData = new PlayerGuildData();
                playerFactionData._stepMode = GuildStepMode.RemoveMember;
                playerFactionData._dataInt = SessionValues.ChosenSettlement.Tile;

                Network.Listener.EnqueuePacket(PacketHeader.GuildManager, playerFactionData);
            };

            RT_Dialog_YesNo d5 = new RT_Dialog_YesNo("Are you sure you want to demote this player?", r5);

            RT_Dialog_YesNo d4 = new RT_Dialog_YesNo("Are you sure you want to promote this player?", r4);

            RT_Dialog_YesNo d3 = new RT_Dialog_YesNo("Are you sure you want to kick this player?", r3);

            RT_Dialog_Buttons d2 = new RT_Dialog_Buttons("Power Management Menu", "Choose what you want to manage",
                new string[] { "Promote", "Demote" },
                new Action[] { delegate { RT_Dialog_Base.PushNewDialog(d4); }, delegate { RT_Dialog_Base.PushNewDialog(d5); } },
                delegate { RT_Dialog_Base.PushNewDialog(RT_Dialog_Base.PreviousDialog); });

            RT_Dialog_Buttons d1 = new RT_Dialog_Buttons("Management Menu", "Choose what you want to manage",
                new string[] { "Powers", "Kick" },
                new Action[] { delegate { RT_Dialog_Base.PushNewDialog(d2); }, delegate { RT_Dialog_Base.PushNewDialog(d3); } });

            RT_Dialog_Base.PushNewDialog(d1);
        }

        public static void OnFactionOpenOnNonMember()
        {
            Action r1 = delegate
            {
                PlayerGuildData playerFactionData = new PlayerGuildData();
                playerFactionData._stepMode = GuildStepMode.AddMember;
                playerFactionData._dataInt = SessionValues.ChosenSettlement.Tile;

                Network.Listener.EnqueuePacket(PacketHeader.GuildManager, playerFactionData);
            };

            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("Do you want to invite this player to your faction?", r1, null);
            RT_Dialog_Base.PushNewDialog(d1);
        }

        private static void OnCreateFaction()
        {
            ClientValues.HasFaction = true;

            string[] messages = new string[]
            {
                "Your faction has been created!",
                "You can now access its menu through the same button"
            };

            RT_Dialog_Wait.Instance.Close();
            RT_Dialog_Message d1 = new RT_Dialog_Message("MESSAGE", messages);
            RT_Dialog_Base.PushNewDialog(d1);
        }

        private static void OnDeleteFaction()
        {
            ClientValues.HasFaction = false;

            if (!ClientValues.IsInTransfer) RT_Dialog_Wait.Instance.Close();
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("MESSAGE", new string[] { "Your faction has been deleted!" }));
        }

        private static void OnFactionNameInUse()
        {
            RT_Dialog_Wait.Instance.Close();
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "That faction name is already in use!" }));
        }

        private static void OnFactionNoPower()
        {
            RT_Dialog_Wait.Instance.Close();
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "You don't have enough power for this action!" }));
        }

        private static void OnFactionGetInvited(PlayerGuildData factionManifest)
        {
            Action r1 = delegate
            {
                ClientValues.HasFaction = true;

                factionManifest._stepMode = GuildStepMode.AcceptInvite;

                Network.Listener.EnqueuePacket(PacketHeader.GuildManager, factionManifest);
            };

            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo($"Invited to {factionManifest._file.Name}, accept?", r1, null);
            RT_Dialog_Base.PushNewDialog(d1);
        }

        private static void OnFactionGetKicked()
        {
            ClientValues.HasFaction = false;

            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("MESSAGE", new string[] { "You have been kicked from your faction!" }));
        }

        private static void OnFactionAdminProtection()
        {
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "You can't do this action as a faction admin!" }));
        }

        private static void OnFactionMemberList(PlayerGuildData factionManifest)
        {
            RT_Dialog_Wait.Instance.Close();

            List<string> toDisplay = new List<string>();
            for (int i = 0; i < factionManifest._file.CurrentUids.Count; i++)
            {
                toDisplay.Add($"{factionManifest._file.CurrentLabels[i]} " +
                    $"- {(FactionRanks)factionManifest._file.CurrentRanks[i]}");
            }

            RT_Dialog_Listing d1 = new RT_Dialog_Listing("Faction Members",
                "All faction members are depicted here", toDisplay.ToArray());

            RT_Dialog_Base.PushNewDialog(d1);
        }
    }
}
