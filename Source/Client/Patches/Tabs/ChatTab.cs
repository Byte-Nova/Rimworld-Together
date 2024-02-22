using RimWorld;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.GameClient.Values;
using UnityEngine;
using Verse;

namespace RimworldTogether.GameClient.Patches.Tabs
{
    public class ChatTab : MainTabWindow
    {
        public override Vector2 RequestedTabSize => new Vector2(350f, 400f);

        private Vector2 scrollPosition = Vector2.zero;

        private int startAcceptingInputAtFrame;
            
        private bool AcceptsInput => startAcceptingInputAtFrame <= Time.frameCount;

        public ChatTab()
        {
            layer = WindowLayer.GameUI;

            forcePause = false;
            absorbInputAroundWindow = false;

            soundAppear = SoundDefOf.CommsWindow_Open;
            //soundClose = SoundDefOf.CommsWindow_Close;

            closeOnAccept = false;
            closeOnCancel = true;
        }

        public override void DoWindowContents(Rect rect)
        {
            if (ChatManager.notificationIndex == 1) ChatManager.ToggleNotificationIcon(false);

            DrawPlayerCount(rect);

            DrawPinCheckbox(rect);

            Widgets.DrawLineHorizontal(rect.x, rect.y + 25f, rect.width);

            GenerateList(new Rect(new Vector2(rect.x, rect.y + 32f), new Vector2(rect.width, 300f)));

            DrawInput(rect);

            CheckForEnterKey();

            if (ChatManager.shouldScrollChat) ScrollToLastMessage();
        }

        private void DrawPlayerCount(Rect rect)
        {
            string message = $"Online Players: [{ServerValues.currentPlayers}]";

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, Text.CalcSize(message).x, Text.CalcSize(message).y), message);
        }

        private void GenerateList(Rect rect)
        {
            float height = 6f;

            foreach (string str in ChatManager.chatMessageCache.ToArray()) height += Text.CalcHeight(str, rect.width);

            Rect viewRect = new Rect(rect.x, rect.y, rect.width - 16f, height);

            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);

            float num = 0;
            float num2 = scrollPosition.y - 30f;
            float num3 = scrollPosition.y + rect.height;
            int num4 = 0;

            int index = 0;
            foreach (string str in ChatManager.chatMessageCache.ToArray())
            {
                if (num > num2 && num < num3)
                {
                    Rect rect2 = new Rect(0f, rect.y + num, viewRect.width, Text.CalcHeight(str, rect.width));
                    DrawCustomRow(rect2, str, index);
                }

                num += Text.CalcHeight(str, rect.width) + 0f;
                num4++;
                index++;
            }
            Widgets.EndScrollView();
        }

        private void DrawInput(Rect rect)
        {
            Text.Font = GameFont.Small;
            string inputOne = Widgets.TextField(new Rect(rect.xMin, rect.yMax - 25f, rect.width, 25f), ChatManager.currentChatInput);
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
            scrollPosition.Set(scrollPosition.x, scrollPosition.y + Mathf.Infinity);
            ClientValues.ToggleChatScroll(false);
        }

        private void DrawCustomRow(Rect rect, string message, int index)
        {
            Text.Font = GameFont.Small;
            Rect fixedRect = new Rect(new Vector2(rect.x + 10f, rect.y + 5f), new Vector2(rect.width - 36f, rect.height));
            //if (index % 2 == 0) Widgets.DrawHighlight(fixedRect);
            Widgets.Label(fixedRect, message);
        }
    }
}
