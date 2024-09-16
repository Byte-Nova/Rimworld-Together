using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using static GameClient.DialogManagerHelper;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public class RT_Dialog_ListingWithTuple : Window
    {
        public override Vector2 InitialSize => new Vector2(400f, 400f);

        public readonly string title;

        public readonly string description;

        public readonly string[] elements;

        private readonly Action actionAccept;

        private readonly Vector2 selectButton = new Vector2(100f, 25f);

        private Vector2 scrollPosition = Vector2.zero;

        public string[] valueString;

        public int[] valueInt;

        public RT_Dialog_ListingWithTuple(string title, string description, string[] elements, Action actionAccept = null)
        {
            DialogManager.dialogTupleListing = this;
            this.title = title;
            this.description = description;
            this.elements = elements;
            this.actionAccept = actionAccept;

            forcePause = true;
            closeOnAccept = false;
            closeOnCancel = false;
            absorbInputAroundWindow = true;
            soundAppear = SoundDefOf.CommsWindow_Open;

            valueString = new string[elements.Length];
            valueInt = new int[elements.Length];
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

            FillMainRect(new Rect(0f, descriptionLineDif2 + 10f, rect.width, rect.height - defaultButtonSize.y - 85f));

            if (Widgets.ButtonText(GetRectForLocation(rect, defaultButtonSize, RectLocation.BottomCenter), "Apply"))
            {
                DialogManager.dialogTupleListingResultString = elements;
                DialogManager.dialogTupleListingResultInt = valueInt;
                actionAccept?.Invoke();
                Close();
            }
        }

        private void FillMainRect(Rect mainRect)
        {
            float height = 6f + elements.Length * 30f;
            Rect viewRect = new Rect(0f, 0f, mainRect.width - 16f, height);
            Widgets.BeginScrollView(mainRect, ref scrollPosition, viewRect);
            float num = 0;
            float num2 = scrollPosition.y - 30f;
            float num3 = scrollPosition.y + mainRect.height;
            int num4 = 0;

            for (int i = 0; i < elements.Length; i++)
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

            Widgets.Label(fixedRect, element);
            string buttonLabel = valueString[index];
            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMax - selectButton.x, rect.yMax - selectButton.y), selectButton), buttonLabel))
            {
                ShowFloatMenu(index);
            }
        }

        private void ShowFloatMenu(int index)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            list.Add(new FloatMenuOption(ModType.Required.ToString(), delegate         
            { 
                valueString[index] = ModType.Required.ToString();
                valueInt[index] = (int)ModType.Required; 
            }));

            list.Add(new FloatMenuOption(ModType.Optional.ToString(), delegate         
            { 
                valueString[index] = ModType.Optional.ToString();
                valueInt[index] = (int)ModType.Optional; 
            }));

            list.Add(new FloatMenuOption(ModType.Forbidden.ToString(), delegate         
            { 
                valueString[index] = ModType.Forbidden.ToString();
                valueInt[index] = (int)ModType.Forbidden; 
            }));

            Find.WindowStack.Add(new FloatMenu(list));
        }
    }
}