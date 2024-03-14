using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace GameClient
{
    public class RT_Dialog_1Input : Window, RT_WindowInputs
    {
        public override Vector2 InitialSize => new Vector2(400f, 200f);

        private bool AcceptsInput => startAcceptingInputAtFrame <= Time.frameCount;

        private int startAcceptingInputAtFrame;

        private string title;

        private float buttonX = 150f;
        private float buttonY = 38f;

        private Action actionYes;
        private Action actionNo;

        private string inputOneLabel;

        private char censorSymbol = '*';
        private string Str_censorSymbol = "";

        private bool inputOneCensored;
        private string inputOneDisplay;

        public List<string> inputResultList;

        public virtual List<object> inputList { 
            get {
                List<object> returnList = new List<object>();
                returnList.Add(inputResultList[0]);
                return returnList;
            } 
        }

        public RT_Dialog_1Input(string title, string inputOneLabel, Action actionYes, Action actionNo, bool inputOneCensored = false)
        {
            this.Str_censorSymbol = censorSymbol.ToString();
            this.title = title;
            this.actionYes = actionYes;
            this.actionNo = actionNo;
            this.inputOneLabel = inputOneLabel;
            this.inputOneCensored = inputOneCensored;
            this.inputResultList = new List<string>() {""};

            forcePause = true;
            absorbInputAroundWindow = true;

            soundAppear = SoundDefOf.CommsWindow_Open;
            //soundClose = SoundDefOf.CommsWindow_Close;

            closeOnAccept = false;
            closeOnCancel = false;
        }

        public override void DoWindowContents(Rect rect)
        {
            //initialize window size values
            float centeredX = rect.width / 2;
            float horizontalLineDif = Text.CalcSize(title).y + StandardMargin / 2;

            float inputOneLabelDif = Text.CalcSize(inputOneLabel).y + StandardMargin;
            float inputOneDif = inputOneLabelDif + 30f;

            //draw Title and seperator line
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(title).x / 2, rect.y, Text.CalcSize(title).x, Text.CalcSize(title).y), title);
            Widgets.DrawLineHorizontal(rect.x, horizontalLineDif, rect.width);

            DrawInputOne(centeredX, inputOneLabelDif, inputOneDif);

            //draw confirm button
            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMin, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "Confirm"))
            {
                CacheInputs();
                if (actionYes != null) actionYes.Invoke();
                else DialogManager.PopDialog();
            }

            //draw cancel button
            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMax - buttonX, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "Cancel"))
            {
                CacheInputs();
                if (actionNo != null) actionNo.Invoke();
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

        

        public virtual void CacheInputs() {
            DialogManager.inputCache = inputList;
        }

        public virtual void SubstituteInputs(List<object> newInputs)
        {

            //exception handling
            if (newInputs.Count < 2)
            {
                Logs.Error("[RimWorld Together] > ERROR: newInputs in SubstituteInputs at RT_Dialog_1Input has too few elements; No changes will be made");
                return;
            }
            else if (newInputs.Count > 2)
            {
                Logs.Warning("[RimWorld Together] > WARNING: newInputs in SubstituteInputs at RT_Dialog_1Input has more elements than necessary, some elements will not be used ");
            }

            //for each value in inputResultList, set it to the corrosponding value in newInputs
            for (int index = 0; index < inputResultList.Count; index++)
            {
                if (inputResultList[index].GetType() != newInputs[index].GetType())
                {
                    Logs.Error($"[RimWorld Together] > ERROR: newInputs in RT_Dialog_2Inputs.SubstituteInputs contained non-matching types at index {index}, No changes will be made");
                    return;
                }
                inputResultList[index] = (string)newInputs[index];
            }
        }

    }
}
