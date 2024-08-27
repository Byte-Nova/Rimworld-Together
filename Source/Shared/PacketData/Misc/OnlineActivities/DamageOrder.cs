using System;

namespace Shared
{
    [Serializable]
    public class DamageOrder
    {
        public int targetIndex;

        public string defName;

        public string hitPartDefName;

        public float damageAmount;

        public string weaponDefName;

        public float armorPenetration;
        
        public bool ignoreArmor;
    }
}