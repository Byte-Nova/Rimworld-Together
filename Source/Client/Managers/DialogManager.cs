using System.Threading;
using System.Collections.Generic;
using Verse;

namespace GameClient
{
    public static class DialogManager
    {
        // InputCache
        // Any time a dialog that has inputs is left (it is popped from the stack or a new dialog is pushed).
        // It will save its own list of inputs to InputCache.
        // Inputs can also be manually set to save.
        public static List<object> inputCache;

        //an internal stack to keep track of windows
        //(this makes it easier and more accurate to find the last window pushed)
        private static Stack<Window> windowStack = new Stack<Window>();

        public static Window currentDialog;
        public static Window previousDialog;

        public static RT_WindowInputs currentDialogInputs;

        public static void PushNewDialog(Window window)
        {
            try
            {
                Logs.Message($"[Rimworld Together] > Pushing {window.ToString()}");

                //Hide the current window
                if (windowStack.Count > 0) Find.WindowStack.TryRemove(windowStack.Peek());

                //Add the new window to the internal stack
                windowStack.Push(window);

                //Get an instance of the new window as RT_WindowInputs so input info can be retrieved later
                if (window is RT_WindowInputs) currentDialogInputs = (RT_WindowInputs)window;

                //Draw the new window
                Find.WindowStack.Add(window);
                listWindows();
            }
            catch (System.Exception ex) { Logs.Message(ex.ToString()); }
        }

        public static void ClearInternalStack()
        {
            windowStack.Clear();

            Logs.Message("cleared window stack");
        }

        public static void ClearStack()
        {
            while (windowStack.Count > 0)
            {
                Logs.Message($"[Rimworld Together] > popping {windowStack.Peek().ToString()}");

                Find.WindowStack.TryRemove(windowStack.Pop(), true);

                if (windowStack.Count > 0) Find.WindowStack.Add(windowStack.Peek());
            }
        }

        public static void PopDialog() 
        {
            Logs.Message($"[Rimworld Together] > popping {windowStack.Peek().ToString()}");

            Find.WindowStack.TryRemove(windowStack.Pop(), true);

            if(windowStack.Count > 0 ) Find.WindowStack.Add(windowStack.Peek());
        }

        public static void PopDialog(Window window)
        {
            Logs.Message($"[Rimworld Together] > popping {windowStack.Peek().ToString()}");
            Find.WindowStack.TryRemove(windowStack.Pop(), true);
            Find.WindowStack.Add(windowStack.Peek());
        }

        public static void PopDialogOnly()
        {
            Logs.Message($"[Rimworld Together] > popping {windowStack.Peek().ToString()}");
            Find.WindowStack.TryRemove(windowStack.Pop(), true);
        }

        public static void WaitForDialogInput(Window window)
        {
            PushNewDialog(window);

            while (currentDialog == window) Thread.Sleep(100);

            return;
        }

        private static void listWindows()
        {
            Window[] winArray = windowStack.ToArray();

            for (int i = winArray.Length-1; i >= 0;i--)
            {
                Logs.Message($"Window at {i} is {winArray[i]}");
            }
        }
    }
}
