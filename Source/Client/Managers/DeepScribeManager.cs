using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using RimWorld;
using RimWorld.Planet;
using Shared;
using UnityEngine.Assertions.Must;
using Verse;
using Verse.Noise;
using static UnityEngine.GraphicsBuffer;

namespace GameClient
{
    //Classes that handle transforming different game things into a useful class object

    public static class DeepScribeManager
    {
        public static Thing[] GetAllTransferedItems(TransferManifestJSON transferManifestJSON)
        {
            List<Thing> allTransferedItems = new List<Thing>();

            foreach (Pawn pawn in HumanScribeManager.GetHumansFromString(transferManifestJSON)) allTransferedItems.Add(pawn);

            foreach (Pawn animal in AnimalScribeManager.GetAnimalsFromString(transferManifestJSON)) allTransferedItems.Add(animal);

            foreach (Thing thing in ThingScribeManager.GetItemsFromString(transferManifestJSON)) allTransferedItems.Add(thing);

            return allTransferedItems.ToArray();
        }
    }

    //Class that handles transformation of humans

    public static class HumanScribeManager
    {
        //Functions

        public static Pawn[] GetHumansFromString(TransferManifestJSON transferManifestJSON)
        {
            List<Pawn> humans = new List<Pawn>();

            for (int i = 0; i < transferManifestJSON.humanDetailsJSONS.Count(); i++)
            {
                HumanDetailsJSON humanDetails = Serializer.SerializeFromString<HumanDetailsJSON>(transferManifestJSON.humanDetailsJSONS[i]);

                humans.Add(StringToHuman(humanDetails));
            }

            return humans.ToArray();
        }

        public static HumanDetailsJSON HumanToString(Pawn pawn, bool passInventory = true)
        {
            HumanDetailsJSON humanDetailsJSON = new HumanDetailsJSON();

            GetPawnHediffs(pawn, humanDetailsJSON);

            GetPawnXenotype(pawn, humanDetailsJSON);

            GetPawnXenogenes(pawn, humanDetailsJSON);

            GetPawnEndogenes(pawn, humanDetailsJSON);

            GetPawnBioDetails(pawn, humanDetailsJSON);

            GetPawnFavoriteColor(pawn, humanDetailsJSON);

            GetPawnStory(pawn, humanDetailsJSON);

            GetPawnSkills(pawn, humanDetailsJSON);

            GetPawnTraits(pawn, humanDetailsJSON);

            GetPawnApparel(pawn, humanDetailsJSON);

            GetPawnEquipment(pawn, humanDetailsJSON);

            GetPawnInventory(pawn, humanDetailsJSON, passInventory);

            GetPawnPosition(pawn, humanDetailsJSON);

            GetPawnRotation(pawn, humanDetailsJSON);

            return humanDetailsJSON;
        }

        public static Pawn StringToHuman(HumanDetailsJSON humanDetailsJSON)
        {
            Pawn human = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);

            SetPawnHediffs(human, humanDetailsJSON);

            SetPawnXenotype(human, humanDetailsJSON);

            SetPawnXenogenes(human, humanDetailsJSON);

            SetPawnEndogenes(human, humanDetailsJSON);

            SetPawnBioDetails(human, humanDetailsJSON);

            SetPawnFavoriteColor(human, humanDetailsJSON);

            SetPawnStory(human, humanDetailsJSON);

            SetPawnSkills(human, humanDetailsJSON);

            SetPawnTraits(human, humanDetailsJSON);

            SetPawnApparel(human, humanDetailsJSON);

            SetPawnEquipment(human, humanDetailsJSON);

            SetPawnInventory(human, humanDetailsJSON);

            SetPawnPosition(human, humanDetailsJSON);

            SetPawnRotation(human, humanDetailsJSON);

            return human;
        }

        //Getters

        private static void GetPawnBioDetails(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            try
            {
                humanDetailsJSON.defName = pawn.def.defName;
                humanDetailsJSON.name = pawn.LabelShortCap.ToString();
                humanDetailsJSON.biologicalAge = pawn.ageTracker.AgeBiologicalTicks.ToString();
                humanDetailsJSON.chronologicalAge = pawn.ageTracker.AgeChronologicalTicks.ToString();
                humanDetailsJSON.gender = pawn.gender.ToString();

                humanDetailsJSON.hairDefName = pawn.story.hairDef.defName.ToString();
                humanDetailsJSON.hairColor = pawn.story.HairColor.ToString();
                humanDetailsJSON.headTypeDefName = pawn.story.headType.defName.ToString();
                humanDetailsJSON.skinColor = pawn.story.SkinColor.ToString();
                humanDetailsJSON.beardDefName = pawn.style.beardDef.defName.ToString();
                humanDetailsJSON.bodyTypeDefName = pawn.story.bodyType.defName.ToString();
                humanDetailsJSON.FaceTattooDefName = pawn.style.FaceTattoo.defName.ToString();
                humanDetailsJSON.BodyTattooDefName = pawn.style.BodyTattoo.defName.ToString();
            }
            catch (Exception e) { Log.Warning($"Failed to load biological details from human {pawn.Label}. Reason: {e}"); }
        }

