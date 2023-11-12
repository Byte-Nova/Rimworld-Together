using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.GameClient.Values;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.JSON.Actions;
using RimworldTogether.Shared.JSON.Things;
using RimworldTogether.Shared.Serializers;
using UnityEngine.Assertions.Must;
using Verse;

namespace RimworldTogether.GameClient.Managers
{
    public static class DeepScribeManager
    {
        public static Thing[] GetAllTransferedItems(TransferManifestJSON transferManifestJSON)
        {
            List<Thing> allTransferedItems = new List<Thing>();

            foreach (Pawn pawn in GetHumansFromString(transferManifestJSON)) allTransferedItems.Add(pawn);

            foreach (Pawn animal in GetAnimalsFromString(transferManifestJSON)) allTransferedItems.Add(animal);

            foreach (Thing thing in GetItemsFromString(transferManifestJSON)) allTransferedItems.Add(thing);

            return allTransferedItems.ToArray();
        }

        public static HumanDetailsJSON TransformHumanToString(Pawn human, bool passInventory = true)
        {
            HumanDetailsJSON humanDetailsJSON = new HumanDetailsJSON();

            try
            {
                humanDetailsJSON.defName = human.def.defName;
                humanDetailsJSON.name = human.LabelShortCap.ToString();
                humanDetailsJSON.biologicalAge = human.ageTracker.AgeBiologicalTicks.ToString();
                humanDetailsJSON.chronologicalAge = human.ageTracker.AgeChronologicalTicks.ToString();
                humanDetailsJSON.gender = human.gender.ToString();

                humanDetailsJSON.hairDefName = human.story.hairDef.defName.ToString();
                humanDetailsJSON.hairColor = human.story.HairColor.ToString();
                humanDetailsJSON.headTypeDefName = human.story.headType.defName.ToString();
                humanDetailsJSON.skinColor = human.story.SkinColor.ToString();
                humanDetailsJSON.beardDefName = human.style.beardDef.defName.ToString();
                humanDetailsJSON.bodyTypeDefName = human.story.bodyType.defName.ToString();
                humanDetailsJSON.FaceTattooDefName = human.style.FaceTattoo.defName.ToString();
                humanDetailsJSON.BodyTattooDefName = human.style.BodyTattoo.defName.ToString();
            }
            catch (Exception e) { Log.Warning($"Failed to load biological details from human {human.Label}. Reason: {e}"); }

            if (human.health.hediffSet.hediffs.Count() > 0)
            {
                foreach (Hediff hd in human.health.hediffSet.hediffs)
                {
                    try
                    {
                        humanDetailsJSON.hediffDefNames.Add(hd.def.defName);

                        if (hd.Part != null) humanDetailsJSON.hediffPartDefName.Add(hd.Part.def.defName.ToString());
                        else humanDetailsJSON.hediffPartDefName.Add("null");

                        humanDetailsJSON.hediffSeverity.Add(hd.Severity.ToString());
                        humanDetailsJSON.heddifPermanent.Add(hd.IsPermanent());
                    }
                    catch (Exception e) { Log.Warning($"Failed to load heddif {hd} from human {human.Label}. Reason: {e}"); }
                }
            }

            try
            {
                if (human.genes.Xenotype != null) humanDetailsJSON.xenotypeDefName = human.genes.Xenotype.defName.ToString();
                else humanDetailsJSON.xenotypeDefName = "null";

                if (human.genes.CustomXenotype != null) humanDetailsJSON.customXenotypeName = human.genes.xenotypeName.ToString();
                else humanDetailsJSON.customXenotypeName = "null";
            }
            catch (Exception e) { Log.Warning($"Failed to load xenotype from human {human.Label}. Reason: {e}"); }

            if (human.genes.Xenogenes.Count() > 0)
            {
                foreach (Gene gene in human.genes.Xenogenes)
                {
                    try
                    {
                        humanDetailsJSON.geneDefNames.Add(gene.def.defName);

                        foreach (AbilityDef ability in gene.def.abilities)
                        {
                            humanDetailsJSON.geneAbilityDefNames.Add(ability.defName);
                        }
                    }
                    catch (Exception e) { Log.Warning($"Failed to load gene {gene} from human {human.Label}. Reason: {e}"); }
                }
            }

            if (human.genes.Endogenes.Count() > 0)
            {
                foreach (Gene endogene in human.genes.Endogenes)
                {
                    try
                    {
                        humanDetailsJSON.endogeneDefNames.Add(endogene.def.defName.ToString());
                    }
                    catch (Exception e) { Log.Warning($"Failed to load endogene {endogene} from human {human.Label}. Reason: {e}"); }
                }
            }

            try { humanDetailsJSON.favoriteColor = human.story.favoriteColor.ToString(); }
            catch (Exception e) { Log.Warning($"Failed to load favorite color from human {human.Label}. Reason: {e}"); }

            try
            {
                if (human.story.Childhood != null) humanDetailsJSON.childhoodStory = human.story.Childhood.defName.ToString();
                else humanDetailsJSON.childhoodStory = "null";

                if (human.story.Adulthood != null) humanDetailsJSON.adulthoodStory = human.story.Adulthood.defName.ToString();
                else humanDetailsJSON.adulthoodStory = "null";
            }
            catch (Exception e) { Log.Warning($"Failed to load backstories from human {human.Label}. Reason: {e}"); }

            if (human.skills.skills.Count() > 0)
            {
                foreach (SkillRecord skill in human.skills.skills)
                {
                    try
                    {
                        humanDetailsJSON.skillDefNames.Add(skill.def.defName);
                        humanDetailsJSON.skillLevels.Add(skill.levelInt.ToString());
                        humanDetailsJSON.passions.Add(skill.passion.ToString());
                    }
                    catch (Exception e) { Log.Warning($"Failed to load skill {skill} from human {human.Label}. Reason: {e}"); }
                }
            }

            if (human.story.traits.allTraits.Count() > 0)
            {
                foreach (Trait trait in human.story.traits.allTraits)
                {
                    try
                    {
                        humanDetailsJSON.traitDefNames.Add(trait.def.defName);
                        humanDetailsJSON.traitDegrees.Add(trait.Degree.ToString());
                    }
                    catch (Exception e) { Log.Warning($"Failed to load trait {trait} from human {human.Label}. Reason: {e}"); }
                }
            }

            if (human.apparel.WornApparel.Count() > 0)
            {
                foreach (Apparel ap in human.apparel.WornApparel)
                {
                    try
                    {
                        string thingString = Serializer.SerializeToString(TransformItemToString(ap, 1));
                        humanDetailsJSON.deflatedApparels.Add(thingString);
                        humanDetailsJSON.apparelWornByCorpse.Add(ap.WornByCorpse);
                    }
                    catch (Exception e) { Log.Warning($"Failed to load apparel {ap} from human {human.Label}. Reason: {e}"); }
                }
            }

            if (human.equipment.Primary == null) humanDetailsJSON.deflatedWeapon = "null";
            else
            {
                try
                {
                    ThingWithComps weapon = human.equipment.Primary;
                    string thingString = Serializer.SerializeToString(TransformItemToString(weapon, weapon.stackCount));
                    humanDetailsJSON.deflatedWeapon = thingString;
                }
                catch (Exception e) { Log.Warning($"Failed to load weapon from human {human.Label}. Reason: {e}"); }
            }

            if (human.inventory.innerContainer.Count() == 0 || !passInventory) { }
            else
            {
                foreach (Thing thing in human.inventory.innerContainer)
                {
                    string thingString = Serializer.SerializeToString(TransformItemToString(thing, thing.stackCount));
                    humanDetailsJSON.deflatedInventoryItems.Add(thingString);
                }
            }

            try { humanDetailsJSON.position = $"{human.Position.x}|{human.Position.y}|{human.Position.z}"; }
            catch
            {
                humanDetailsJSON.position = "null";
                Log.Message("Failed to set human position");
            }

            return humanDetailsJSON;
        }

