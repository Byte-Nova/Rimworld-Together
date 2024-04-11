using RimWorld;
using UnityEngine;
using Verse;
using System.Diagnostics;
using System;
using System.Linq;


namespace GameClient
{
    public class ChatTab : MainTabWindow
    {
        public override Vector2 InitialSize => new Vector2(0f, 0f);

        private Vector2 scrollPosition = Vector2.zero;

        private int startAcceptingInputAtFrame;
            
        private bool AcceptsInput => startAcceptingInputAtFrame <= Time.frameCount;

        //MainTabWindows will close if you click outside their box.
        //This is ingrained into their type
        //To side step this, we are using the tab window to call another window to open
        public ChatTab()
        {
        }

        public override void PreOpen()
        {
            base.PreOpen();

            if (ChatManager.isChatTabOpen) DialogManager.PopDialog(DialogManager.chatDialog);
            else                           DialogManager.PushNewDialog(DialogManager.chatDialog);

            ChatManager.isChatTabOpen = !ChatManager.isChatTabOpen;
        }


        public override void PostOpen()
        {
            base.PostOpen();

            Find.MainTabsRoot.ToggleTab(DefDatabase<MainButtonDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == "Chat"), false);
        }

        public override void DoWindowContents(Rect rect)
        {
          
        }
    }



}
