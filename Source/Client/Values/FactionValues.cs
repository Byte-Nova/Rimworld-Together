using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RimworldTogether.GameClient.Values
{
    public static class FactionValues
    {
        public static FactionDef neutralPlayerDef;
        public static FactionDef allyPlayerDef;
        public static FactionDef enemyPlayerDef;
        public static FactionDef yourOnlineFactionDef;

        public static List<Faction> playerFactions = new List<Faction>();
        public static Faction neutralPlayer;
        public static Faction allyPlayer;
        public static Faction enemyPlayer;
        public static Faction yourOnlineFaction;

        public static void SetPlayerFactionDefs()
        {
            FactionDef[] factions = DefDatabase<FactionDef>.AllDefs.ToArray();
            neutralPlayerDef = factions.First(fetch => fetch.defName == "RTNeutral");
            allyPlayerDef = factions.First(fetch => fetch.defName == "RTAlly");
            enemyPlayerDef = factions.First(fetch => fetch.defName == "RTEnemy");
            yourOnlineFactionDef = factions.First(fetch => fetch.defName == "RTFaction");
        }

        public static void FindPlayerFactionsInWorld()
        {
            Faction[] factions = Find.FactionManager.AllFactions.ToArray();
            neutralPlayer = factions.First(fetch => fetch.def.defName == "RTNeutral");
            allyPlayer = factions.First(fetch => fetch.def.defName == "RTAlly");
            enemyPlayer = factions.First(fetch => fetch.def.defName == "RTEnemy");
            yourOnlineFaction = factions.First(fetch => fetch.def.defName == "RTFaction");

            playerFactions.Clear();
            playerFactions.Add(neutralPlayer);
            playerFactions.Add(allyPlayer);
            playerFactions.Add(enemyPlayer);
            playerFactions.Add(yourOnlineFaction);
        }
    }
}
