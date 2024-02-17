
using System.Collections.Generic;
using RimworldTogether.GameClient.Dialogs;
using RimworldTogether.GameClient.Values;
using Verse;

namespace RimworldTogether.GameClient.Managers.Actions
{
    public static class DialogManager
    {
        //      inputCache
        // Any time a dialog that has inputs is left (it is popped from the stack or a new dialog is pushed)
        // ,it will save its own list of inputs to inputCache
        // inputs can also be manually set to save.
        public static List<object> inputCache;

        public static Window currentDialog;
        public static Window previousDialog;

        public static RT_WindowInputs currentDialogInputs;

        public static void PushNewDialog(Window window)
        {
            if (ClientValues.isReadyToPlay || Current.ProgramState == ProgramState.Entry)
            {
                //set the new window as the current window
                previousDialog = currentDialog;
                currentDialog = window;

                //Get an instance of the new window as RT_WindowInputs so input info can be retrieved later
                if(window is RT_WindowInputs) currentDialogInputs = (RT_WindowInputs)window;

                Find.WindowStack.Add(window);
            }
        }

        public static void PopDialog() {
             Find.WindowStack.TryRemove(Find.WindowStack[Find.WindowStack.Count-1],true);
        }
        
        public static void PopDialog(Window window)
        {
            Find.WindowStack.TryRemove(window, true);
        }

        public static void WaitForDialogInput(Window window){
            if (ClientValues.isReadyToPlay || Current.ProgramState == ProgramState.Entry)
            {
                previousDialog = currentDialog;
                currentDialog = window;
                window.forcePause = true;
                Find.WindowStack.Add(window);
            }
        }
    }
}