        public static Pawn[] GetHumansFromString(TransferManifestJSON transferManifestJSON)
        {
            List<Pawn> humans = new List<Pawn>();

            for (int i = 0; i < transferManifestJSON.humanDetailsJSONS.Count(); i++)
            {
                HumanDetailsJSON humanDetails = Serializer.SerializeFromString<HumanDetailsJSON>(transferManifestJSON.humanDetailsJSONS[i]);

                humans.Add(GetHumanSimple(humanDetails));
            }

            return humans.ToArray();
        }

        public static Pawn GetHumanSimple(HumanDetailsJSON humanDetails)
        {
            Pawn human = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);

            try
            {
                try
                {
                    human.Name = new NameSingle(humanDetails.name);
                    human.ageTracker.AgeBiologicalTicks = long.Parse(humanDetails.biologicalAge);
                    human.ageTracker.AgeChronologicalTicks = long.Parse(humanDetails.chronologicalAge);

                    Enum.TryParse(humanDetails.gender, true, out Gender humanGender);
                    human.gender = humanGender;

                    human.story.hairDef = DefDatabase<HairDef>.AllDefs.ToList().Find(x => x.defName == humanDetails.hairDefName);

                    human.story.headType = DefDatabase<HeadTypeDef>.AllDefs.ToList().Find(x => x.defName == humanDetails.headTypeDefName);

                    human.style.beardDef = DefDatabase<BeardDef>.AllDefs.ToList().Find(x => x.defName == humanDetails.beardDefName);

                    human.story.bodyType = DefDatabase<BodyTypeDef>.AllDefs.ToList().Find(x => x.defName == humanDetails.bodyTypeDefName);

                    human.style.FaceTattoo = DefDatabase<TattooDef>.AllDefs.ToList().Find(x => x.defName == humanDetails.FaceTattooDefName);

                    human.style.BodyTattoo = DefDatabase<TattooDef>.AllDefs.ToList().Find(x => x.defName == humanDetails.BodyTattooDefName);
                }
                catch (Exception e) { Log.Warning($"Failed to load biological details in human {humanDetails.name}. Reason: {e}"); }

                try
                {
                    string hairColor = humanDetails.hairColor.Replace("RGBA(", "").Replace(")", "");
                    string[] isolatedHair = hairColor.Split(',');
                    float r = float.Parse(isolatedHair[0]);
                    float g = float.Parse(isolatedHair[1]);
                    float b = float.Parse(isolatedHair[2]);
                    float a = float.Parse(isolatedHair[3]);
                    human.story.HairColor = new UnityEngine.Color(r, g, b, a);

                    string skinColor = humanDetails.skinColor.Replace("RGBA(", "").Replace(")", "");
                    string[] isolatedSkin = skinColor.Split(',');
                    r = float.Parse(isolatedSkin[0]);
                    g = float.Parse(isolatedSkin[1]);
                    b = float.Parse(isolatedSkin[2]);
                    a = float.Parse(isolatedSkin[3]);
                    human.story.SkinColorBase = new UnityEngine.Color(r, g, b, a);

                    string favoriteColor = humanDetails.favoriteColor.Replace("RGBA(", "").Replace(")", "");
                    string[] isolatedFavoriteColor = favoriteColor.Split(',');
                    r = float.Parse(isolatedFavoriteColor[0]);
                    g = float.Parse(isolatedFavoriteColor[1]);
                    b = float.Parse(isolatedFavoriteColor[2]);
                    a = float.Parse(isolatedFavoriteColor[3]);
                    human.story.favoriteColor = new UnityEngine.Color(r, g, b, a);
                }
                catch (Exception e) { Log.Warning($"Failed to load colors in human {humanDetails.name}. Reason: {e}"); }

                try
                {
                    if (humanDetails.childhoodStory != "null")
                    {
                        human.story.Childhood = DefDatabase<BackstoryDef>.AllDefs.ToList().Find(x => x.defName == humanDetails.childhoodStory);
                    }

                    if (humanDetails.adulthoodStory != "null")
                    {
                        human.story.Adulthood = DefDatabase<BackstoryDef>.AllDefs.ToList().Find(x => x.defName == humanDetails.adulthoodStory);
                    }
                }
                catch (Exception e) { Log.Warning($"Failed to load stories in human {humanDetails.name}. Reason: {e}"); }

                try
                {
                    if (humanDetails.xenotypeDefName != "null")
                    {
                        human.genes.SetXenotype(DefDatabase<XenotypeDef>.AllDefs.ToList().Find(x => x.defName == humanDetails.xenotypeDefName));
                    }

                    if (humanDetails.customXenotypeName != "null")
                    {
                        human.genes.xenotypeName = humanDetails.customXenotypeName;
                    }
                }
                catch (Exception e) { Log.Warning($"Failed to load xenotypes in human {humanDetails.name}. Reason: {e}"); }

                if (humanDetails.skillDefNames.Count() > 0)
                {
                    for (int i2 = 0; i2 < humanDetails.skillDefNames.Count(); i2++)
                    {
                        try
                        {
                            human.skills.skills[i2].levelInt = int.Parse(humanDetails.skillLevels[i2]);

                            Enum.TryParse(humanDetails.passions[i2], true, out Passion passion);
                            human.skills.skills[i2].passion = passion;
                        }
                        catch (Exception e) { Log.Warning($"Failed to load skill {humanDetails.skillDefNames[i2]} to human {humanDetails.name}. Reason: {e}"); }
                    }
                }

                try
                {
                    human.story.traits.allTraits.Clear();
                }
                catch (Exception e) { Log.Warning($"Failed to remove traits of human {humanDetails.name}. Reason: {e}"); }

                if (humanDetails.traitDefNames.Count() > 0)
                {
                    for (int i3 = 0; i3 < humanDetails.traitDefNames.Count(); i3++)
                    {
                        try
                        {
                            TraitDef traitDef = DefDatabase<TraitDef>.AllDefs.ToList().Find(x => x.defName == humanDetails.traitDefNames[i3]);
                            Trait trait = new Trait(traitDef, int.Parse(humanDetails.traitDegrees[i3]));
                            human.story.traits.GainTrait(trait);
                        }
                        catch (Exception e) { Log.Warning($"Failed to load trait {humanDetails.traitDefNames[i3]} to human {humanDetails.name}. Reason: {e}"); }
                    }
                }

                try
                {
                    human.health.RemoveAllHediffs();
                    human.health.Reset();
                }
                catch (Exception e) { Log.Warning($"Failed to remove heddifs of human {humanDetails.name}. Reason: {e}"); }

                if (humanDetails.hediffDefNames.Count() > 0)
                {
                    for (int i4 = 0; i4 < humanDetails.hediffDefNames.Count(); i4++)
                    {
                        try
                        {
                            HediffDef hediffDef = DefDatabase<HediffDef>.AllDefs.ToList().Find(x => x.defName == humanDetails.hediffDefNames[i4]);

                            BodyPartRecord bodyPart = null;
                            if (humanDetails.hediffPartDefName[i4] == "null") bodyPart = null;
                            else bodyPart = human.RaceProps.body.AllParts.ToList().Find(x => x.def.defName == humanDetails.hediffPartDefName[i4]);

                            Hediff hediff = HediffMaker.MakeHediff(hediffDef, human, bodyPart);

                            hediff.Severity = float.Parse(humanDetails.hediffSeverity[i4]);

                            if (humanDetails.heddifPermanent[i4])
                            {
                                HediffComp_GetsPermanent hediffComp = hediff.TryGetComp<HediffComp_GetsPermanent>();
                                hediffComp.IsPermanent = true;
                            }

                            human.health.AddHediff(hediff);
                        }
                        catch (Exception e) { Log.Warning($"Failed to load heddif in {humanDetails.hediffPartDefName[i4]} to human {humanDetails.name}. Reason: {e}"); }
                    }
                }

                try
                {
                    human.apparel.DestroyAll();
                    human.apparel.DropAllOrMoveAllToInventory();
                }
                catch (Exception e) { Log.Warning($"Failed to destroy apparel in human {humanDetails.name}. Reason: {e}"); }

                if (humanDetails.deflatedApparels.Count() > 0)
                {
                    for (int i5 = 0; i5 < humanDetails.deflatedApparels.Count(); i5++)
                    {
                        try
                        {
                            Apparel apparel = (Apparel)GetItemSimple(Serializer.SerializeFromString<ItemDetailsJSON>(humanDetails.deflatedApparels[i5]));

                            if (humanDetails.apparelWornByCorpse[i5]) apparel.WornByCorpse.MustBeTrue();
                            else apparel.WornByCorpse.MustBeFalse();

                            human.apparel.Wear(apparel);
                        }
                        catch { Log.Warning($"Failed to load apparel in human {humanDetails.name}"); }
                    }
                }

                try
                {
                    human.equipment.DestroyAllEquipment();
                }
                catch (Exception e) { Log.Warning($"Failed to destroy equipment in human {humanDetails.name}. Reason: {e}"); }

                if (humanDetails.deflatedWeapon != "null")
                {
                    try
                    {
                        ThingWithComps thing = (ThingWithComps)GetItemSimple(Serializer.SerializeFromString<ItemDetailsJSON>(humanDetails.deflatedWeapon));
                        human.equipment.AddEquipment(thing);
                    }
                    catch { Log.Warning($"Failed to load weapon in human {humanDetails.name}"); }
                }

                if (humanDetails.deflatedInventoryItems.Count() > 0)
                {
                    foreach (string str in humanDetails.deflatedInventoryItems)
                    {
                        try
                        {
                            Thing thing = GetItemSimple(Serializer.SerializeFromString<ItemDetailsJSON>(str));
                            human.inventory.TryAddAndUnforbid(thing);
                        }
                        catch { Log.Warning($"Failed to add thing to pawn {human.Label}"); }
                    }
                }

                if (humanDetails.position != "null")
                {
                    try
                    {
                        string[] positionSplit = humanDetails.position.Split('|');

                        human.Position = new IntVec3(int.Parse(positionSplit[0]), int.Parse(positionSplit[1]),
                            int.Parse(positionSplit[2]));
                    }
                    catch { Log.Message($"Failed to set human position in human {human.Label}"); }
                }
            }
            catch (Exception e) { Log.Error($"Failed to load human {humanDetails.name}. Reason: {e}"); }

