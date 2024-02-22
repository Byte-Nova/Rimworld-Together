using System;
using RimWorld;
using RimworldTogether.GameClient.Managers.Actions;
using UnityEngine;
using Verse;

namespace RimworldTogether.GameClient.Dialogs
{
    public class RT_Dialog_3Button : Window
    {
        public override Vector2 InitialSize => new Vector2(350f, 285f);

        private string title = "";
        private string description = "";

        private float buttonX = 250f;
        private float buttonY = 38f;

        private Action actionOne;
        private Action actionTwo;
        private Action actionThree;
        private Action actionCancel;

        private string actionOneName;
        private string actionTwoName;
        private string actionThreeName;

        public RT_Dialog_3Button(string title, string description, string actionOneName, string actionTwoName, string actionThreeName, Action actionOne, Action actionTwo, Action actionThree, Action actionCancel)
        {
            DialogManager.dialog3Button = this;
            this.title = title;
            this.description = description;
            this.actionOne = actionOne;
            this.actionTwo = actionTwo;
            this.actionThree = actionThree;
            this.actionOneName = actionOneName;
            this.actionTwoName = actionTwoName;
            this.actionThreeName = actionThreeName;
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
            float horizontalLineDif = Text.CalcSize(description).y + StandardMargin / 2;
            float windowDescriptionDif = Text.CalcSize(description).y + StandardMargin;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(title).x / 2, rect.y, Text.CalcSize(title).x, Text.CalcSize(title).y), title);

            Widgets.DrawLineHorizontal(rect.x, horizontalLineDif, rect.width);

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(centeredX - Text.CalcSize(description).x / 2, windowDescriptionDif, Text.CalcSize(description).x, Text.CalcSize(description).y), description);

            if (Widgets.ButtonText(new Rect(new Vector2(centeredX - buttonX / 2, rect.yMax - buttonY * 4 - 30f), new Vector2(buttonX, buttonY)), actionOneName))
            {
                if (actionOne != null) actionOne.Invoke();
                Close();
            }

            if (Widgets.ButtonText(new Rect(new Vector2(centeredX - buttonX / 2, rect.yMax - buttonY * 3 - 20f), new Vector2(buttonX, buttonY)), actionTwoName))
            {
                if (actionTwo != null) actionTwo.Invoke();
                Close();
            }

            if (Widgets.ButtonText(new Rect(new Vector2(centeredX - buttonX / 2, rect.yMax - buttonY * 2 - 10f), new Vector2(buttonX, buttonY)), actionThreeName))
            {
                if (actionThree != null) actionThree.Invoke();
                Close();
            }

            if (Widgets.ButtonText(new Rect(new Vector2(centeredX - buttonX / 2 + buttonX * 0.125f, rect.yMax - buttonY), new Vector2(buttonX * 0.75f, buttonY)), "Cancel"))
            {
                if (actionCancel != null) actionCancel.Invoke();
                Close();
            }
        }
    }
}
