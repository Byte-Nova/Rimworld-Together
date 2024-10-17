using static Shared.CommonEnumerators;

namespace Shared
{
    public class PawnOrderComponent
    {
        public string _jobDefName;

        public int _jobThingCount;

        public string _pawnId;

        public bool _isDrafted;

        public int[] _updatedPosition;
        
        public int _updatedRotation;

        public PawnTargetComponent _targetComponent = new PawnTargetComponent();
    }
}