        private static void GetPawnHediffs(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            if (pawn.health.hediffSet.hediffs.Count() > 0)
            {
                foreach (Hediff hd in pawn.health.hediffSet.hediffs)
                {
                    try
                    {
                        humanDetailsJSON.hediffDefNames.Add(hd.def.defName);

                        if (hd.Part != null) humanDetailsJSON.hediffPartDefName.Add(hd.Part.def.defName.ToString());
                        else humanDetailsJSON.hediffPartDefName.Add("null");

                        humanDetailsJSON.hediffSeverity.Add(hd.Severity.ToString());
                        humanDetailsJSON.heddifPermanent.Add(hd.IsPermanent());
                    }
                    catch (Exception e) { Log.Warning($"Failed to load heddif {hd} from human {pawn.Label}. Reason: {e}"); }
                }
            }
        }

        private static void GetPawnXenotype(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            try
            {
                if (pawn.genes.Xenotype != null) humanDetailsJSON.xenotypeDefName = pawn.genes.Xenotype.defName.ToString();
                else humanDetailsJSON.xenotypeDefName = "null";

                if (pawn.genes.CustomXenotype != null) humanDetailsJSON.customXenotypeName = pawn.genes.xenotypeName.ToString();
                else humanDetailsJSON.customXenotypeName = "null";
            }
            catch (Exception e) { Log.Warning($"Failed to load xenotype from human {pawn.Label}. Reason: {e}"); }
        }

        private static void GetPawnXenogenes(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            if (pawn.genes.Xenogenes.Count() > 0)
            {
                foreach (Gene gene in pawn.genes.Xenogenes)
                {
                    try { humanDetailsJSON.xenogeneDefNames.Add(gene.def.defName); }
                    catch (Exception e) { Log.Warning($"Failed to load gene {gene} from human {pawn.Label}. Reason: {e}"); }
                }
            }
        }

        private static void GetPawnEndogenes(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            if (pawn.genes.Endogenes.Count() > 0)
            {
                foreach (Gene endogene in pawn.genes.Endogenes)
                {
                    try { humanDetailsJSON.endogeneDefNames.Add(endogene.def.defName.ToString()); }
                    catch (Exception e) { Log.Warning($"Failed to load endogene {endogene} from human {pawn.Label}. Reason: {e}"); }
                }
            }
        }

        private static void GetPawnFavoriteColor(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            try { humanDetailsJSON.favoriteColor = pawn.story.favoriteColor.ToString(); }
            catch (Exception e) { Log.Warning($"Failed to load favorite color from human {pawn.Label}. Reason: {e}"); }
        }

        private static void GetPawnStory(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            try
            {
                if (pawn.story.Childhood != null) humanDetailsJSON.childhoodStory = pawn.story.Childhood.defName.ToString();
                else humanDetailsJSON.childhoodStory = "null";

                if (pawn.story.Adulthood != null) humanDetailsJSON.adulthoodStory = pawn.story.Adulthood.defName.ToString();
                else humanDetailsJSON.adulthoodStory = "null";
            }
            catch (Exception e) { Log.Warning($"Failed to load backstories from human {pawn.Label}. Reason: {e}"); }
        }

        private static void GetPawnSkills(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            if (pawn.skills.skills.Count() > 0)
            {
                foreach (SkillRecord skill in pawn.skills.skills)
                {
                    try
                    {
                        humanDetailsJSON.skillDefNames.Add(skill.def.defName);
                        humanDetailsJSON.skillLevels.Add(skill.levelInt.ToString());
                        humanDetailsJSON.passions.Add(skill.passion.ToString());
                    }
                    catch (Exception e) { Log.Warning($"Failed to load skill {skill} from human {pawn.Label}. Reason: {e}"); }
                }
            }
        }

        private static void GetPawnTraits(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            if (pawn.story.traits.allTraits.Count() > 0)
            {
                foreach (Trait trait in pawn.story.traits.allTraits)
                {
                    try
                    {
                        humanDetailsJSON.traitDefNames.Add(trait.def.defName);
                        humanDetailsJSON.traitDegrees.Add(trait.Degree.ToString());
                    }
                    catch (Exception e) { Log.Warning($"Failed to load trait {trait} from human {pawn.Label}. Reason: {e}"); }
                }
            }
        }

        private static void GetPawnApparel(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            if (pawn.apparel.WornApparel.Count() > 0)
            {
                foreach (Apparel ap in pawn.apparel.WornApparel)
                {
                    try
                    {
                        ItemDetailsJSON itemDetailsJSON = ThingScribeManager.ItemToString(ap, 1);
                        humanDetailsJSON.equippedApparel.Add(itemDetailsJSON);
                        humanDetailsJSON.apparelWornByCorpse.Add(ap.WornByCorpse);
                    }
                    catch (Exception e) { Log.Warning($"Failed to load apparel {ap} from human {pawn.Label}. Reason: {e}"); }
                }
            }
        }

        private static void GetPawnEquipment(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            if (pawn.equipment.Primary != null)
            {
                try
                {
                    ThingWithComps weapon = pawn.equipment.Primary;
                    ItemDetailsJSON weaponDetails = ThingScribeManager.ItemToString(weapon, weapon.stackCount);
                    humanDetailsJSON.equippedWeapon = weaponDetails;
                }
                catch (Exception e) { Log.Warning($"Failed to load weapon from human {pawn.Label}. Reason: {e}"); }
            }
        }

        private static void GetPawnInventory(Pawn pawn, HumanDetailsJSON humanDetailsJSON, bool passInventory)
        {
            if (pawn.inventory.innerContainer.Count() == 0 || !passInventory) { }
            else
            {
                foreach (Thing thing in pawn.inventory.innerContainer)
                {
                    ItemDetailsJSON itemDetailsJSON = ThingScribeManager.ItemToString(thing, thing.stackCount);
                    humanDetailsJSON.inventoryItems.Add(itemDetailsJSON);
                }
            }
        }

