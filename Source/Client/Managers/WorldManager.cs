﻿using RimWorld;
using Shared;
using Verse;

namespace GameClient
{
    public static class WorldManager
    {
        public static void ParseWorldPacket(Packet packet)
        {
            DialogManager.clearStack();

            WorldDetailsJSON worldDetailsJSON = (WorldDetailsJSON)Serializer.ConvertBytesToObject(packet.contents);

            switch (int.Parse(worldDetailsJSON.worldStepMode))
            {
                case (int)CommonEnumerators.WorldStepMode.Required:
                    OnRequireWorld();
                    break;

                case (int)CommonEnumerators.WorldStepMode.Existing:
                    OnExistingWorld(worldDetailsJSON);
                    break;
            }
        }

        public static void OnRequireWorld()
        {

            ClientValues.ToggleGenerateWorld(true);

            Page toUse = new Page_SelectScenario();
            toUse.next = new Page_SelectStartingSite();
            DialogManager.PushNewDialog(toUse);

            RT_Dialog_OK_Loop d1 = new RT_Dialog_OK_Loop("MESSAGE", new string[] { "You are the first person joining the server!",
                "Configure the world that everyone will play on" });

            DialogManager.PushNewDialog(d1);
        }

        public static void OnExistingWorld(WorldDetailsJSON worldDetailsJSON)
        {

            WorldGeneratorManager.SetValuesFromServer(worldDetailsJSON);

            DialogManager.PushNewDialog(new Page_SelectScenario());

            RT_Dialog_OK_Loop d1 = new RT_Dialog_OK_Loop("MESSAGE", new string[] { "You are joining an existing server for the first time!",
                "Configure your playstyle to your liking", "Some settings might be disabled by the server" });

            DialogManager.PushNewDialog(d1);
        }
    }
}
