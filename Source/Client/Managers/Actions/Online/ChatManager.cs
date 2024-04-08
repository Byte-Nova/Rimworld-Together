using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Shared;
using UnityEngine;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
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
            ChatData chatData = new ChatData();
            chatData.usernames.Add(username);
            chatData.messages.Add(messageToSend);

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.ChatPacket), chatData);
            Network.listener.EnqueuePacket(packet);
        }

        public static void ReceiveMessages(Packet packet)
        {
            ChatData chatData = (ChatData)Serializer.ConvertBytesToObject(packet.contents);

            for (int i = 0; i < chatData.usernames.Count(); i++)
            {
                AddMessageToChat(chatData.usernames[i],
                    chatData.messages[i],
                    (UserColor)int.Parse(chatData.userColors[i]),
                    (MessageColor)int.Parse(chatData.messageColors[i]));
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

        public static void CleanChat()
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
