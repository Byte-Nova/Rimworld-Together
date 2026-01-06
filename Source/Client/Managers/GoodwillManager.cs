using GameClient.Dialogs;
using GameClient.Misc;
using GameClient.WorldObjects;
using RimWorld;
using RimWorld.Planet;
using Shared;
using System.Collections.Generic;
using System.Linq;
using Verse;
using static Shared.CommonEnumerators;
using static UnityEngine.GraphicsBuffer;
using TCPNetwork.Packets.Goodwills;
using Shared.Files.Sites;


namespace GameClient.Managers
{
    //Class that handles settlement and site player goodwills

    public static class GoodwillManager
    {
        [HandlesPacket(PacketHeader.GoodWillManager)]
        private static void ParsePacket(byte[] bytes)
        {
            FactionGoodwillData data = Serializer.ConvertBytesToObject<FactionGoodwillData>(bytes);

            ChangeStructureGoodwill(data);
            RT_Dialog_Wait.Instance.Close();
        }

        //Tries to request a goodwill change depending on the values given

        public static void TryRequestGoodwill(Goodwill type, GoodwillTarget target)
        {
            int tileToUse = 0;
            if (target == GoodwillTarget.Settlement) tileToUse = SessionHandler.ChosenSettlement.Tile;
            else if (target == GoodwillTarget.Site) tileToUse = SessionHandler.ChosenSite.Tile;

            Faction factionToUse = null;
            if (target == GoodwillTarget.Settlement) factionToUse = SessionHandler.ChosenSettlement.Faction;
            else if (target == GoodwillTarget.Site) factionToUse = SessionHandler.ChosenSite.Faction;

            if (type == Goodwill.Enemy)
            {
                if (factionToUse == SessionHandler.EnemyFaction)
                {
                    RT_Dialog_Message d1 = new RT_Dialog_Message("ERROR", new string[] { "Chosen settlement is already marked as enemy!" });
                    RT_Dialog_Base.PushNewDialog(d1);
                }
                else RequestChangeStructureGoodwill(tileToUse, Goodwill.Enemy);
            }

            else if (type == Goodwill.Neutral)
            {
                if (factionToUse == SessionHandler.NeutralFaction)
                {
                    RT_Dialog_Message d1 = new RT_Dialog_Message("ERROR", new string[] { "Chosen settlement is already marked as neutral!" });
                    RT_Dialog_Base.PushNewDialog(d1);
                }
                else RequestChangeStructureGoodwill(tileToUse, Goodwill.Neutral);
            }

            else if (type == Goodwill.Ally)
            {
                if (factionToUse == SessionHandler.AllyFaction)
                {
                    RT_Dialog_Message d1 = new RT_Dialog_Message("ERROR", new string[] { "Chosen settlement is already marked as ally!" });
                    RT_Dialog_Base.PushNewDialog(d1);
                }
                else RequestChangeStructureGoodwill(tileToUse, Goodwill.Ally);
            }
        }

        //Requests a structure goodwill change to the server

        public static void RequestChangeStructureGoodwill(int structureTile, Goodwill goodwill)
        {
            FactionGoodwillData factionGoodwillData = new FactionGoodwillData();
            factionGoodwillData._tile = structureTile;
            factionGoodwillData._goodwill = goodwill;

            ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.GoodWillManager, factionGoodwillData);

            RT_Dialog_Wait d1 = new RT_Dialog_Wait("Changing settlement goodwill");
            RT_Dialog_Base.PushNewDialog(d1);
        }

        //Changes a structure goodwill from a packet

        public static void ChangeStructureGoodwill(FactionGoodwillData data)
        {
            ChangeSettlementGoodwills(data);
            ChangeSiteGoodwills(data);
        }

        private static void ChangeSettlementGoodwills(FactionGoodwillData factionGoodwillData)
        {
            foreach (SettlementGoodwill _ in factionGoodwillData._settlements)
            {
                RTSettlement settlement = (RTSettlement)Find.WorldObjects.AllWorldObjects.First(fetch => fetch.Tile == _.Tile && fetch is RTSettlement);
                if (settlement.Faction == Faction.OfPlayer) continue;
                else
                {
                    SettlementManager.PlayerSettlements.Remove(settlement);
                    Find.WorldObjects.Remove(settlement);

                    WorldObjectDef def = DefDatabase<WorldObjectDef>.AllDefs.First(fetch => fetch.defName == "RTSettlement");
                    RTSettlement newSettlement = (RTSettlement)WorldObjectMaker.MakeWorldObject(def);
                    newSettlement.Tile = settlement.Tile;
                    newSettlement.Name = settlement.Name;
                    newSettlement.SetFaction(PlanetManagerHelper.GetPlayerFactionFromGoodwill(_.Goodwill));

                    SettlementManager.PlayerSettlements.Add(newSettlement);
                    Find.WorldObjects.Add(newSettlement);
                }
            }
        }

        private static void ChangeSiteGoodwills(FactionGoodwillData factionGoodwillData)
        {
            foreach (SiteGoodwill _ in factionGoodwillData._sites) 
            {
                RTSite site = (RTSite)Find.WorldObjects.AllWorldObjects.First(fetch => fetch.Tile == _.Tile && fetch is RTSite);

                SiteManager.RecalculateSiteGoodwill(site, _.Goodwill);
            }
        }
    }
}
