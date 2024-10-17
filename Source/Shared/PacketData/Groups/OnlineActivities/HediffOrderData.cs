using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class HediffOrderData
    {
        public OnlineActivityTargetFaction _pawnFaction;

        public OnlineActivityApplyMode _applyMode;

        public HediffComponent _hediffComponent = new HediffComponent();

        public string targetID;
    }
}