using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using static GameClient.DialogManagerHelper;

namespace GameClient
{
    public class RT_Dialog_ListingWithTuple : Window
    {
        public override Vector2 InitialSize => new Vector2(400f, 400f);

        public readonly string title;

        public readonly string description;

        public readonly string[] keys;

        public string[] values;

        private readonly Action actionAccept;

        private readonly Vector2 selectButton = new Vector2(100f, 25f);

        private Vector2 scrollPosition = Vector2.zero;

        public string[] valueString;

        public int[] valueInt;

        public RT_Dialog_ListingWithTuple(string title, string description, string[] keys, string[] values, Action actionAccept = null)
        {
            DialogManager.dialogTupleListing = this;
            this.title = title;
            this.description = description;
            this.keys = keys;
            this.values = values;
            this.actionAccept = actionAccept;

            forcePause = true;
            closeOnAccept = false;
            closeOnCancel = false;
            absorbInputAroundWindow = true;
            soundAppear = SoundDefOf.CommsWindow_Open;

            List<string> strings = new List<string>();
            for (int i = 0; i < keys.Length; i++) strings.Add(values[0]);
            valueString = strings.ToArray();

            List<int> ints = new List<int>();
            for (int i = 0; i < keys.Length; i++) ints.Add(0);
            valueInt = ints.ToArray();
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

            Text.Font = GameFont.Small;
            if (Widgets.ButtonText(GetRectForLocation(rect, defaultButtonSize, RectLocation.BottomCenter), "Accept"))
            {
                DialogManager.dialogTupleListingResultString = keys;
                DialogManager.dialogTupleListingResultInt = valueInt;
                actionAccept?.Invoke();
                Close();
            }
        }

        private void FillMainRect(Rect mainRect)
        {
            float height = 6f + keys.Length * 30f;
            Rect viewRect = new Rect(0f, 0f, mainRect.width - 16f, height);
            Widgets.BeginScrollView(mainRect, ref scrollPosition, viewRect);
            float num = 0;
            float num2 = scrollPosition.y - 30f;
            float num3 = scrollPosition.y + mainRect.height;
            int num4 = 0;

            for (int i = 0; i < keys.Length; i++)
            {
                if (num > num2 && num < num3)
                {
                    Rect rect = new Rect(0f, num, viewRect.width, 30f);
                    DrawCustomRow(rect, keys[i], num4);
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

            for (int i = 0; i < values.Length; i++)
            {
                list.Add(new FloatMenuOption(values[i], delegate
                {
                    valueString[index] = values[i];
                    valueInt[index] = i;
                }));
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }
    }
}