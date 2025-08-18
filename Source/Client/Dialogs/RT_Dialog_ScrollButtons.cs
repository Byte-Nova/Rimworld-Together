using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace GameClient.Dialogs
{
    public class RT_Dialog_ScrollButtons : RT_Dialog_Base
    {
        public override Vector2 InitialSize => new Vector2(500f, 350f);

        private string[] ButtonNames { get; set; }

        public static int SelectedScrollButton { get; private set; }

        public static RT_Dialog_Base Instance { get; private set; } = null;

        public RT_Dialog_ScrollButtons(string title, string description, string[] buttonNames, Action actionSelect, Action actionCancel)
        {
            this.Title = title;
            this.Description = description;
            this.ButtonNames = buttonNames;
            this.OnAccept = actionSelect;
            this.OnCancel = actionCancel;
            Instance = this;

            closeOnAccept = false;
            closeOnCancel = true;
        }

        public override void DoWindowContents(Rect rect)
        {
            float centeredX = rect.width / 2;

            float windowDescriptionDif = Text.CalcSize(Description).y + StandardMargin;
            float descriptionLineDif1 = windowDescriptionDif - Text.CalcSize(Description).y * 0.25f;
            float descriptionLineDif2 = windowDescriptionDif + Text.CalcSize(Description).y * 1.1f;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(Title).x / 2, rect.y, Text.CalcSize(Title).x, Text.CalcSize(Title).y), Title);

            Widgets.DrawLineHorizontal(rect.x, descriptionLineDif1, rect.width);

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(Description).x / 2, windowDescriptionDif, Text.CalcSize(Description).x, Text.CalcSize(Description).y), Description);
            Text.Font = GameFont.Medium;

            Widgets.DrawLineHorizontal(rect.x, descriptionLineDif2, rect.width);

            GenerateList(new Rect(rect.x, rect.yMax - DefaultButtonSize.y * 5 - 40, rect.width, 175f), ButtonNames);

            if (Widgets.ButtonText(new Rect(new Vector2(centeredX - DefaultButtonSize.x / 2, rect.yMax - DefaultButtonSize.y), DefaultButtonSize), "Cancel"))
            {
                OnBack();
            }
        }

        private void OnBack()
        {
            if (OnCancel != null) OnCancel.Invoke();

            Close();
        }

        private void GenerateList(Rect mainRect, string[] buttons)
        {
            float yPadding = 0;
            float extraLenght = 32f;
            float num2 = ScrollPosition.y - 30f;
            float num3 = ScrollPosition.y + mainRect.height;
            float height = 6f + buttons.Count() * DefaultButtonSize.y;

            Rect viewRect = new Rect(mainRect.x, mainRect.y, mainRect.width - 16f, height);
            Widgets.BeginScrollView(mainRect, ref ScrollPosition, viewRect);

            int index = 0;
            foreach (string str in buttons)
            {
                if (yPadding > num2 && yPadding < num3)
                {
                    Rect rect = new Rect(0f, mainRect.y + yPadding, viewRect.width + extraLenght, DefaultButtonSize.y);
                    DrawCustomRow(rect, str);
                    index++;
                }

                yPadding += DefaultButtonSize.y;
            }

            Widgets.EndScrollView();
        }

        private void DrawCustomRow(Rect rect, string buttonName)
        {
            Text.Font = GameFont.Small;
            Rect fixedRect = new Rect(new Vector2(rect.x + 10f, rect.y + 5f), new Vector2(rect.width - 36f, rect.height));

            if (Widgets.ButtonText(fixedRect, buttonName))
            {
                for (int i = 0; i < ButtonNames.Count(); i++)
                {
                    if (ButtonNames[i] == buttonName)
                    {
                        SelectedScrollButton = i;
                        OnAccept?.Invoke();
                        Close();
                    }
                }
            }
        }
    }
}
