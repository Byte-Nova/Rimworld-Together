using System;
using UnityEngine;
using Verse;

namespace GameClient.Dialogs
{
    public class RT_Dialog_Buttons : RT_Dialog_Base
    {
        private string[] Labels { get; set; }

        private Action[] Actions { get; set; }

        public RT_Dialog_Buttons(string title, string description, string[] labels, Action[] actions, Action onCancel = null)
        {
            this.Title = title;
            this.Description = description;
            this.Labels = labels;
            this.Actions = actions;
            this.OnCancel = onCancel;

            closeOnAccept = false;
            closeOnCancel = false;
        }

        public override void PreOpen() { CalculateWindowSize(); }

        public override void DoWindowContents(Rect rect)
        {
            float centeredX = rect.width / 2;
            float horizontalLineDif = Text.CalcSize(Description).y + StandardMargin / 2;
            float windowDescriptionDif = Text.CalcSize(Description).y + StandardMargin;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(Title).x / 2, rect.y, Text.CalcSize(Title).x, Text.CalcSize(Title).y), Title);
            Widgets.DrawLineHorizontal(rect.x, horizontalLineDif, rect.width);
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(centeredX - Text.CalcSize(Description).x / 2, windowDescriptionDif, 
                Text.CalcSize(Description).x, Text.CalcSize(Description).y), Description);

            DrawCancelButton(centeredX, rect.yMax - DefaultButtonSize.y);

            DrawButton(centeredX, rect.yMax - DefaultButtonSize.y * 2 - 10f, 0);
            if (Labels.Length > 1) DrawButton(centeredX, rect.yMax - DefaultButtonSize.y * 3 - 20f, 1);
            if (Labels.Length > 2) DrawButton(centeredX, rect.yMax - DefaultButtonSize.y * 4 - 30f, 2);
        }

        private void CalculateWindowSize()
        {
            Vector2 sizeVector;

            switch (Labels.Length)
            {
                case 2:
                    sizeVector = new Vector2(350f, 250f);
                    windowRect = new Rect(new Vector2((UI.screenWidth - sizeVector.x) / 2f, (UI.screenHeight - sizeVector.y) / 2f), sizeVector);
                    windowRect.Rounded();
                    break;

                case 3:
                    sizeVector = new Vector2(350f, 285f);
                    windowRect = new Rect(new Vector2((UI.screenWidth - sizeVector.x) / 2f, (UI.screenHeight - sizeVector.y) / 2f), sizeVector);
                    windowRect.Rounded();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DrawButton(float centeredX, float height, int index)
        {
            if (Widgets.ButtonText(new Rect(new Vector2(centeredX - DefaultButtonSize.x / 2, height), DefaultButtonSize), Labels[index]))
            {
                Actions[index]?.Invoke();
                Close();
            }
        }

        private void DrawCancelButton(float centeredX, float height)
        {
            if (Widgets.ButtonText(new Rect(new Vector2(centeredX - DefaultButtonSize.x / 2, height), DefaultButtonSize), "Cancel"))
            {
                OnCancel?.Invoke();
                Close();
            }
        }
    }
}
