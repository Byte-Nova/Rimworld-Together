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

        private Action actionToWaitFor;
        private Action postWaitAction;

        private int tick = 0;

        public RT_Dialog_Wait(string description, Action actionToWaitFor = null, Action postWait = null)
        {
            DialogManager.dialogWait = this;
            this.description = description;
            this.actionToWaitFor = actionToWaitFor;
            this.postWaitAction = postWait;

            forcePause = true;
            absorbInputAroundWindow = true;
            soundAppear = SoundDefOf.CommsWindow_Open;
            //soundClose = SoundDefOf.CommsWindow_Close;

            closeOnAccept = false;
            closeOnCancel = false;
        }


        public override void DoWindowContents(Rect rect)
        {
            if (tick == 5 && actionToWaitFor != null)
            {
                actionToWaitFor.Invoke();
                Close();
            }
            tick++;

            float centeredX = rect.width / 2;
            float horizontalLineDif = Text.CalcSize(description).y + StandardMargin / 2;
            float windowDescriptionDif = Text.CalcSize(description).y + StandardMargin;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(title).x / 2, rect.y, Text.CalcSize(title).x, Text.CalcSize(title).y), title);

            Widgets.DrawLineHorizontal(rect.x, horizontalLineDif, rect.width);

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(description).x / 2, windowDescriptionDif, Text.CalcSize(description).x, Text.CalcSize(description).y), description);
        }

        public override void PostClose()
        {
            Master.threadDispatcher.Enqueue(postWaitAction);
        }
    }
}
