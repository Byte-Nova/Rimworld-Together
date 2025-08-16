using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Dialogs
{
    public class RT_Dialog_ListingWithTuple : RT_Dialog_Base
    {
        public override Vector2 InitialSize => new Vector2(500f, 400f);

        public string[] Keys { get; private set; }

        public string[] Values { get; private set; }

        public string[] ValueString { get; private set; }

        public int[] ValueInt { get; private set; }

        public bool searchBoxEnabled { get; set; }
        public string searchBoxInput { get; set; }

        public string[] displayKeys {  get; set; }
        public string[] displayValueString { get; set; }
        public int[] displayKeysOrgIndex { get; set; }

        public static string[] DialogTupleListingResultString { get; private set; }

        public static int[] DialogTupleListingResultInt { get; private set; }

        public RT_Dialog_ListingWithTuple(string title, string description, string[] keys, string[] values, int[] defaultValues = null, Action actionAccept = null, bool searchBoxEnabled = false)
        {
            this.Title = title;
            this.Description = description;
            this.Keys = keys;
            this.displayKeys = keys;
            this.Values = values;
            this.OnAccept = actionAccept;
            this.searchBoxEnabled = searchBoxEnabled;
            this.searchBoxInput = string.Empty;

            closeOnAccept = false;
            closeOnCancel = false;

            List<string> strings = new List<string>();
            for (int i = 0; i < keys.Length; i++) strings.Add(values[0]);
            ValueString = strings.ToArray();
            displayValueString = strings.ToArray();

            List<int> ints = new List<int>();
            for (int i = 0; i < keys.Length; i++) ints.Add(0);
            ValueInt = ints.ToArray();

            displayKeysOrgIndex = Enumerable.Range(0, Keys.Length).ToArray();

            if (defaultValues != null)
            {
                for (int i = 0; i < ValueString.Length; i++)
                {
                    ValueString[i] = values[defaultValues[i]];
                    displayValueString[i] = values[defaultValues[i]];
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

            if (searchBoxEnabled)
            {
                string tempSearchBoxInput = Widgets.TextField(GetRectForLocation(rect, LongButtonSize, RectLocation.TopLeft), searchBoxInput, 16);
                if (tempSearchBoxInput != searchBoxInput)
                {
                    searchBoxInput = tempSearchBoxInput;
                    SetDisplayVariables();
                }
            }

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
            float Height = 6f + displayKeys.Length * 30f;
            Rect viewRect = new Rect(0f, 0f, mainRect.width - 16f, Height);
            Widgets.BeginScrollView(mainRect, ref ScrollPosition, viewRect);
            float currentY = 0;
            float visibleStartY = ScrollPosition.y - 30f;
            float visibleEndY = ScrollPosition.y + mainRect.height;

            for (int rowCount = 0; rowCount < displayKeys.Length; rowCount++)
            {
                if (currentY > visibleStartY && currentY < visibleEndY)
                {
                    Rect rect = new Rect(0f, currentY, viewRect.width, 30f);
                    DrawCustomRow(rect, displayKeys[rowCount], rowCount);
                }
                currentY += 30f;
            }

            Widgets.EndScrollView();
        }

        private void DrawCustomRow(Rect rect, string element, int displayindex)
        {
            Text.Font = GameFont.Small;
            Rect fixedRect = new Rect(new Vector2(rect.x, rect.y + 5f), new Vector2(rect.width - 16f, rect.height - 5f));
            if (displayindex % 2 == 0) Widgets.DrawHighlight(fixedRect);

            Widgets.Label(fixedRect, element);
            string buttonLabel = ValueString[displayKeysOrgIndex[displayindex]];
            if (Widgets.ButtonText(new Rect(new Vector2(rect.xMax - LongButtonSize.x, rect.yMax - LongButtonSize.y), LongButtonSize), buttonLabel))
            {
                ShowFloatMenu(displayindex, false);
            }
        }

        private void ShowFloatMenu(int displayindex, bool globalChange)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            foreach (string str in Values)
            {
                Action changeSingleValue = delegate
                {
                    ValueString[displayKeysOrgIndex[displayindex]] = str;
                    ValueInt[displayKeysOrgIndex[displayindex]] = GetValueFromString(str);
                };

                Action changeAllValues = delegate
                {
                    for (int i = 0; i < displayValueString.Length; i++)
                    {
                        ValueString[displayKeysOrgIndex[i]] = str;
                        ValueInt[displayKeysOrgIndex[i]] = GetValueFromString(ValueString[displayKeysOrgIndex[i]]);
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

        private void SetDisplayVariables()
        {
            if (string.IsNullOrEmpty(searchBoxInput))
            {
                displayKeys = Keys;
                displayValueString = ValueString;
                displayKeysOrgIndex = Enumerable.Range(0, Keys.Length).ToArray();
            }
            else
            {
                List<string> filteredKeys = new List<string>();
                List<string> filteredValues = new List<string>();
                List<int> filteredKeysOrgIndex = new List<int>();
                for (int i = 0; i < Keys.Length; i++)
                {
                    if (Keys[i].Contains(searchBoxInput, StringComparison.OrdinalIgnoreCase))
                    {
                        filteredKeys.Add(Keys[i]);
                        filteredValues.Add(ValueString[i]);
                        filteredKeysOrgIndex.Add(i);
                    }
                }
                displayKeys = filteredKeys.ToArray();
                displayValueString = filteredValues.ToArray();
                displayKeysOrgIndex = filteredKeysOrgIndex.ToArray();
            }
        }
    }
}