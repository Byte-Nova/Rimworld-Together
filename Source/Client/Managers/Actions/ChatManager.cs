using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimworldTogether.GameClient.Values;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using UnityEngine;
using Verse;
using static Shared.Misc.CommonEnumerators;

namespace RimworldTogether.GameClient.Managers.Actions
{
    [StaticConstructorOnStartup]
    public static class ChatManager
    {
        public static Dictionary<UserColor, string> userColorDictionary = new Dictionary<UserColor, string>()
        {
            { UserColor.Normal, "<color=grey>" },
            { UserColor.Admin, "<color=red>" },
            { UserColor.Console, "<color=yellow>" }
        };

        public static Dictionary<MessageColor, string> messageColorDictionary = new Dictionary<MessageColor, string>()
        {
            { MessageColor.Normal, "<color=white>" },
            { MessageColor.Admin, "<color=white>" },
            { MessageColor.Console, "<color=yellow>" }
        };

        public static int notificationIndex;

        public static Texture2D iconChatOn;

        public static Texture2D iconChatOff;

        public static string username;

        public static string currentChatInput;

        public static bool shouldScrollChat;

        public static bool chatAutoscroll;

        public static List<string> chatMessageCache = new List<string>();

        public static void SendMessage(string messageToSend)
        {
            ChatMessagesJSON chatMessagesJSON = new ChatMessagesJSON();
            chatMessagesJSON.usernames.Add(username);
            chatMessagesJSON.messages.Add(messageToSend);

            Packet packet = Packet.CreatePacketFromJSON("ChatPacket", chatMessagesJSON);
            Network.Network.serverListener.SendData(packet);
        }

        public static void ReceiveMessages(Packet packet)
        {
            ChatMessagesJSON chatMessagesJSON = (ChatMessagesJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

            for (int i = 0; i < chatMessagesJSON.usernames.Count(); i++)
            {
                AddMessageToChat(chatMessagesJSON.usernames[i],
                    chatMessagesJSON.messages[i],
                    (UserColor)int.Parse(chatMessagesJSON.userColors[i]),
                    (MessageColor)int.Parse(chatMessagesJSON.messageColors[i]));
            }

            ToggleNotificationIcon(true);
        }

        public static void AddMessageToChat(string username, string message, UserColor userColor, MessageColor messageColor)
        {
            if (chatMessageCache.Count() > 100) chatMessageCache.RemoveAt(0);

            chatMessageCache.Add($"[{DateTime.Now.ToString("hh:mm tt")}] " +
                $"[{userColorDictionary[userColor]}{username}</color>]: " +
                $"{messageColorDictionary[messageColor]}{message}</color>");

            if (chatAutoscroll) ClientValues.ToggleChatScroll(true);
        }

        public static void ToggleNotificationIcon(bool mode)
        {
            if (ClientValues.isReadyToPlay)
            {
                MainButtonDef chatButtonDef = DefDatabase<MainButtonDef>.GetNamed("Chat");
                if (mode) AccessTools.Field(typeof(MainButtonDef), "icon").SetValue(chatButtonDef, iconChatOn);
                else AccessTools.Field(typeof(MainButtonDef), "icon").SetValue(chatButtonDef, iconChatOff);

                notificationIndex = mode ? 1 : 0;
            }
        }

        public static void ClearChat()
        {
            currentChatInput = "";
            chatMessageCache.Clear();
        }
    }

    [StaticConstructorOnStartup]
    static class IconHelper
    {
        static IconHelper()
        {
            ChatManager.iconChatOff = ContentFinder<Texture2D>.Get("UI/ChatIconOff");
            ChatManager.iconChatOn = ContentFinder<Texture2D>.Get("UI/ChatIconOn");
        }
    }
}
