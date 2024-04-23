using HugsLib.Utils;
using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace GameClient
{
    public class RT_Dialog_Wait : Window
    {
        public override Vector2 InitialSize => new Vector2(300f, 100f);

        private string title = "WAIT";
        private string description = "";

        private int framesRan = 0;

        Action actionToWaitOn;

        public RT_Dialog_Wait(string description, Action actionToWaitOn = null)
        {
            this.description = description;
            this.actionToWaitOn = actionToWaitOn;

            forcePause = true;
            absorbInputAroundWindow = true;
            soundAppear = SoundDefOf.CommsWindow_Open;

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

            //If a wait dialog is waiting for a process to finish,
            //The wait dialog will close itself and begin running the process
            //The box won't be "undrawn" until after the process finishes
            //magic voodoo witchery

            if ((actionToWaitOn != null) && (framesRan >= 5)) DialogManager.PopDialog();
            else framesRan++;
        }

        public override void PostClose()
        {
            base.PostOpen();

            if (actionToWaitOn != null) actionToWaitOn.Invoke();
        }

        private void AllowCloseDialog()
        {
            if (!ServerValues.isAdmin) return;

            if (HugsLibUtility.ShiftIsHeld) closeOnCancel = true;
            else closeOnCancel = false;
        }
    }
}
