using RimWorld;
using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimworldTogether
{
    public class RT_Dialog_Listing : Window
    {
        public override Vector2 InitialSize => new Vector2(500f, 400f);

        public string title;

        public string description;

        public string[] elements;

        private Action actionOK;

        private Vector2 scrollPosition = Vector2.zero;

        private float buttonX = 100f;

        private float buttonY = 37f;

        public RT_Dialog_Listing(string title, string description, string[] elements, Action actionOK = null)
        {
            DialogManager.dialogListing = this;
            this.title = title;
            this.description = description;
            this.elements = elements;
            this.actionOK = actionOK;

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
            
            FillMainRect(new Rect(0f, descriptionLineDif2 + 10f, rect.width, rect.height - buttonY - 85f));

            if (Widgets.ButtonText(new Rect(new Vector2(centeredX - buttonX / 2, rect.yMax - buttonY), new Vector2(buttonX, buttonY)), "OK"))
            {
                if (actionOK != null) actionOK.Invoke();
                Close();
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
        }
    }
}