            return human;
        }

        public static AnimalDetailsJSON TransformAnimalToString(Pawn animal)
        {
            AnimalDetailsJSON animalDetailsJSON = new AnimalDetailsJSON();

            try
            {
                animalDetailsJSON.defName = animal.def.defName;
                animalDetailsJSON.name = animal.Name.ToString();
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

            if (animalDetailsJSON.position != "null")
            {
                try { animalDetailsJSON.position = $"{animal.Position.x}|{animal.Position.y}|{animal.Position.z}"; }
                catch { Log.Message("Failed to set pawn position"); }
            }

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

                try
                {
                    string[] positionSplit = animalDetails.position.Split('|');

                    animal.Position = new IntVec3(int.Parse(positionSplit[0]), int.Parse(positionSplit[1]),
                        int.Parse(positionSplit[2]));
                }
                catch { Log.Warning($"Failed to set animal position in animal {animal.Label}"); }
            }
            catch (Exception e) { Log.Error($"Failed to load animal {animalDetails.defName}. Reason: {e}"); }

            return animal;
        }

        public static ItemDetailsJSON TransformItemToString(Thing thing, int thingCount)
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

                try { itemDetailsJSON.position = $"{thing.Position.x}|{thing.Position.y}|{thing.Position.z}"; }
                catch { Log.Message("Failed to set thing position"); }