        private static void GetPawnPosition(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            try
            {
                humanDetailsJSON.position = new string[] { pawn.Position.x.ToString(),
                    pawn.Position.y.ToString(), pawn.Position.z.ToString() };
            }
            catch { Log.Message("Failed to set human position"); }
        }

        private static void GetPawnRotation(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            try { humanDetailsJSON.rotation = pawn.Rotation.AsInt.ToString(); }
            catch { Log.Message("Failed to get human rotation"); }
        }

        //Setters

        private static void SetPawnBioDetails(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            try
            {
                pawn.Name = new NameSingle(humanDetailsJSON.name);
                pawn.ageTracker.AgeBiologicalTicks = long.Parse(humanDetailsJSON.biologicalAge);
                pawn.ageTracker.AgeChronologicalTicks = long.Parse(humanDetailsJSON.chronologicalAge);

                Enum.TryParse(humanDetailsJSON.gender, true, out Gender humanGender);
                pawn.gender = humanGender;

                pawn.story.hairDef = DefDatabase<HairDef>.AllDefs.ToList().Find(x => x.defName == humanDetailsJSON.hairDefName);
                pawn.story.headType = DefDatabase<HeadTypeDef>.AllDefs.ToList().Find(x => x.defName == humanDetailsJSON.headTypeDefName);
                pawn.style.beardDef = DefDatabase<BeardDef>.AllDefs.ToList().Find(x => x.defName == humanDetailsJSON.beardDefName);
                pawn.story.bodyType = DefDatabase<BodyTypeDef>.AllDefs.ToList().Find(x => x.defName == humanDetailsJSON.bodyTypeDefName);
                pawn.style.FaceTattoo = DefDatabase<TattooDef>.AllDefs.ToList().Find(x => x.defName == humanDetailsJSON.FaceTattooDefName);
                pawn.style.BodyTattoo = DefDatabase<TattooDef>.AllDefs.ToList().Find(x => x.defName == humanDetailsJSON.BodyTattooDefName);

                string hairColor = humanDetailsJSON.hairColor.Replace("RGBA(", "").Replace(")", "");
                string[] isolatedHair = hairColor.Split(',');
                float r = float.Parse(isolatedHair[0]);
                float g = float.Parse(isolatedHair[1]);
                float b = float.Parse(isolatedHair[2]);
                float a = float.Parse(isolatedHair[3]);
                pawn.story.HairColor = new UnityEngine.Color(r, g, b, a);

                string skinColor = humanDetailsJSON.skinColor.Replace("RGBA(", "").Replace(")", "");
                string[] isolatedSkin = skinColor.Split(',');
                r = float.Parse(isolatedSkin[0]);
                g = float.Parse(isolatedSkin[1]);
                b = float.Parse(isolatedSkin[2]);
                a = float.Parse(isolatedSkin[3]);
                pawn.story.SkinColorBase = new UnityEngine.Color(r, g, b, a);
            }
            catch (Exception e) { Log.Warning($"Failed to load biological details in human {humanDetailsJSON.name}. Reason: {e}"); }
        }

