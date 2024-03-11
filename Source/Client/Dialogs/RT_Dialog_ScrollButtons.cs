using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace GameClient
{
    public class RT_Dialog_ScrollButtons : Window, RT_WindowInputs
    {
        public override Vector2 InitialSize => new Vector2(350f, 350f);

        private Vector2 scrollPosition = Vector2.zero;

        private string title = "";
        private string description = "";

        private float buttonX = 250f;
        private float buttonY = 38f;

        private string[] buttonNames;
        private int selectedScrollButton;


        private Action actionSelect;
        private Action actionCancel;

        List<int> inputResultList;

        public virtual List<object> inputList
        {
            get
            {
                List<object> returnList = new List<object>();
                returnList.Add(selectedScrollButton);
                return returnList;
            }
        }
        public RT_Dialog_ScrollButtons(string title, string description, string[] buttonNames, Action actionSelect, Action actionCancel)
        {
            this.title = title;
            this.description = description;
            this.buttonNames = buttonNames;
            this.actionSelect = actionSelect;
            this.actionCancel = actionCancel;
            this.inputResultList = new List<int>(){0};


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

            float windowDescriptionDif = Text.CalcSize(description).y + StandardMargin;
            float descriptionLineDif1 = windowDescriptionDif - Text.CalcSize(description).y * 0.25f;
            float descriptionLineDif2 = windowDescriptionDif + Text.CalcSize(description).y * 1.1f;

            //draw title and seperator line
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(title).x / 2, rect.y, Text.CalcSize(title).x, Text.CalcSize(title).y), title);
            Widgets.DrawLineHorizontal(rect.x, descriptionLineDif1, rect.width);

            //draw scroll box title and seperator line
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(description).x / 2, windowDescriptionDif, Text.CalcSize(description).x, Text.CalcSize(description).y), description);
            Widgets.DrawLineHorizontal(rect.x, descriptionLineDif2, rect.width);

            //draw scroll box list
            Text.Font = GameFont.Medium;
            GenerateList(new Rect(rect.x, rect.yMax - buttonY * 5 - 40, rect.width, 175f), buttonNames);

            //draw cancel button
            if (Widgets.ButtonText(new Rect(new Vector2(centeredX - (buttonX / 2), rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "Cancel"))
            {
                if (actionCancel != null) actionCancel.Invoke();
                else DialogManager.PopDialog();
            }
        }

        private void GenerateList(Rect mainRect, string[] buttons)
        {
            float height = 6f + buttons.Count() * buttonY;

            Rect viewRect = new Rect(mainRect.x, mainRect.y, mainRect.width - 16f, height);

            Widgets.BeginScrollView(mainRect, ref scrollPosition, viewRect);

            float yPadding = 0;
            float extraLenght = 32f;
            float num2 = scrollPosition.y - 30f;
            float num3 = scrollPosition.y + mainRect.height;

            int index = 0;
            foreach (string str in buttons)
            {
                if (yPadding > num2 && yPadding < num3)
                {
                    Rect rect = new Rect(0f, mainRect.y + yPadding, viewRect.width + extraLenght, buttonY);
                    DrawCustomRow(rect, str);
                    index++;
                }

                yPadding += buttonY;
            }

            Widgets.EndScrollView();
        }

        private void DrawCustomRow(Rect rect, string buttonName)
        {
            Text.Font = GameFont.Small;
            Rect fixedRect = new Rect(new Vector2(rect.x + 10f, rect.y + 5f), new Vector2(rect.width - 36f, rect.height));

            if (Widgets.ButtonText(fixedRect, buttonName))
            {
                for (int i = 0; i < buttonNames.Count(); i++)
                {
                    if (buttonNames[i] == buttonName)
                    {
                        selectedScrollButton = i;
                        CacheInputs();
                        actionSelect.Invoke();
                    }
                }
            }
        }
        public virtual void CacheInputs()
        {
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
                inputResultList[index] = (int)newInputs[index];
            }
        }
    }
}