                itemDetailsJSON.rotation = thing.Rotation.AsInt.ToString();

                if (TransferManagerHelper.CheckIfThingIsMinified(thing)) itemDetailsJSON.isMinified = true;
                else itemDetailsJSON.isMinified = false;
            }
            catch (Exception e) { Log.Warning($"Failed to get item details from item {thing.Label}. Exception: {e}"); }

            return itemDetailsJSON;
        }

        public static Thing[] GetItemsFromString(TransferManifestJSON transferManifestJSON)
        {
            List<Thing> things = new List<Thing>();

            for (int i = 0; i < transferManifestJSON.itemDetailsJSONS.Count(); i++)
            {
                ItemDetailsJSON itemDetailsJSON = Serializer.SerializeFromString<ItemDetailsJSON>(transferManifestJSON.itemDetailsJSONS[i]);

                things.Add(GetItemSimple(itemDetailsJSON));
            }

            return things.ToArray();
        }

        public static Thing GetItemSimple(ItemDetailsJSON itemDetails)
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

                if (itemDetails.position != "null")
                {
                    try
                    {
                        string[] positionSplit = itemDetails.position.Split('|');

                        toGet.Position = new IntVec3(int.Parse(positionSplit[0]), int.Parse(positionSplit[1]),
                            int.Parse(positionSplit[2]));

                        toGet.Rotation = new Rot4(int.Parse(itemDetails.rotation));
                    }
                    catch { Log.Warning($"Failed to load item position for {itemDetails.defName}"); }
                }

                if (itemDetails.isMinified) toGet.TryMakeMinified();
            }
            catch (Exception e) { Log.Error($"Failed to load item {itemDetails.defName}. Reason: {e}"); }

            return toGet;
        }

        public static MapDetailsJSON TransformMapToString(Map map,bool containItems, bool containHumans, bool containAnimals)
        {
            MapDetailsJSON mapDetailsJSON = new MapDetailsJSON();

            mapDetailsJSON.mapTile = map.Tile.ToString();

            mapDetailsJSON.mapSize = $"{map.Size.x}|{map.Size.y}|{map.Size.z}";

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
                            if (containHumans)
                            {
                                string humanString = Serializer.SerializeToString(TransformHumanToString(thing as Pawn));
                                if (thing.Faction == Faction.OfPlayer) mapDetailsJSON.playerHumanDetailsJSON.Add(humanString);
                                else mapDetailsJSON.humanDetailsJSONS.Add(humanString);
                            }
                        }

                        else if (TransferManagerHelper.CheckIfThingIsAnimal(thing))
                        {
                            if (containAnimals)
                            {
                                string animalString = Serializer.SerializeToString(TransformAnimalToString(thing as Pawn));
                                if (thing.Faction == Faction.OfPlayer) mapDetailsJSON.playerAnimalDetailsJSON.Add(animalString);
                                else mapDetailsJSON.animalDetailsJSON.Add(animalString);
                            }
                        }

                        else
                        {
                            ItemDetailsJSON itemDetailsJSON = TransformItemToString(thing, thing.stackCount);
                            string thingString = Serializer.SerializeToString(itemDetailsJSON);

                            if (thing.def.alwaysHaulable)
                            {
                                if (containItems) mapDetailsJSON.playerItemDetailsJSON.Add(thingString);
                                else continue;
                            }
                            else mapDetailsJSON.itemDetailsJSONS.Add(thingString);
                        }
                    }

                    if (map.roofGrid.RoofAt(vectorToCheck) == null) mapDetailsJSON.roofDefNames.Add("null");
                    else mapDetailsJSON.roofDefNames.Add(map.roofGrid.RoofAt(vectorToCheck).defName.ToString());
                }
            }

            return mapDetailsJSON;
        }

        public static Map GetMapSimple(MapDetailsJSON mapDetailsJSON, bool containItems, bool containHumans, bool containAnimals, bool lessLoot)
        {
            Map map = null;

            string[] splitSize = mapDetailsJSON.mapSize.Split('|');

            IntVec3 mapSize = new IntVec3(int.Parse(splitSize[0]), int.Parse(splitSize[1]),
                int.Parse(splitSize[2]));

            try { map = GetOrGenerateMapUtility.GetOrGenerateMap(ClientValues.chosenSettlement.Tile, mapSize, null); }
            catch (Exception e) { Log.Warning($"Critical error generating map. Exception: {e}"); }

            map.fogGrid.ClearAllFog();

            List<Thing> thingList = map.listerThings.AllThings.ToList();
            foreach (Thing thing in thingList)
            {
                try
                {
                    if (TransferManagerHelper.CheckIfThingIsHuman(thing)) thing.Destroy();
                    else if (TransferManagerHelper.CheckIfThingIsAnimal(thing)) thing.Destroy();
                    else if (thing.def.destroyable) thing.Destroy();
                }
                catch { Log.Warning($"Failed to destroy thing {thing.def.defName} at {thing.Position}"); }
            }

            List<Thing> thingsToGetInThisTile = new List<Thing>();

            foreach (string str in mapDetailsJSON.itemDetailsJSONS)
            {
                try
                {
                    Thing toGet = GetItemSimple(Serializer.SerializeFromString<ItemDetailsJSON>(str));
                    thingsToGetInThisTile.Add(toGet);
                }
                catch { }
            }

            if (containItems)
            {
                Random rnd = new Random();

                foreach (string str in mapDetailsJSON.playerItemDetailsJSON)
                {
                    try
                    {
                        Thing toGet = GetItemSimple(Serializer.SerializeFromString<ItemDetailsJSON>(str));

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
                try { GenPlace.TryPlaceThing(thing, thing.Position, map, ThingPlaceMode.Direct, rot:thing.Rotation); }
                catch { Log.Warning($"Failed to place thing {thing.def.defName} at {thing.Position}"); }
            }

            if (containHumans)
            {
                foreach (string str in mapDetailsJSON.humanDetailsJSONS)
                {
                    HumanDetailsJSON humanDetailsJSON = Serializer.SerializeFromString<HumanDetailsJSON>(str);

                    try
                    {
                        Pawn human = GetHumanSimple(humanDetailsJSON);
                        human.SetFaction(FactionValues.yourOnlineFaction);

                        GenSpawn.Spawn(human, human.Position, map, Rot4.Random);
                    }
                    catch { Log.Warning($"Failed to spawn human {humanDetailsJSON.name}"); }
                }

                foreach (string str in mapDetailsJSON.playerHumanDetailsJSON)
                {
                    HumanDetailsJSON humanDetailsJSON = Serializer.SerializeFromString<HumanDetailsJSON>(str);

                    try
                    {
                        Pawn human = GetHumanSimple(humanDetailsJSON);
                        human.SetFaction(FactionValues.neutralPlayer);

                        GenSpawn.Spawn(human, human.Position, map, Rot4.Random);
                    }
                    catch { Log.Warning($"Failed to spawn human {humanDetailsJSON.name}"); }
                }
            }

            if (containAnimals)
            {
                foreach (string str in mapDetailsJSON.animalDetailsJSON)
                {
                    AnimalDetailsJSON animalDetailsJSON = Serializer.SerializeFromString<AnimalDetailsJSON>(str);

                    try
                    {
                        Pawn animal = GetAnimalSimple(animalDetailsJSON);
                        animal.SetFaction(FactionValues.yourOnlineFaction);

                        GenSpawn.Spawn(animal, animal.Position, map, Rot4.Random);
                    }
                    catch { Log.Warning($"Failed to spawn animal {animalDetailsJSON.name}"); }
                }

                foreach (string str in mapDetailsJSON.playerAnimalDetailsJSON)
                {
                    AnimalDetailsJSON animalDetailsJSON = Serializer.SerializeFromString<AnimalDetailsJSON>(str);

                    try
                    {
                        Pawn animal = GetAnimalSimple(animalDetailsJSON);
                        animal.SetFaction(FactionValues.neutralPlayer);

                        GenSpawn.Spawn(animal, animal.Position, map, Rot4.Random);
                    }
                    catch { Log.Warning($"Failed to spawn animal {animalDetailsJSON.name}"); }
                }
            }

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

            map.roofCollapseBuffer.Clear();
            map.roofGrid.Drawer.SetDirty();

            CellIndices cellIndices = map.cellIndices;
            if (map.fogGrid.fogGrid == null) map.fogGrid.fogGrid = new bool[cellIndices.NumGridCells];
            foreach (IntVec3 allCell in map.AllCells) map.fogGrid.fogGrid[cellIndices.CellToIndex(allCell)] = true;
            if (Current.ProgramState == ProgramState.Playing) map.roofGrid.Drawer.SetDirty();

            FloodFillerFog.FloodUnfog(MapGenerator.PlayerStartSpot, map);
            List<IntVec3> rootsToUnfog = MapGenerator.rootsToUnfog;
            for (int i = 0; i < rootsToUnfog.Count; i++) FloodFillerFog.FloodUnfog(rootsToUnfog[i], map);

            return map;
        }
    }
}
