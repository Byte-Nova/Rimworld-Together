using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace GameClient
{
    public class RT_Dialog_Error_List : Window
    {
        public override Vector2 InitialSize => new Vector2(500f, 150f);

        private string title = "ERROR";
        private string currentError;
        private string[] errorList;

        private int currentErrorIndex = 0;

        private float buttonX = 150f;
        private float buttonY = 38f;

        private Action actionOK;

        public RT_Dialog_Error_List(string[] errorList, Action actionOK = null)
        {
            this.errorList = errorList;
            this.actionOK = actionOK;

            currentError = errorList[currentErrorIndex];

            forcePause = true;
            absorbInputAroundWindow = true;

            soundAppear = SoundDefOf.CommsWindow_Open;
            //soundClose = SoundDefOf.CommsWindow_Close;

            closeOnAccept = false;
            closeOnCancel = false;
        }

        public override void DoWindowContents(Rect rect)
        {
            float centeredX = rect.width / 2;
            float horizontalLineDif = Text.CalcSize(currentError).y + StandardMargin / 2;
            float windowDescriptionDif = Text.CalcSize(currentError).y + StandardMargin;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(title).x / 2, rect.y, Text.CalcSize(title).x, Text.CalcSize(title).y), title);

            Widgets.DrawLineHorizontal(rect.x, horizontalLineDif, rect.width);

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(currentError).x / 2, windowDescriptionDif, Text.CalcSize(currentError).x, Text.CalcSize(currentError).y), currentError);

            if (Widgets.ButtonText(new Rect(new Vector2(centeredX - buttonX / 2, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "OK"))
            {
                if (currentErrorIndex < errorList.Length - 1)
                {
                    currentErrorIndex++;
                    currentError = errorList[currentErrorIndex];
                }

                else
                {
                    if (actionOK != null) actionOK.Invoke();
                    else DialogManager.PopDialog();
                }
            }
        }
    }
}
