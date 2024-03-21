using System.Threading;
using System.Collections.Generic;
using Verse;
using System.Linq;
using static Shared.CommonEnumerators;
using System;

namespace GameClient
{
    public static class DialogManager
    {
        //      inputCache
        // Any time a dialog that has inputs is left (it is popped from the stack or a new dialog is pushed)
        // ,it will save its own list of inputs to inputCache
        // inputs can also be manually set to save.
        public static List<object> inputCache;

        //      inputReserve
        //  Unlike inputCache, inputReserve never automatically gets updated, and must be set using
        //  the
        public static List<object> inputReserve;
        //an internal stack to keep track of windows
        //(this makes it easier and more accurate to find the last window pushed)
        private static Stack<Window> windowStack = new Stack<Window>();

        public static Window currentDialog;
        public static Window previousDialog;

        public static RT_WindowInputs currentDialogInputs;

        public static void PushNewDialog(Window window)
        {
            if (ClientValues.isReadyToPlay || Current.ProgramState == ProgramState.Entry)
            {
                try
                {
                    //Hide the current window
                    if (windowStack.Count > 0)
                        Find.WindowStack.TryRemove(windowStack.Peek());

                    //add the new window to the internal stack
                    windowStack.Push(window);

                    //Get an instance of the new window as RT_WindowInputs so input info can be retrieved later
                    if (window is RT_WindowInputs) currentDialogInputs = (RT_WindowInputs)window;

                    //draw the new window
                    Find.WindowStack.Add(window);
                }
                catch (Exception e) { Logger.WriteToConsole(e.ToString(), LogMode.Error); }
            }
        }

        public static void PopInternalStack()
        {
            if (windowStack.Count > 0) windowStack.Pop();
        }

        public static void clearInternalStack()
        {
            windowStack.Clear();
        }

        public static void clearStack()
        {
            while (windowStack.Count > 0)
            {
                Find.WindowStack.TryRemove(windowStack.Pop(), true);
                if (windowStack.Count > 0)
                    Find.WindowStack.Add(windowStack.Peek());
            }
        }

        public static void PopDialog() {

            if (windowStack.Count > 0)
            {
                Find.WindowStack.TryRemove(windowStack.Pop(), true);
                if (windowStack.Count > 0) Find.WindowStack.Add(windowStack.Peek());
            }
        }

        public static void PopDialog(Window window)
        {
            if (windowStack.Count > 0)
            {
                Find.WindowStack.TryRemove(windowStack.Pop(), true);
                if (windowStack.Count > 0) Find.WindowStack.Add(windowStack.Peek());
            }
        }

        public static void setInputReserve()
        {
            currentDialogInputs.CacheInputs();
            inputReserve = new List<object>(inputCache);
        }

        public static string[] SubstituteInputs(List<object> newInputs)
        {
            string[] inputResults = new string[] { };

            //Exception handling
            if (newInputs.Count < 2)
            {
                Logger.WriteToConsole("newInputs in SubstituteInputs at RT_Dialog_1Input has too few elements; No changes will be made", LogMode.Error);
                return null;
            }

            else if (newInputs.Count > 2)
            {
                Logger.WriteToConsole("newInputs in SubstituteInputs at RT_Dialog_1Input has more elements than necessary, some elements will not be used", LogMode.Warning);
                return null;
            }

            //For each value in inputResultList, set it to the corrosponding value in newInputs
            for (int index = 0; index < inputResults.Count(); index++)
            {
                if (inputResults[index].GetType() != newInputs[index].GetType())
                {
                    Logger.WriteToConsole("newInputs in RT_Dialog_2Inputs.SubstituteInputs contained non-matching types at index {index}, No changes will be made", LogMode.Error);
                    return null;
                }

                inputResults[index] = (string)newInputs[index];
            }

            return inputResults;
        }
    }
}
