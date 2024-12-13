using System;
using UnityEngine;
using Verse;

namespace GameClient
{
    public static class DialogManager
    {
        public static RT_Dialog_Wait dialogWait;
        
        public static RT_Dialog_YesNo dialogYesNo;

        public static RT_Dialog_2Button dialog2Button;
        
        public static RT_Dialog_3Button dialog3Button;

        public static RT_Dialog_OK dialogOK;

        public static RT_Dialog_OK_Loop dialogOKLoop;

        public static RT_Dialog_Error dialogError;

        public static RT_Dialog_Error_Loop dialogErrorLoop;

        public static RT_Dialog_SiteMenu dialogSiteMenu;

        public static RT_Dialog_SiteMenu_Config dialogSiteMenuConfig;

        public static RT_Dialog_SiteMenu_Info dialogSiteMenuInfo;

        public static RT_Dialog_1Input dialog1Input;

        public static string dialog1ResultOne;

        public static RT_Dialog_2Input dialog2Input;

        public static string dialog2ResultOne;

        public static string dialog2ResultTwo;

        public static RT_Dialog_3Input dialog3Input;

        public static string dialog3ResultOne;

        public static string dialog3ResultTwo;

        public static string dialog3ResultThree;

        public static RT_Dialog_ScrollButtons dialogScrollButtons;

        public static int selectedScrollButton;

        public static RT_Dialog_TransferMenu dialogTransferMenu;

        public static RT_Dialog_ItemListing dialogItemListing;

        public static RT_Dialog_Listing dialogListing;

        public static RT_Dialog_ListingWithButton dialogButtonListing;

        public static int dialogButtonListingResultInt;

        public static string dialogButtonListingResultString;

        public static RT_Dialog_ListingWithTuple dialogTupleListing;

        public static string[] dialogTupleListingResultString;

        public static int[] dialogTupleListingResultInt;

        public static Window currentDialog;
        public static Window previousDialog;

        public static void PushNewDialog(Window window)
        {
            if (ClientValues.isReadyToPlay || Current.ProgramState == ProgramState.Entry)
            {
                previousDialog = currentDialog;
                currentDialog = window;

                Find.WindowStack.Add(window);
            }
        }

        public static void PopDialog(Window window) { window?.Close(); }

        public static void PopWaitDialog() { dialogWait?.Close(); }
    }

    public static class DialogManagerHelper
    {
        public enum RectLocation { TopLeft, TopCenter, TopRight, MiddleLeft, MiddleCenter, MiddleRight, BottomLeft, BottomCenter, BottomRight  }

        public static readonly Vector2 defaultButtonSize = new Vector2(150f, 38f);

        public static Rect GetRectForLocation(Rect origin, Vector2 reference, RectLocation desiredLocation)
        {
            return desiredLocation switch
            {
                RectLocation.TopLeft => new Rect(new Vector2(origin.xMin, origin.yMin), reference),
                RectLocation.TopCenter => new Rect(new Vector2(origin.width / 2 - (reference.x / 2), origin.yMin), reference),
                RectLocation.TopRight => new Rect(new Vector2(origin.xMax - reference.x, origin.yMin), reference),
                RectLocation.MiddleLeft => new Rect(new Vector2(origin.xMin, origin.height / 2 - (reference.y / 2)), reference),
                RectLocation.MiddleCenter => new Rect(new Vector2(origin.width / 2 - (reference.x / 2), origin.height / 2 - (reference.y / 2)), reference),
                RectLocation.MiddleRight => new Rect(new Vector2(origin.xMax - reference.x, origin.height / 2 - (reference.y / 2)), reference),
                RectLocation.BottomLeft => new Rect(new Vector2(origin.xMin, origin.yMax - reference.y), reference),
                RectLocation.BottomCenter => new Rect(new Vector2(origin.width / 2 - (reference.x / 2), origin.yMax - reference.y), reference),
                RectLocation.BottomRight => new Rect(new Vector2(origin.xMax - reference.x, origin.yMax - reference.y), reference),
                _ => throw new IndexOutOfRangeException()
            };
        }
    }
}
