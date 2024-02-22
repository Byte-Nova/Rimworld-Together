using RimworldTogether.GameClient.Dialogs;
using RimworldTogether.GameClient.Values;
using Verse;

namespace RimworldTogether.GameClient.Managers.Actions
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
        public static int dialogListingWithButtonResult;

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
}