        private static void SetPawnHediffs(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            try
            {
                pawn.health.RemoveAllHediffs();
                pawn.health.Reset();
            }
            catch (Exception e) { Log.Warning($"Failed to remove heddifs of human {humanDetailsJSON.name}. Reason: {e}"); }

            if (humanDetailsJSON.hediffDefNames.Count() > 0)
            {
                for (int i4 = 0; i4 < humanDetailsJSON.hediffDefNames.Count(); i4++)
                {
                    try
                    {
                        HediffDef hediffDef = DefDatabase<HediffDef>.AllDefs.ToList().Find(x => x.defName == humanDetailsJSON.hediffDefNames[i4]);

                        BodyPartRecord bodyPart = null;
                        if (humanDetailsJSON.hediffPartDefName[i4] == "null") bodyPart = null;
                        else bodyPart = pawn.RaceProps.body.AllParts.ToList().Find(x => x.def.defName == humanDetailsJSON.hediffPartDefName[i4]);

                        Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn, bodyPart);

                        hediff.Severity = float.Parse(humanDetailsJSON.hediffSeverity[i4]);

                        if (humanDetailsJSON.heddifPermanent[i4])
                        {
                            HediffComp_GetsPermanent hediffComp = hediff.TryGetComp<HediffComp_GetsPermanent>();
                            hediffComp.IsPermanent = true;
                        }

                        pawn.health.AddHediff(hediff);
                    }
                    catch (Exception e) { Log.Warning($"Failed to load heddif in {humanDetailsJSON.hediffPartDefName[i4]} to human {humanDetailsJSON.name}. Reason: {e}"); }
                }
            }
        }

        private static void SetPawnXenotype(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            try
            {
                if (humanDetailsJSON.xenotypeDefName != "null")
                {
                    pawn.genes.SetXenotype(DefDatabase<XenotypeDef>.AllDefs.ToList().Find(x => x.defName == humanDetailsJSON.xenotypeDefName));
                }

                if (humanDetailsJSON.customXenotypeName != "null")
                {
                    pawn.genes.xenotypeName = humanDetailsJSON.customXenotypeName;
                }
            }
            catch (Exception e) { Log.Warning($"Failed to load xenotypes in human {humanDetailsJSON.name}. Reason: {e}"); }
        }

        private static void SetPawnXenogenes(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            try
            {
                if (humanDetailsJSON.xenogeneDefNames.Count() > 0)
                {
                    foreach(string str in humanDetailsJSON.xenogeneDefNames)
                    {
                        GeneDef def = DefDatabase<GeneDef>.AllDefs.First(fetch => fetch.defName == str);
                        pawn.genes.AddGene(def, true);
                    }
                }
            }
            catch { Log.Warning($"Failed to load xenogenes for human {humanDetailsJSON.name}"); }
        }

        private static void SetPawnEndogenes(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            try
            {
                if (humanDetailsJSON.endogeneDefNames.Count() > 0)
                {
                    foreach (string str in humanDetailsJSON.endogeneDefNames)
                    {
                        GeneDef def = DefDatabase<GeneDef>.AllDefs.First(fetch => fetch.defName == str);
                        pawn.genes.AddGene(def, false);
                    }
                }
            }
            catch { Log.Warning($"Failed to load endogenes for human {humanDetailsJSON.name}"); }
        }

        private static void SetPawnFavoriteColor(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            try
            {
                float r;
                float g;
                float b;
                float a;

                string favoriteColor = humanDetailsJSON.favoriteColor.Replace("RGBA(", "").Replace(")", "");
                string[] isolatedFavoriteColor = favoriteColor.Split(',');
                r = float.Parse(isolatedFavoriteColor[0]);
                g = float.Parse(isolatedFavoriteColor[1]);
                b = float.Parse(isolatedFavoriteColor[2]);
                a = float.Parse(isolatedFavoriteColor[3]);
                pawn.story.favoriteColor = new UnityEngine.Color(r, g, b, a);
            }
            catch (Exception e) { Log.Warning($"Failed to load colors in human {humanDetailsJSON.name}. Reason: {e}"); }
        }

        private static void SetPawnStory(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            try
            {
                if (humanDetailsJSON.childhoodStory != "null")
                {
                    pawn.story.Childhood = DefDatabase<BackstoryDef>.AllDefs.ToList().Find(x => x.defName == humanDetailsJSON.childhoodStory);
                }

                if (humanDetailsJSON.adulthoodStory != "null")
                {
                    pawn.story.Adulthood = DefDatabase<BackstoryDef>.AllDefs.ToList().Find(x => x.defName == humanDetailsJSON.adulthoodStory);
                }
            }
            catch (Exception e) { Log.Warning($"Failed to load stories in human {humanDetailsJSON.name}. Reason: {e}"); }
        }

        private static void SetPawnSkills(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            if (humanDetailsJSON.skillDefNames.Count() > 0)
            {
                for (int i2 = 0; i2 < humanDetailsJSON.skillDefNames.Count(); i2++)
                {
                    try
                    {
                        pawn.skills.skills[i2].levelInt = int.Parse(humanDetailsJSON.skillLevels[i2]);

                        Enum.TryParse(humanDetailsJSON.passions[i2], true, out Passion passion);
                        pawn.skills.skills[i2].passion = passion;
                    }
                    catch (Exception e) { Log.Warning($"Failed to load skill {humanDetailsJSON.skillDefNames[i2]} to human {humanDetailsJSON.name}. Reason: {e}"); }
                }
            }
        }

        private static void SetPawnTraits(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            try { pawn.story.traits.allTraits.Clear(); }
            catch (Exception e) { Log.Warning($"Failed to remove traits of human {humanDetailsJSON.name}. Reason: {e}"); }

            if (humanDetailsJSON.traitDefNames.Count() > 0)
            {
                for (int i3 = 0; i3 < humanDetailsJSON.traitDefNames.Count(); i3++)
                {
                    try
                    {
                        TraitDef traitDef = DefDatabase<TraitDef>.AllDefs.ToList().Find(x => x.defName == humanDetailsJSON.traitDefNames[i3]);
                        Trait trait = new Trait(traitDef, int.Parse(humanDetailsJSON.traitDegrees[i3]));
                        pawn.story.traits.GainTrait(trait);
                    }
                    catch (Exception e) { Log.Warning($"Failed to load trait {humanDetailsJSON.traitDefNames[i3]} to human {humanDetailsJSON.name}. Reason: {e}"); }
                }
            }
        }

        private static void SetPawnApparel(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            try
            {
                pawn.apparel.DestroyAll();
                pawn.apparel.DropAllOrMoveAllToInventory();
            }
            catch (Exception e) { Log.Warning($"Failed to destroy apparel in human {humanDetailsJSON.name}. Reason: {e}"); }

            if (humanDetailsJSON.equippedApparel.Count() > 0)
            {
                for (int i5 = 0; i5 < humanDetailsJSON.equippedApparel.Count(); i5++)
                {
                    try
                    {
                        Apparel apparel = (Apparel)ThingScribeManager.StringToItem(humanDetailsJSON.equippedApparel[i5]);
                        if (humanDetailsJSON.apparelWornByCorpse[i5]) apparel.WornByCorpse.MustBeTrue();
                        else apparel.WornByCorpse.MustBeFalse();

                        pawn.apparel.Wear(apparel);
                    }
                    catch { Log.Warning($"Failed to load apparel in human {humanDetailsJSON.name}"); }
                }
            }
        }

        private static void SetPawnEquipment(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            try
            {
                pawn.equipment.DestroyAllEquipment();
            }
            catch (Exception e) { Log.Warning($"Failed to destroy equipment in human {humanDetailsJSON.name}. Reason: {e}"); }

            if (humanDetailsJSON.equippedWeapon != null)
            {
                try
                {
                    ThingWithComps thing = (ThingWithComps)ThingScribeManager.StringToItem(humanDetailsJSON.equippedWeapon);
                    pawn.equipment.AddEquipment(thing);
                }
                catch { Log.Warning($"Failed to load weapon in human {humanDetailsJSON.name}"); }
            }
        }

        private static void SetPawnInventory(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            if (humanDetailsJSON.inventoryItems.Count() > 0)
            {
                foreach (ItemDetailsJSON item in humanDetailsJSON.inventoryItems)
                {
                    try
                    {
                        Thing thing = ThingScribeManager.StringToItem(item);
                        pawn.inventory.TryAddAndUnforbid(thing);
                    }
                    catch { Log.Warning($"Failed to add thing to pawn {pawn.Label}"); }
                }
            }
        }

        private static void SetPawnPosition(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            if (humanDetailsJSON.position != null)
            {
                try
                {
                    pawn.Position = new IntVec3(int.Parse(humanDetailsJSON.position[0]), int.Parse(humanDetailsJSON.position[1]),
                        int.Parse(humanDetailsJSON.position[2]));
                }
                catch { Log.Message($"Failed to set position in human {pawn.Label}"); }
            }
        }

        private static void SetPawnRotation(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            if (humanDetailsJSON.rotation != "null")
            {
                try { pawn.Rotation = new Rot4(int.Parse(humanDetailsJSON.rotation)); }
                catch { Log.Message($"Failed to set rotation in human {pawn.Label}"); }
            }
        }
    }

    //Class that handles transformation of animals

    public static class AnimalScribeManager
    {
        public static AnimalDetailsJSON TransformAnimalToString(Pawn animal)
        {
            AnimalDetailsJSON animalDetailsJSON = new AnimalDetailsJSON();

            try
            {
                animalDetailsJSON.defName = animal.def.defName;
                animalDetailsJSON.name = animal.LabelShortCap.ToString();
                animalDetailsJSON.biologicalAge = animal.ageTracker.AgeBiologicalTicks.ToString();
                animalDetailsJSON.chronologicalAge = animal.ageTracker.AgeChronologicalTicks.ToString();
                animalDetailsJSON.gender = animal.gender.ToString();
            }
            catch { }

            if (animal.health.hediffSet.hediffs.Count() > 0)
            {
                foreach (Hediff hd in animal.health.hediffSet.hediffs)
                {
                    try
                    {
                        animalDetailsJSON.hediffDefNames.Add(hd.def.defName);

                        if (hd.Part != null) animalDetailsJSON.hediffPart.Add(hd.Part.def.defName.ToString());
                        else animalDetailsJSON.hediffPart.Add("null");

                        animalDetailsJSON.hediffSeverity.Add(hd.Severity.ToString());
                        animalDetailsJSON.heddifPermanent.Add(hd.IsPermanent());
                    }
                    catch (Exception e) { Log.Warning($"Failed to get headdifs from animal {animal.Name}. Exception: {e}"); }
                }
            }

            foreach (TrainableDef trainable in DefDatabase<TrainableDef>.AllDefsListForReading)
            {
                try
                {
                    animalDetailsJSON.trainableDefNames.Add(trainable.defName);
                    animalDetailsJSON.canTrain.Add(animal.training.CanAssignToTrain(trainable).Accepted);
                    animalDetailsJSON.hasLearned.Add(animal.training.HasLearned(trainable));
                    animalDetailsJSON.isDisabled.Add(animal.training.GetWanted(trainable));
                }
                catch { }
            }

            try
            {
                animalDetailsJSON.position = new string[] { animal.Position.x.ToString(),
                        animal.Position.y.ToString(), animal.Position.z.ToString() };
            }
            catch { Log.Message("Failed to get animal position"); }

            try { animalDetailsJSON.rotation = animal.Rotation.AsInt.ToString(); }
            catch { Log.Message("Failed to get animal rotation"); }

            return animalDetailsJSON;
        }

        public static Pawn[] GetAnimalsFromString(TransferManifestJSON transferManifestJSON)
        {
            List<Pawn> animals = new List<Pawn>();

            for (int i = 0; i < transferManifestJSON.animalDetailsJSON.Count(); i++)
            {
                AnimalDetailsJSON animalDetails = Serializer.SerializeFromString<AnimalDetailsJSON>(transferManifestJSON.animalDetailsJSON[i]);

                animals.Add(GetAnimalSimple(animalDetails));
            }

            return animals.ToArray();
        }

        public static Pawn GetAnimalSimple(AnimalDetailsJSON animalDetails)
        {
            Pawn animal = PawnGenerator.GeneratePawn(PawnKindDef.Named(animalDetails.defName), Faction.OfPlayer);

            try
            {
                try
                {
                    animal.Name = new NameSingle(animalDetails.name);
                    animal.ageTracker.AgeBiologicalTicks = long.Parse(animalDetails.biologicalAge);
                    animal.ageTracker.AgeChronologicalTicks = long.Parse(animalDetails.chronologicalAge);

                    Enum.TryParse(animalDetails.gender, true, out Gender animalGender);
                    animal.gender = animalGender;
                }
                catch { }

                try
                {
                    animal.health.RemoveAllHediffs();
                    animal.health.Reset();
                }
                catch { Log.Warning($"Failed to remove heddifs of animal {animalDetails.name}."); }

                if (animalDetails.hediffDefNames.Count() > 0)
                {
                    for (int i2 = 0; i2 < animalDetails.hediffDefNames.Count(); i2++)
                    {
                        try
                        {
                            HediffDef hediffDef = DefDatabase<HediffDef>.AllDefs.ToList().Find(x => x.defName == animalDetails.hediffDefNames[i2]);

                            BodyPartRecord bodyPart = null;
                            if (animalDetails.hediffPart[i2] == "null") bodyPart = null;
                            else bodyPart = animal.RaceProps.body.AllParts.ToList().Find(x => x.def.defName == animalDetails.hediffPart[i2]);

                            Hediff hediff = HediffMaker.MakeHediff(hediffDef, animal, bodyPart);

                            hediff.Severity = float.Parse(animalDetails.hediffSeverity[i2]);

                            if (animalDetails.heddifPermanent[i2])
                            {
                                HediffComp_GetsPermanent hediffComp = hediff.TryGetComp<HediffComp_GetsPermanent>();
                                hediffComp.IsPermanent = true;
                            }

                            animal.health.AddHediff(hediff);
                        }
                        catch (Exception e) { Log.Warning($"Failed to load heddif in {animalDetails.hediffPart[i2]} in animal {animalDetails.defName}. Reason: {e}"); }
                    }
                }

                if (animalDetails.trainableDefNames.Count() > 0)
                {
                    for (int i3 = 0; i3 < animalDetails.trainableDefNames.Count(); i3++)
                    {
                        try
                        {
                            TrainableDef trainable = DefDatabase<TrainableDef>.AllDefs.ToList().Find(x => x.defName == animalDetails.trainableDefNames[i3]);
                            animal.training.Train(trainable, null, complete: animalDetails.hasLearned[i3]);
                            if (animalDetails.canTrain[i3]) animal.training.Train(trainable, null, complete: animalDetails.hasLearned[i3]);
                            if (animalDetails.isDisabled[i3]) animal.training.SetWantedRecursive(trainable, true);
                        }
                        catch { }
                    }
                }

                if (animal.Position != null)
                {
                    try
                    {
                        animal.Position = new IntVec3(int.Parse(animalDetails.position[0]), int.Parse(animalDetails.position[1]),
                            int.Parse(animalDetails.position[2]));
                    }
                    catch { Log.Warning($"Failed to set position in animal {animal.Label}"); }
                }

                if (animalDetails.rotation != "null")
                {
                    try
                    {
                        animal.Rotation = new Rot4(int.Parse(animalDetails.rotation));
                    }
                    catch { Log.Message($"Failed to set rotation in animal {animal.Label}"); }
                }
            }
            catch (Exception e) { Log.Error($"Failed to load animal {animalDetails.defName}. Reason: {e}"); }

            return animal;
        }
    }

    //Class that handles transformation of things

    public static class ThingScribeManager
    {
        //Functions

        public static Thing[] GetItemsFromString(TransferManifestJSON transferManifestJSON)
        {
            List<Thing> things = new List<Thing>();

            for (int i = 0; i < transferManifestJSON.itemDetailsJSONS.Count(); i++)
            {
                ItemDetailsJSON itemDetailsJSON = Serializer.SerializeFromString<ItemDetailsJSON>(transferManifestJSON.itemDetailsJSONS[i]);

                things.Add(StringToItem(itemDetailsJSON));
            }

            return things.ToArray();
        }

        public static ItemDetailsJSON ItemToString(Thing thing, int thingCount)
        {
            ItemDetailsJSON itemDetailsJSON = new ItemDetailsJSON();

            try
            {
                itemDetailsJSON.defName = thing.def.defName;

                if (TransferManagerHelper.CheckIfThingHasMaterial(thing)) itemDetailsJSON.materialDefName = thing.Stuff.defName;
                else itemDetailsJSON.materialDefName = null;

                itemDetailsJSON.quantity = thingCount.ToString();

                itemDetailsJSON.quality = TransferManagerHelper.GetThingQuality(thing);

                itemDetailsJSON.hitpoints = thing.HitPoints.ToString();

                try
                {
                    itemDetailsJSON.position = new string[] { thing.Position.x.ToString(),
                        thing.Position.y.ToString(), thing.Position.z.ToString() };
                }
                catch { Log.Warning($"Failed to get position of thing {itemDetailsJSON.defName}"); }

                try { itemDetailsJSON.rotation = thing.Rotation.AsInt.ToString(); }
                catch { Log.Warning($"Failed to get rotation of thing {itemDetailsJSON.defName}"); }

                if (TransferManagerHelper.CheckIfThingIsMinified(thing)) itemDetailsJSON.isMinified = true;
                else itemDetailsJSON.isMinified = false;
            }
            catch (Exception e) { Log.Warning($"Failed to get item details from item {thing.Label}. Exception: {e}"); }

            return itemDetailsJSON;
        }

        public static Thing StringToItem(ItemDetailsJSON itemDetails)
        {
            Thing toGet = null;
            ThingDef thingDef = null;
            ThingDef defMaterial = null;

            try
            {
                thingDef = DefDatabase<ThingDef>.AllDefs.ToList().Find(x => x.defName == itemDetails.defName);
                defMaterial = DefDatabase<ThingDef>.AllDefs.ToList().Find(x => x.defName == itemDetails.materialDefName);
                toGet = ThingMaker.MakeThing(thingDef, defMaterial);

                try { toGet.stackCount = int.Parse(itemDetails.quantity); }
                catch { Log.Warning($"Failed to load item quantity for {itemDetails.defName}"); }

                if (toGet.stackCount == 0)
                {
                    Log.Warning($"Item {itemDetails.defName} had a stack of 0, returning");
                    return null;
                }

                if (itemDetails.quality != "null")
                {
                    try
                    {
                        CompQuality compQuality = toGet.TryGetComp<CompQuality>();
                        if (compQuality != null)
                        {
                            QualityCategory iCategory = (QualityCategory)int.Parse(itemDetails.quality);
                            compQuality.SetQuality(iCategory, ArtGenerationContext.Outsider);
                        }
                    }
                    catch { Log.Warning($"Failed to load item quality for {itemDetails.defName}"); }
                }

                if (itemDetails.hitpoints != "null")
                {
                    try { toGet.HitPoints = int.Parse(itemDetails.hitpoints); }
                    catch { Log.Warning($"Failed to load item hitpoints for {itemDetails.defName}"); }
                }

                if (itemDetails.position != null)
                {
                    try
                    {
                        toGet.Position = new IntVec3(int.Parse(itemDetails.position[0]), int.Parse(itemDetails.position[1]),
                            int.Parse(itemDetails.position[2]));
                    }
                    catch { Log.Warning($"Failed to set position for item {itemDetails.defName}"); }
                }

                if (itemDetails.rotation != "null")
                {
                    try { toGet.Rotation = new Rot4(int.Parse(itemDetails.rotation)); }
                    catch { Log.Warning($"Failed to set rotation for item {itemDetails.defName}"); }
                }

                if (itemDetails.isMinified) toGet.TryMakeMinified();
            }
            catch (Exception e) { Log.Error($"Failed to set item {itemDetails.defName}. Reason: {e}"); }

            return toGet;
        }
    }

    //Class that handles transformation of maps

    public static class MapScribeManager
    {
        //Functions

        public static MapDetailsJSON MapToString(Map map, bool containsItems, bool containsHumans, bool containsAnimals)
        {
            MapDetailsJSON mapDetailsJSON = new MapDetailsJSON();

            GetMapTile(mapDetailsJSON, map);

            GetMapSize(mapDetailsJSON, map);

            GetMapThings(mapDetailsJSON, map, containsItems, containsHumans, containsAnimals);

            return mapDetailsJSON;
        }

        public static Map StringToMap(MapDetailsJSON mapDetailsJSON, bool containsItems, bool containsHumans, bool containsAnimals, bool lessLoot)
        {
            Map map = SetEmptyMap(mapDetailsJSON);

            SetMapThings(mapDetailsJSON, map, containsItems, lessLoot);

            if (containsHumans) SetMapHumans(mapDetailsJSON, map);

            if (containsAnimals) SetMapAnimals(mapDetailsJSON, map);

            SetMapTerrain(mapDetailsJSON, map);

            SetMapFog(map);

            SetMapRoof(map);

            return map;
        }

        //Getters

        private static void GetMapTile(MapDetailsJSON mapDetailsJSON, Map map)
        {
            mapDetailsJSON.mapTile = map.Tile.ToString();
        }

        private static void GetMapSize(MapDetailsJSON mapDetailsJSON, Map map)
        {
            mapDetailsJSON.mapSize = $"{map.Size.x}|{map.Size.y}|{map.Size.z}";
        }

        private static void GetMapThings(MapDetailsJSON mapDetailsJSON, Map map, bool containsItems, bool containsHumans, bool containsAnimals)
        {
            for (int z = 0; z < map.Size.z; ++z)
            {
                for (int x = 0; x < map.Size.x; ++x)
                {
                    IntVec3 vectorToCheck = new IntVec3(x, map.Size.y, z);

                    mapDetailsJSON.tileDefNames.Add(map.terrainGrid.TerrainAt(vectorToCheck).defName.ToString());

                    foreach (Thing thing in map.thingGrid.ThingsListAt(vectorToCheck).ToList())
                    {
                        if (TransferManagerHelper.CheckIfThingIsHuman(thing))
                        {
                            if (containsHumans)
                            {
                                HumanDetailsJSON humanDetailsJSON = HumanScribeManager.HumanToString(thing as Pawn);
                                if (thing.Faction == Faction.OfPlayer) mapDetailsJSON.factionHumans.Add(humanDetailsJSON);
                                else mapDetailsJSON.nonFactionHumans.Add(humanDetailsJSON);
                            }
                        }

                        else if (TransferManagerHelper.CheckIfThingIsAnimal(thing))
                        {
                            if (containsAnimals)
                            {
                                AnimalDetailsJSON animalDetailsJSON = AnimalScribeManager.TransformAnimalToString(thing as Pawn);
                                if (thing.Faction == Faction.OfPlayer) mapDetailsJSON.factionAnimals.Add(animalDetailsJSON);
                                else mapDetailsJSON.nonFactionAnimals.Add(animalDetailsJSON);
                            }
                        }

                        else
                        {
                            ItemDetailsJSON itemDetailsJSON = ThingScribeManager.ItemToString(thing, thing.stackCount);

                            if (thing.def.alwaysHaulable)
                            {
                                if (containsItems) mapDetailsJSON.factionThings.Add(itemDetailsJSON);
                                else continue;
                            }
                            else mapDetailsJSON.nonFactionThings.Add(itemDetailsJSON);
                        }
                    }

                    if (map.roofGrid.RoofAt(vectorToCheck) == null) mapDetailsJSON.roofDefNames.Add("null");
                    else mapDetailsJSON.roofDefNames.Add(map.roofGrid.RoofAt(vectorToCheck).defName.ToString());
                }
            }
        }

        //Setters

        private static Map SetEmptyMap(MapDetailsJSON mapDetailsJSON)
        {
            string[] splitSize = mapDetailsJSON.mapSize.Split('|');

            IntVec3 mapSize = new IntVec3(int.Parse(splitSize[0]), int.Parse(splitSize[1]),
                int.Parse(splitSize[2]));

            PlanetManagerHelper.SetOverrideGenerators();
            Map toReturn = GetOrGenerateMapUtility.GetOrGenerateMap(ClientValues.chosenSettlement.Tile, mapSize, null);
            PlanetManagerHelper.SetDefaultGenerators();
            return toReturn;
        }

        private static void SetMapThings(MapDetailsJSON mapDetailsJSON, Map map, bool containsItems, bool lessLoot)
        {
            List<Thing> thingsToGetInThisTile = new List<Thing>();

            foreach (ItemDetailsJSON item in mapDetailsJSON.nonFactionThings)
            {
                try
                {
                    Thing toGet = ThingScribeManager.StringToItem(item);
                    thingsToGetInThisTile.Add(toGet);
                }
                catch { }
            }

            if (containsItems)
            {
                Random rnd = new Random();

                foreach (ItemDetailsJSON item in mapDetailsJSON.factionThings)
                {
                    try
                    {
                        Thing toGet = ThingScribeManager.StringToItem(item);

                        if (lessLoot)
                        {
                            if (rnd.Next(1, 100) > 70) thingsToGetInThisTile.Add(toGet);
                            else continue;
                        }
                        else thingsToGetInThisTile.Add(toGet);
                    }
                    catch { }
                }
            }

            foreach (Thing thing in thingsToGetInThisTile)
            {
                try { GenPlace.TryPlaceThing(thing, thing.Position, map, ThingPlaceMode.Direct, rot: thing.Rotation); }
                catch { Log.Warning($"Failed to place thing {thing.def.defName} at {thing.Position}"); }
            }
        }

        private static void SetMapHumans(MapDetailsJSON mapDetailsJSON, Map map)
        {
            foreach (HumanDetailsJSON pawn in mapDetailsJSON.nonFactionHumans)
            {
                try
                {
                    Pawn human = HumanScribeManager.StringToHuman(pawn);
                    human.SetFaction(FactionValues.yourOnlineFaction);

                    GenSpawn.Spawn(human, human.Position, map, human.Rotation);
                }
                catch { Log.Warning($"Failed to spawn human {pawn.name}"); }
            }

            foreach (HumanDetailsJSON pawn in mapDetailsJSON.factionHumans)
            {
                try
                {
                    Pawn human = HumanScribeManager.StringToHuman(pawn);
                    human.SetFaction(FactionValues.neutralPlayer);

                    GenSpawn.Spawn(human, human.Position, map, human.Rotation);
                }
                catch { Log.Warning($"Failed to spawn human {pawn.name}"); }
            }
        }

        private static void SetMapAnimals(MapDetailsJSON mapDetailsJSON, Map map)
        {
            foreach (AnimalDetailsJSON pawn in mapDetailsJSON.nonFactionAnimals)
            {
                try
                {
                    Pawn animal = AnimalScribeManager.GetAnimalSimple(pawn);
                    animal.SetFaction(FactionValues.yourOnlineFaction);

                    GenSpawn.Spawn(animal, animal.Position, map, animal.Rotation);
                }
                catch { Log.Warning($"Failed to spawn animal {pawn.name}"); }
            }

            foreach (AnimalDetailsJSON pawn in mapDetailsJSON.factionAnimals)
            {
                try
                {
                    Pawn animal = AnimalScribeManager.GetAnimalSimple(pawn);
                    animal.SetFaction(FactionValues.neutralPlayer);

                    GenSpawn.Spawn(animal, animal.Position, map, animal.Rotation);
                }
                catch { Log.Warning($"Failed to spawn animal {pawn.name}"); }
            }
        }

        private static void SetMapTerrain(MapDetailsJSON mapDetailsJSON, Map map)
        {
            int index = 0;

            for (int z = 0; z < map.Size.z; ++z)
            {
                for (int x = 0; x < map.Size.x; ++x)
                {
                    IntVec3 vectorToCheck = new IntVec3(x, map.Size.y, z);

                    try
                    {
                        TerrainDef terrainToUse = DefDatabase<TerrainDef>.AllDefs.ToList().Find(fetch => fetch.defName ==
                            mapDetailsJSON.tileDefNames[index]);

                        map.terrainGrid.SetTerrain(vectorToCheck, terrainToUse);

                    }
                    catch { Log.Warning($"Failed to set terrain at {vectorToCheck}"); }

                    try
                    {
                        RoofDef roofToUse = DefDatabase<RoofDef>.AllDefs.ToList().Find(fetch => fetch.defName ==
                                    mapDetailsJSON.roofDefNames[index]);

                        map.roofGrid.SetRoof(vectorToCheck, roofToUse);
                    }
                    catch { Log.Warning($"Failed to set roof at {vectorToCheck}"); }

                    index++;
                }
            }
        }

        private static void SetMapFog(Map map)
        {
            FloodFillerFog.FloodUnfog(MapGenerator.PlayerStartSpot, map);
        }

        private static void SetMapRoof(Map map)
        {
            map.roofCollapseBuffer.Clear();
            map.roofGrid.Drawer.SetDirty();
        }
    }
}
