using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace GameClient
{
    public class RT_Dialog_2Input : Window, RT_WindowInputs
    {
        public override Vector2 InitialSize => new Vector2(400f, 280f);

        private bool AcceptsInput => startAcceptingInputAtFrame <= Time.frameCount;

        private int startAcceptingInputAtFrame;

        private string title;

        private float buttonX = 150f;
        private float buttonY = 38f;

        private Action actionConfirm;
        private Action actionCancel;

        private string inputOneLabel;
        private string inputTwoLabel;

        private char censorSymbol = '*';
        private string Str_censorSymbol = "";

        private bool inputOneCensored;
        private string inputOneDisplay;

        private bool inputTwoCensored;
        private string inputTwoDisplay;

        private List<string> inputResultList;

        public virtual List<object> inputList
        {
            get
            {
                List<object> returnList = new List<object>();
                returnList.Add(inputResultList[0]);
                returnList.Add(inputResultList[1]);
                return returnList;
            }
        }

        public RT_Dialog_2Input(string title, string inputOneLabel, string inputTwoLabel, Action actionConfirm, Action actionCancel, 
            bool inputOneCensored = false, bool inputTwoCensored = false)
        {

            this.Str_censorSymbol = censorSymbol.ToString();
            this.title = title;
            this.actionConfirm = actionConfirm;
            this.actionCancel = actionCancel;
            this.inputOneLabel = inputOneLabel;
            this.inputTwoLabel = inputTwoLabel;
            this.inputOneCensored = inputOneCensored;
            this.inputTwoCensored = inputTwoCensored;
            this.inputResultList = new List<string>(){ "","" };

            forcePause = true;
            absorbInputAroundWindow = true;

            soundAppear = SoundDefOf.CommsWindow_Open;
            //soundClose = SoundDefOf.CommsWindow_Close;

            closeOnAccept = false;
            closeOnCancel = false;
        }

        public override void DoWindowContents(Rect rect)
        {
            //intialize window size values
            float centeredX = rect.width / 2;
            float horizontalLineDif = Text.CalcSize(title).y + StandardMargin / 2;

            float inputOneLabelDif = Text.CalcSize(inputOneLabel).y + StandardMargin;
            float inputOneDif = inputOneLabelDif + 30f;

            float inputTwoLabelDif = inputOneDif + Text.CalcSize(inputTwoLabel).y + StandardMargin * 2;
            float inputTwoDif = inputTwoLabelDif + 30f;

            //draw Title and seperator line
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(title).x / 2, rect.y, Text.CalcSize(title).x, Text.CalcSize(title).y), title);
            Widgets.DrawLineHorizontal(rect.x, horizontalLineDif, rect.width);

            //draw textFields
            DrawInputOne(centeredX, inputOneLabelDif, inputOneDif);
            DrawInputTwo(centeredX, inputTwoLabelDif, inputTwoDif);  

            //draw confirm button
            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMin, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "Confirm"))
            {
                if (inputResultList != null)
                    Logs.Message($"inputs are: {inputResultList[0]} and {inputResultList[1]}");
                else Logs.Message($"inputResultList is null");

                CacheInputs();
                if (actionConfirm != null) actionConfirm.Invoke();
                else DialogManager.PopDialog();

            }

            //draw cancel button
            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMax - buttonX, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "Cancel"))
            {
                CacheInputs();
                if (actionCancel != null) actionCancel.Invoke();
                else DialogManager.PopDialog();
            }
        }

        private void DrawInputOne(float centeredX, float labelDif, float normalDif)
        {
            //Draw TextField label
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(inputOneLabel).x / 2, labelDif, Text.CalcSize(inputOneLabel).x, Text.CalcSize(inputOneLabel).y), inputOneLabel);

            //Handle nullrefrences
            if (inputResultList[0] == null) inputResultList[0] = "";
            if (inputOneDisplay == null) inputOneDisplay = "";

            //if censorship is on, set the Input display to censorship symbol
            //else set it to the input string
            if (inputOneCensored) inputOneDisplay = new string('*', inputResultList[0].Length);
            else inputOneDisplay = inputResultList[0];

            //Draw the textField using input display string
            Text.Font = GameFont.Small;
            string inputDisplayBefore = inputOneDisplay;
            inputOneDisplay = Widgets.TextField(new Rect(centeredX - (200f / 2), normalDif, 200f, 30f), inputDisplayBefore);

            //if new input is detected, add it to the final input string
            if ((inputOneDisplay.Length > inputResultList[0].Length) && (inputOneDisplay.Length <= 32)) inputResultList[0] += inputOneDisplay.Substring(inputResultList[0].Length);
            else if (inputDisplayBefore != inputOneDisplay) inputResultList[0] = DialogShortcuts.replaceNonCensoredSymbols(inputResultList[0], inputOneDisplay, inputOneCensored, Str_censorSymbol);
        }

        private void DrawInputTwo(float centeredX, float labelDif, float normalDif)
        {
            //Draw TextField label
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(inputTwoLabel).x / 2, labelDif, Text.CalcSize(inputTwoLabel).x, Text.CalcSize(inputTwoLabel).y), inputTwoLabel);

            //Handle nullrefrences
            if (inputResultList[1] == null) inputResultList[1] = "";
            if (inputTwoDisplay == null) inputTwoDisplay = "";

            //if censorship is on, set the Input display to censorship symbol
            //else set it to the input string
            if (inputTwoCensored) inputTwoDisplay = new string('*', inputResultList[1].Length);
            else inputTwoDisplay = inputResultList[1];

            //Draw the textField using inputTwoDisplay
            Text.Font = GameFont.Small;
            string inputDisplayBefore = inputOneDisplay;
            inputTwoDisplay = Widgets.TextField(new Rect(centeredX - (200f / 2), normalDif, 200f, 30f), inputTwoDisplay);

            //if new input is detected, add it to the final input string
            if ((inputTwoDisplay.Length > inputResultList[1].Length) && (inputTwoDisplay.Length <= 32)) inputResultList[1] += inputTwoDisplay.Substring(inputResultList[1].Length);
            else if (inputDisplayBefore != inputTwoDisplay) inputResultList[1] = DialogShortcuts.replaceNonCensoredSymbols(inputResultList[1], inputTwoDisplay, inputTwoCensored, Str_censorSymbol);

        }

        public virtual void CacheInputs()
        {
            DialogManager.inputCache = inputList;
            Logs.Message($"inputCache is {DialogManager.inputCache != null}");
        }

        public virtual void SubstituteInputs(List<object> newInputs)
        {
            //Exception handling

            if (newInputs.Count < 2)
            {
                Logs.Error("[RimWorld Together] > ERROR: newInputs in RT_Dialog_2Inputs.SubstituteInputs has too few elements; No changes will be made");
                return;
            }

            else if (newInputs.Count > 2)
            {
                Logs.Warning("[RimWorld Together] > WARNING: newInputs in RT_Dialog_2Inputs.SubstituteInputs has more elements than necessary, some elements will not be used ");
            }

            //For each value in inputResultList, set it to the corrosponding value in newInputs
            Logs.Message($"input result count: {inputResultList.Count}");
            for (int index = 0; index < inputResultList.Count;index++)
            {
                if (inputResultList[index].GetType() != newInputs[index].GetType())
                {
                    Logs.Error($"[RimWorld Together] > ERROR: newInputs in RT_Dialog_2Inputs.SubstituteInputs contained non-matching types at index {index}, No changes will be made");
                    return;
                }

                inputResultList[index] = (string)newInputs[index];
            }

            Logs.Message($"{inputResultList[0]} : {inputResultList[1]}");
        }
    }
}
