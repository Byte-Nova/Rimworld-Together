﻿using RimWorld;
using Shared;
using static Shared.CommonEnumerators;
using Verse;

namespace GameClient
{
    public static class WorldManager
    {
        public static void ParseWorldPacket(Packet packet)
        {
            WorldData worldData = Serializer.ConvertBytesToObject<WorldData>(packet.contents);

            switch (worldData.worldStepMode)
            {
                case WorldStepMode.Required:
                    OnRequireWorld();
                    break;

                case WorldStepMode.Existing:
                    OnExistingWorld(worldData);
                    break;
            }
        }

        public static void OnRequireWorld()
        {
            DialogManager.PopWaitDialog();

            ClientValues.ToggleGenerateWorld(true);

            Page toUse = new Page_SelectScenario();
            toUse.next = new Page_SelectStartingSite();
            DialogManager.PushNewDialog(toUse);

            RT_Dialog_OK_Loop d1 = new RT_Dialog_OK_Loop(new string[] { "RTDialogFirstPlayer".Translate(),
                "RTDialogYouConfigure".Translate()});

            DialogManager.PushNewDialog(d1);
        }

        public static void OnExistingWorld(WorldData worldData)
        {
            DialogManager.PopWaitDialog();

            WorldGeneratorManager.SetValuesFromServer(worldData);

            DialogManager.PushNewDialog(new Page_SelectScenario());
        }
    }
}
