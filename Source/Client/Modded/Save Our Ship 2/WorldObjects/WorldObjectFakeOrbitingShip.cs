using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public class WorldObjectFakeOrbitingShip : WorldObject
    {
        public override Vector3 DrawPos
        {
            get
            {
                return drawPos;
            }
        }
        public override string Label
        {
            get
            {
                if (name == null)
                {
                    return base.Label;
                }
                return name;
            }
        }
        public string name;
        public Vector3 drawPos;
        Vector3 targetDrawPos = new Vector3(0, 0, 0);
        Vector3 originDrawPos = new Vector3(0, 0, 0);
        public float radius;
        public float phi;
        public float theta;
        public float altitude;

        public void OrbitSet() 
        {
            Vector3 v = Vector3.SlerpUnclamped(new Vector3(0, 0, 1) * radius, new Vector3(0, 0, 1) * radius * -1, theta * -1);
            drawPos = new Vector3(v.x, phi, v.z);
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (FactionValues.playerFactions.Contains(Faction))
            {
                var gizmoList = new List<Gizmo>();
                gizmoList.Clear();

                Command_Action command_Goodwill = new Command_Action
                {
                    defaultLabel = "Change Goodwill",
                    defaultDesc = "Change the goodwill of this settlement",
                    icon = ContentFinder<Texture2D>.Get("Commands/Goodwill"),
                    action = delegate
                    {
                        SessionValues.chosenWorldObject = this;

                        Action r1 = delegate {
                            GoodwillManager.TryRequestGoodwill(Goodwill.Enemy,
                            GoodwillTarget.Ship);
                        };

                        Action r2 = delegate {
                            GoodwillManager.TryRequestGoodwill(Goodwill.Neutral,
                            GoodwillTarget.Ship);
                        };

                        Action r3 = delegate {
                            GoodwillManager.TryRequestGoodwill(Goodwill.Ally,
                            GoodwillTarget.Ship);
                        };

                        RT_Dialog_3Button d1 = new RT_Dialog_3Button("Change Goodwill", "Set settlement's goodwill to",
                            "Enemy", "Neutral", "Ally", r1, r2, r3, null);

                        DialogManager.PushNewDialog(d1);
                    }
                };

                Command_Action command_FactionMenu = new Command_Action
                {
                    defaultLabel = "Faction Menu",
                    defaultDesc = "Access your faction menu",
                    icon = ContentFinder<Texture2D>.Get("Commands/FactionMenu"),
                    action = delegate
                    {
                        SessionValues.chosenWorldObject = this;

                        if (SessionValues.chosenSettlement.Faction == FactionValues.yourOnlineFaction) FactionManager.OnFactionOpenOnMember();
                        else FactionManager.OnFactionOpenOnNonMember();
                    }
                };

                Command_Action command_Event = new Command_Action
                {
                    defaultLabel = "Send Event",
                    defaultDesc = "Send an event to this settlement",
                    icon = ContentFinder<Texture2D>.Get("Commands/Event"),
                    action = delegate
                    {
                        SessionValues.chosenWorldObject = this;

                        EventManager.ShowEventMenu();
                    }
                };

                if (this.Faction != FactionValues.yourOnlineFaction) gizmoList.Add(command_Goodwill);
                if (ServerValues.hasFaction) gizmoList.Add(command_FactionMenu);
                gizmoList.Add(command_Event);
                return gizmoList;
            }

            else if (this.Faction == Find.FactionManager.OfPlayer)
            {
                var gizmoList = new List<Gizmo>();

                Command_Action command_FactionMenu = new Command_Action
                {
                    defaultLabel = "Faction Menu",
                    defaultDesc = "Access your faction menu",
                    icon = ContentFinder<Texture2D>.Get("Commands/FactionMenu"),
                    action = delegate
                    {
                        SessionValues.chosenWorldObject = this;

                        if (ServerValues.hasFaction) FactionManager.OnFactionOpen();
                        else FactionManager.OnNoFactionOpen();
                    }
                };

                Command_Action command_GlobalMarketMenu = new Command_Action
                {
                    defaultLabel = "Global Market Menu",
                    defaultDesc = "Access the global market",
                    icon = ContentFinder<Texture2D>.Get("Commands/GlobalMarket"),
                    action = delegate
                    {
                        SessionValues.chosenSettlement = Find.WorldObjects.Settlements.First(fetch => fetch.Faction == Faction.OfPlayer);

                        if (SessionValues.actionValues.EnableMarket)
                        {
                            if (RimworldManager.CheckIfPlayerHasConsoleInMap(SessionValues.chosenSettlement.Map)) MarketManager.RequestReloadStock();
                            else DialogManager.PushNewDialog(new RT_Dialog_Error("You need a comms console to use the market!"));
                        }
                        else DialogManager.PushNewDialog(new RT_Dialog_Error("The market has been disabled in this server!"));
                    }
                };

                gizmoList.Add(command_GlobalMarketMenu);
                gizmoList.Add(command_FactionMenu);
                return gizmoList;
            }
            return base.GetGizmos();
        }
    }
}
