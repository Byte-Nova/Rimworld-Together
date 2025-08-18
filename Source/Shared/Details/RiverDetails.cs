using System;
using Shared.Misc;

namespace Shared
{
    [Serializable]
    public class RiverDetails
    {
        public string RiverDefName { get; set; }

        public int FromTile { get; set; }

        public int ToTile { get; set; }
        public RiverDetails(int fromTile, int toTile, string defname)
        {
            FromTile = fromTile;
            ToTile = toTile;
            RiverDefName = Pools.StringPool.GetOrAddString(defname);
        }
    }
}