using GameClient.Dialogs;
using GameClient.Managers;
using GameClient.Values;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.Sound;
using static Shared.CommonEnumerators;

namespace GameClient.Patches.Pages
{
    [HarmonyPatch(typeof(Page_SelectScenario), nameof(Page_SelectScenario.DoWindowContents))]
    public static class Patch_Page_SelectScenario_DoWindowContents
    {
        public static bool executedMessage;

        [HarmonyPrefix]
        public static bool DoPre(Rect rect, Page_SelectScenario __instance)
        {
            if (SessionValues.CurrentNetworkState == ClientNetworkState.Disconnected) return true;

            if (!ClientValues.IsGeneratingFreshWorld && SessionValues.ScenarioFile.EnforceScenario)
            {
                if (executedMessage) return true;
                else
                {
                    Action toDo = delegate
                    {
                        Scenario scenario = (Scenario)typeof(Page_SelectScenario).GetField("curScen", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
                        Page_SelectScenario.BeginScenarioConfiguration(scenario, __instance);
                        GameParameterManager.SetScenario(SessionValues.ScenarioFile);

                        RT_Dialog_Base.PushNewDialog(__instance.next);
                        __instance.Close();

                        executedMessage = false;
                    };
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("MESSAGE", new string[] { "Scenario will be forced by the server" }, toDo));

                    executedMessage = true;
                }
            }

            else
            {
                if (Widgets.ButtonText(RT_Dialog_Base.GetRectForLocation(rect, RT_Dialog_Base.SmallButtonSize, RT_Dialog_Base.RectLocation.BottomLeft), "") || KeyBindingDefOf.Cancel.KeyDownEvent)
                {
                    __instance.Close();
                    DisconnectionManager.SetIntentionalDisconnect(true, DisconnectionManager.DCReason.QuitToMenu);
                    ClientNetwork.Instance.ClientListener.DisconnectFlag = true;
                }
            }

            return true;
        }

        [HarmonyPostfix]
        public static void DoPost(Rect rect)
        {
            if (SessionValues.CurrentNetworkState == ClientNetworkState.Disconnected) return;

            if (Widgets.ButtonText(RT_Dialog_Base.GetRectForLocation(rect, RT_Dialog_Base.SmallButtonSize, RT_Dialog_Base.RectLocation.BottomLeft), "Disconnect")) { };
        }
    }

    [HarmonyPatch(typeof(Page_SelectScenario), "GoToScenarioEditor")]
    public static class Patch_Page_SelectScenario_GoToScenarioEditor
    {
        [HarmonyPrefix]
        public static bool DoPre()
        {
            if (SessionValues.CurrentNetworkState == ClientNetworkState.Disconnected) return true;
            else if (SessionValues.ActionValues.EnableCustomScenarios) return true;
            else
            {
                RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "This server doesn't allow custom scenarios!" }));
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(Page_SelectScenario), "DoScenarioSelectionList")]
    public static class Patch_Page_SelectScenario_DoScenarioSelectionList
    {
        private static float totalScenarioListHeight;
        private static Vector2 scenariosScrollPosition = Vector2.zero;
        private static Scenario curScen;

        [HarmonyPrefix]
        public static bool DoPre(Rect rect, ref Scenario ___curScen)
        {
            if (SessionValues.CurrentNetworkState == ClientNetworkState.Disconnected) return true;
            if (SessionValues.ActionValues.EnableCustomScenarios) return true;

            if (curScen != null) ___curScen = curScen;
            rect.xMax += 2f;
            Rect rect2 = new Rect(0f, 0f, rect.width - 16f - 2f, totalScenarioListHeight + 250f);
            Widgets.BeginScrollView(rect, ref scenariosScrollPosition, rect2);
            Rect rect3 = rect2.AtZero();
            rect3.height = 999999f;

            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.ColumnWidth = rect2.width;
            listing_Standard.Begin(rect3);

            Text.Font = GameFont.Small;
            ListScenariosOnListing(listing_Standard, ScenarioLister.ScenariosInCategory(ScenarioCategory.FromDef));

            listing_Standard.End();
            totalScenarioListHeight = listing_Standard.CurHeight;
            Widgets.EndScrollView();
            return false;
        }

        private static void ListScenariosOnListing(Listing_Standard listing, IEnumerable<Scenario> scenarios)
        {
            bool flag = false;
            foreach (Scenario scenario in scenarios)
            {
                if (scenario.showInUI)
                {
                    if (flag) listing.Gap(6f);

                    Scenario scen = scenario;
                    Rect rect = listing.GetRect(68f).ContractedBy(4f);
                    DoScenarioListEntry(rect, scen);
                    flag = true;
                }
            }

            if (!flag)
            {
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
                listing.Label("(" + "NoneLower".Translate() + ")");
                GUI.color = Color.white;
            }
        }

        private static void DoScenarioListEntry(Rect rect, Scenario scen)
        {
            bool flag = curScen == scen;
            Widgets.DrawOptionBackground(rect, flag);
            MouseoverSounds.DoRegion(rect);
            Rect rect2 = rect.ContractedBy(4f);
            Text.Font = GameFont.Small;
            Rect rect3 = rect2;
            rect3.height = Text.CalcHeight(scen.name, rect3.width);
            Widgets.Label(rect3, scen.name);
            Text.Font = GameFont.Tiny;
            Rect rect4 = rect2;
            rect4.yMin = rect3.yMax;

            if (!Text.TinyFontSupported)
            {
                rect4.yMin -= 6f;
                rect4.height += 6f;
            }

            Widgets.Label(rect4, scen.GetSummary());
            if (!scen.enabled) return;

            if (!flag && Widgets.ButtonInvisible(rect))
            {
                curScen = scen;
                SoundDefOf.Click.PlayOneShotOnCamera();
            }
        }
    }
}
