using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class CreationOrderData
    {
        public byte[] _dataToCreate;
        
        public CreationType _creationType;
    }
}