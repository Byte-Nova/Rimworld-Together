using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace GameClient
{
    public static class OnlineFactionManager
    {
        public static void ParseFactionPacket(Packet packet)
        {
            FactionManifestJSON factionManifest = (FactionManifestJSON)Serializer.ConvertBytesToObject(packet.contents);

            switch (int.Parse(factionManifest.manifestMode))
            {
                case (int)CommonEnumerators.FactionManifestMode.Create:
                    OnCreateFaction();
                    break;

                case (int)CommonEnumerators.FactionManifestMode.Delete:
                    OnDeleteFaction();
                    break;

                case (int)CommonEnumerators.FactionManifestMode.NameInUse:
                    OnFactionNameInUse();
                    break;

                case (int)CommonEnumerators.FactionManifestMode.NoPower:
                    OnFactionNoPower();
                    break;

                case (int)CommonEnumerators.FactionManifestMode.AddMember:
                    OnFactionGetInvited(factionManifest);
                    break;

                case (int)CommonEnumerators.FactionManifestMode.RemoveMember:
                    OnFactionGetKicked();
                    break;

                case (int)CommonEnumerators.FactionManifestMode.AdminProtection:
                    OnFactionAdminProtection();
                    break;

                case (int)CommonEnumerators.FactionManifestMode.MemberList:
                    OnFactionMemberList(factionManifest);
                    break;
            }
        }

        public static void OnFactionOpen()
        {
            Action r3 = delegate
            {
                DialogManager.PushNewDialog(new RT_Dialog_Wait("RimworldTogether.FactionMemberList".Translate()));

                FactionManifestJSON factionManifestJSON = new FactionManifestJSON();
                factionManifestJSON.manifestMode = ((int)CommonEnumerators.FactionManifestMode.MemberList).ToString();

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.FactionPacket), factionManifestJSON);
                Network.listener.EnqueuePacket(packet);
            };

            Action r2 = delegate
            {
                FactionManifestJSON factionManifestJSON = new FactionManifestJSON();
                factionManifestJSON.manifestMode = ((int)CommonEnumerators.FactionManifestMode.RemoveMember).ToString();
                factionManifestJSON.manifestDetails = ClientValues.chosenSettlement.Tile.ToString();

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.FactionPacket), factionManifestJSON);
                Network.listener.EnqueuePacket(packet);
            };

            Action r1 = delegate
            {
                DialogManager.PushNewDialog(new RT_Dialog_Wait("RimworldTogether.FactionDeletion".Translate()));

                FactionManifestJSON factionManifestJSON = new FactionManifestJSON();
                factionManifestJSON.manifestMode = ((int)CommonEnumerators.FactionManifestMode.Delete).ToString();

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.FactionPacket), factionManifestJSON);
                Network.listener.EnqueuePacket(packet);
            };

            RT_Dialog_YesNo d3 = new RT_Dialog_YesNo("RimworldTogether.FactionLeave".Translate(), r2, null);

            RT_Dialog_YesNo d2 = new RT_Dialog_YesNo("RimworldTogether.FactionDelete".Translate(), r1, null);

            RT_Dialog_3Button d1 = new RT_Dialog_3Button("RimworldTogether.FactionManagement".Translate(), "RimworldTogether.FactionManagmentTab".Translate(),
                "RimworldTogether.Members".Translate(), "RimworldTogether.Delete".Translate(), "RimworldTogether.Leave".Translate(),
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
                    DialogManager.PushNewDialog(new RT_Dialog_Error("RimworldTogether.FactionInvalid".Translate()));
                }

                else
                {
                    DialogManager.PushNewDialog(new RT_Dialog_Wait("RimworldTogether.FactionCreatio".Translate()));

                    FactionManifestJSON factionManifestJSON = new FactionManifestJSON();
                    factionManifestJSON.manifestMode = ((int)CommonEnumerators.FactionManifestMode.Create).ToString();
                    factionManifestJSON.manifestDetails = DialogManager.dialog1ResultOne;

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.FactionPacket), factionManifestJSON);
                    Network.listener.EnqueuePacket(packet);
                }
            };
            RT_Dialog_1Input d2 = new RT_Dialog_1Input("RimworldTogether.FactionName".Translate(), "RimworldTogether.FactionNaming".Translate(), r2, null);

            Action r1 = delegate { DialogManager.PushNewDialog(d2); };
            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("RimworldTogether.FactionCreate".Translate(), r1, null);

            DialogManager.PushNewDialog(d1);
        }

        public static void OnFactionOpenOnMember()
        {
            Action r1 = delegate
            {
                FactionManifestJSON factionManifestJSON = new FactionManifestJSON();
                factionManifestJSON.manifestMode = ((int)CommonEnumerators.FactionManifestMode.Promote).ToString();
                factionManifestJSON.manifestDetails = ClientValues.chosenSettlement.Tile.ToString();

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.FactionPacket), factionManifestJSON);
                Network.listener.EnqueuePacket(packet);
            };

            Action r2 = delegate
            {
                FactionManifestJSON factionManifestJSON = new FactionManifestJSON();
                factionManifestJSON.manifestMode = ((int)CommonEnumerators.FactionManifestMode.Demote).ToString();
                factionManifestJSON.manifestDetails = ClientValues.chosenSettlement.Tile.ToString();

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.FactionPacket), factionManifestJSON);
                Network.listener.EnqueuePacket(packet);
            };

            Action r3 = delegate
            {
                FactionManifestJSON factionManifestJSON = new FactionManifestJSON();
                factionManifestJSON.manifestMode = ((int)CommonEnumerators.FactionManifestMode.RemoveMember).ToString();
                factionManifestJSON.manifestDetails = ClientValues.chosenSettlement.Tile.ToString();

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.FactionPacket), factionManifestJSON);
                Network.listener.EnqueuePacket(packet);
            };

            RT_Dialog_YesNo d5 = new RT_Dialog_YesNo("RimworldTogether.FactionDemote".Translate(), 
                r2,
                delegate { DialogManager.PushNewDialog(DialogManager.previousDialog); });

            RT_Dialog_YesNo d4 = new RT_Dialog_YesNo("RimworldTogether.FactionPromote".Translate(), 
                r1,
                delegate { DialogManager.PushNewDialog(DialogManager.previousDialog); });

            RT_Dialog_YesNo d3 = new RT_Dialog_YesNo("RimworldTogether.FactionKick".Translate(), 
                r3,
                delegate { DialogManager.PushNewDialog(DialogManager.previousDialog); });

            RT_Dialog_2Button d2 = new RT_Dialog_2Button("RimworldTogether.PowerManagementMenu".Translate(), "RimworldTogether.ManagementSelection".Translate(),
                "RimworldTogether.Promote".Translate(), "RimworldTogether.Demote".Translate(),
                delegate { DialogManager.PushNewDialog(d4); },
                delegate { DialogManager.PushNewDialog(d5); },
                null);

            RT_Dialog_2Button d1 = new RT_Dialog_2Button("RimworldTogether.ManagementMenu".Translate(), "RimworldTogether.ManagementSelection".Translate(), 
                "RimworldTogether.Powers".Translate(), "RimworldTogether.Kick".Translate(), 
                delegate { DialogManager.PushNewDialog(d2); }, 
                delegate { DialogManager.PushNewDialog(d3); }, 
                null);

            DialogManager.PushNewDialog(d1);
        }

        public static void OnFactionOpenOnNonMember()
        {
            Action r1 = delegate
            {
                FactionManifestJSON factionManifestJSON = new FactionManifestJSON();
                factionManifestJSON.manifestMode = ((int)CommonEnumerators.FactionManifestMode.AddMember).ToString();
                factionManifestJSON.manifestDetails = ClientValues.chosenSettlement.Tile.ToString();

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.FactionPacket), factionManifestJSON);
                Network.listener.EnqueuePacket(packet);
            };

            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("RimworldTogether.FactionInvite".Translate(), r1, null);
            DialogManager.PushNewDialog(d1);
        }

        private static void OnCreateFaction()
        {
            ServerValues.hasFaction = true;

            string[] messages = new string[]
            {
                "RimworldTogether.FactionCreated".Translate(),
                "RimworldTogether.FactionMenu".Translate()
            };

            DialogManager.PopWaitDialog();
            RT_Dialog_OK_Loop d1 = new RT_Dialog_OK_Loop(messages);
            DialogManager.PushNewDialog(d1);
        }

        private static void OnDeleteFaction()
        {
            ServerValues.hasFaction = false;

            if (!ClientValues.isInTransfer) DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_Error("RimworldTogether.FactionDelete".Translate()));
        }

        private static void OnFactionNameInUse()
        {
            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_Error("RimworldTogether.FactionOccupied".Translate()));
        }

        private static void OnFactionNoPower()
        {
            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_Error("RimworldTogether.FactionActionBan".Translate()));
        }

        private static void OnFactionGetInvited(FactionManifestJSON factionManifest)
        {
            Action r1 = delegate
            {
                ServerValues.hasFaction = true;

                factionManifest.manifestMode = ((int)CommonEnumerators.FactionManifestMode.AcceptInvite).ToString();

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.FactionPacket), factionManifest);
                Network.listener.EnqueuePacket(packet);
            };

            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo($"AcceptInvite".Translate(factionManifest.manifestDetails), r1, null);
            DialogManager.PushNewDialog(d1);
        }

        private static void OnFactionGetKicked()
        {
            ServerValues.hasFaction = false;

            DialogManager.PushNewDialog(new RT_Dialog_OK("RimworldTogether.FactionKicked".Translate()));
        }

        private static void OnFactionAdminProtection()
        {
            DialogManager.PushNewDialog(new RT_Dialog_Error("RimworldTogether.FactionLeaderActionBan".Translate()));
        }

        private static void OnFactionMemberList(FactionManifestJSON factionManifest)
        {
            DialogManager.PopWaitDialog();

            List<string> unraveledDetails = new List<string>();
            for (int i = 0; i < factionManifest.manifestComplexDetails.Count(); i++)
            {
                unraveledDetails.Add($"{factionManifest.manifestComplexDetails[i]} " +
                    $"- {(CommonEnumerators.FactionRanks)int.Parse(factionManifest.manifestSecondaryComplexDetails[i])}");
            }

            RT_Dialog_Listing d1 = new RT_Dialog_Listing("RimworldTogether.FactionMembers".Translate(), 
                "RimworldTogether.MembersTab".Translate(), unraveledDetails.ToArray());

            DialogManager.PushNewDialog(d1);
        }
    }
}
