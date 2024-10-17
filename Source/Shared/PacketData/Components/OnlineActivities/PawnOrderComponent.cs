using static Shared.CommonEnumerators;

namespace Shared
{
    public class PawnOrderComponent
    {
        public string _jobDefName;

        public string _pawnHash;

        public bool _isDrafted;

        public int[] _updatedPosition;
        
        public int _updatedRotation;

        public PawnTargetComponent _targetComponent = new PawnTargetComponent();

        public PawnTargetComponent _globalTargetComponent = new PawnTargetComponent();

        public PawnTargetComponent[] _queuedTargetComponentsA = new PawnTargetComponent[0];

        public PawnTargetComponent[] _queuedTargetComponentsB = new PawnTargetComponent[0];
    }
}