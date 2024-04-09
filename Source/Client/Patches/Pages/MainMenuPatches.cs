using HarmonyLib;
using RimWorld;
using Shared;
using System.Linq;
using UnityEngine;
using Verse;

namespace GameClient
{
    public class MainMenuPatches
    {
        [HarmonyPatch(typeof(MainMenuDrawer), "DoMainMenuControls")]
        public static class PatchButton
        {
            [HarmonyPrefix]
            public static bool DoPre(Rect rect)
            {
                if (Current.ProgramState == ProgramState.Entry)
                {
                    Vector2 buttonSize = new Vector2(170f, 45f);
                    Vector2 buttonLocation = new Vector2(rect.x, rect.y);
                    if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), ""))
                    {
                        if (Network.isConnectedToServer || Network.isTryingToConnect) return true;
                        else DialogShortcuts.ShowConnectDialogs();
                    }

                    buttonSize = new Vector2(45f, 45f);
                    buttonLocation = new Vector2(rect.x - 50f, rect.y);
                    if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), ""))
                    {
                        if (Network.isConnectedToServer || Network.isTryingToConnect) return true;

                        ClientValues.ToggleQuickConnecting(true);

                        string[] details = PreferenceManager.LoadConnectionDetails();
                        Network.ip = details[0];
                        Network.port = details[1];

                        DialogManager.PushNewDialog(new RT_Dialog_Wait("Trying to connect to server"));
                        Network.StartConnection();

                        details = PreferenceManager.LoadLoginDetails();
                        JoinDetailsJSON loginDetails = new JoinDetailsJSON();
                        loginDetails.username = details[0];
                        loginDetails.password = Hasher.GetHashFromString(details[1]);
                        loginDetails.clientVersion = CommonValues.executableVersion;
                        loginDetails.runningMods = ModManager.GetRunningModList().ToList();

                        ChatManager.username = loginDetails.username;

                        Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.LoginClientPacket), loginDetails);
                        Network.listener.EnqueuePacket(packet);
                    }
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

                    buttonSize = new Vector2(45f, 45f);
                    buttonLocation = new Vector2(rect.x - 50f, rect.y);
                    if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), "▶"))
                    {

                    }
                }
            }
        }
    }
}