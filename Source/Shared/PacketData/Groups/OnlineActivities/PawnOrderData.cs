using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class PawnOrderData
    {
        public PawnOrderComponent[] _pawnOrders = new PawnOrderComponent[0];
    }
}