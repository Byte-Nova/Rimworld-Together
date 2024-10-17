using System.Collections.Generic;
using Verse;

namespace GameClient
{
    public static class ExtensionManager
    {
        public static string GetThingHash(Thing thing)
        {
            return thing.ThingID;
        }

        public static void SetThingHash(Thing thing, string newHash)
        {
            thing.ThingID = newHash;
        }
    }
}