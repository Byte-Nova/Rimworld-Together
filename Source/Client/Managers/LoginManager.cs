using GameClient.Dialogs;
using GameClient.Misc;
using TCPNetwork.Packets;
using Shared;
using static Shared.CommonEnumerators;

namespace GameClient.Managers
{
    //Class that handles loging responses from the server

    public static class LoginManager
    {
        //Parses the received packet into an order

        [HandlesPacket(PacketHeader.LoginManager)]
        private static void ParsePacket(byte[] bytes)
        {
            LoginData data = Serializer.ConvertBytesToObject<LoginData>(bytes);

            Printer.Warning(data, LogImportanceMode.Extreme);

            switch (data._tryResponse)
            {
                case LoginResponse.InvalidLogin:
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "Login details are invalid! Please try again!" }));
                    break;

                case LoginResponse.BannedLogin:
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "You are banned from this server!" }));
                    break;

                case LoginResponse.RegisterError:
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "There was an error registering! Please try again!" }));
                    break;

                case LoginResponse.ExtraLogin:
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "You connected from another place!" }));
                    break;

                case LoginResponse.WrongMods:
                    ModManagerH.GetConflictingMods(bytes);
                    break;

                case LoginResponse.ServerFull:
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "Server is full!" }));
                    break;

                case LoginResponse.Whitelist:
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "Server is whitelisted!" }));
                    break;

                case LoginResponse.WrongVersion:
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { $"Mod version mismatch! Expected version '{data._extraDetails[0]}'" }));
                    break;

                case LoginResponse.NoWorld:
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { $"Server is currently being set up! Join again later!" }));
                    break;
            }
        }
    }
}
