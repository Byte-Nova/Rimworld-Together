using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class DamageOrder
    {
        public OnlineActivityTargetType targetType;
        public int targetIndex;

        public string defName;
        public string hitPartDefName;
        public float damageAmount;
        public string weaponDefName;
        public float armorPenetration;
        public bool ignoreArmor;
    }
}