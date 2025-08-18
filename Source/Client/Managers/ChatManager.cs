using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GameClient.Misc;
using GameClient.Values;
using HarmonyLib;
using RimWorld;
using Shared;
using UnityEngine;
using Verse;
using Verse.Sound;
using static Shared.CommonEnumerators;
using TCPNetwork.Packets;

namespace GameClient.Managers
{
    [StaticConstructorOnStartup]
    public static class ChatManager
    {
        public static Vector2 ChatBoxPosition = new Vector2(0, UI.screenHeight - 35f - 600f);
        private static MainButtonDef ChatButtonDef { get; set; } = DefDatabase<MainButtonDef>.GetNamed("Chat");

        //Data
        public static string CurrentChatInput { get; set; } = "";
        public static List<string> ChatMessageCache { get; set; } = new List<string>();

        //Booleans
        public static bool IsChatTabOpen { get; set; }
        public static bool IsChatIconActive { get; set; }
        public static bool ShouldScrollChat { get; set; }
        public static bool ChatAutoscroll = true;

        //Chat clock
        private static Task ChatClockTask { get; set; }
        private static Semaphore Semaphore { get; set; } = new Semaphore(1, 1);

        //Icons
        public static int ChatIconIndex { get; set; }
        public static List<Texture2D> ChatIcons { get; set; } = new List<Texture2D>();

        [HandlesPacket(PacketHeader.ChatManager)]
        private static void ParsePacket(byte[] bytes)
        {
            ChatData data = Serializer.ConvertBytesToObject<ChatData>(bytes);

            Printer.Warning(data, LogImportanceMode.Extreme);

            bool hasBeenTagged = false;
            if (ChatManagerHelper.GetMessageWords(data._message).Contains($"@{ClientValues.Username}"))
            {
                hasBeenTagged = true;
                data._message = data._message.Replace($"@{ClientValues.Username}", $"<color=red>@{ClientValues.Username}</color>");
            }

            AddMessageToChat(data._username, data._message, data._usernameColor, data._messageColor);

            if (!ClientValues.IsReadyToPlay) return;

            if (!IsChatTabOpen) ToggleChatIcon(true);

            if (hasBeenTagged) ChatSounds.SystemChatDing.PlayOneShotOnCamera();
        }

        public static void SendMessage(string messageToSend)
        {
            ChatSounds.OwnChatDing.PlayOneShotOnCamera();

            ChatData chatData = new ChatData();
            chatData._username = ClientValues.Username;
            chatData._message = messageToSend;

            ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.ChatManager, chatData);
        }

        public static void AddMessageToChat(string username, string message, UserColor userColor, MessageColor messageColor)
        {
            if (ChatMessageCache.Count() > 100) ChatMessageCache.RemoveAt(0);

            ChatMessageCache.Add($"<color=grey>{DateTime.Now.ToString("HH:mm")}</color> " + $"{ChatManagerHelper.userColorDictionary[userColor]}{username}</color>: " +
                $"{ChatManagerHelper.messageColorDictionary[messageColor]}{ChatManagerHelper.ParseMessage(message)}</color>");

            if (ChatAutoscroll) ClientValues.ToggleChatScroll(true);
        }

        public static void CleanChat()
        {
            CurrentChatInput = "";
            ChatMessageCache = new List<string>();

            IsChatTabOpen = false;
            IsChatIconActive = false;
            ChatAutoscroll = true;
        }

        public static void ToggleChatIcon(bool mode)
        {
            if (!ClientValues.IsReadyToPlay) return;

            IsChatIconActive = mode;

            if (mode)
            {
                Semaphore.WaitOne();

                ChatClockTask ??= Threader.GenerateThread(Threader.Mode.Chat);

                Semaphore.Release();
            }
        }

        public static void UpdateChatIcon()
        {
            ChatIconIndex++;
            if (ChatIconIndex > ChatIcons.Count) ChatIconIndex = 0;
            AccessTools.Field(typeof(MainButtonDef), "icon").SetValue(ChatButtonDef, ChatIcons[ChatIconIndex]);
        }

        private static void TurnOffChatIcon() { AccessTools.Field(typeof(MainButtonDef), "icon").SetValue(ChatButtonDef, ChatIcons[0]); }

