using GameClient.Defs;
using GameClient.Dialogs;
using GameClient.Managers;
using GameClient.Misc;
using RimWorld;
using RimWorld.Planet;
using Shared;
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
    public class RTSite : MapParent
    {
        private Material cachedMat;

        public override string Label => base.Label;

        public SitePartDef MainSitePartDef => MainSitePart.def;

        public List<RTSitePart> parts = new List<RTSitePart>();

        private RTSitePart MainSitePart { get { return parts[0]; } }

        public override Texture2D ExpandingIcon => MainSitePartDef.ExpandingIconTexture;

        public override Material Material
        {
            get
            {
                if (cachedMat == null)
                {
                    cachedMat = MaterialPool.MatFrom(color: (!MainSitePartDef.applyFactionColorToSiteTexture || base.Faction == null) ? 
                        Color.white : base.Faction.Color, texPath: MainSitePartDef.siteTexture, shader: ShaderDatabase.WorldOverlayTransparentLit, renderQueue: 3550);
                }

                return cachedMat;
            }
        }

        public void AddPart(RTSitePart part)
        {
            if (!part.def.forceMutators.NullOrEmpty())
            {
                foreach (TileMutatorDef forceMutator in part.def.forceMutators)
                {
                    base.Tile.Tile.AddMutator(forceMutator);
                }
            }

            parts.Add(part);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            List<Gizmo> gizmoList = new List<Gizmo>();

            Command_Action command_DestroySite = new Command_Action
            {
                defaultLabel = "Destroy site",
                defaultDesc = "Destroy this site",
                icon = ContentFinder<Texture2D>.Get("Commands/Site"),
                action = delegate
                {
                    if (SessionHandler.CurrentActionValues.SiteAction.IsEnabled)
                    {
                        SessionHandler.ChosenSite = this;
                        SiteManager.RequestDestroySite();
                    }
                    else RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "This feature has been disabled in this server!" }));
                }
            };

            if (Faction == Find.FactionManager.OfPlayer || Faction == SessionHandler.GuildFaction) gizmoList.Add(command_DestroySite);

            return gizmoList;
        }

        public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
        {
            List<Gizmo> gizmoList = new List<Gizmo>();

            Command_Action command_Info = new Command_Action
            {
                defaultLabel = "Info",
                defaultDesc = "Shows if the player is connected",
                icon = ContentFinder<Texture2D>.Get("Commands/Worker"),
                action = delegate
                {
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Wait());
                    SessionHandler.ChosenCaravan = caravan;
                    SessionHandler.ChosenSite = this;
                    SiteManager.AskForInformation();
                }
            };

            if (Faction == Find.FactionManager.OfPlayer || Faction == SessionHandler.GuildFaction) gizmoList.Add(command_Info);

            return gizmoList;
        }
    }
}
