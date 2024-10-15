﻿using System;
using RimWorld;
using UnityEngine;
using Verse;
using static GameClient.DialogManagerHelper;

namespace GameClient
{
    public class RT_Dialog_1Input : Window
    {
        public override Vector2 InitialSize => new Vector2(400f, 200f);

        private bool AcceptsInput => startAcceptingInputAtFrame <= Time.frameCount;

        private readonly int startAcceptingInputAtFrame;

        private readonly string title;

        private readonly float buttonX = 150f;

        private readonly float buttonY = 38f;

        private readonly Action actionYes;

        private readonly Action actionNo;

        private readonly string inputOneLabel;

        public string inputOneResult;

        private readonly bool inputOneCensored;

        private string inputOneCensoredResult;

        public RT_Dialog_1Input(string title, string inputOneLabel, Action actionYes, Action actionNo, bool inputOneCensored = false)
        {
            DialogManager.dialog1Input = this;
            this.title = title;
            this.actionYes = actionYes;
            this.actionNo = actionNo;
            this.inputOneLabel = inputOneLabel;
            this.inputOneCensored = inputOneCensored;

            forcePause = true;
            absorbInputAroundWindow = true;
            soundAppear = SoundDefOf.CommsWindow_Open;
            
            closeOnAccept = false;
            closeOnCancel = false;
        }

        public override void DoWindowContents(Rect rect)
        {
            float centeredX = rect.width / 2;
            float horizontalLineDif = Text.CalcSize(title).y + StandardMargin / 2;

            float inputOneLabelDif = Text.CalcSize(inputOneLabel).y + StandardMargin;
            float inputOneDif = inputOneLabelDif + 28f;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(title).x / 2, rect.y, Text.CalcSize(title).x, Text.CalcSize(title).y), title);
            Widgets.DrawLineHorizontal(rect.x, horizontalLineDif, rect.width);

            DrawInputOne(centeredX, inputOneLabelDif, inputOneDif);

            if (Widgets.ButtonText(GetRectForLocation(rect, defaultButtonSize, RectLocation.BottomLeft), "RTDialogConfirm".Translate()))
            {
                DialogManager.dialog1ResultOne = inputOneResult;
                actionYes?.Invoke();
                Close();
            }

            if (Widgets.ButtonText(GetRectForLocation(rect, defaultButtonSize, RectLocation.BottomRight), "RTDialogCancel".Translate()))
            {
                actionNo?.Invoke();
                Close();
            }
        }

        private void DrawInputOne(float centeredX, float labelDif, float normalDif)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(inputOneLabel).x / 2, labelDif, Text.CalcSize(inputOneLabel).x, Text.CalcSize(inputOneLabel).y), inputOneLabel);

            string inputOne = Widgets.TextField(new Rect(centeredX - (200f / 2), normalDif + 10f, 200f, 30f), inputOneResult);
            if (AcceptsInput && inputOne.Length <= 32) inputOneResult = inputOne;

            if (inputOneCensored)
            {
                string censorOne = Widgets.TextField(new Rect(centeredX - (200f / 2), normalDif, 200f, 30f), inputOneCensoredResult);
                if (AcceptsInput && censorOne.Length <= 32)
                {
                    Text.Font = GameFont.Medium;
                    inputOneCensoredResult = new string('\u2588', inputOne.Length);
                    Text.Font = GameFont.Small;
                }
            }
        }
    }
}
