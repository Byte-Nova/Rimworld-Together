using UnityEngine;
using Verse;

namespace GameClient.Dialogs
{
    public class RT_Dialog_Wait : RT_Dialog_Base
    {
        public override Vector2 InitialSize => new Vector2(300f, 100f);

        public static RT_Dialog_Base Instance { get; private set; } = null;

        public RT_Dialog_Wait(string description)
        {
            Instance = this;
            this.Title = "WAIT";
            this.Description = description;

            closeOnAccept = false;
            closeOnCancel = false;
        }


        public override void DoWindowContents(Rect rect)
        {
            float centeredX = rect.width / 2;
            float horizontalLineDif = Text.CalcSize(Description).y + StandardMargin / 2;
            float windowDescriptionDif = Text.CalcSize(Description).y + StandardMargin;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(Title).x / 2, rect.y, Text.CalcSize(Title).x, Text.CalcSize(Title).y), Title);

            Widgets.DrawLineHorizontal(rect.x, horizontalLineDif, rect.width);

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(Description).x / 2, windowDescriptionDif, Text.CalcSize(Description).x, Text.CalcSize(Description).y), Description);
        }
    }
}
