using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Shared;
using UnityEngine;
using Verse;
using Verse.Sound;
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

        public static Vector2 chatBoxPosition = new Vector2(UI.screenWidth - 350f, UI.screenHeight - 35 - 400f);

        private static MainButtonDef chatButtonDef = DefDatabase<MainButtonDef>.GetNamed("Chat");

        //Data
        public static string currentChatInput;
        public static List<string> chatMessageCache = new List<string>();

        //Booleans
        public static bool isChatTabOpen;
        public static bool chatAutoscroll;
        public static bool shouldScrollChat;
        public static bool isChatIconActive;

        //Chat clock
        private static Task chatClockTask;
        private static readonly Semaphore semaphore = new Semaphore(1, 1);

        //Icons
        public static int chatIconIndex;
        public static List<Texture2D> chatIcons = new List<Texture2D>();

        public static void SendMessage(string messageToSend)
        {
            ChatSounds.OwnChatDing.PlayOneShotOnCamera();
    
            ChatMessagesJSON chatMessagesJSON = new ChatMessagesJSON();
            chatMessagesJSON.usernames.Add(ClientValues.username);
            chatMessagesJSON.messages.Add(messageToSend);
    
            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.ChatPacket), chatMessagesJSON);
            Network.listener.EnqueuePacket(packet);
        }
    
        public static void ReceiveMessages(Packet packet)
        {
            ChatMessagesJSON chatMessagesJSON = (ChatMessagesJSON)Serializer.ConvertBytesToObject(packet.contents);

            for (int i = 0; i < chatMessagesJSON.usernames.Count(); i++) 
            {
                AddMessageToChat(chatMessagesJSON.usernames[i], chatMessagesJSON.messages[i], 
                    (UserColor)int.Parse(chatMessagesJSON.userColors[i]), 
                    (MessageColor)int.Parse(chatMessagesJSON.messageColors[i]));
            }

            if (!ClientValues.isReadyToPlay) return;

            if (!isChatTabOpen) ToggleChatIcon(true);

            if (ClientValues.muteSoundBool) return;

            if (!isChatTabOpen) ChatSounds.SystemChatDing.PlayOneShotOnCamera();
        }

        public static void AddMessageToChat(string username, string message, UserColor userColor, MessageColor messageColor)
        {
            if (chatMessageCache.Count() > 100) chatMessageCache.RemoveAt(0);

            chatMessageCache.Add($"[{DateTime.Now.ToString("hh:mm tt")}] " + $"[{userColorDictionary[userColor]}{username}</color>]: " +
                $"{messageColorDictionary[messageColor]}{message}</color>");

            if (chatAutoscroll) ClientValues.ToggleChatScroll(true);
        }

        public static void CleanChat()
        {
            currentChatInput = "";
            chatMessageCache = new List<string>();

            isChatTabOpen = false;
            isChatIconActive = false;
        }

        public static void ToggleChatIcon(bool mode)
        {
            if (!ClientValues.isReadyToPlay) return;

            isChatIconActive = mode;

            if (mode)
            {
                semaphore.WaitOne();

                if (chatClockTask == null) chatClockTask = Threader.GenerateThread(Threader.Mode.Chat);

                semaphore.Release();
            }
        }

        public static void UpdateChatIcon()
        {
            chatIconIndex++;
            if(chatIconIndex > chatIcons.Count) chatIconIndex = 0;
            AccessTools.Field(typeof(MainButtonDef), "icon").SetValue(chatButtonDef, chatIcons[chatIconIndex]);
        }

        private static void TurnOffChatIcon() { AccessTools.Field(typeof(MainButtonDef), "icon").SetValue(chatButtonDef, chatIcons[0]); }

        public static void ChatClock()
        {
            while(isChatIconActive)
            {
                Master.threadDispatcher.Enqueue(UpdateChatIcon);

                Thread.Sleep(250);
            }

            chatIconIndex = 0;

            Master.threadDispatcher.Enqueue(TurnOffChatIcon);

            chatClockTask = null;
        }
    }

    [StaticConstructorOnStartup]
    public static class ChatIcons
    {
        static ChatIcons()
        {
            ChatManager.chatIcons.Add(ContentFinder<Texture2D>.Get("UI/ChatIconOff"));
            ChatManager.chatIcons.Add(ContentFinder<Texture2D>.Get("UI/ChatIconOn"));
            ChatManager.chatIcons.Add(ContentFinder<Texture2D>.Get("UI/ChatIconMid"));
            ChatManager.chatIcons.Add(ContentFinder<Texture2D>.Get("UI/ChatIconOff"));
        }
    }

    //TODO
    //Apply different sounds depending on the message type, since right now only "Own" and "System" play

    [DefOf]
    public static class ChatSounds
    {
        public static SoundDef OwnChatDing;
        public static SoundDef AllyChatDing;
        public static SoundDef NeutralChatDing;
        public static SoundDef HostileChatDing;
        public static SoundDef SystemChatDing;
    }
}
