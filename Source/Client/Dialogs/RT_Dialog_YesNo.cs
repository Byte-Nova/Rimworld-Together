using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace GameClient
{
    public class RT_Dialog_YesNo : Window
    {
        public override Vector2 InitialSize => new Vector2(400f, 150f);

        private readonly string title = "RTDialogOption".Translate();

        private readonly string description = "";

        private readonly float buttonX = 150f;

        private readonly float buttonY = 38f;

        private readonly Action actionYes;
        
        private readonly Action actionNo;

        public RT_Dialog_YesNo(string description, Action actionYes, Action actionNo)
        {
            DialogManager.dialogYesNo = this;
            this.description = description;
            this.actionYes = actionYes;
            this.actionNo = actionNo;

            forcePause = true;
            absorbInputAroundWindow = true;

            soundAppear = SoundDefOf.CommsWindow_Open;
            

            closeOnAccept = false;
            closeOnCancel = false;
        }

        public override void DoWindowContents(Rect rect)
        {
            float centeredX = rect.width / 2;
            float horizontalLineDif = Text.CalcSize(description).y + StandardMargin / 2;
            float windowDescriptionDif = Text.CalcSize(description).y + StandardMargin;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(title).x / 2, rect.y, Text.CalcSize(title).x, Text.CalcSize(title).y), title);

            Widgets.DrawLineHorizontal(rect.x, horizontalLineDif, rect.width);

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(description).x / 2, windowDescriptionDif, Text.CalcSize(description).x, Text.CalcSize(description).y), description);

            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMin, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "RTDialogConfirm".Translate()))
            {
                if (actionYes != null) actionYes.Invoke();
                Close();
            }

            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMax - buttonX, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "RTDialogCancel".Translate()))
            {
                if (actionNo != null) actionNo.Invoke();
                Close();
            }
        }
    }
}
