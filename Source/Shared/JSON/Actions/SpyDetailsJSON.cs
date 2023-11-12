using System;

namespace RimworldTogether.Shared.JSON.Actions
{
    [Serializable]
    public class SpyDetailsJSON
    {
        public string spyStepMode;

        public string targetTile;

        public byte[] mapDetails;
    }
}
