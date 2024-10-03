using Shared;

namespace GameClient
{
    public static class ResponseShortcutManager
    {
        public static void ParsePacket(Packet packet)
        {
            ResponseShortcutData data = Serializer.ConvertBytesToObject<ResponseShortcutData>(packet.contents);

            switch(data.stepMode)
            {
                case CommonEnumerators.ResponseStepMode.IllegalAction:
                    DialogManager.PopWaitDialog();
                    DialogManager.PushNewDialog(new RT_Dialog_Error("Kicked for ilegal actions!"));
                    break;
                
                case CommonEnumerators.ResponseStepMode.UserUnavailable:
                    DialogManager.PopWaitDialog();
                    DialogManager.PushNewDialog(new RT_Dialog_Error("Player is not currently available!"));
                    break;

                case CommonEnumerators.ResponseStepMode.Pop:
                    DialogManager.PopWaitDialog();
                    break;
            }
        }
    }
}