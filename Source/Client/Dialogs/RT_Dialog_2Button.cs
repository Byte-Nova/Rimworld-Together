﻿using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace GameClient
{
    public class RT_Dialog_2Button : Window
    {
        public override Vector2 InitialSize => new Vector2(350f, 250f);

        private string title = "";
        private string description = "";

        private float buttonX = 250f;
        private float buttonY = 38f;

        private Action actionOne;
        private Action actionTwo;
        private Action actionCancel;

        private string actionOneName;
        private string actionTwoName;

        public RT_Dialog_2Button(string title, string description, string actionOneName, string actionTwoName, Action actionOne, Action actionTwo, Action actionCancel)
        {
            DialogManager.dialog2Button = this;
            this.title = title;
            this.description = description;
            this.actionOne = actionOne;
            this.actionTwo = actionTwo;
            this.actionOneName = actionOneName;
            this.actionTwoName = actionTwoName;
            this.actionCancel = actionCancel;

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

            if (Widgets.ButtonText(new Rect(new Vector2(centeredX - buttonX / 2, rect.yMax - buttonY * 3 - 20f), new Vector2(buttonX, buttonY)), actionOneName))
            {
                if (actionOne != null) actionOne.Invoke();
                Close();
            }

            if (Widgets.ButtonText(new Rect(new Vector2(centeredX - buttonX / 2, rect.yMax - buttonY * 2 - 10f), new Vector2(buttonX, buttonY)), actionTwoName))
            {
                if (actionTwo != null) actionTwo.Invoke();
                Close();
            }

            if (Widgets.ButtonText(new Rect(new Vector2(centeredX - buttonX / 2 + buttonX * 0.125f, rect.yMax - buttonY), new Vector2(buttonX * 0.75f, buttonY)), "RTDialogCancel".Translate()))
            {
                if (actionCancel != null) actionCancel.Invoke();
                Close();
            }
        }
    }
}
