using System.Linq;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using RimWorld;
using RimworldTogether.GameClient.Dialogs;
using RimworldTogether.GameClient.Managers;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.GameClient.Misc;
using RimworldTogether.GameClient.Values;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Misc;
using RimworldTogether.Shared.Network;
using UnityEngine;
using Verse;

namespace RimworldTogether.GameClient.Patches.Pages
{
    public class MainMenuPatch
    {
        [HarmonyPatch(typeof(MainMenuDrawer), "DoMainMenuControls")]
        public static class PatchButton
        {
            private static void DefaultServer(string name, string password)
            {
                Network.Network.ip = "127.0.0.1";
                Network.Network.port = "25555";
                Threader.GenerateThread(Threader.Mode.Start);
                Thread.Sleep(500);
                LoginDetailsJSON loginDetails = new LoginDetailsJSON();
                loginDetails.username = name;
                loginDetails.password = Hasher.GetHash(password);
                loginDetails.clientVersion = ClientValues.versionCode;
                loginDetails.runningMods = ModManager.GetRunningModList().ToList();

                ChatManager.username = loginDetails.username;
                Saver.SaveLoginDetails(DialogManager.dialog2ResultOne, DialogManager.dialog2ResultTwo);

                string[] contents = new string[] { Serializer.SerializeToString(loginDetails) };
                Packet packet = new Packet("LoginClientPacket", contents);
                Network.Network.SendData(packet);
            }

            [HarmonyPrefix]
            public static bool DoPre(Rect rect)
            {
                if (Current.ProgramState == ProgramState.Entry)
                {
                    Vector2 buttonSize = new Vector2(170f, 45f);
                    Vector2 buttonLocation = new Vector2(rect.x, rect.y);
                    if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), "")) DialogShortcuts.ShowConnectDialogs();
                    if (CommandLineParamsManager.instantConnect == "true")
                    {
                        DefaultServer(CommandLineParamsManager.name, CommandLineParamsManager.name);
                        return true;
                    }

                    if (CommandLineParamsManager.fastConnect == "true")
                        if (Widgets.ButtonText(new Rect(buttonLocation.x - 200, buttonLocation.y, buttonSize.x, buttonSize.y), ""))
                            DefaultServer(CommandLineParamsManager.name, CommandLineParamsManager.name);
                }

                return true;
            }

            [HarmonyPostfix]
            public static void DoPost(Rect rect)
            {
                if (Current.ProgramState == ProgramState.Entry)
                {
                    Vector2 buttonSize = new Vector2(170f, 45f);
                    Vector2 buttonLocation = new Vector2(rect.x, rect.y);
                    if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), "Play Together"))
                    {
                    }

                    if (CommandLineParamsManager.fastConnect == "true")
                        Widgets.ButtonText(new Rect(buttonLocation.x - 200, buttonLocation.y, buttonSize.x, buttonSize.y), "FastConnect");
                }
            }
        }
    }
}