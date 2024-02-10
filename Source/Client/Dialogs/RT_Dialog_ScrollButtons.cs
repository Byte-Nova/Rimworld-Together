using System;
using System.Linq;
using RimWorld;
using RimworldTogether.GameClient.Managers.Actions;
using UnityEngine;
using Verse;

namespace RimworldTogether.GameClient.Dialogs
{
    public class RT_Dialog_ScrollButtons : Window
    {
        public override Vector2 InitialSize => new Vector2(350f, 350f);

        private Vector2 scrollPosition = Vector2.zero;

        private string title = "";
        private string description = "";

        private float buttonX = 250f;
        private float buttonY = 38f;

        private string[] buttonNames;

        private Action actionSelect;
        private Action actionCancel;
            
        public RT_Dialog_ScrollButtons(string title, string description, string[] buttonNames, Action actionSelect, Action actionCancel)
        {
            DialogManager.dialogScrollButtons = this;
            this.title = title;
            this.description = description;
            this.buttonNames = buttonNames;
            this.actionSelect = actionSelect;
            this.actionCancel = actionCancel;

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

            GenerateList(new Rect(rect.x, rect.yMax - buttonY * 5 - 40, rect.width, 175f), buttonNames);

            if (Widgets.ButtonText(new Rect(new Vector2(centeredX - (buttonX / 2), rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "Cancel"))
            {
                OnCancel();
            }
        }

        private void OnCancel()
        {
            if (actionCancel != null) actionCancel.Invoke();

            Close();
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
                        DialogManager.selectedScrollButton = i;
                        actionSelect.Invoke();
                    }
                }
            }
        }
    }
}
