using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace GameClient
{
    public class ChatTab : MainTabWindow
    {
        public override Vector2 RequestedTabSize => new Vector2(800f, 600f);

        private Vector2 scrollPositionPlayers = Vector2.zero;
        private Vector2 scrollPositionChat = Vector2.zero;

        private int startAcceptingInputAtFrame;
            
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

            windowRect.y = ChatManager.chatBoxPosition.y;
            windowRect.x = ChatManager.chatBoxPosition.x;
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

            DrawPlayerCount(rect);
            DrawPlayerList(new(rect.x, rect.y + 25f, 160f, rect.height - 50f));

            DrawPinCheckbox(rect);

            Widgets.DrawLineHorizontal(rect.x, rect.y + 25f, rect.width);
            Widgets.DrawLineVertical(rect.x + 160f, rect.y + 25f, rect.height - 50f);

            GenerateList(new(rect.x, rect.y + 32f, rect.width, rect.height - 60f));

            DrawInput(rect);

            CheckForEnterKey();

            if (ChatManager.shouldScrollChat) ScrollToLastMessage();
        }

        private void DrawPlayerCount(Rect rect)
        {
            string message = $"{ServerValues.currentPlayers} Online Players";

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, Text.CalcSize(message).x, Text.CalcSize(message).y), message);
        }

        private void DrawPlayerList(Rect mainRect)
        {
            List<string> orderedList = ServerValues.currentPlayerNames;

            orderedList.Sort();

            float height = 6f + (float)orderedList.Count() * 25f;
            Rect viewRect = new Rect(mainRect.x, mainRect.y, mainRect.width - 16f, height);

            Widgets.BeginScrollView(mainRect, ref scrollPositionPlayers, viewRect);

            float num = 0;
            float num2 = scrollPositionPlayers.y - 25f;
            float num3 = scrollPositionPlayers.y + mainRect.height;
            int num4 = 0;

            foreach (string str in orderedList)
            {
                if (num > num2 && num < num3)
                {
                    Rect rect = new Rect(0f, mainRect.y + num, viewRect.width, 25f);
                    DrawCustomRow(rect, str, num4);
                }

                num += 25f;
                num4++;
            }

            Widgets.EndScrollView();
        }

        private void GenerateList(Rect mainRect)
        {
            float height = 6f;

            foreach (string str in ChatManager.chatMessageCache.ToArray()) height += Text.CalcHeight(str, mainRect.width);

            Rect viewRect = new Rect(mainRect.x, mainRect.y, mainRect.width - 16f, height);

            Widgets.BeginScrollView(mainRect, ref scrollPositionChat, viewRect);

            float num = 0;
            float num2 = scrollPositionChat.y - 30f;
            float num3 = scrollPositionChat.y + mainRect.height;

            foreach (string str in ChatManager.chatMessageCache.ToArray())
            {
                if (num > num2 && num < num3)
                {
                    float offset = 160f;
                    Rect rect2 = new Rect(offset , mainRect.y + num, viewRect.width - offset, Text.CalcHeight(str, mainRect.width));
                    DrawCustomRow(rect2, str);
                }

                num += Text.CalcHeight(str, mainRect.width) + 0f;
            }

            Widgets.EndScrollView();
        }

        private void DrawInput(Rect rect)
        {
            Text.Font = GameFont.Small;
            string inputOne = Widgets.TextField(new Rect(rect.xMin + 160f, rect.yMax - 25f, rect.width - 160f, 25f), ChatManager.currentChatInput);
            if (AcceptsInput && inputOne.Length <= 512) ChatManager.currentChatInput = inputOne;
        }

        private void DrawPinCheckbox(Rect rect)
        {
            string message = "Auto Scroll";

            Text.Font = GameFont.Small;
            Widgets.CheckboxLabeled(new Rect(rect.xMax - Text.CalcSize(message).x * 1.5f, rect.y, Text.CalcSize(message).x * 2, 
                Text.CalcSize(message).y), message, ref ChatManager.chatAutoscroll, placeCheckboxNearText:true);
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
            Rect fixedRect = new Rect(rect.x + 10f, rect.y + 5f, rect.width - 36f, rect.height);
            Widgets.Label(fixedRect, message);
        }

        private void DrawCustomRow(Rect rect, string str, int index)
        {
            Text.Font = GameFont.Small;
            //if (index % 2 == 0) Widgets.DrawLightHighlight(rect);

            Rect fixedRect = new Rect(rect.x + 10f, rect.y + 5f, rect.width - 10f, rect.height);
            Widgets.Label(fixedRect, str);

            if (Widgets.ButtonInvisible(fixedRect, false)) ChatManager.currentChatInput += $"@{str}";
            Widgets.DrawHighlightIfMouseover(fixedRect);
        }
    }
}
