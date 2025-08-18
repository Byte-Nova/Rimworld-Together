using GameClient.Dialogs;
using GameClient.Managers;
using GameClient.Values;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.WorldObjects
{
    public class RTSettlement : MapParent
    {
        private string nameInt;

        private Material cachedMat;

        public override string Label => nameInt ?? base.Label;

        public override Texture2D ExpandingIcon => base.Faction.def.FactionIcon;

        public string Name
        {
            get { return nameInt; }
            set { nameInt = value; }
        }

        public override Material Material
        {
            get
            {
                if (cachedMat == null)
                {
                    cachedMat = MaterialPool.MatFrom(base.Faction.def.settlementTexturePath, 
                        ShaderDatabase.WorldOverlayTransparentLit, base.Faction.Color, 3550);
                }

                return cachedMat;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            List<Gizmo> gizmos = new List<Gizmo>();

            Command_Action command_Goodwill = new Command_Action
            {
                defaultLabel = "Change Goodwill",
                defaultDesc = "Change the goodwill of this settlement",
                icon = ContentFinder<Texture2D>.Get("Commands/Goodwill"),
                action = delegate
                {
                    SessionValues.ChosenSettlement = this;

                    Action r1 = delegate
                    {
                        GoodwillManager.TryRequestGoodwill(Goodwill.Enemy,
                        GoodwillTarget.Settlement);
                    };

                    Action r2 = delegate
                    {
                        GoodwillManager.TryRequestGoodwill(Goodwill.Neutral,
                        GoodwillTarget.Settlement);
                    };

                    Action r3 = delegate
                    {
                        GoodwillManager.TryRequestGoodwill(Goodwill.Ally,
                        GoodwillTarget.Settlement);
                    };

                    RT_Dialog_Buttons d1 = new RT_Dialog_Buttons("Change Goodwill", "Set settlement's goodwill to",
                        new string[] { "Enemy", "Neutral", "Ally" },
                        new Action[] { r1, r2, r3 },
                        null);

                    RT_Dialog_Base.PushNewDialog(d1);
                }
            };

            Command_Action command_FactionMenu = new Command_Action
            {
                defaultLabel = "Faction Menu",
                defaultDesc = "Access your faction menu",
                icon = ContentFinder<Texture2D>.Get("Commands/FactionMenu"),
                action = delegate
                {
                    SessionValues.ChosenSettlement = this;

                    if (SessionValues.ActionValues.EnableFactions)
                    {
                        if (SessionValues.ChosenSettlement.Faction == ClientValues.YourOnlineFaction) GuildManager.OnFactionOpenOnMember();
                        else GuildManager.OnFactionOpenOnNonMember();
                    }
                    else RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "This feature has been disabled in this server!" }));
                }
            };

            Command_Action command_Caravan = new Command_Action
            {
                defaultLabel = "Form Caravan",
                defaultDesc = "Form a new caravan",
                icon = ContentFinder<Texture2D>.Get("UI/Commands/FormCaravan"),
                action = delegate
                {
                    SessionValues.ChosenSettlement = this;

                    Dialog_FormCaravan d1 = new Dialog_FormCaravan(this.Map, mapAboutToBeRemoved: true);
                    RT_Dialog_Base.PushNewDialog(d1);
                }
            };

            Command_Action command_Aid = new Command_Action
            {
                defaultLabel = "Aid",
                defaultDesc = "Send aid to this settlement",
                icon = ContentFinder<Texture2D>.Get("Commands/Aid"),
                action = delegate
                {
                    SessionValues.ChosenSettlement = this;

                    if (SessionValues.ActionValues.EnableAids)
                    {
                        List<string> pawnNames = new List<string>();
                        foreach (Pawn pawn in RimworldManager.GetAllSettlementsPawns(Faction.OfPlayer, false)) pawnNames.Add(pawn.LabelCapNoCount);
                        RT_Dialog_Base.PushNewDialog(new RT_Dialog_ListingWithButton("Aid menu", "Select the pawn you want to send for aid",
                            pawnNames.ToArray(), AidManager.SendAidRequest));
                    }
                    else RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "This feature has been disabled in this server!" }));
                }
            };

            Command_Action command_Event = new Command_Action
            {
                defaultLabel = "Send Event",
                defaultDesc = "Send an event to this settlement",
                icon = ContentFinder<Texture2D>.Get("Commands/Event"),
                action = delegate
                {
                    SessionValues.ChosenSettlement = this;

                    if (SessionValues.ActionValues.EnableEvents) EventManager.ShowEventMenu();
                    else RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "This feature has been disabled in this server!" }));
                }
            };

            Command_Action command_Zoom = new Command_Action
            {
                defaultLabel = "View",
                defaultDesc = "View this settlement",
                icon = ContentFinder<Texture2D>.Get("Commands/View"),
                action = delegate
                {
                    SessionValues.ChosenSettlement = this;

                    ActivityManager.RequestActivity(ActivityType.Zoom,
                        SessionValues.ChosenSettlement.Tile);
                }
            };

            Command_Action command_StopZoom = new Command_Action
            {
                defaultLabel = "Stop viewing",
                defaultDesc = "Stops viewing this settlement",
                icon = ContentFinder<Texture2D>.Get("Commands/View"),
                action = delegate
                {
                    SessionValues.ChosenSettlement = this;
                    SessionValues.ChosenSettlement.Destroy();
                    Find.WorldObjects.Add(SessionValues.ChosenSettlement);
                }
            };

            Command_Action command_Info = new Command_Action
            {
                defaultLabel = "Info",
                defaultDesc = "Shows if the player is connected",
                icon = ContentFinder<Texture2D>.Get("Commands/Info"),
                action = delegate
                {
                    SessionValues.ChosenSettlement = this;
                    InformationManager.AskForInformation();
                }
            };

            Command_Action command_Wealth = new Command_Action
            {
                defaultLabel = "Wealth",
                defaultDesc = "Shows the selected settlement's wealth",
                icon = ContentFinder<Texture2D>.Get("Commands/Wealth"),
                action = delegate
                {
                    SessionValues.ChosenSettlement = this;
                    InformationManager.AskForWealth();
                }
            };

            gizmos.Add(command_Info);
            gizmos.Add(command_Wealth);
            gizmos.Add(command_Goodwill);
            gizmos.Add(command_Event);
            gizmos.Add(command_Aid);

            if (this.Map == null) gizmos.Add(command_Zoom);
            else gizmos.Add(command_StopZoom);

            if (this.Map != null) gizmos.Add(command_Caravan);
            if (ClientValues.HasFaction) gizmos.Add(command_FactionMenu);

            return gizmos;
        }

        public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
        {
            List<Gizmo> gizmos = new List<Gizmo>();

            Command_Action command_Raid = new Command_Action
            {
                defaultLabel = "Raid",
                defaultDesc = "Raid this location",
                icon = ContentFinder<Texture2D>.Get("Commands/Raid"),
                action = delegate
                {
                    SessionValues.ChosenSettlement = this;
                    SessionValues.ChosenCaravan = caravan;

                    ActivityManager.RequestActivity(ActivityType.Raid,
                        SessionValues.ChosenSettlement.Tile);
                }
            };

            Command_Action command_Transfer = new Command_Action
            {
                defaultLabel = "Transfer Items",
                defaultDesc = "Transfer items between settlements",
                icon = ContentFinder<Texture2D>.Get("Commands/Transfer"),
                action = delegate
                {
                    SessionValues.ChosenSettlement = this;
                    SessionValues.ChosenCaravan = caravan;

                    if (!SessionValues.ActionValues.EnableTrading)
                    {
                        RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "This feature has been disabled in this server!" }));
                        return;
                    }

                    else
                    {
                        Settlement settlement = Find.World.worldObjects.Settlements.First(fetch => fetch.Faction != Faction.OfPlayer);
                        Pawn negotiator = RimworldManager.GetIfSocialPawnInCaravan(SessionValues.ChosenCaravan);

                        if (negotiator != null)
                        {
                            ClientValues.ToggleTradeStep(ClientValues.TradeMode.Sending);
                            Find.WindowStack.Add(new Dialog_Trade(negotiator, settlement));
                        }

                        else
                        {
                            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "You do not have any pawn capable of trading!" }));
                        }
                    }
                }
            };

            gizmos.Add(command_Raid);
            gizmos.Add(command_Transfer);

            return gizmos;
        }
    }
}
