using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace GameClient
{
    public class RT_Dialog_ListingWithButton : Window, RT_WindowInputs
    {
        public override Vector2 InitialSize => new Vector2(400f, 400f);

        public string title;

        public string description;

        public string[] elements;

        private Action actionClick;

        private Action actionCancel;

        private Vector2 scrollPosition = Vector2.zero;

        private float buttonX = 150f;
        private float buttonY = 38f;

        private float selectButtonX = 47f;
        private float selectButtonY = 25f;

        private int SelectedButton;

        public List<int> inputResultList;

        public virtual List<object> inputList
        {
            get
            {
                List<object> returnList = new List<object>();
                returnList.Add(SelectedButton);
                return returnList;
            }
        }

        public RT_Dialog_ListingWithButton(string title, string description, string[] elements, Action actionClick = null, Action actionCancel = null)
        {
            this.title = title;
            this.description = description;
            this.elements = elements;
            this.actionClick = actionClick;
            this.actionCancel = actionCancel;
            inputResultList = new List<int>(1);
            forcePause = true;
            absorbInputAroundWindow = true;
            this.inputResultList = new List<int>() {0};

            soundAppear = SoundDefOf.CommsWindow_Open;
            //soundClose = SoundDefOf.CommsWindow_Close;

            closeOnAccept = false;
            closeOnCancel = false;
        }

        public override void DoWindowContents(Rect rect)
        {
            float centeredX = rect.width / 2;

            float windowDescriptionDif = Text.CalcSize(description).y + StandardMargin;
            float descriptionLineDif1 = windowDescriptionDif - Text.CalcSize(description).y * 0.25f;
            float descriptionLineDif2 = windowDescriptionDif + Text.CalcSize(description).y * 1.1f;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(title).x / 2, rect.y, Text.CalcSize(title).x, Text.CalcSize(title).y), title);

            Widgets.DrawLineHorizontal(rect.x, descriptionLineDif1, rect.width);

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(description).x / 2, windowDescriptionDif, Text.CalcSize(description).x, Text.CalcSize(description).y), description);
            Text.Font = GameFont.Medium;

            Widgets.DrawLineHorizontal(rect.x, descriptionLineDif2, rect.width);

            FillMainRect(new Rect(0f, descriptionLineDif2 + 10f, rect.width, rect.height - buttonY - 85f));

            if (Widgets.ButtonText(new Rect(new Vector2(centeredX - buttonX / 2, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "Close"))
            {
                CacheInputs();
                if (actionCancel != null) actionCancel.Invoke();
                else DialogManager.PopDialog();
            }
        }

        private void FillMainRect(Rect mainRect)
        {
            float height = 6f + (float)elements.Count() * 30f;
            Rect viewRect = new Rect(0f, 0f, mainRect.width - 16f, height);
            Widgets.BeginScrollView(mainRect, ref scrollPosition, viewRect);
            float num = 0;
            float num2 = scrollPosition.y - 30f;
            float num3 = scrollPosition.y + mainRect.height;
            int num4 = 0;

            for (int i = 0; i < elements.Count(); i++)
            {
                if (num > num2 && num < num3)
                {
                    Rect rect = new Rect(0f, num, viewRect.width, 30f);
                    DrawCustomRow(rect, elements[i], num4);
                }

                num += 30f;
                num4++;
            }

            Widgets.EndScrollView();
        }

        private void DrawCustomRow(Rect rect, string element, int index)
        {
            Text.Font = GameFont.Small;
            Rect fixedRect = new Rect(new Vector2(rect.x, rect.y + 5f), new Vector2(rect.width - 16f, rect.height - 5f));
            if (index % 2 == 0) Widgets.DrawHighlight(fixedRect);

            Widgets.Label(fixedRect, $"{element}");
            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMax - selectButtonX, rect.yMax - selectButtonY), new Vector2(selectButtonX, selectButtonY)), "Select"))
            {
                SelectedButton = index;
                if (actionClick != null) actionClick.Invoke();
                else DialogManager.PopDialog();

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
