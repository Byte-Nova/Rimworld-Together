using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimworldTogether.GameClient.Dialogs;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.GameClient.Values;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimworldTogether.GameClient.Patches.Pages
{
    public class SelectScenarioPatch
    {
        [HarmonyPatch(typeof(Page_SelectScenario), "DoWindowContents")]
        public static class PatchSelectScenarioPage
        {
            [HarmonyPrefix]
            public static bool DoPre(Rect rect, Page_SelectScenario __instance)
            {
                if (ClientValues.isLoadingPrefabWorld)
                {
                    Vector2 buttonSize = new Vector2(150f, 38f);
                    Vector2 buttonLocation = new Vector2(rect.xMin, rect.yMax - buttonSize.y);
                    if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), "") || KeyBindingDefOf.Cancel.KeyDownEvent)
                    {
                        __instance.Close();
                        Network.Network.serverListener.disconnectFlag = true;
                    }
                }

                return true;
            }

            [HarmonyPostfix]
            public static void DoPost(Rect rect)
            {
                if (ClientValues.isLoadingPrefabWorld)
                {
                    Text.Font = GameFont.Small;
                    Vector2 buttonSize = new Vector2(150f, 38f);
                    Vector2 buttonLocation = new Vector2(rect.xMin, rect.yMax - buttonSize.y);
                    if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), "Disconnect")) { }
                }
            }
        }

        [HarmonyPatch(typeof(Page_SelectScenario), "GoToScenarioEditor")]
        public static class PatchCustomScenarioCreate
        {
            [HarmonyPrefix]
            public static bool DoPre()
            {
                if (!ClientValues.isLoadingPrefabWorld || ServerValues.AllowCustomScenarios) return true;
                else
                {
                    DialogManager.PushNewDialog(new RT_Dialog_Error("This server doesn't allow custom scenarios!"));
                    return false;
                }
            }
        }

        [HarmonyPatch(typeof(Page_SelectScenario), "DoScenarioSelectionList")]
        public static class PatchCustomScenarioList
        {
            private static float totalScenarioListHeight;
            private static Vector2 scenariosScrollPosition = Vector2.zero;
            private static Scenario curScen;

            [HarmonyPrefix]
            public static bool DoPre(Rect rect, ref Scenario ___curScen)
            {
                if (!ClientValues.isLoadingPrefabWorld || ServerValues.AllowCustomScenarios) return true;
                else
                {
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
}
