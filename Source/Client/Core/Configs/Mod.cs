using UnityEngine;
using Verse;

namespace GameClient
{
    public class Mod : Verse.Mod
    {
        public Mod (ModContentPack content) : base(content) { }

        public override void DoSettingsWindowContents(Rect inRect){base.DoSettingsWindowContents(inRect);}
    }
}