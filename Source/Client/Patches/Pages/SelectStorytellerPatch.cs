using HarmonyLib;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimworldTogether
{
    [HarmonyPatch(typeof(Page_SelectStoryteller), "DoWindowContents")]
    public static class PatchSelectStorytellerPage
    {
        [HarmonyPostfix]
        public static void DoPost(Rect rect)
        {
            if (ClientValues.isLoadingPrefabWorld)
            {
                Text.Font = GameFont.Small;
                Vector2 buttonSize = new Vector2(150f, 38f);
                Vector2 buttonLocation = new Vector2(rect.xMax - buttonSize.x, rect.yMax - buttonSize.y);
                if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), "Join")) { }
            }
        }
    }

    [HarmonyPatch(typeof(Page_SelectStoryteller), "PreOpen")]
    public static class PatchDifficultyOverdrive
    {
        [HarmonyPrefix]
        public static bool DoPre(ref DifficultyDef ___difficulty, ref Difficulty ___difficultyValues)
        {
            if (ClientValues.isLoadingPrefabWorld && DifficultyValues.UseCustomDifficulty)
            {
                ___difficulty = DifficultyDefOf.Rough;
                ___difficultyValues = new Difficulty(___difficulty);

                Find.GameInitData.permadeathChosen = true;
            }

            return true;
        }
    }

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(StorytellerUI), "DrawStorytellerSelectionInterface")]
    public static class PatchSelectStorytellerDifficulty
    {
        private static readonly Texture2D StorytellerHighlightTex = ContentFinder<Texture2D>.Get("UI/HeroArt/Storytellers/Highlight");

        private static Vector2 scrollPosition = default(Vector2);

        private static Vector2 explanationScrollPosition = default(Vector2);

        private static Rect explanationInnerRect = default(Rect);

        [HarmonyPrefix]
        public static bool DoPre(Rect rect, ref StorytellerDef chosenStoryteller, ref DifficultyDef difficulty, ref Difficulty difficultyValues, Listing_Standard infoListing)
        {
            if (!DifficultyValues.UseCustomDifficulty || !Network.isConnectedToServer) return true;
            else
            {
                Widgets.BeginGroup(rect);
                Rect outRect = new Rect(0f, 0f, Storyteller.PortraitSizeTiny.x + 16f, rect.height);
                Widgets.BeginScrollView(viewRect: new Rect(0f, 0f, Storyteller.PortraitSizeTiny.x, (float)DefDatabase<StorytellerDef>.AllDefs.Count() * (Storyteller.PortraitSizeTiny.y + 10f)), outRect: outRect, scrollPosition: ref scrollPosition);
                Rect rect2 = new Rect(0f, 0f, Storyteller.PortraitSizeTiny.x, Storyteller.PortraitSizeTiny.y).ContractedBy(4f);

                foreach (StorytellerDef item in DefDatabase<StorytellerDef>.AllDefs.OrderBy((StorytellerDef tel) => tel.listOrder))
                {
                    if (item.listVisible)
                    {
                        bool flag = chosenStoryteller == item;
                        Widgets.DrawOptionBackground(rect2, flag);
                        if (Widgets.ButtonImage(rect2, item.portraitTinyTex, Color.white, new Color(0.72f, 0.68f, 0.59f)))
                        {
                            TutorSystem.Notify_Event("ChooseStoryteller");
                            chosenStoryteller = item;
                        }

                        if (flag) GUI.DrawTexture(rect2, StorytellerHighlightTex);

                        rect2.y += rect2.height + 8f;
                    }
                }

                Widgets.EndScrollView();
                Rect outRect2 = new Rect(outRect.xMax + 8f, 0f, rect.width - outRect.width - 8f, rect.height);
                explanationInnerRect.width = outRect2.width - 16f;
                Widgets.BeginScrollView(outRect2, ref explanationScrollPosition, explanationInnerRect);
                Text.Font = GameFont.Small;
                Widgets.Label(new Rect(0f, 0f, 300f, 999f), "HowStorytellersWork".Translate());
                Rect rect3 = new Rect(0f, 120f, 290f, 9999f);
                float num = 300f;

                if (chosenStoryteller != null && chosenStoryteller.listVisible)
                {
                    Rect position = new Rect(390f - outRect2.x, rect.height - Storyteller.PortraitSizeLarge.y - 1f, Storyteller.PortraitSizeLarge.x, Storyteller.PortraitSizeLarge.y);
                    GUI.DrawTexture(position, chosenStoryteller.portraitLargeTex);
                    Text.Anchor = TextAnchor.UpperLeft;
                    infoListing.Begin(rect3);
                    Text.Font = GameFont.Medium;
                    infoListing.Indent(15f);
                    infoListing.Label(chosenStoryteller.label);
                    infoListing.Outdent(15f);
                    Text.Font = GameFont.Small;
                    infoListing.Gap(8f);
                    infoListing.Label(chosenStoryteller.description, 160f);
                    infoListing.Gap(6f);

                    num = rect3.y + infoListing.CurHeight;
                    infoListing.End();
                }

                explanationInnerRect.height = num;
                Widgets.EndScrollView();
                Widgets.EndGroup();

                return false;
            }
        }
    }
}
