using Verse;

namespace GameClient
{
    public static class AdminMenuManager
    {
        private static readonly string dialogTitle = "RTModAdminMenu".Translate();

        private static readonly string dialogDescription = "RTModAdminMenuDesc".Translate();

        private static readonly string[] menuButtons = new string[] { "RTModAdminMenuModManager".Translate(), "RTModAdminMenuCustomDifficulty".Translate() };

        public static void ShowAdminMenu()
        {
            RT_Dialog_ScrollButtons d1 = new RT_Dialog_ScrollButtons(dialogTitle, dialogDescription, 
                menuButtons, delegate { OpenSpecificMenu(); }, null);
                
            DialogManager.PushNewDialog(d1);
        }

        public static void OpenSpecificMenu()
        {
            switch (DialogManager.selectedScrollButton)
            {
                case 0:
                    ModManager.OpenModManagerMenu(false);
                    break;

                case 1:
                    DifficultyManager.OpenDifficultyMenu();
                    break;
            }
        }
    }
}