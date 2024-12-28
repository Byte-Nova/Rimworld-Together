using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace GameClient
{
    public static class FactionValues
    {
        public static List<Faction> playerFactions = new List<Faction>();
        public static Faction neutralPlayer;
        public static Faction allyPlayer;
        public static Faction enemyPlayer;
        public static Faction yourOnlineFaction;

        public static void FindPlayerFactionsInWorld()
        {
            Faction[] factions = Find.FactionManager.AllFactions.ToArray();
            neutralPlayer = factions.First(fetch => fetch.def.defName == RTFactionDefOf.RTNeutral.defName);
            allyPlayer = factions.First(fetch => fetch.def.defName == RTFactionDefOf.RTAlly.defName);
            enemyPlayer = factions.First(fetch => fetch.def.defName == RTFactionDefOf.RTEnemy.defName);
            yourOnlineFaction = factions.First(fetch => fetch.def.defName == RTFactionDefOf.RTFaction.defName);

            playerFactions.Clear();
            playerFactions.Add(neutralPlayer);
            playerFactions.Add(allyPlayer);
            playerFactions.Add(enemyPlayer);
            playerFactions.Add(yourOnlineFaction);
        }
    }
}
