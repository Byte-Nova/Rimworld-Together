using HugsLib.Utils;
using RimWorld;
using RimworldTogether.GameClient.Managers.Actions;
using UnityEngine;
using Verse;

namespace RimworldTogether.GameClient.Dialogs
{
    public class RT_Dialog_Wait : Window
    {
        public override Vector2 InitialSize => new Vector2(300f, 100f);

        private string title = "WAIT";
        private string description = "";

        public RT_Dialog_Wait(string description)
        {
            DialogManager.dialogWait = this;
            this.description = description;

            forcePause = true;
            absorbInputAroundWindow = true;
            soundAppear = SoundDefOf.CommsWindow_Open;
            //soundClose = SoundDefOf.CommsWindow_Close;

            closeOnAccept = false;
            closeOnCancel = false;
        }


        public override void DoWindowContents(Rect rect)
        {
            AllowCloseDialog();

            float centeredX = rect.width / 2;
            float horizontalLineDif = Text.CalcSize(description).y + StandardMargin / 2;
            float windowDescriptionDif = Text.CalcSize(description).y + StandardMargin;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(title).x / 2, rect.y, Text.CalcSize(title).x, Text.CalcSize(title).y), title);

            Widgets.DrawLineHorizontal(rect.x, horizontalLineDif, rect.width);

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(description).x / 2, windowDescriptionDif, Text.CalcSize(description).x, Text.CalcSize(description).y), description);
        }

        private void AllowCloseDialog()
        {
            if (HugsLibUtility.ShiftIsHeld) closeOnCancel = true;
            else closeOnCancel = false;
        }
    }
}
