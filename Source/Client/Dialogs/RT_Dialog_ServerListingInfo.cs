using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameClient.Managers;
using GameClient.Misc;
using HarmonyLib;
using Rimworld_Together_Master_Server.Data;
using TCPNetwork.Packets;
using Shared;
using UnityEngine;
using Verse;
using static Shared.CommonEnumerators;
using Shared.Files.Configs.Mods;
using GameClient.Hooks.TCPNetwork;

namespace GameClient.Dialogs
{
    public class RT_Dialog_ServerListingInfo : Window
    {
        public override Vector2 InitialSize => new Vector2(600f, 250f);

        private static FieldInfo ModsConfigData;


        private static FieldInfo ModsConfigDataActiveMods;

        private ServerInfo ServerInfo { get; set; }

        public RT_Dialog_ServerListingInfo(ServerInfo info) 
        {
            this.ServerInfo = info;
            ModsConfigData = AccessTools.Field(typeof(ModsConfig), "data");
            ModsConfigDataActiveMods = AccessTools.Field(AccessTools.TypeByName("Verse.ModsConfig+ModsConfigData"), "activeMods");
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Vector2 titleSize = Text.CalcSize(ServerInfo._name);
            float centeredx = inRect.width / 2;
            Rect titleRect = new Rect(centeredx - titleSize.x / 2, inRect.y, titleSize.x, titleSize.y);
            Widgets.Label(titleRect, ServerInfo._name);

            Widgets.DrawLineHorizontal(0, titleSize.y + 3f, inRect.width);

            Text.Font = GameFont.Small;
            Rect descriptionRect = new Rect(inRect.x, titleSize.y + 6f, inRect.width / 3 * 2, inRect.height - 55f);
            Widgets.Label(descriptionRect, ServerInfo._description);

            Rect connectRect = new Rect(inRect.width - 135f, inRect.height - 55f, 125f, 45f);
            Rect playerCountRect = new Rect(connectRect.x, connectRect.y - 30f, 125f, 45f);

            Widgets.Label(playerCountRect, $"Population: {ServerInfo._currentPlayerCount}/{ServerInfo._maximumPlayerCount}");

            if (Widgets.ButtonText(connectRect, "Connect")) ConnectToServer();
        }

        private void ConnectToServer() 
        {
            ClientNetwork.Ip = ServerInfo._ip;
            ClientNetwork.Port = ServerInfo._port.ToString();
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Wait("Trying to connect to server"));
            ClientNetwork _ = new ClientNetwork();
            Close();
        }
    }
}
