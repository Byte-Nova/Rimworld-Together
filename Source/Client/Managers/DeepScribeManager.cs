using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Shared;
using UnityEngine.Assertions.Must;
using Verse;

namespace GameClient
{
    //Class that handles transformation of humans

    public static class HumanScribeManager
    {
        //Functions

        public static Pawn[] GetHumansFromString(TransferManifestJSON transferManifestJSON)
        {
            List<Pawn> humans = new List<Pawn>();

            for (int i = 0; i < transferManifestJSON.humanDetailsJSONS.Count(); i++)
            {
                HumanDetailsJSON humanDetails = (HumanDetailsJSON)Serializer.ConvertBytesToObject(transferManifestJSON.humanDetailsJSONS[i]);

                humans.Add(StringToHuman(humanDetails));
            }

            return humans.ToArray();
        }

        public static HumanDetailsJSON HumanToString(Pawn pawn, bool passInventory = true)
        {
            HumanDetailsJSON humanDetailsJSON = new HumanDetailsJSON();

            GetPawnBioDetails(pawn, humanDetailsJSON);

            GetPawnKind(pawn, humanDetailsJSON);

            GetPawnFaction(pawn, humanDetailsJSON);

            GetPawnHediffs(pawn, humanDetailsJSON);

            GetPawnXenotype(pawn, humanDetailsJSON);

            GetPawnXenogenes(pawn, humanDetailsJSON);

            GetPawnEndogenes(pawn, humanDetailsJSON);

            GetPawnStory(pawn, humanDetailsJSON);

            GetPawnSkills(pawn, humanDetailsJSON);

            GetPawnTraits(pawn, humanDetailsJSON);

            GetPawnApparel(pawn, humanDetailsJSON);

            GetPawnEquipment(pawn, humanDetailsJSON);

            GetPawnInventory(pawn, humanDetailsJSON, passInventory);

            GetPawnFavoriteColor(pawn, humanDetailsJSON);

            GetPawnPosition(pawn, humanDetailsJSON);

            GetPawnRotation(pawn, humanDetailsJSON);

            return humanDetailsJSON;
        }

        public static Pawn StringToHuman(HumanDetailsJSON humanDetailsJSON)
        {
            PawnKindDef kind = SetPawnKind(humanDetailsJSON);

            Faction faction = SetPawnFaction(humanDetailsJSON);

            Pawn pawn = SetPawn(kind, faction, humanDetailsJSON);

            SetPawnHediffs(pawn, humanDetailsJSON);

            SetPawnXenotype(pawn, humanDetailsJSON);

            SetPawnXenogenes(pawn, humanDetailsJSON);

            SetPawnEndogenes(pawn, humanDetailsJSON);

            SetPawnBioDetails(pawn, humanDetailsJSON);

            SetPawnStory(pawn, humanDetailsJSON);

            SetPawnSkills(pawn, humanDetailsJSON);

            SetPawnTraits(pawn, humanDetailsJSON);

            SetPawnApparel(pawn, humanDetailsJSON);

            SetPawnEquipment(pawn, humanDetailsJSON);

            SetPawnInventory(pawn, humanDetailsJSON);

            SetPawnFavoriteColor(pawn, humanDetailsJSON);

            SetPawnPosition(pawn, humanDetailsJSON);

            SetPawnRotation(pawn, humanDetailsJSON);

            return pawn;
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
            catch { Log.Warning($"Failed to get biological details from human {pawn.Label}"); }
        }

        private static void GetPawnKind(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            try { humanDetailsJSON.kindDef = pawn.kindDef.defName; }
            catch { Log.Warning($"Failed to get kind from human {pawn.Label}"); }
        }

        private static void GetPawnFaction(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            if (pawn.Faction == null) return;

            try { humanDetailsJSON.factionDef = pawn.Faction.def.defName; }
            catch { Log.Warning($"Failed to get faction from human {pawn.Label}"); }
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
                    catch { Log.Warning($"Failed to get heddif {hd} from human {pawn.Label}"); }
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
            catch { Log.Warning($"Failed to get xenotype from human {pawn.Label}"); }
        }

