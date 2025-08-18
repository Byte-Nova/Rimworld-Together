using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace GameClient.Dialogs
{
    public class RT_Dialog_ListingWithTuple : RT_Dialog_Base
    {
        public override Vector2 InitialSize => new Vector2(500f, 400f);

        public string[] Keys { get; private set; }

        public string[] Values { get; private set; }

        public string[] ValueString { get; private set; }

        public int[] ValueInt { get; private set; }

        public static string[] DialogTupleListingResultString { get; private set; }

        public static int[] DialogTupleListingResultInt { get; private set; }

        public RT_Dialog_ListingWithTuple(string title, string description, string[] keys, string[] values, int[] defaultValues = null, Action actionAccept = null)
        {
            this.Title = title;
            this.Description = description;
            this.Keys = keys;
            this.Values = values;
            this.OnAccept = actionAccept;

            closeOnAccept = false;
            closeOnCancel = false;

            List<string> strings = new List<string>();
            for (int i = 0; i < keys.Length; i++) strings.Add(values[0]);
            ValueString = strings.ToArray();

            List<int> ints = new List<int>();
            for (int i = 0; i < keys.Length; i++) ints.Add(0);
            ValueInt = ints.ToArray();

            if (defaultValues != null)
            {
                for (int i = 0; i < ValueString.Length; i++)
                {
                    ValueString[i] = values[defaultValues[i]];
                    ValueInt[i] = defaultValues[i];
                }
            }
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

            FillMainRect(new Rect(0f, descriptionLineDif2 + 10f, rect.width, rect.height - DefaultButtonSize.y - 85f));

            Text.Font = GameFont.Small;

            if (Widgets.ButtonText(GetRectForLocation(rect, TinyButtonSize, RectLocation.TopRight), "▶")) ShowFloatMenu(-1, true);

            if (Widgets.ButtonText(GetRectForLocation(rect, DefaultButtonSize, RectLocation.BottomCenter), "Accept"))
            {
                DialogTupleListingResultString = Keys;
                DialogTupleListingResultInt = ValueInt;
                OnAccept?.Invoke();
                Close();
            }
        }

        private void FillMainRect(Rect mainRect)
        {
            float height = 6f + Keys.Length * 30f;
            Rect viewRect = new Rect(0f, 0f, mainRect.width - 16f, height);
            Widgets.BeginScrollView(mainRect, ref ScrollPosition, viewRect);
            float num = 0;
            float num2 = ScrollPosition.y - 30f;
            float num3 = ScrollPosition.y + mainRect.height;
            int num4 = 0;

            for (int i = 0; i < Keys.Length; i++)
            {
                if (num > num2 && num < num3)
                {
                    Rect rect = new Rect(0f, num, viewRect.width, 30f);
                    DrawCustomRow(rect, Keys[i], num4);
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
            string buttonLabel = ValueString[index];
            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMax - LongButtonSize.x, rect.yMax - LongButtonSize.y), LongButtonSize), buttonLabel))
            {
                ShowFloatMenu(index, false);
            }
        }

        private void ShowFloatMenu(int index, bool globalChange)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            foreach (string str in Values)
            {
                Action changeSingleValue = delegate
                {
                    ValueString[index] = str;
                    ValueInt[index] = GetValueFromString(str);
                };

                Action changeAllValues = delegate
                {
                    for (int i = 0; i < ValueString.Length; i++)
                    {
                        ValueString[i] = str;
                        ValueInt[i] = GetValueFromString(ValueString[i]);
                    }
                };

                list.Add(new FloatMenuOption(str, delegate
                {
                    if (globalChange) changeAllValues();
                    else changeSingleValue();
                }));
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }

        private int GetValueFromString(string str) { return Values.FirstIndexOf(fetch => fetch == str); }
    }
}