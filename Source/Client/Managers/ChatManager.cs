using GameClient.Core.Configs;
using GameClient.Defs;
using GameClient.Misc;
using HarmonyLib;
using RimWorld;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TCPNetwork.Packets;
using UnityEngine;
using Verse;
using Verse.Sound;
using static Shared.CommonEnumerators;

namespace GameClient.Managers
{
    [StaticConstructorOnStartup]
    public static class ChatManager
    {
        private static MainButtonDef ChatButtonDef { get; set; } = DefDatabase<MainButtonDef>.GetNamed("Chat");

        public static string CurrentChatInput { get; set; } = string.Empty;

        public static List<string> ChatMessageCache { get; set; } = new List<string>();

        public static bool IsChatTabOpen { get; set; } = false;

        public static bool ShouldScrollChat { get; set; } = false;

        public static Vector2 ChatBoxPosition = new Vector2(0, UI.screenHeight - 35f - 600f);

        public static bool ChatAutoscroll = true;

        [HandlesPacket(PacketHeader.ChatManager)]
        private static void ParsePacket(byte[] bytes)
        {
            ChatData data = Serializer.ConvertBytesToObject<ChatData>(bytes);

            AddMessageToChat(data._username, data._message, data._usernameColor, data._messageColor);
        }

        public static void SendMessage(string messageToSend)
        {
            RTChatDefSounds.ChatSend.PlayOneShotOnCamera();

            ChatData chatData = new ChatData();
            chatData._username = SessionHandler.Username;
            chatData._message = messageToSend;

            ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.ChatManager, chatData);
        }

        public static void AddMessageToChat(string username, string message, ChatColor userColor, ChatColor messageColor)
        {
            if (ChatMessageCache.Count() > 100) ChatMessageCache.RemoveAt(0);

            if (ChatManagerH.CheckIfHasBeenTagged(message))
                message = message.Replace($"@{SessionHandler.Username}", $"<color=red>@{SessionHandler.Username}</color>");

            ChatMessageCache.Add($"<color=grey>{DateTime.Now:HH:mm}</color> " +
                $"{ChatManagerH.messageColorDictionary[userColor]}{username}</color>: " +
                $"{ChatManagerH.messageColorDictionary[messageColor]}{ChatManagerH.ParseMessage(message)}</color>");

            if (ChatAutoscroll) ShouldScrollChat = true;

            if (!IsChatTabOpen)
            {
                ToggleChatIcon(true);

                if (!ModConfigGetter.MuteChatSoundBool)
                    RTChatDefSounds.ChatReceive.PlayOneShotOnCamera();
            }
        }

        [OnSessionEnd]
        private static void CleanChat()
        {
            CurrentChatInput = string.Empty;
            ChatMessageCache = new List<string>();
            IsChatTabOpen = false;
        }

        public static void ToggleChatIcon(bool mode)
        {
            if (mode) AccessTools.Field(typeof(MainButtonDef), "icon").SetValue(ChatButtonDef, RTChatDefs.ChatOn);
            else AccessTools.Field(typeof(MainButtonDef), "icon").SetValue(ChatButtonDef, RTChatDefs.ChatOff);
        }
    }

    public static class ChatManagerH
    {
        public static Dictionary<ChatColor, string> messageColorDictionary = new Dictionary<ChatColor, string>()
        {
            { ChatColor.Normal, "<color=white>" },
            { ChatColor.Admin, "<color=red>" },
            { ChatColor.Console, "<color=yellow>" },
            { ChatColor.Private, "<color=#3ae0dd>" },
            { ChatColor.Discord, "<color=#b36bff>" },
            { ChatColor.Server, "<color=white>" } // that extra space made server look like it was doubled spaced lmao!!
        };

        public static string[] GetMessageWords(string message) { return message.Split(' '); }

        public static bool CheckIfHasBeenTagged(string message) { return GetMessageWords(message).Contains($"@{SessionHandler.Username}"); }

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
                        case "[/]":
                            if (codeType.Count > 0) message = message.ReplaceFirst(verification, $"</{codeType.Pop()}>");
                            verification = "";
                            break;

                        case "[b]":
                            message = message.Replace(verification, "<b>");
                            codeType.Push("b");
                            verification = "";
                            break;

                        case "[i]":
                            message = message.Replace(verification, "<i>");
                            codeType.Push("i");
                            verification = "";
                            break;

                        case "[n]":
                            if (fromBroadcast)
                            {
                                message = message.Replace(verification, "\n");
                                verification = "";
                            }
                            break;

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
}