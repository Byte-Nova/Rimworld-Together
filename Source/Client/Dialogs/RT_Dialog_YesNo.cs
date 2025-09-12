using System;
using UnityEngine;
using Verse;

namespace GameClient.Dialogs
{
    public class RT_Dialog_YesNo : RT_Dialog_Base
    {
        public override Vector2 InitialSize => new Vector2(400f, 150f);

        private string YesText { get; set; }

        private string NoText { get; set; }

        public RT_Dialog_YesNo(string description, Action actionYes, Action actionNo = null, string yText = "Yes", string nText = "No", bool closableX = false)
        {
            this.Title = "OPTION";
            this.Description = description;
            this.OnAccept = actionYes;
            this.OnCancel = actionNo;
            this.YesText = yText;
            this.NoText = nText;

            closeOnAccept = false;
            closeOnCancel = false;

            if (closableX) this.doCloseX = true;
        }

        public override void DoWindowContents(Rect rect)
        {
            float centeredX = rect.width / 2;
            float horizontalLineDif = Text.CalcSize(Description).y + StandardMargin / 2;
            float windowDescriptionDif = Text.CalcSize(Description).y + StandardMargin;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(Title).x / 2, rect.y, Text.CalcSize(Title).x, Text.CalcSize(Title).y), Title);

            Widgets.DrawLineHorizontal(rect.x, horizontalLineDif, rect.width);

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(Description).x / 2, windowDescriptionDif, Text.CalcSize(Description).x, Text.CalcSize(Description).y), Description);

            if (Widgets.ButtonText(GetRectForLocation(rect, SmallButtonSize, RectLocation.BottomLeft), YesText))
            {
                OnAccept?.Invoke();
                Close();
            }

            if (Widgets.ButtonText(GetRectForLocation(rect, SmallButtonSize, RectLocation.BottomRight), NoText))
            {
                OnCancel?.Invoke();
                Close();
            }
        }
    }
}
