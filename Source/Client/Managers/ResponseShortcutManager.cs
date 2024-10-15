using Shared;
using Verse;

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
                    DialogManager.PushNewDialog(new RT_Dialog_Error("RTResponseIllegalActions".Translate()));
                    break;
                
                case CommonEnumerators.ResponseStepMode.UserUnavailable:
                    DialogManager.PopWaitDialog();
                    DialogManager.PushNewDialog(new RT_Dialog_Error("RTPlayerNotAvailable".Translate()));
                    break;

                case CommonEnumerators.ResponseStepMode.Pop:
                    DialogManager.PopWaitDialog();
                    break;
            }
        }
    }
}