using GameClient.Dialogs;
using GameClient.Hooks.TCPNetwork;
using GameClient.Misc;
using RimWorld;
using Shared;
using Shared.Files.Guilds;
using System;
using System.Collections.Generic;
using TCPNetwork.Packets;
using Verse;
using static Shared.CommonEnumerators;
using static Shared.Files.Guilds.GuildMember;

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

                case GuildStepMode.Invite:
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

                case GuildStepMode.Promote:
                    OnFactionPromote();
                    break;

                case GuildStepMode.Demote:
                    OnFactionDemote();
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

                ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.GuildManager, playerFactionData);
            };

            Action r2 = delegate
            {
                PlayerGuildData playerFactionData = new PlayerGuildData();
                playerFactionData._stepMode = GuildStepMode.RemoveMember;
                playerFactionData._dataInt = Find.AnyPlayerHomeMap.Tile;

                ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.GuildManager, playerFactionData);
            };

            Action r1 = delegate
            {
                RT_Dialog_Base.PushNewDialog(new RT_Dialog_Wait("Waiting for guild deletion"));

                PlayerGuildData playerFactionData = new PlayerGuildData();
                playerFactionData._stepMode = GuildStepMode.Delete;

                ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.GuildManager, playerFactionData);
            };

            RT_Dialog_YesNo d3 = new RT_Dialog_YesNo("Are you sure you want to LEAVE your guild?", r2, null);

            RT_Dialog_YesNo d2 = new RT_Dialog_YesNo("Are you sure you want to DELETE your guild?", r1, null);

            RT_Dialog_Buttons d1 = new RT_Dialog_Buttons("Guild Management", "Manage your guild from here",
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
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "Guild name is invalid! Please try again!" }));
                }

                else
                {
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Wait("Waiting for guild creation"));

                    PlayerGuildData playerFactionData = new PlayerGuildData();
                    playerFactionData._stepMode = GuildStepMode.Create;
                    playerFactionData._guild.Name = RT_Dialog_Inputs.DialogInputResults[0];

                    ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.GuildManager, playerFactionData);
                }
            };
            RT_Dialog_Inputs d2 = new RT_Dialog_Inputs("New Guild Name", new string[] { "Input the name of your new guild" }, new bool[] { false }, r2);

            Action r1 = delegate { RT_Dialog_Base.PushNewDialog(d2); };
            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("You are not a member of any guild! Create one?", r1, null);

            RT_Dialog_Base.PushNewDialog(d1);
        }

        public static void OnFactionOpenOnMember()
        {
            Action r5 = delegate
            {
                PlayerGuildData playerFactionData = new PlayerGuildData();
                playerFactionData._stepMode = GuildStepMode.Demote;
                playerFactionData._dataInt = SessionHandler.ChosenSettlement.Tile;

                ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.GuildManager, playerFactionData);
            };

            Action r4 = delegate
            {
                PlayerGuildData playerFactionData = new PlayerGuildData();
                playerFactionData._stepMode = GuildStepMode.Promote;
                playerFactionData._dataInt = SessionHandler.ChosenSettlement.Tile;

                ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.GuildManager, playerFactionData);
            };

            Action r3 = delegate
            {
                PlayerGuildData playerFactionData = new PlayerGuildData();
                playerFactionData._stepMode = GuildStepMode.RemoveMember;
                playerFactionData._dataInt = SessionHandler.ChosenSettlement.Tile;

                ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.GuildManager, playerFactionData);
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
                playerFactionData._stepMode = GuildStepMode.Invite;
                playerFactionData._dataInt = SessionHandler.ChosenSettlement.Tile;

                ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.GuildManager, playerFactionData);
            };

            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("Do you want to invite this player to your guild?", r1, null);
            RT_Dialog_Base.PushNewDialog(d1);
        }

        private static void OnCreateFaction()
        {
            SessionHandler.HasFaction = true;

            string[] messages = new string[]
            {
                "Your guild has been created!",
                "You can now access its menu through the same button"
            };

            RT_Dialog_Wait.Instance.Close();
            RT_Dialog_Message d1 = new RT_Dialog_Message("MESSAGE", messages);
            RT_Dialog_Base.PushNewDialog(d1);
        }

        private static void OnDeleteFaction()
        {
            SessionHandler.HasFaction = false;

            if (!SessionHandler.IsInTransfer) RT_Dialog_Wait.Instance.Close();
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("MESSAGE", new string[] { "Your guild has been deleted!" }));
        }

        private static void OnFactionNameInUse()
        {
            RT_Dialog_Wait.Instance.Close();
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "That guild name is already in use!" }));
        }

        private static void OnFactionGetInvited(PlayerGuildData factionManifest)
        {
            Action r1 = delegate
            {
                SessionHandler.HasFaction = true;

                factionManifest._stepMode = GuildStepMode.AddMember;

                ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.GuildManager, factionManifest);

                RimworldManager.GenerateLetter("Joined guild", "You have joined a guild!", LetterDefOf.PositiveEvent);
            };

            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo($"Invited to {factionManifest._guild.Name}, accept?", r1, null);
            RT_Dialog_Base.PushNewDialog(d1);
        }

        private static void OnFactionGetKicked()
        {
            SessionHandler.HasFaction = false;

            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("MESSAGE", new string[] { "You have been kicked from your guild!" }));
        }

        private static void OnFactionAdminProtection()
        {
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "You can't do this action as a guild admin!" }));
        }

        private static void OnFactionPromote()
        {
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("MESSAGE", new string[] { "You have been promoted in your guild!" }));
        }

        private static void OnFactionDemote()
        {
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("MESSAGE", new string[] { "You have been demoted in your guild!" }));
        }

        private static void OnFactionMemberList(PlayerGuildData factionManifest)
        {
            RT_Dialog_Wait.Instance.Close();

            List<string> toDisplay = new List<string>();

            for (int i = 0; i < factionManifest._guild.GuildMembers.Count; i++)
            {
                GuildMember member = factionManifest._guild.GuildMembers[i];

                toDisplay.Add($"{member.Username} - {(GuildRanks)member.Rank}");
            }

            RT_Dialog_Listing d1 = new RT_Dialog_Listing("Guild Members",
                "All guild members are depicted here", toDisplay.ToArray());

            RT_Dialog_Base.PushNewDialog(d1);
        }
    }
}
