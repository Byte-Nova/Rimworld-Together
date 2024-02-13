using System;
using System.Drawing.Drawing2D;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimworldTogether.GameClient.Managers.Actions;
using UnityEngine;
using Verse;

namespace RimworldTogether.GameClient.Dialogs
{
    public class RT_Dialog_3Input : Window
    {
        //public override Vector2 InitialSize => new Vector2(400f, 370f);
        public override Vector2 InitialSize => new Vector2(800f, 540f);
        private bool AcceptsInput => startAcceptingInputAtFrame <= Time.frameCount;

        private int startAcceptingInputAtFrame;

        private string title;

        private Vector2 windowPos;
        private Vector2 BoxPos;

        private float buttonX = 150f;
        private float buttonY = 38f;

        bool onOne = true;
        TimeSpan lastTime = TimeSpan.Zero;
        TimeSpan deciSec = TimeSpan.Zero;

        private Action actionConfirm;
        private Action actionCancel;

        private string inputOneLabel;
        private string inputTwoLabel;
        private string inputThreeLabel;

        public string inputOneResult;
        public string inputTwoResult;
        public string inputThreeResult;

        private bool inputOneCensored;
        private string inputOneDisplay;

        private bool inputTwoCensored;
        private string inputTwoDisplay;

        public bool inputThreeCensored;
        private string inputThreeDisplay;

        public RT_Dialog_3Input(string title, string inputOneLabel, string inputTwoLabel, string inputThreeLabel,
            Action actionConfirm, Action actionCancel, bool inputOneCensored = false, bool inputTwoCensored = false,
            bool inputThreeCensored = false)
        {
            DialogManager.dialog3Input = this;

            Log.Message($"Screen width: {Screen.width}\nScreen Height {Screen.height}");

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

        bool once = true;
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
            //Draw TextField label
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(inputOneLabel).x / 2, labelDif, Text.CalcSize(inputOneLabel).x, Text.CalcSize(inputOneLabel).y), inputOneLabel);

            //Handle nullrefrences
            if (inputOneResult == null) inputOneResult = "";
            if (inputOneDisplay == null) inputOneDisplay = "";

            //if censorship is on, set the Input display to censorship symbol
            //else set it to the input string
            if (inputOneCensored) inputOneDisplay = new string('*', inputOneResult.Length);
            else inputOneDisplay = inputOneResult;

            //Draw the textField using input display string
            Text.Font = GameFont.Small;
            inputOneDisplay = Widgets.TextField(new Rect(centeredX - (200f / 2), normalDif, 200f, 30f), inputOneDisplay);

            //if new input is detected, add it to the final input string
            if (inputOneDisplay.Length > inputOneResult.Length) inputOneResult += inputOneDisplay.Substring(inputOneResult.Length);
            if (inputOneDisplay.Length < inputOneResult.Length) inputOneResult = inputOneResult.Substring(0, inputOneDisplay.Length);

        }

        private void DrawInputTwo(float centeredX, float labelDif, float normalDif)
        {
            //Draw TextField label
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(inputTwoLabel).x / 2, labelDif, Text.CalcSize(inputTwoLabel).x, Text.CalcSize(inputTwoLabel).y), inputTwoLabel);

            //Handle nullrefrences
            if (inputTwoResult == null) inputTwoResult = "";
            if (inputTwoDisplay == null) inputTwoDisplay = "";

            //if censorship is on, set the Input display to censorship symbol
            //else set it to the input string
            if (inputTwoCensored) inputTwoDisplay = new string('*', inputTwoResult.Length);
            else inputTwoDisplay = inputTwoResult;

            //Draw the textField using inputTwoDisplay
            Text.Font = GameFont.Small;
            inputTwoDisplay = Widgets.TextField(new Rect(centeredX - (200f / 2), normalDif, 200f, 30f), inputTwoDisplay);

            //if new input is detected, add it to the final input string
            if (inputTwoDisplay.Length > inputTwoResult.Length) inputTwoResult += inputTwoDisplay.Substring(inputTwoResult.Length);
            if (inputTwoDisplay.Length < inputTwoResult.Length) inputTwoResult = inputTwoResult.Substring(0, inputTwoDisplay.Length);


        }

        private void DrawInputThree(float centeredX, float labelDif, float normalDif)
        {
            //Draw TextField label
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(inputThreeLabel).x / 2, labelDif, Text.CalcSize(inputTwoLabel).x, Text.CalcSize(inputThreeLabel).y), inputThreeLabel);

            //Handle nullrefrences
            if (inputThreeResult == null) inputThreeResult = "";
            if (inputThreeDisplay == null) inputThreeDisplay = "";

            //if censorship is on, set the Input display to censorship symbol
            //else set it to the input string
            if (inputThreeCensored) inputThreeDisplay = new string('*', inputThreeResult.Length);
            else inputThreeDisplay = inputThreeResult;

            //Draw the textField using inputThreeDisplay
            Text.Font = GameFont.Small;
            inputThreeDisplay = Widgets.TextField(new Rect(centeredX - (200f / 2), normalDif, 200f, 30f), inputTwoDisplay);

            //if new input is detected, add it to the final input string
            if (inputThreeDisplay.Length > inputThreeResult.Length) inputThreeResult += inputThreeDisplay.Substring(inputThreeResult.Length);
            if (inputThreeDisplay.Length < inputThreeResult.Length) inputThreeResult = inputThreeResult.Substring(0, inputThreeDisplay.Length);


        }
    }
}
