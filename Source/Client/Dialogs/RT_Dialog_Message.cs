using System;
using UnityEngine;
using Verse;

namespace GameClient.Dialogs
{
    public class RT_Dialog_Message : RT_Dialog_Base
    {
        public override Vector2 InitialSize => new Vector2(500f, 150f);

        private string CurrentMessage { get; set; }

        private string[] Messages { get; set; }

        private int Index { get; set; } = 0;

        public RT_Dialog_Message(string title, string[] messages, Action onConfirm = null)
        {
            if (title != null) this.Title = title;
            else this.Title = "Message";

            this.Messages = messages;
            this.OnAccept = onConfirm;
            CurrentMessage = messages[Index];

            closeOnAccept = false;
            closeOnCancel = false;
        }

        public override void DoWindowContents(Rect rect)
        {
            float centeredX = rect.width / 2;
            float horizontalLineDif = Text.CalcSize(CurrentMessage).y + StandardMargin / 2;
            float windowDescriptionDif = Text.CalcSize(CurrentMessage).y + StandardMargin;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(Title).x / 2, rect.y, Text.CalcSize(Title).x, Text.CalcSize(Title).y), Title);
            Widgets.DrawLineHorizontal(rect.x, horizontalLineDif, rect.width);
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(centeredX - Text.CalcSize(CurrentMessage).x / 2, windowDescriptionDif, 
                Text.CalcSize(CurrentMessage).x, Text.CalcSize(CurrentMessage).y), CurrentMessage);

            if (Widgets.ButtonText(GetRectForLocation(rect, DefaultButtonSize, RectLocation.BottomCenter), "OK"))
            {
                if (Index < Messages.Length - 1)
                {
                    Index++;
                    CurrentMessage = Messages[Index];
                }

                else
                {
                    OnAccept?.Invoke();
                    Close();
                }
            }
        }
    }
}
