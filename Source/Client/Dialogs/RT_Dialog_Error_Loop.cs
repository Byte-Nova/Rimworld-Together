﻿using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace GameClient
{
    public class RT_Dialog_Error_Loop : Window
    {
        public override Vector2 InitialSize => new Vector2(500f, 150f);

        private string title = "RTDialogERROR".Translate();
        private string descriptionDummy;
        private string[] descriptionLoop;

        private int currentDescriptionIndex = 0;

        private float buttonX = 150f;
        private float buttonY = 38f;

        private Action actionOK;

        public RT_Dialog_Error_Loop(string[] descriptionLoop, Action actionOK = null)
        {
            DialogManager.dialogErrorLoop = this;
            this.descriptionLoop = descriptionLoop;
            this.actionOK = actionOK;

            descriptionDummy = descriptionLoop[currentDescriptionIndex];

            forcePause = true;
            absorbInputAroundWindow = true;

            soundAppear = SoundDefOf.CommsWindow_Open;
            

            closeOnAccept = false;
            closeOnCancel = false;
        }

        public override void DoWindowContents(Rect rect)
        {
            float centeredX = rect.width / 2;
            float horizontalLineDif = Text.CalcSize(descriptionDummy).y + StandardMargin / 2;
            float windowDescriptionDif = Text.CalcSize(descriptionDummy).y + StandardMargin;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(title).x / 2, rect.y, Text.CalcSize(title).x, Text.CalcSize(title).y), title);

            Widgets.DrawLineHorizontal(rect.x, horizontalLineDif, rect.width);

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(descriptionDummy).x / 2, windowDescriptionDif, Text.CalcSize(descriptionDummy).x, Text.CalcSize(descriptionDummy).y), descriptionDummy);

            if (Widgets.ButtonText(new Rect(new Vector2(centeredX - buttonX / 2, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "RTDialogOK".Translate()))
            {
                if (currentDescriptionIndex < descriptionLoop.Length - 1)
                {
                    currentDescriptionIndex++;
                    descriptionDummy = descriptionLoop[currentDescriptionIndex];
                }

                else
                {
                    if (actionOK != null) actionOK.Invoke();
                    Close();
                }
            }
        }
    }
}
