using System;
using System.Collections.Generic;
using System.Drawing;

namespace Shared
{
    [Serializable]
    public class FactionData
    {
        public string Name;
        public float colorFromSpectrum;
        public bool neverFlee;

        public string localDefName;
        public string defName;
        public string fixedName;
        public bool autoFlee;
        public bool canSiege;
        public bool canStageAttacks;
        public bool canUseAvoidGrid;
        public float earliestRaidDays;
        public bool rescueesCanJoin;
        public bool naturalEnemy;
        public bool permanentEnemy;
        public bool permanentEnemyToEveryoneExceptPlayer;
        public byte techLevel;
        public string factionIconPath;
        public string settlementTexturePath;
        public bool hidden;
    }
}