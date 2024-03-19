using HugsLib.Utils;
using RimWorld;
using UnityEngine;
using Verse;
using System;

namespace GameClient
{
    public class RT_Dialog_Wait : Window
    {
        public override Vector2 InitialSize => new Vector2(300f, 100f);

        private string title = "WAIT";
        private string description = "";

        int FramesRan = 0;

        Action ActionToWaitOn;

        public RT_Dialog_Wait(string description, Action ActionToWaitOn = null)
        {
            this.description = description;

            forcePause = true;
            absorbInputAroundWindow = true;
            soundAppear = SoundDefOf.CommsWindow_Open;
            this.ActionToWaitOn = ActionToWaitOn;
            //soundClose = SoundDefOf.CommsWindow_Close;
            this.
            closeOnAccept = false;
            closeOnCancel = false;

        }


        public override void DoWindowContents(Rect rect)
        {
            //AllowCloseDialog();

            float centeredX = rect.width / 2;
            float horizontalLineDif = Text.CalcSize(description).y + StandardMargin / 2;
            float windowDescriptionDif = Text.CalcSize(description).y + StandardMargin;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(title).x / 2, rect.y, Text.CalcSize(title).x, Text.CalcSize(title).y), title);

            Widgets.DrawLineHorizontal(rect.x, horizontalLineDif, rect.width);

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(description).x / 2, windowDescriptionDif, Text.CalcSize(description).x, Text.CalcSize(description).y), description);

            //If a wait dialog is waiting for a process to finish,
            //The wait dialog will close itself and begin running the process
            //The box won't be "undrawn" until after the process finishes
            //magic voodoo witchery
            if ((ActionToWaitOn != null) && (FramesRan == 5)) {
                DialogManager.PopDialog();
            }
            FramesRan++;
        }

        public override void PostClose()
        {
            base.PostOpen();
            if(ActionToWaitOn != null) ActionToWaitOn.Invoke();
        }

        private void AllowCloseDialog()
        {
            if (HugsLibUtility.ShiftIsHeld) closeOnCancel = true;
            else closeOnCancel = false;
        }
    }
}
