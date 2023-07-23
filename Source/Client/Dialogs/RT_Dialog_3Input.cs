using System;
using RimWorld;
using RimworldTogether.GameClient.Managers;
using UnityEngine;
using Verse;

namespace RimworldTogether.GameClient.Dialogs
{
    public class RT_Dialog_3Input : Window
    {
        public override Vector2 InitialSize => new Vector2(400f, 370f);

        private bool AcceptsInput => startAcceptingInputAtFrame <= Time.frameCount;

        private int startAcceptingInputAtFrame;

        private string title;

        private float buttonX = 150f;
        private float buttonY = 38f;

        private Action actionConfirm;
        private Action actionCancel;

        private string inputOneLabel;
        private string inputTwoLabel;
        private string inputThreeLabel;

        public string inputOneResult;
        public string inputTwoResult;
        public string inputThreeResult;

        private bool inputOneCensored;
        private string inputOneCensoredResult;

        private bool inputTwoCensored;
        private string inputTwoCensoredResult;

        public bool inputThreeCensored;
        private string inputThreeCensoredResult;

        public RT_Dialog_3Input(string title, string inputOneLabel, string inputTwoLabel, string inputThreeLabel, 
            Action actionConfirm, Action actionCancel, bool inputOneCensored = false, bool inputTwoCensored = false, 
            bool inputThreeCensored = false)
        {
            DialogManager.dialog3Input = this;
            this.title = title;
            this.actionConfirm = actionConfirm;
            this.actionCancel = actionCancel;
            this.inputOneLabel = inputOneLabel;
            this.inputTwoLabel = inputTwoLabel;
            this.inputThreeLabel = inputThreeLabel;
            this.inputOneCensored = inputOneCensored;
            this.inputTwoCensored = inputTwoCensored;
            this.inputThreeCensored = inputThreeCensored;

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
            float horizontalLineDif = Text.CalcSize(title).y + StandardMargin / 2;

            float inputOneLabelDif = Text.CalcSize(inputOneLabel).y + StandardMargin;
            float inputOneDif = inputOneLabelDif + 30f;

            float inputTwoLabelDif = inputOneDif + Text.CalcSize(inputTwoLabel).y + StandardMargin * 2;
            float inputTwoDif = inputTwoLabelDif + 30f;

            float inputThreeLabelDif = inputTwoDif + Text.CalcSize(inputThreeLabel).y + StandardMargin * 2;
            float inputThreeDif = inputThreeLabelDif + 30f;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(title).x / 2, rect.y, Text.CalcSize(title).x, Text.CalcSize(title).y), title);
            Widgets.DrawLineHorizontal(rect.x, horizontalLineDif, rect.width);

            DrawInputOne(centeredX, inputOneLabelDif, inputOneDif);

            DrawInputTwo(centeredX, inputTwoLabelDif, inputTwoDif);

            DrawInputThree(centeredX, inputThreeLabelDif, inputThreeDif);

            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMin, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "Confirm"))
            {
                DialogManager.dialog3ResultOne = inputOneResult;
                DialogManager.dialog3ResultTwo = inputTwoResult;
                DialogManager.dialog3ResultThree = inputThreeResult;

                if (actionConfirm != null) actionConfirm.Invoke();
                Close();
            }

            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMax - buttonX, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "Cancel"))
            {
                if (actionCancel != null) actionCancel.Invoke();
                Close();
            }
        }

        private void DrawInputOne(float centeredX, float labelDif, float normalDif)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(inputOneLabel).x / 2, labelDif, Text.CalcSize(inputOneLabel).x, Text.CalcSize(inputOneLabel).y), inputOneLabel);

            Text.Font = GameFont.Small;
            string inputOne = Widgets.TextField(new Rect(centeredX - (200f / 2), normalDif, 200f, 30f), inputOneResult);
            if (AcceptsInput && inputOne.Length <= 32) inputOneResult = inputOne;

            if (inputOneCensored)
            {
                string censorOne = Widgets.TextField(new Rect(centeredX - (200f / 2), normalDif, 200f, 30f), inputOneCensoredResult);
                if (AcceptsInput && censorOne.Length <= 32)
                {
                    Text.Font = GameFont.Medium;
                    inputOneCensoredResult = new string('█', inputOne.Length);
                    Text.Font = GameFont.Small;
                }
            }
        }

        private void DrawInputTwo(float centeredX, float labelDif, float normalDif)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(inputTwoLabel).x / 2, labelDif, Text.CalcSize(inputTwoLabel).x, Text.CalcSize(inputTwoLabel).y), inputTwoLabel);

            Text.Font = GameFont.Small;
            string inputTwo = Widgets.TextField(new Rect(centeredX - (200f / 2), normalDif, 200f, 30f), inputTwoResult);
            if (AcceptsInput && inputTwo.Length <= 32) inputTwoResult = inputTwo;

            if (inputTwoCensored)
            {
                string censorOne = Widgets.TextField(new Rect(centeredX - (200f / 2), normalDif, 200f, 30f), inputTwoCensoredResult);
                if (AcceptsInput && censorOne.Length <= 32)
                {
                    Text.Font = GameFont.Medium;
                    inputTwoCensoredResult = new string('█', inputTwo.Length);
                    Text.Font = GameFont.Small;
                }
            }
        }

        private void DrawInputThree(float centeredX, float labelDif, float normalDif)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(inputThreeLabel).x / 2, labelDif, Text.CalcSize(inputThreeLabel).x, Text.CalcSize(inputThreeLabel).y), inputThreeLabel);

            Text.Font = GameFont.Small;
            string inputThree = Widgets.TextField(new Rect(centeredX - (200f / 2), normalDif, 200f, 30f), inputThreeResult);
            if (AcceptsInput && inputThree.Length <= 32) inputThreeResult = inputThree;

            if (inputThreeCensored)
            {
                string censorOne = Widgets.TextField(new Rect(centeredX - (200f / 2), normalDif, 200f, 30f), inputThreeCensoredResult);
                if (AcceptsInput && censorOne.Length <= 32)
                {
                    Text.Font = GameFont.Medium;
                    inputThreeCensoredResult = new string('█', inputThree.Length);
                    Text.Font = GameFont.Small;
                }
            }
        }
    }
}
