using Shared;
using static Shared.CommonEnumerators;
using System;

namespace GameClient
{
    //Class that handles loging responses from the server

    public static class LoginManager
    {
        //Parses the received packet into an order

        public static void ReceiveLoginResponse(Packet packet)
        {
            LoginData loginData = (LoginData)Serializer.ConvertBytesToObject(packet.contents);
            Action postDisconnectAction = delegate { };

            switch (loginData.tryResponse)
            {
                case LoginResponse.InvalidLogin:
                    postDisconnectAction = delegate { DialogManager.PushNewDialog(new RT_Dialog_Error("Login details are invalid! Please try again!")); };
                    break;

                case LoginResponse.BannedLogin:
                    postDisconnectAction = delegate { DialogManager.PushNewDialog(new RT_Dialog_Error("You are banned from this server!")); };
                    break;

                case LoginResponse.RegisterInUse:
                    postDisconnectAction = delegate { DialogManager.PushNewDialog(new RT_Dialog_Error("That username is already in use! Please try again!")); };
                    break;

                case LoginResponse.RegisterError:
                    postDisconnectAction = delegate { DialogManager.PushNewDialog(new RT_Dialog_Error("There was an error registering! Please try again!")); };
                    break;

                case LoginResponse.ExtraLogin:
                    postDisconnectAction = delegate { DialogManager.PushNewDialog(new RT_Dialog_Error("You connected from another place!")); };
                    break;

                case LoginResponse.WrongMods:
                    postDisconnectAction = delegate { ModManager.GetConflictingMods(packet); };
                    break;

                case LoginResponse.ServerFull:
                    postDisconnectAction = delegate { DialogManager.PushNewDialog(new RT_Dialog_Error("Server is full!")); };
                    break;

                case LoginResponse.Whitelist:
                    postDisconnectAction = delegate { DialogManager.PushNewDialog(new RT_Dialog_Error("Server is whitelisted!")); };
                    break;

                case LoginResponse.WrongVersion:
                    postDisconnectAction = delegate { DialogManager.PushNewDialog(new RT_Dialog_Error($"Mod version mismatch! Expected version {loginData.extraDetails[0]}")); };
                    break;

                case LoginResponse.NoWorld:
                    postDisconnectAction = delegate { DialogManager.PushNewDialog(new RT_Dialog_Error($"Server is currently being set up! Join again later!")); };
                    break;
            }

            ClientValues.SetIntentionalDisconnect(true, DisconnectionManager.DCReason.Custom, loginData.ToString(), postDisconnectAction);
            Network.listener.disconnectFlag = true;
        }
    }
}
