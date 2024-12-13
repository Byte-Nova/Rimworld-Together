using System;

namespace Shared
{
    [Serializable]
    public class DamageOrderData
    {
        public string targetHash;

        public string _defName;

        public string _hitPartDefName;

        public float _damageAmount;

        public string _weaponDefName;

        public float _armorPenetration;
        
        public bool _ignoreArmor;
    }
}