        private static void GetPawnXenogenes(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            if (pawn.genes.Xenogenes.Count() > 0)
            {
                foreach (Gene gene in pawn.genes.Xenogenes)
                {
                    try { humanDetailsJSON.xenogeneDefNames.Add(gene.def.defName); }
                    catch { Log.Warning($"Failed to get gene {gene} from human {pawn.Label}"); }
                }
            }
        }

        private static void GetPawnEndogenes(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            if (pawn.genes.Endogenes.Count() > 0)
            {
                foreach (Gene gene in pawn.genes.Endogenes)
                {
                    try { humanDetailsJSON.endogeneDefNames.Add(gene.def.defName.ToString()); }
                    catch { Log.Warning($"Failed to get endogene {gene} from human {pawn.Label}"); }
                }
            }
        }

        private static void GetPawnFavoriteColor(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            try { humanDetailsJSON.favoriteColor = pawn.story.favoriteColor.ToString(); }
            catch { Log.Warning($"Failed to get favorite color from human {pawn.Label}"); }
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
            catch { Log.Warning($"Failed to get backstories from human {pawn.Label}"); }
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
                    catch { Log.Warning($"Failed to get skill {skill} from human {pawn.Label}"); }
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
                    catch { Log.Warning($"Failed to get trait {trait} from human {pawn.Label}"); }
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
                    catch { Log.Warning($"Failed to get apparel {ap} from human {pawn.Label}"); }
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
                catch { Log.Warning($"Failed to get weapon from human {pawn.Label}"); }
            }
        }

