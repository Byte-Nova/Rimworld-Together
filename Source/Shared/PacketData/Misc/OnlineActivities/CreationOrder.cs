using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class CreationOrder
    {
        public byte[] dataToCreate;
        
        public CreationType creationType;
    }
}