using GameClient.Dialogs;
using Shared;

namespace GameClient.Managers
{
    public static class ResponseShortcutManager
    {
        [HandlesPacket(PacketHeader.ResponseShortcutManager)]
        private static void ParsePacket(byte[] bytes)
        {
            ResponseShortcutData data = Serializer.ConvertBytesToObject<ResponseShortcutData>(bytes);

            switch (data._stepMode)
            {
                case CommonEnumerators.ResponseStepMode.IllegalAction:
                    RT_Dialog_Wait.Instance.Close();
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "Kicked for ilegal actions!" }));
                    break;

                case CommonEnumerators.ResponseStepMode.UserUnavailable:
                    RT_Dialog_Wait.Instance.Close();
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "Player is not currently available!" }));
                    break;

                case CommonEnumerators.ResponseStepMode.Pop:
                    RT_Dialog_Wait.Instance.Close();
                    break;
            }
        }
    }
}