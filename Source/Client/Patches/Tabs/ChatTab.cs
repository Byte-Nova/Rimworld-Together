using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace GameClient
{
    public class ChatTab : MainTabWindow
    {
        public override Vector2 RequestedTabSize => new Vector2(800f, 600f);

        private Vector2 scrollPositionPlayers = Vector2.zero;
        private Vector2 scrollPositionChat = Vector2.zero;

        private readonly int startAcceptingInputAtFrame;
            
        private bool AcceptsInput => startAcceptingInputAtFrame <= Time.frameCount;

        public ChatTab()
        {
            layer = WindowLayer.GameUI;

            forcePause = false;
            draggable = true;
            focusWhenOpened = false;
            drawShadow = false;
            closeOnAccept = false;
            closeOnCancel = false;
            preventCameraMotion = false;
            drawInScreenshotMode = false;

            soundAppear = SoundDefOf.CommsWindow_Open;

            closeOnAccept = false;
            closeOnCancel = true;
        }

        public override void PreOpen()
        {
            base.PreOpen();

            windowRect.x = ChatManager.chatBoxPosition.x;
            windowRect.y = ChatManager.chatBoxPosition.y;
        }

        public override void PostOpen()
        {
            base.PostOpen();

            ChatManager.isChatTabOpen = true;
            ChatManager.ToggleChatIcon(false);
        }

        public override void PostClose()
        {
            base.PostClose();

            ChatManager.isChatTabOpen = false;
        }

        public override void DoWindowContents(Rect rect)
        {
            ChatManager.chatBoxPosition.x = windowRect.x;
            ChatManager.chatBoxPosition.y = windowRect.y;

            Widgets.DrawLineHorizontal(rect.x, rect.y + 25f, rect.width);
            Widgets.DrawLineVertical(rect.x + 160f, rect.y + 25f, rect.height);

            DrawPlayerCount(rect);
            DrawPlayerList(new(rect.x, rect.y + 25f, 160f, rect.height - 50f));
            DrawMessageList(new(rect.x + 160f, rect.y + 32f, rect.width - 160f, rect.height - 60f));

            DrawPinCheckbox(rect);
            DrawInput(rect);

            CheckForEnterKey();
            if (ChatManager.shouldScrollChat) ScrollToLastMessage();
        }

        private void DrawPlayerCount(Rect rect)
        {
            string toShow = ServerValues.currentPlayers > 1 ? "RTChatOnlinePlayers".Translate(ServerValues.currentPlayers) : "RTChatOnlinePlayer".Translate(ServerValues.currentPlayers) ;

            Text.Font = GameFont.Small;
            Widgets.Label(new(rect.x, rect.y, Text.CalcSize(toShow).x, Text.CalcSize(toShow).y), $"<color=grey>{toShow}</color>");
        }

        private void DrawPlayerList(Rect mainRect)
        {
            List<string> orderedList = ServerValues.currentPlayerNames;
            orderedList.Sort();

            float height = 6f + orderedList.Count() * 25f;
            Rect viewRect = new(mainRect.x, mainRect.y, mainRect.width - 16f, height);

            Widgets.BeginScrollView(mainRect, ref scrollPositionPlayers, viewRect);

            float num = 0;
            float num2 = scrollPositionPlayers.y - 25f;
            float num3 = scrollPositionPlayers.y + mainRect.height;

            foreach (string str in orderedList)
            {
                if (num > num2 && num < num3)
                {
                    Rect rect = new(0f, mainRect.y + num, viewRect.width, 25f);
                    DrawCustomRowPlayerList(rect, str);
                }

                num += 25f;
            }

            Widgets.EndScrollView();
        }

        private void DrawMessageList(Rect mainRect)
        {
            float height = 6f;
            float heightCalcWidthOffset = 160f;
            float chatScrollbarSafezone = 30f;

            foreach (string str in ChatManager.chatMessageCache.ToArray()) height += Text.CalcHeight(str, mainRect.width - chatScrollbarSafezone);

            Rect viewRect = new(mainRect.x, mainRect.y, mainRect.width - chatScrollbarSafezone, height);

            Widgets.BeginScrollView(mainRect, ref scrollPositionChat, viewRect);

            float num = 0;
            float num2 = scrollPositionChat.y - chatScrollbarSafezone;
            float num3 = scrollPositionChat.y + mainRect.height;

            foreach (string str in ChatManager.chatMessageCache.ToArray())
            {
                if (num > num2 && num < num3)
                {
                    Rect rect2 = new(160f , mainRect.y + num, viewRect.width, Text.CalcHeight(str, mainRect.width - heightCalcWidthOffset - chatScrollbarSafezone));
                    DrawCustomRow(rect2, str);
                }

                num += Text.CalcHeight(str, mainRect.width - chatScrollbarSafezone);
            }

            Widgets.EndScrollView();
        }

        private void DrawInput(Rect rect)
        {
            Text.Font = GameFont.Small;
            string inputOne = Widgets.TextField(new(rect.xMin + 165f, rect.yMax - 25f, rect.width - 165f, 25f), ChatManager.currentChatInput);
            if (AcceptsInput && inputOne.Length <= 512) ChatManager.currentChatInput = inputOne;
        }

        private void DrawPinCheckbox(Rect rect)
        {
            string pinText = "RTAutoScroll".Translate();

            Text.Font = GameFont.Small;
            Widgets.CheckboxLabeled(new Rect(rect.xMax - Text.CalcSize(pinText).x * 1.5f, rect.y, Text.CalcSize(pinText).x * 2,
                Text.CalcSize(pinText).y), pinText, ref ChatManager.chatAutoscroll, placeCheckboxNearText: true);
        }

        private void CheckForEnterKey()
        {
            bool keyPressed = !string.IsNullOrWhiteSpace(ChatManager.currentChatInput) && (Event.current.keyCode == KeyCode.Return || 
                Event.current.keyCode == KeyCode.KeypadEnter);

            if (keyPressed)
            {
                ChatManager.SendMessage(ChatManager.currentChatInput);
                ChatManager.currentChatInput = "";
            }
        }

        private void ScrollToLastMessage()
        {
            scrollPositionChat.Set(scrollPositionChat.x, scrollPositionChat.y + Mathf.Infinity);
            ClientValues.ToggleChatScroll(false);
        }

        private void DrawCustomRow(Rect rect, string message)
        {
            Text.Font = GameFont.Small;
            Rect fixedRect = new(rect.x + 10f, rect.y + 5f, rect.width, rect.height);
            Widgets.Label(fixedRect, message);
        }

        private void DrawCustomRowPlayerList(Rect rect, string str)
        {
            Text.Font = GameFont.Small;

            Rect fixedRect = new(rect.x + 10f, rect.y + 5f, rect.width - 10f, rect.height);
            Widgets.Label(fixedRect, str);

            if (Widgets.ButtonInvisible(fixedRect, false)) ChatManager.currentChatInput += $"@{str}";
            Widgets.DrawHighlightIfMouseover(fixedRect);
        }
    }
}
