using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace GameClient.Dialogs
{
    public class RT_Dialog_Base : Window
    {
        public static Window CurrentDialog { get; private set; } = null;

        public static Window PreviousDialog { get; private set; } = null;

        public static Vector2 DefaultButtonSize { get; private set; } = new(250f, 38f);

        public static Vector2 SmallButtonSize { get; private set; } = new(150f, 38f);

        public static Vector2 SmallerButtonSize { get; private set; } = new(137f, 38f);

        public static Vector2 TinyButtonSize { get; private set; } = new(47f, 25f);

        public static Vector2 SlimButtonSize { get; private set; } = new(100f, 38f);

        public static Vector2 LongButtonSize { get; private set; } = new Vector2(100f, 25f);

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public Action OnAccept { get; set; } = null;

        public Action OnCancel { get; set; } = null;

        public bool AcceptsInput => StartAcceptingInputAtFrame <= Time.frameCount;

        public int StartAcceptingInputAtFrame { get; set; }

        public Vector2 ScrollPosition = Vector2.zero;

        public RT_Dialog_Base() 
        { 
            forcePause = true;
            absorbInputAroundWindow = true;
            soundAppear = SoundDefOf.CommsWindow_Open;
        }

        public override void DoWindowContents(Rect inRect) { }

        public static void PushNewDialog(Window window)
        {
            PreviousDialog = CurrentDialog;
            Find.WindowStack.Add(window);
            CurrentDialog = window;
        }

        public enum RectLocation { TopLeft, TopCenter, TopRight, MiddleLeft, MiddleCenter, MiddleRight, BottomLeft, BottomCenter, BottomRight }

        public static Rect GetRectForLocation(Rect origin, Vector2 reference, RectLocation desiredLocation)
        {
            return desiredLocation switch
            {
                RectLocation.TopLeft => new Rect(new Vector2(origin.xMin, origin.yMin), reference),
                RectLocation.TopCenter => new Rect(new Vector2(origin.width / 2 - reference.x / 2, origin.yMin), reference),
                RectLocation.TopRight => new Rect(new Vector2(origin.xMax - reference.x, origin.yMin), reference),
                RectLocation.MiddleLeft => new Rect(new Vector2(origin.xMin, origin.height / 2 - reference.y / 2), reference),
                RectLocation.MiddleCenter => new Rect(new Vector2(origin.width / 2 - reference.x / 2, origin.height / 2 - reference.y / 2), reference),
                RectLocation.MiddleRight => new Rect(new Vector2(origin.xMax - reference.x, origin.height / 2 - reference.y / 2), reference),
                RectLocation.BottomLeft => new Rect(new Vector2(origin.xMin, origin.yMax - reference.y), reference),
                RectLocation.BottomCenter => new Rect(new Vector2(origin.width / 2 - reference.x / 2, origin.yMax - reference.y), reference),
                RectLocation.BottomRight => new Rect(new Vector2(origin.xMax - reference.x, origin.yMax - reference.y), reference),
                _ => throw new IndexOutOfRangeException()
            };
        }
    }
}
