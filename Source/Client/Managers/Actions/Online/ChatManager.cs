﻿using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Shared;
using UnityEngine;
using Verse;
using Verse.Sound;
using static Shared.CommonEnumerators;

namespace GameClient
{
    //TODO
    //Apply different sounds depending on the message type, since right now only "Own" and "System" play

    [DefOf]
    public static class SoundDefs
    {
        public static SoundDef OwnChatDing;
        public static SoundDef AllyChatDing;
        public static SoundDef NeutralChatDing;
        public static SoundDef HostileChatDing;
        public static SoundDef SystemChatDing;
    }

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

        public static void SendMessage( string messageToSend )
        {
            SoundDefs.OwnChatDing.PlayOneShotOnCamera();
    
            ChatMessagesJSON chatMessagesJSON = new ChatMessagesJSON();
            chatMessagesJSON.usernames.Add(username);
            chatMessagesJSON.messages.Add(messageToSend);
    
            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.ChatPacket), chatMessagesJSON);
            Network.listener.EnqueuePacket(packet);
        }
    
        public static void ReceiveMessages(Packet packet)
        {
            ChatMessagesJSON chatMessagesJSON = (ChatMessagesJSON)Serializer.ConvertBytesToObject(packet.contents);

            bool doSound = false;
            for (int i = 0; i < chatMessagesJSON.usernames.Count(); i++) 
            {
                if (chatMessagesJSON.usernames[i] != username) doSound = true;        
            
                AddMessageToChat(chatMessagesJSON.usernames[i], chatMessagesJSON.messages[i], 
                    (UserColor)int.Parse(chatMessagesJSON.userColors[i]), (MessageColor)int.Parse( chatMessagesJSON.messageColors[i] ));
            }

            if (!ClientValues.isReadyToPlay) return;

            ToggleNotificationIcon(true);

            if (ClientValues.muteSoundBool) return;

            if (doSound) SoundDefs.SystemChatDing.PlayOneShotOnCamera();
        }

        public static void AddMessageToChat(string username, string message, UserColor userColor, MessageColor messageColor)
        {
            if (chatMessageCache.Count() > 100) chatMessageCache.RemoveAt(0);

            chatMessageCache.Add($"[{DateTime.Now.ToString("hh:mm tt")}] " + $"[{userColorDictionary[userColor]}{username}</color>]: " +
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
