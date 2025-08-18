using System;
using UnityEngine;
using Verse;

namespace GameClient.Dialogs
{
    public class RT_Dialog_Inputs : RT_Dialog_Base
    {
        // Parameters

        private float InputWidth { get; set; } = 300f;

        private float InputHeight { get; set; } = 30f;

        private int MaxChars { get; set; } = 512;

        // Inputs

        private string[] Labels { get; set; } = new string[] { };

        private bool[] Censors { get; set; } = new bool[] { };

        private string[] Results { get; set; } = new string[] { };

        private string[] CensorResult { get; set; } = new string[] { };

        public static string[] DialogInputResults { get; set; }

        public RT_Dialog_Inputs(string title, string[] labels, bool[] censors,
            Action onConfirm = null, Action onCancel = null, string onConfirmText = "Confirm",
            string OnCancel = "Cancel",
            string[]? defaultValues = null
        )
        {
            this.Title = title;
            this.OnAccept = onConfirm;
            this.OnCancel = onCancel;

            this.Labels = labels;
            this.Censors = censors;
            Results = new string[] { "", "", "" };
            CensorResult = new string[] { "", "", "" };
            SetDefaultValues(defaultValues);

            closeOnAccept = false;
            closeOnCancel = false;
        }

        private void SetDefaultValues(string[]? defaultValues)
        {
            if (defaultValues == null || defaultValues.Length == 0) return;
            
            for (int i = 0; i < defaultValues.Length; i++)
            {
                if (i >= Results.Length) break;

                Results[i] = defaultValues[i];
            }
        }

        public override void PreOpen() { CalculateWindowSize(); }

        public override void DoWindowContents(Rect rect)
        {
            float centeredX = rect.width / 2;
            float titleSeparator = Text.CalcSize(Title).y + StandardMargin / 2;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(Title).x / 2, rect.y, Text.CalcSize(Title).x, Text.CalcSize(Title).y), Title);
            Widgets.DrawLineHorizontal(rect.x, titleSeparator, rect.width);
            Text.Font = GameFont.Small;

            float inputOneLabelDif = Text.CalcSize(Labels[0]).y + StandardMargin;
            float inputOneDif = inputOneLabelDif + 30f;
            DrawInput(centeredX, inputOneLabelDif, inputOneDif, 0);

            if (Labels.Length > 1)
            {
                float inputTwoLabelDif = inputOneDif + Text.CalcSize(Labels[1]).y + StandardMargin * 2;
                float inputTwoDif = inputTwoLabelDif + 30f;
                DrawInput(centeredX, inputTwoLabelDif, inputTwoDif, 1);

                if (Labels.Length == 3)
                {
                    float inputThreeLabelDif = inputTwoDif + Text.CalcSize(Labels[2]).y + StandardMargin * 2;
                    float inputThreeDif = inputThreeLabelDif + 30f;
                    DrawInput(centeredX, inputThreeLabelDif, inputThreeDif, 2);
                }
            }

            if (Widgets.ButtonText(GetRectForLocation(rect, SmallButtonSize, RectLocation.BottomLeft), "Confirm"))
            {
                RT_Dialog_Inputs.DialogInputResults = new string[] { Results[0], Results[1], Results[2] };
                OnAccept?.Invoke();
                Close();
            }

            if (Widgets.ButtonText(GetRectForLocation(rect, SmallButtonSize, RectLocation.BottomRight), "Cancel"))
            {
                OnCancel?.Invoke();
                Close();
            }
        }

        private void CalculateWindowSize()
        {
            Vector2 sizeVector;

            switch (Labels.Length)
            {
                case 1:
                    sizeVector = new Vector2(400f, 190f);
                    windowRect = new Rect(new Vector2((UI.screenWidth - sizeVector.x) / 2f, (UI.screenHeight - sizeVector.y) / 2f), sizeVector);
                    windowRect.Rounded();
                    break;

                case 2:
                    sizeVector = new Vector2(400f, 280f);
                    windowRect = new Rect(new Vector2((UI.screenWidth - sizeVector.x) / 2f, (UI.screenHeight - sizeVector.y) / 2f), sizeVector);
                    windowRect.Rounded();
                    break;

                case 3:
                    sizeVector = new Vector2(400f, 370f);
                    windowRect = new Rect(new Vector2((UI.screenWidth - sizeVector.x) / 2f, (UI.screenHeight - sizeVector.y) / 2f), sizeVector);
                    windowRect.Rounded();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DrawInput(float centeredX, float labelDif, float normalDif, int index)
        {
            Widgets.Label(new Rect(centeredX - Text.CalcSize(Labels[index]).x / 2, labelDif, Text.CalcSize(Labels[index]).x, Text.CalcSize(Labels[index]).y), Labels[index]);
            string input = Widgets.TextField(new Rect(centeredX - InputWidth / 2, normalDif, InputWidth, InputHeight), Results[index]);
            if (AcceptsInput && input.Length <= MaxChars) Results[index] = input;

            if (Censors[index])
            {
                string censorOne = Widgets.TextField(new Rect(centeredX - InputWidth / 2, normalDif, InputWidth, InputHeight), CensorResult[index]);
                if (AcceptsInput && censorOne.Length <= MaxChars)
                {
                    Text.Font = GameFont.Medium;
                    CensorResult[index] = new string('█', input.Length);
                    Text.Font = GameFont.Small;
                }
            }
        }
    }
}