        public static void ChatClock()
        {
            while (IsChatIconActive)
            {
                MainThreadHandler.Instance.Enqueue(UpdateChatIcon);

                Thread.Sleep(250);
            }

            ChatIconIndex = 0;

            MainThreadHandler.Instance.Enqueue(TurnOffChatIcon);

            ChatClockTask = null;
        }
    }

    public static class ChatManagerHelper
    {
        public static Dictionary<UserColor, string> userColorDictionary = new Dictionary<UserColor, string>()
        {
            { UserColor.Normal, "<color=white>" },
            { UserColor.Admin, "<color=red>" },
            { UserColor.Console, "<color=yellow>" },
            { UserColor.Private, "<color=#3ae0dd>" },
            { UserColor.Discord, "<color=#9656ce>" },
            { UserColor.Server, "<color=#6d90c9>"}
        };

        public static Dictionary<MessageColor, string> messageColorDictionary = new Dictionary<MessageColor, string>()
        {
            { MessageColor.Normal, "<color=white>" },
            { MessageColor.Admin, "<color=white>" },
            { MessageColor.Console, "<color=yellow>" },
            { MessageColor.Private, "<color=#3ae0dd>" },
            { MessageColor.Discord, "<color=white>" },
            { MessageColor.Server, " <color=white>" }
        };

        public static string[] GetMessageWords(string message)
        {
            return message.Split(' ');
        }

        public static string ParseMessage(string message, bool fromBroadcast = false)
        {
            bool verifying = false;
            string verification = "";
            Stack<string> codeType = new Stack<string>();

            message = Regex.Replace(message, @"\*\*\*(.+?)\*\*\*", "[b][i]$1[/][/]");
            message = Regex.Replace(message, @"\*\*(.+?)\*\*", "[b]$1[/]");
            message = Regex.Replace(message, @"\*(.+?)\*", "[i]$1[/]");
            message = Regex.Replace(message, @"\&([a-fA-F0-9]{6})(.+?)\&\&", "[$1]$2[/]");

            foreach (char c in message)
            {
                if (c == '[') verifying = true;

                if (verifying)
                {
                    verification += c;
                    if (c == ']') verifying = false;
                }

                if (verification != "" && !verifying)
                {
                    switch (verification.ToLower())
                    {
                        //Check for TAG CLOSING

                        case "[/]":
                            if (codeType.Count > 0) message = message.ReplaceFirst(verification, $"</{codeType.Pop()}>");
                            verification = "";
                            break;

                        //Check for BOLD

                        case "[b]":
                            message = message.Replace(verification, "<b>");
                            codeType.Push("b");
                            verification = "";
                            break;

                        //Check for CURSIVE

                        case "[i]":
                            message = message.Replace(verification, "<i>");
                            codeType.Push("i");
                            verification = "";
                            break;

                        //Check for NEW LINE (broadcasts only)

                        case "[n]":
                            if (fromBroadcast)
                            {
                                message = message.Replace(verification, "\n");
                                verification = "";
                            }
                            break;

                        //Check for CUSTOM COLOR

                        default:
                            if (Regex.IsMatch(verification, @"\[[a-fA-F0-9]{6}\]"))
                            {
                                string verificationReplacement = verification.Replace("[", "<color=#").Replace("]", ">");
                                message = message.Replace(verification, verificationReplacement);
                                codeType.Push("color");
                                verification = "";
                            }
                            break;
                    }
                }
            }

            while (codeType.Count > 0) message += $"</{codeType.Pop()}>";

            return message;
        }
    }

    [StaticConstructorOnStartup]
    public static class ChatIcons
    {
        static ChatIcons()
        {
            ChatManager.ChatIcons.Add(ContentFinder<Texture2D>.Get("UI/ChatIconOff"));
            ChatManager.ChatIcons.Add(ContentFinder<Texture2D>.Get("UI/ChatIconOn"));
            ChatManager.ChatIcons.Add(ContentFinder<Texture2D>.Get("UI/ChatIconMid"));
            ChatManager.ChatIcons.Add(ContentFinder<Texture2D>.Get("UI/ChatIconOff"));
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