        private static void GetPawnInventory(Pawn pawn, HumanDetailsJSON humanDetailsJSON, bool passInventory)
        {
            if (pawn.inventory.innerContainer.Count() != 0 && passInventory)
            {
                foreach (Thing thing in pawn.inventory.innerContainer)
                {
                    try
                    {
                        ItemDetailsJSON itemDetailsJSON = ThingScribeManager.ItemToString(thing, thing.stackCount);
                        humanDetailsJSON.inventoryItems.Add(itemDetailsJSON);
                    }
                    catch { Log.Warning($"Failed to get item from human {pawn.Label}"); }
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
            catch { Log.Message("Failed to get human position"); }
        }

        private static void GetPawnRotation(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            try { humanDetailsJSON.rotation = pawn.Rotation.AsInt; }
            catch { Log.Message("Failed to get human rotation"); }
        }

        //Setters

        private static PawnKindDef SetPawnKind(HumanDetailsJSON humanDetailsJSON)
        {
            try { return DefDatabase<PawnKindDef>.AllDefs.First(fetch => fetch.defName == humanDetailsJSON.kindDef); }
            catch { Log.Warning($"Failed to set kind in human {humanDetailsJSON.name}"); }

            return null;
        }

        private static Faction SetPawnFaction(HumanDetailsJSON humanDetailsJSON)
        {
            if (humanDetailsJSON.factionDef == null) return null;

            try { return Find.FactionManager.AllFactions.First(fetch => fetch.def.defName == humanDetailsJSON.factionDef); }
            catch { Log.Warning($"Failed to set faction in human {humanDetailsJSON.name}"); }

            return null;
        }

        private static Pawn SetPawn(PawnKindDef kind, Faction faction, HumanDetailsJSON humanDetailsJSON)
        {
            try { return PawnGenerator.GeneratePawn(kind, faction); }
            catch { Log.Warning($"Failed to set biological details in human {humanDetailsJSON.name}"); }

            return null;
        }

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
            catch { Log.Warning($"Failed to set biological details in human {humanDetailsJSON.name}"); }
        }

        private static void SetPawnHediffs(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            try
            {
                pawn.health.RemoveAllHediffs();
                pawn.health.Reset();
            }
            catch { Log.Warning($"Failed to remove heddifs of human {humanDetailsJSON.name}"); }

            if (humanDetailsJSON.hediffDefNames.Count() > 0)
            {
                for (int i = 0; i < humanDetailsJSON.hediffDefNames.Count(); i++)
                {
                    try
                    {
                        HediffDef hediffDef = DefDatabase<HediffDef>.AllDefs.ToList().Find(x => x.defName == humanDetailsJSON.hediffDefNames[i]);
                        BodyPartRecord bodyPart = null;

                        if (humanDetailsJSON.hediffPartDefName[i] != "null")
                        {
                            bodyPart = pawn.RaceProps.body.AllParts.ToList().Find(x => 
                                x.def.defName == humanDetailsJSON.hediffPartDefName[i]);
                        }

                        Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                        hediff.Severity = float.Parse(humanDetailsJSON.hediffSeverity[i]);

                        if (humanDetailsJSON.heddifPermanent[i])
                        {
                            HediffComp_GetsPermanent hediffComp = hediff.TryGetComp<HediffComp_GetsPermanent>();
                            hediffComp.IsPermanent = true;
                        }

                        pawn.health.AddHediff(hediff, bodyPart);
                    }
                    catch { Log.Warning($"Failed to set heddif in {humanDetailsJSON.hediffPartDefName[i]} to human {humanDetailsJSON.name}"); }
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
            catch { Log.Warning($"Failed to set xenotypes in human {humanDetailsJSON.name}"); }
        }

        private static void SetPawnXenogenes(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            try { pawn.genes.Xenogenes.Clear(); }
            catch { Log.Warning($"Failed to clear xenogenes for human {humanDetailsJSON.name}"); }

            if (humanDetailsJSON.xenogeneDefNames.Count() > 0)
            {
                foreach (string str in humanDetailsJSON.xenogeneDefNames)
                {
                    try
                    {
                        GeneDef def = DefDatabase<GeneDef>.AllDefs.First(fetch => fetch.defName == str);
                        pawn.genes.AddGene(def, true);
                    }
                    catch { Log.Warning($"Failed to set xenogenes for human {humanDetailsJSON.name}"); }
                }
            }
        }

        private static void SetPawnEndogenes(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            try { pawn.genes.Endogenes.Clear(); }
            catch { Log.Warning($"Failed to clear endogenes for human {humanDetailsJSON.name}"); }

            if (humanDetailsJSON.endogeneDefNames.Count() > 0)
            {
                foreach (string str in humanDetailsJSON.endogeneDefNames)
                {
                    try
                    {
                        GeneDef def = DefDatabase<GeneDef>.AllDefs.First(fetch => fetch.defName == str);
                        pawn.genes.AddGene(def, false);
                    }
                    catch { Log.Warning($"Failed to set endogenes for human {humanDetailsJSON.name}"); }
                }
            }
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
            catch { Log.Warning($"Failed to set colors in human {humanDetailsJSON.name}"); }
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
            catch { Log.Warning($"Failed to set stories in human {humanDetailsJSON.name}"); }
        }

        private static void SetPawnSkills(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            if (humanDetailsJSON.skillDefNames.Count() > 0)
            {
                for (int i = 0; i < humanDetailsJSON.skillDefNames.Count(); i++)
                {
                    try
                    {
                        pawn.skills.skills[i].levelInt = int.Parse(humanDetailsJSON.skillLevels[i]);

                        Enum.TryParse(humanDetailsJSON.passions[i], true, out Passion passion);
                        pawn.skills.skills[i].passion = passion;
                    }
                    catch { Log.Warning($"Failed to set skill {humanDetailsJSON.skillDefNames[i]} to human {humanDetailsJSON.name}"); }
                }
            }
        }

        private static void SetPawnTraits(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            try { pawn.story.traits.allTraits.Clear(); }
            catch { Log.Warning($"Failed to remove traits of human {humanDetailsJSON.name}"); }

            if (humanDetailsJSON.traitDefNames.Count() > 0)
            {
                for (int i = 0; i < humanDetailsJSON.traitDefNames.Count(); i++)
                {
                    try
                    {
                        TraitDef traitDef = DefDatabase<TraitDef>.AllDefs.ToList().Find(x => x.defName == humanDetailsJSON.traitDefNames[i]);
                        Trait trait = new Trait(traitDef, int.Parse(humanDetailsJSON.traitDegrees[i]));
                        pawn.story.traits.GainTrait(trait);
                    }
                    catch { Log.Warning($"Failed to set trait {humanDetailsJSON.traitDefNames[i]} to human {humanDetailsJSON.name}"); }
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
            catch { Log.Warning($"Failed to destroy apparel in human {humanDetailsJSON.name}"); }

            if (humanDetailsJSON.equippedApparel.Count() > 0)
            {
                for (int i = 0; i < humanDetailsJSON.equippedApparel.Count(); i++)
                {
                    try
                    {
                        Apparel apparel = (Apparel)ThingScribeManager.StringToItem(humanDetailsJSON.equippedApparel[i]);
                        if (humanDetailsJSON.apparelWornByCorpse[i]) apparel.WornByCorpse.MustBeTrue();
                        else apparel.WornByCorpse.MustBeFalse();

                        pawn.apparel.Wear(apparel);
                    }
                    catch { Log.Warning($"Failed to set apparel in human {humanDetailsJSON.name}"); }
                }
            }
        }

        private static void SetPawnEquipment(Pawn pawn, HumanDetailsJSON humanDetailsJSON)
        {
            try { pawn.equipment.DestroyAllEquipment(); }
            catch { Log.Warning($"Failed to destroy equipment in human {humanDetailsJSON.name}"); }

            if (humanDetailsJSON.equippedWeapon != null)
            {
                try
                {
                    ThingWithComps thing = (ThingWithComps)ThingScribeManager.StringToItem(humanDetailsJSON.equippedWeapon);
                    pawn.equipment.AddEquipment(thing);
                }
                catch { Log.Warning($"Failed to set weapon in human {humanDetailsJSON.name}"); }
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
            try { pawn.Rotation = new Rot4(humanDetailsJSON.rotation); }
            catch { Log.Message($"Failed to set rotation in human {pawn.Label}"); }
        }
    }

    //Class that handles transformation of animals

    public static class AnimalScribeManager
    {
        //Functions

        public static Pawn[] GetAnimalsFromString(TransferManifestJSON transferManifestJSON)
        {
            List<Pawn> animals = new List<Pawn>();

            for (int i = 0; i < transferManifestJSON.animalDetailsJSON.Count(); i++)
            {
                AnimalDetailsJSON animalDetails = (AnimalDetailsJSON)Serializer.ConvertBytesToObject(transferManifestJSON.animalDetailsJSON[i]);

                animals.Add(StringToAnimal(animalDetails));
            }

            return animals.ToArray();
        }

        public static AnimalDetailsJSON AnimalToString(Pawn animal)
        {
            AnimalDetailsJSON animalDetailsJSON = new AnimalDetailsJSON();

            GetAnimalBioDetails(animal, animalDetailsJSON);

            GetAnimalKind(animal, animalDetailsJSON);

            GetAnimalFaction(animal, animalDetailsJSON);

            GetAnimalHediffs(animal, animalDetailsJSON);

            GetAnimalSkills(animal, animalDetailsJSON);

            GetAnimalPosition(animal, animalDetailsJSON);

            GetAnimalRotation(animal, animalDetailsJSON);

            return animalDetailsJSON;
        }

        public static Pawn StringToAnimal(AnimalDetailsJSON animalDetailsJSON)
        {
            PawnKindDef kind = SetAnimalKind(animalDetailsJSON);

            Faction faction = SetAnimalFaction(animalDetailsJSON);

            Pawn animal = SetAnimal(kind, faction, animalDetailsJSON);

            SetAnimalBioDetails(animal, animalDetailsJSON);

            SetAnimalHediffs(animal, animalDetailsJSON);

            SetAnimalSkills(animal, animalDetailsJSON);

            SetAnimalPosition(animal, animalDetailsJSON);

            SetAnimalRotation(animal, animalDetailsJSON);

            return animal;
        }

        //Getters

        private static void GetAnimalBioDetails(Pawn animal, AnimalDetailsJSON animalDetailsJSON)
        {
            try
            {
                animalDetailsJSON.defName = animal.def.defName;
                animalDetailsJSON.name = animal.LabelShortCap.ToString();
                animalDetailsJSON.biologicalAge = animal.ageTracker.AgeBiologicalTicks.ToString();
                animalDetailsJSON.chronologicalAge = animal.ageTracker.AgeChronologicalTicks.ToString();
                animalDetailsJSON.gender = animal.gender.ToString();
            }
            catch { Log.Warning($"Failed to get biodetails of animal {animal.def.defName}"); }
        }

        private static void GetAnimalKind(Pawn animal, AnimalDetailsJSON animalDetailsJSON)
        {
            try { animalDetailsJSON.kindDef = animal.kindDef.defName; }
            catch { Log.Warning($"Failed to get kind from human {animal.Label}"); }
        }

        private static void GetAnimalFaction(Pawn animal, AnimalDetailsJSON animalDetailsJSON)
        {
            if (animal.Faction == null) return;

            try { animalDetailsJSON.factionDef = animal.Faction.def.defName; }
            catch { Log.Warning($"Failed to get faction from animal {animal.def.defName}"); }
        }

        private static void GetAnimalHediffs(Pawn animal, AnimalDetailsJSON animalDetailsJSON)
        {
            if (animal.health.hediffSet.hediffs.Count() > 0)
            {
                foreach (Hediff hd in animal.health.hediffSet.hediffs)
                {
                    try
                    {
                        animalDetailsJSON.hediffDefNames.Add(hd.def.defName);

                        if (hd.Part != null) animalDetailsJSON.hediffPartDefName.Add(hd.Part.def.defName.ToString());
                        else animalDetailsJSON.hediffPartDefName.Add("null");

                        animalDetailsJSON.hediffSeverity.Add(hd.Severity.ToString());
                        animalDetailsJSON.heddifPermanent.Add(hd.IsPermanent());
                    }
                    catch { Log.Warning($"Failed to get headdifs from animal {animal.def.defName}"); }
                }
            }
        }

        private static void GetAnimalSkills(Pawn animal, AnimalDetailsJSON animalDetailsJSON)
        {
            if (animal.training == null) return;

            foreach (TrainableDef trainable in DefDatabase<TrainableDef>.AllDefsListForReading)
            {
                try
                {
                    animalDetailsJSON.trainableDefNames.Add(trainable.defName);
                    animalDetailsJSON.canTrain.Add(animal.training.CanAssignToTrain(trainable).Accepted);
                    animalDetailsJSON.hasLearned.Add(animal.training.HasLearned(trainable));
                    animalDetailsJSON.isDisabled.Add(animal.training.GetWanted(trainable));
                }
                catch { Log.Warning($"Failed to get skills of animal {animal.def.defName}"); }
            }
        }

        private static void GetAnimalPosition(Pawn animal, AnimalDetailsJSON animalDetailsJSON)
        {
            try
            {
                animalDetailsJSON.position = new string[] { animal.Position.x.ToString(),
                        animal.Position.y.ToString(), animal.Position.z.ToString() };
            }
            catch { Log.Message($"Failed to get position of animal {animal.def.defName}"); }
        }

        private static void GetAnimalRotation(Pawn animal, AnimalDetailsJSON animalDetailsJSON)
        {
            try { animalDetailsJSON.rotation = animal.Rotation.AsInt; }
            catch { Log.Message($"Failed to get rotation of animal {animal.def.defName}"); }
        }

        //Setters

        private static PawnKindDef SetAnimalKind(AnimalDetailsJSON animalDetailsJSON)
        {
            try { return DefDatabase<PawnKindDef>.AllDefs.First(fetch => fetch.defName == animalDetailsJSON.defName); }
            catch { Log.Warning($"Failed to set kind in animal {animalDetailsJSON.name}"); }

            return null;
        }

        private static Faction SetAnimalFaction(AnimalDetailsJSON animalDetailsJSON)
        {
            if (animalDetailsJSON.factionDef == null) return null;

            try { return Find.FactionManager.AllFactions.First(fetch => fetch.def.defName == animalDetailsJSON.factionDef); }
            catch { Log.Warning($"Failed to set faction in animal {animalDetailsJSON.name}"); }

            return null;
        }

        private static Pawn SetAnimal(PawnKindDef kind, Faction faction, AnimalDetailsJSON animalDetailsJSON)
        {
            try { return PawnGenerator.GeneratePawn(kind, faction); }
            catch { Log.Warning($"Failed to set animal {animalDetailsJSON.name}"); }

            return null;
        }

        private static void SetAnimalBioDetails(Pawn animal, AnimalDetailsJSON animalDetailsJSON)
        {
            try
            {
                animal.Name = new NameSingle(animalDetailsJSON.name);
                animal.ageTracker.AgeBiologicalTicks = long.Parse(animalDetailsJSON.biologicalAge);
                animal.ageTracker.AgeChronologicalTicks = long.Parse(animalDetailsJSON.chronologicalAge);

                Enum.TryParse(animalDetailsJSON.gender, true, out Gender animalGender);
                animal.gender = animalGender;
            }
            catch { Log.Warning($"Failed to set biodetails of animal {animalDetailsJSON.name}"); }
        }

        private static void SetAnimalHediffs(Pawn animal, AnimalDetailsJSON animalDetailsJSON)
        {
            try
            {
                animal.health.RemoveAllHediffs();
                animal.health.Reset();
            }
            catch { Log.Warning($"Failed to remove heddifs of animal {animalDetailsJSON.name}"); }

            if (animalDetailsJSON.hediffDefNames.Count() > 0)
            {
                for (int i = 0; i < animalDetailsJSON.hediffDefNames.Count(); i++)
                {
                    try
                    {
                        HediffDef hediffDef = DefDatabase<HediffDef>.AllDefs.ToList().Find(x => x.defName == animalDetailsJSON.hediffDefNames[i]);
                        BodyPartRecord bodyPart = null;

                        if (animalDetailsJSON.hediffPartDefName[i] != "null")
                        {
                            bodyPart = animal.RaceProps.body.AllParts.ToList().Find(x =>
                                x.def.defName == animalDetailsJSON.hediffPartDefName[i]);
                        }

                        Hediff hediff = HediffMaker.MakeHediff(hediffDef, animal, bodyPart);
                        hediff.Severity = float.Parse(animalDetailsJSON.hediffSeverity[i]);

                        if (animalDetailsJSON.heddifPermanent[i])
                        {
                            HediffComp_GetsPermanent hediffComp = hediff.TryGetComp<HediffComp_GetsPermanent>();
                            hediffComp.IsPermanent = true;
                        }

                        animal.health.AddHediff(hediff);
                    }
                    catch { Log.Warning($"Failed to set headiffs in animal {animalDetailsJSON.defName}"); }
                }
            }
        }

        private static void SetAnimalSkills(Pawn animal, AnimalDetailsJSON animalDetailsJSON)
        {
            if (animalDetailsJSON.trainableDefNames.Count() > 0)
            {
                for (int i = 0; i < animalDetailsJSON.trainableDefNames.Count(); i++)
                {
                    try
                    {
                        TrainableDef trainable = DefDatabase<TrainableDef>.AllDefs.ToList().Find(x => x.defName == animalDetailsJSON.trainableDefNames[i]);
                        if (animalDetailsJSON.canTrain[i]) animal.training.Train(trainable, null, complete: animalDetailsJSON.hasLearned[i]);
                    }
                    catch { Log.Warning($"Failed to set skills of animal {animalDetailsJSON.name}"); }
                }
            }
        }

        private static void SetAnimalPosition(Pawn animal, AnimalDetailsJSON animalDetailsJSON)
        {
            if (animal.Position != null)
            {
                try
                {
                    animal.Position = new IntVec3(int.Parse(animalDetailsJSON.position[0]), int.Parse(animalDetailsJSON.position[1]),
                        int.Parse(animalDetailsJSON.position[2]));
                }
                catch { Log.Warning($"Failed to set position of animal {animalDetailsJSON.name}"); }
            }
        }

        private static void SetAnimalRotation(Pawn animal, AnimalDetailsJSON animalDetailsJSON)
        {
            try { animal.Rotation = new Rot4(animalDetailsJSON.rotation); }
            catch { Log.Message($"Failed to set rotation of animal {animalDetailsJSON.name}"); }
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
                ItemDetailsJSON itemDetailsJSON = (ItemDetailsJSON)Serializer.ConvertBytesToObject(transferManifestJSON.itemDetailsJSONS[i]);

                things.Add(StringToItem(itemDetailsJSON));
            }

            return things.ToArray();
        }

        public static ItemDetailsJSON ItemToString(Thing thing, int thingCount)
        {
            ItemDetailsJSON itemDetailsJSON = new ItemDetailsJSON();

            Thing toUse = null;
            if (GetItemMinified(thing, itemDetailsJSON)) toUse = thing.GetInnerIfMinified();
            else toUse = thing;

            GetItemName(toUse, itemDetailsJSON);

            GetItemMaterial(toUse, itemDetailsJSON);

            GetItemQuantity(toUse, itemDetailsJSON, thingCount);

            GetItemQuality(toUse, itemDetailsJSON);

            GetItemHitpoints(toUse, itemDetailsJSON);

            GetItemPosition(toUse, itemDetailsJSON);

            GetItemRotation(toUse, itemDetailsJSON);

            return itemDetailsJSON;
        }

        public static Thing StringToItem(ItemDetailsJSON itemDetailsJSON)
        {
            Thing thing = SetItem(itemDetailsJSON);

            SetItemQuantity(thing, itemDetailsJSON);

            SetItemQuality(thing, itemDetailsJSON);

            SetItemHitpoints(thing, itemDetailsJSON);

            SetItemPosition(thing, itemDetailsJSON);

            SetItemRotation(thing, itemDetailsJSON);

            SetItemMinified(thing, itemDetailsJSON);

            return thing;
        }

        //Getters

        private static void GetItemName(Thing thing, ItemDetailsJSON itemDetailsJSON)
        {
            try { itemDetailsJSON.defName = thing.def.defName; }
            catch { Log.Warning($"Failed to get name of thing {thing.def.defName}"); }
        }

        private static void GetItemMaterial(Thing thing, ItemDetailsJSON itemDetailsJSON)
        {
            try 
            {
                if (TransferManagerHelper.CheckIfThingHasMaterial(thing)) itemDetailsJSON.materialDefName = thing.Stuff.defName;
                else itemDetailsJSON.materialDefName = null;
            }
            catch { Log.Warning($"Failed to get material of thing {thing.def.defName}"); }
        }

        private static void GetItemQuantity(Thing thing, ItemDetailsJSON itemDetailsJSON, int thingCount)
        {
            try { itemDetailsJSON.quantity = thingCount; }
            catch { Log.Warning($"Failed to get quantity of thing {thing.def.defName}"); }
        }

        private static void GetItemQuality(Thing thing, ItemDetailsJSON itemDetailsJSON)
        {
            try { itemDetailsJSON.quality = TransferManagerHelper.GetThingQuality(thing); }
            catch { Log.Warning($"Failed to get quality of thing {thing.def.defName}"); }
        }

        private static void GetItemHitpoints(Thing thing, ItemDetailsJSON itemDetailsJSON)
        {
            try { itemDetailsJSON.hitpoints = thing.HitPoints; }
            catch { Log.Warning($"Failed to get hitpoints of thing {thing.def.defName}"); }
        }

        private static void GetItemPosition(Thing thing, ItemDetailsJSON itemDetailsJSON)
        {
            try
            {
                itemDetailsJSON.position = new string[] { thing.Position.x.ToString(),
                    thing.Position.y.ToString(), thing.Position.z.ToString() };
            }
            catch { Log.Warning($"Failed to get position of thing {thing.def.defName}"); }
        }

        private static void GetItemRotation(Thing thing, ItemDetailsJSON itemDetailsJSON)
        {
            try { itemDetailsJSON.rotation = thing.Rotation.AsInt; }
            catch { Log.Warning($"Failed to get rotation of thing {thing.def.defName}"); }
        }

        private static bool GetItemMinified(Thing thing, ItemDetailsJSON itemDetailsJSON)
        {
            try 
            {
                itemDetailsJSON.isMinified = TransferManagerHelper.CheckIfThingIsMinified(thing);
                return itemDetailsJSON.isMinified;
            }
            catch { Log.Warning($"Failed to get minified of thing {thing.def.defName}"); }

            return false;
        }

        //Setters

        private static Thing SetItem(ItemDetailsJSON itemDetailsJSON)
        {
            try
            {
                ThingDef thingDef = DefDatabase<ThingDef>.AllDefs.ToList().Find(x => x.defName == itemDetailsJSON.defName);
                ThingDef defMaterial = DefDatabase<ThingDef>.AllDefs.ToList().Find(x => x.defName == itemDetailsJSON.materialDefName);
                return ThingMaker.MakeThing(thingDef, defMaterial);
            }
            catch { Log.Warning($"Failed to set item for {itemDetailsJSON.defName}"); }

            return null;
        }

        private static void SetItemQuantity(Thing thing, ItemDetailsJSON itemDetailsJSON)
        {
            try { thing.stackCount = itemDetailsJSON.quantity; }
            catch { Log.Warning($"Failed to set item quantity for {itemDetailsJSON.defName}"); }
        }

        private static void SetItemQuality(Thing thing, ItemDetailsJSON itemDetailsJSON)
        {
            if (itemDetailsJSON.quality != "null")
            {
                try
                {
                    CompQuality compQuality = thing.TryGetComp<CompQuality>();
                    if (compQuality != null)
                    {
                        QualityCategory iCategory = (QualityCategory)int.Parse(itemDetailsJSON.quality);
                        compQuality.SetQuality(iCategory, ArtGenerationContext.Outsider);
                    }
                }
                catch { Log.Warning($"Failed to set item quality for {itemDetailsJSON.defName}"); }
            }
        }

        private static void SetItemHitpoints(Thing thing, ItemDetailsJSON itemDetailsJSON)
        {
            try { thing.HitPoints = itemDetailsJSON.hitpoints; }
            catch { Log.Warning($"Failed to set item hitpoints for {itemDetailsJSON.defName}"); }
        }

        private static void SetItemPosition(Thing thing, ItemDetailsJSON itemDetailsJSON)
        {
            if (itemDetailsJSON.position != null)
            {
                try
                {
                    thing.Position = new IntVec3(int.Parse(itemDetailsJSON.position[0]), int.Parse(itemDetailsJSON.position[1]),
                        int.Parse(itemDetailsJSON.position[2]));
                }
                catch { Log.Warning($"Failed to set position for item {itemDetailsJSON.defName}"); }
            }
        }

        private static void SetItemRotation(Thing thing, ItemDetailsJSON itemDetailsJSON)
        {
            try { thing.Rotation = new Rot4(itemDetailsJSON.rotation); }
            catch { Log.Warning($"Failed to set rotation for item {itemDetailsJSON.defName}"); }
        }

        private static void SetItemMinified(Thing thing, ItemDetailsJSON itemDetailsJSON)
        {
            if (itemDetailsJSON.isMinified)
            {
                //INFO
                //This function is where you should transform the item back into a minified.
                //However, this isn't needed and is likely to cause issues with caravans if used
            }
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
                                AnimalDetailsJSON animalDetailsJSON = AnimalScribeManager.AnimalToString(thing as Pawn);
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
                    Pawn animal = AnimalScribeManager.StringToAnimal(pawn);
                    GenSpawn.Spawn(animal, animal.Position, map, animal.Rotation);
                }
                catch { Log.Warning($"Failed to spawn animal {pawn.name}"); }
            }

            foreach (AnimalDetailsJSON pawn in mapDetailsJSON.factionAnimals)
            {
                try
                {
                    Pawn animal = AnimalScribeManager.StringToAnimal(pawn);
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
