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

        public static Pawn[] GetHumansFromString(TransferData transferData)
        {
            List<Pawn> humans = new List<Pawn>();

            for (int i = 0; i < transferData.humanDatas.Count(); i++)
            {
                HumanData humanDetails = (HumanData)Serializer.ConvertBytesToObject(transferData.humanDatas[i]);

                humans.Add(StringToHuman(humanDetails));
            }

            return humans.ToArray();
        }

        public static HumanData HumanToString(Pawn pawn, bool passInventory = true)
        {
            HumanData humanData = new HumanData();

            GetPawnBioDetails(pawn, humanData);

            GetPawnKind(pawn, humanData);

            GetPawnFaction(pawn, humanData);

            GetPawnHediffs(pawn, humanData);

            if (ModsConfig.BiotechActive)
            {
                GetPawnXenotype(pawn, humanData);

                GetPawnXenogenes(pawn, humanData);

                GetPawnEndogenes(pawn, humanData);
            }

            GetPawnStory(pawn, humanData);

            GetPawnSkills(pawn, humanData);

            GetPawnTraits(pawn, humanData);

            GetPawnApparel(pawn, humanData);

            GetPawnEquipment(pawn, humanData);

            GetPawnInventory(pawn, humanData, passInventory);

            GetPawnFavoriteColor(pawn, humanData);

            GetPawnPosition(pawn, humanData);

            GetPawnRotation(pawn, humanData);

            return humanData;
        }

        public static Pawn StringToHuman(HumanData humanData)
        {
            PawnKindDef kind = SetPawnKind(humanData);

            Faction faction = SetPawnFaction(humanData);

            Pawn pawn = SetPawn(kind, faction, humanData);

            SetPawnHediffs(pawn, humanData);

            if (ModsConfig.BiotechActive)
            {
                SetPawnXenotype(pawn, humanData);

                SetPawnXenogenes(pawn, humanData);

                SetPawnEndogenes(pawn, humanData);
            }

            SetPawnBioDetails(pawn, humanData);

            SetPawnStory(pawn, humanData);

            SetPawnSkills(pawn, humanData);

            SetPawnTraits(pawn, humanData);

            SetPawnApparel(pawn, humanData);

            SetPawnEquipment(pawn, humanData);

            SetPawnInventory(pawn, humanData);

            SetPawnFavoriteColor(pawn, humanData);

            SetPawnPosition(pawn, humanData);

            SetPawnRotation(pawn, humanData);

            return pawn;
        }

        //Getters

        private static void GetPawnBioDetails(Pawn pawn, HumanData humanData)
        {
            try
            {
                humanData.defName = pawn.def.defName;
                humanData.name = pawn.LabelShortCap.ToString();
                humanData.biologicalAge = pawn.ageTracker.AgeBiologicalTicks.ToString();
                humanData.chronologicalAge = pawn.ageTracker.AgeChronologicalTicks.ToString();
                humanData.gender = pawn.gender.ToString();
                
                humanData.hairDefName = pawn.story.hairDef.defName.ToString();
                humanData.hairColor = pawn.story.HairColor.ToString();
                humanData.headTypeDefName = pawn.story.headType.defName.ToString();
                humanData.skinColor = pawn.story.SkinColor.ToString();
                humanData.beardDefName = pawn.style.beardDef.defName.ToString();
                humanData.bodyTypeDefName = pawn.story.bodyType.defName.ToString();
                humanData.FaceTattooDefName = pawn.style.FaceTattoo.defName.ToString();
                humanData.BodyTattooDefName = pawn.style.BodyTattoo.defName.ToString();
            }
            catch { Log.Warning($"Failed to get biological details from human {pawn.Label}"); }
        }

        private static void GetPawnKind(Pawn pawn, HumanData humanData)
        {
            try { humanData.kindDef = pawn.kindDef.defName; }
            catch { Log.Warning($"Failed to get kind from human {pawn.Label}"); }
        }

        private static void GetPawnFaction(Pawn pawn, HumanData humanData)
        {
            if (pawn.Faction == null) return;

            try { humanData.factionDef = pawn.Faction.def.defName; }
            catch { Log.Warning($"Failed to get faction from human {pawn.Label}"); }
        }

        private static void GetPawnHediffs(Pawn pawn, HumanData humanData)
        {
            if (pawn.health.hediffSet.hediffs.Count() > 0)
            {
                foreach (Hediff hd in pawn.health.hediffSet.hediffs)
                {
                    try
                    {
                        humanData.hediffDefNames.Add(hd.def.defName);

                        if (hd.Part != null) humanData.hediffPartDefName.Add(hd.Part.def.defName.ToString());
                        else humanData.hediffPartDefName.Add("null");

                        humanData.hediffSeverity.Add(hd.Severity.ToString());
                        humanData.heddifPermanent.Add(hd.IsPermanent());
                    }
                    catch { Log.Warning($"Failed to get heddif {hd} from human {pawn.Label}"); }
                }
            }
        }

        private static void GetPawnXenotype(Pawn pawn, HumanData humanData)
        {
            try
            {
                if (pawn.genes.Xenotype != null) humanData.xenotypeDefName = pawn.genes.Xenotype.defName.ToString();
                else humanData.xenotypeDefName = "null";

                if (pawn.genes.CustomXenotype != null) humanData.customXenotypeName = pawn.genes.xenotypeName.ToString();
                else humanData.customXenotypeName = "null";
            }
            catch { Log.Warning($"Failed to get xenotype from human {pawn.Label}"); }
        }

        private static void GetPawnXenogenes(Pawn pawn, HumanData humanData)
        {
            if (pawn.genes.Xenogenes.Count() > 0)
            {
                foreach (Gene gene in pawn.genes.Xenogenes)
                {
                    try { humanData.xenogeneDefNames.Add(gene.def.defName); }
                    catch { Log.Warning($"Failed to get gene {gene} from human {pawn.Label}"); }
                }
            }
        }

        private static void GetPawnEndogenes(Pawn pawn, HumanData humanData)
        {
            if (pawn.genes.Endogenes.Count() > 0)
            {
                foreach (Gene gene in pawn.genes.Endogenes)
                {
                    try { humanData.endogeneDefNames.Add(gene.def.defName.ToString()); }
                    catch { Log.Warning($"Failed to get endogene {gene} from human {pawn.Label}"); }
                }
            }
        }

        private static void GetPawnFavoriteColor(Pawn pawn, HumanData humanData)
        {
            try { humanData.favoriteColor = pawn.story.favoriteColor.ToString(); }
            catch { Log.Warning($"Failed to get favorite color from human {pawn.Label}"); }
        }

        private static void GetPawnStory(Pawn pawn, HumanData humanData)
        {
            try
            {
                if (pawn.story.Childhood != null) humanData.childhoodStory = pawn.story.Childhood.defName.ToString();
                else humanData.childhoodStory = "null";

                if (pawn.story.Adulthood != null) humanData.adulthoodStory = pawn.story.Adulthood.defName.ToString();
                else humanData.adulthoodStory = "null";
            }
            catch { Log.Warning($"Failed to get backstories from human {pawn.Label}"); }
        }

        private static void GetPawnSkills(Pawn pawn, HumanData humanData)
        {
            if (pawn.skills.skills.Count() > 0)
            {
                foreach (SkillRecord skill in pawn.skills.skills)
                {
                    try
                    {
                        humanData.skillDefNames.Add(skill.def.defName);
                        humanData.skillLevels.Add(skill.levelInt.ToString());
                        humanData.passions.Add(skill.passion.ToString());
                    }
                    catch { Log.Warning($"Failed to get skill {skill} from human {pawn.Label}"); }
                }
            }
        }

        private static void GetPawnTraits(Pawn pawn, HumanData humanData)
        {
            if (pawn.story.traits.allTraits.Count() > 0)
            {
                foreach (Trait trait in pawn.story.traits.allTraits)
                {
                    try
                    {
                        humanData.traitDefNames.Add(trait.def.defName);
                        humanData.traitDegrees.Add(trait.Degree.ToString());
                    }
                    catch { Log.Warning($"Failed to get trait {trait} from human {pawn.Label}"); }
                }
            }
        }

        private static void GetPawnApparel(Pawn pawn, HumanData humanData)
        {
            if (pawn.apparel.WornApparel.Count() > 0)
            {
                foreach (Apparel ap in pawn.apparel.WornApparel)
                {
                    try
                    {
                        ItemData itemData = ThingScribeManager.ItemToString(ap, 1);
                        humanData.equippedApparel.Add(itemData);
                        humanData.apparelWornByCorpse.Add(ap.WornByCorpse);
                    }
                    catch { Log.Warning($"Failed to get apparel {ap} from human {pawn.Label}"); }
                }
            }
        }

        private static void GetPawnEquipment(Pawn pawn, HumanData humanData)
        {
            if (pawn.equipment.Primary != null)
            {
                try
                {
                    ThingWithComps weapon = pawn.equipment.Primary;
                    ItemData weaponDetails = ThingScribeManager.ItemToString(weapon, weapon.stackCount);
                    humanData.equippedWeapon = weaponDetails;
                }
                catch { Log.Warning($"Failed to get weapon from human {pawn.Label}"); }
            }
        }

        private static void GetPawnInventory(Pawn pawn, HumanData humanData, bool passInventory)
        {
            if (pawn.inventory.innerContainer.Count() != 0 && passInventory)
            {
                foreach (Thing thing in pawn.inventory.innerContainer)
                {
                    try
                    {
                        ItemData itemData = ThingScribeManager.ItemToString(thing, thing.stackCount);
                        humanData.inventoryItems.Add(itemData);
                    }
                    catch { Log.Warning($"Failed to get item from human {pawn.Label}"); }
                }
            }
        }

        private static void GetPawnPosition(Pawn pawn, HumanData humanData)
        {
            try
            {
                humanData.position = new string[] { pawn.Position.x.ToString(),
                    pawn.Position.y.ToString(), pawn.Position.z.ToString() };
            }
            catch { Log.Message("Failed to get human position"); }
        }

        private static void GetPawnRotation(Pawn pawn, HumanData humanData)
        {
            try { humanData.rotation = pawn.Rotation.AsInt; }
            catch { Log.Message("Failed to get human rotation"); }
        }

        //Setters

        private static PawnKindDef SetPawnKind(HumanData humanData)
        {
            try { return DefDatabase<PawnKindDef>.AllDefs.First(fetch => fetch.defName == humanData.kindDef); }
            catch { Log.Warning($"Failed to set kind in human {humanData.name}"); }

            return null;
        }

        private static Faction SetPawnFaction(HumanData humanData)
        {
            if (humanData.factionDef == null) return null;

            try { return Find.FactionManager.AllFactions.First(fetch => fetch.def.defName == humanData.factionDef); }
            catch { Log.Warning($"Failed to set faction in human {humanData.name}"); }

            return null;
        }

        private static Pawn SetPawn(PawnKindDef kind, Faction faction, HumanData humanData)
        {
            try { return PawnGenerator.GeneratePawn(kind, faction); }
            catch { Log.Warning($"Failed to set biological details in human {humanData.name}"); }

            return null;
        }

        private static void SetPawnBioDetails(Pawn pawn, HumanData humanData)
        {
            try
            {
                pawn.Name = new NameSingle(humanData.name);
                pawn.ageTracker.AgeBiologicalTicks = long.Parse(humanData.biologicalAge);
                pawn.ageTracker.AgeChronologicalTicks = long.Parse(humanData.chronologicalAge);

                Enum.TryParse(humanData.gender, true, out Gender humanGender);
                pawn.gender = humanGender;

                pawn.story.hairDef = DefDatabase<HairDef>.AllDefs.ToList().Find(x => x.defName == humanData.hairDefName);
                pawn.story.headType = DefDatabase<HeadTypeDef>.AllDefs.ToList().Find(x => x.defName == humanData.headTypeDefName);
                pawn.style.beardDef = DefDatabase<BeardDef>.AllDefs.ToList().Find(x => x.defName == humanData.beardDefName);
                pawn.story.bodyType = DefDatabase<BodyTypeDef>.AllDefs.ToList().Find(x => x.defName == humanData.bodyTypeDefName);
                pawn.style.FaceTattoo = DefDatabase<TattooDef>.AllDefs.ToList().Find(x => x.defName == humanData.FaceTattooDefName);
                pawn.style.BodyTattoo = DefDatabase<TattooDef>.AllDefs.ToList().Find(x => x.defName == humanData.BodyTattooDefName);

                string hairColor = humanData.hairColor.Replace("RGBA(", "").Replace(")", "");
                string[] isolatedHair = hairColor.Split(',');
                float r = float.Parse(isolatedHair[0]);
                float g = float.Parse(isolatedHair[1]);
                float b = float.Parse(isolatedHair[2]);
                float a = float.Parse(isolatedHair[3]);
                pawn.story.HairColor = new UnityEngine.Color(r, g, b, a);

                string skinColor = humanData.skinColor.Replace("RGBA(", "").Replace(")", "");
                string[] isolatedSkin = skinColor.Split(',');
                r = float.Parse(isolatedSkin[0]);
                g = float.Parse(isolatedSkin[1]);
                b = float.Parse(isolatedSkin[2]);
                a = float.Parse(isolatedSkin[3]);
                pawn.story.SkinColorBase = new UnityEngine.Color(r, g, b, a);
            }
            catch { Log.Warning($"Failed to set biological details in human {humanData.name}"); }
        }

        private static void SetPawnHediffs(Pawn pawn, HumanData humanData)
        {
            try
            {
                pawn.health.RemoveAllHediffs();
                pawn.health.Reset();
            }
            catch { Log.Warning($"Failed to remove heddifs of human {humanData.name}"); }

            if (humanData.hediffDefNames.Count() > 0)
            {
                for (int i = 0; i < humanData.hediffDefNames.Count(); i++)
                {
                    try
                    {
                        HediffDef hediffDef = DefDatabase<HediffDef>.AllDefs.ToList().Find(x => x.defName == humanData.hediffDefNames[i]);
                        BodyPartRecord bodyPart = null;

                        if (humanData.hediffPartDefName[i] != "null")
                        {
                            bodyPart = pawn.RaceProps.body.AllParts.ToList().Find(x => 
                                x.def.defName == humanData.hediffPartDefName[i]);
                        }

                        Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                        hediff.Severity = float.Parse(humanData.hediffSeverity[i]);

                        if (humanData.heddifPermanent[i])
                        {
                            HediffComp_GetsPermanent hediffComp = hediff.TryGetComp<HediffComp_GetsPermanent>();
                            hediffComp.IsPermanent = true;
                        }

                        pawn.health.AddHediff(hediff, bodyPart);
                    }
                    catch { Log.Warning($"Failed to set heddif in {humanData.hediffPartDefName[i]} to human {humanData.name}"); }
                }
            }
        }

        private static void SetPawnXenotype(Pawn pawn, HumanData humanData)
        {
            try
            {
                if (humanData.xenotypeDefName != "null")
                {
                    pawn.genes.SetXenotype(DefDatabase<XenotypeDef>.AllDefs.ToList().Find(x => x.defName == humanData.xenotypeDefName));
                }

                if (humanData.customXenotypeName != "null")
                {
                    pawn.genes.xenotypeName = humanData.customXenotypeName;
                }
            }
            catch { Log.Warning($"Failed to set xenotypes in human {humanData.name}"); }
        }

        private static void SetPawnXenogenes(Pawn pawn, HumanData humanData)
        {
            try { pawn.genes.Xenogenes.Clear(); }
            catch { Log.Warning($"Failed to clear xenogenes for human {humanData.name}"); }

            if (humanData.xenogeneDefNames.Count() > 0)
            {
                foreach (string str in humanData.xenogeneDefNames)
                {
                    try
                    {
                        GeneDef def = DefDatabase<GeneDef>.AllDefs.First(fetch => fetch.defName == str);
                        pawn.genes.AddGene(def, true);
                    }
                    catch { Log.Warning($"Failed to set xenogenes for human {humanData.name}"); }
                }
            }
        }

        private static void SetPawnEndogenes(Pawn pawn, HumanData humanData)
        {
            try { pawn.genes.Endogenes.Clear(); }
            catch { Log.Warning($"Failed to clear endogenes for human {humanData.name}"); }

            if (humanData.endogeneDefNames.Count() > 0)
            {
                foreach (string str in humanData.endogeneDefNames)
                {
                    try
                    {
                        GeneDef def = DefDatabase<GeneDef>.AllDefs.First(fetch => fetch.defName == str);
                        pawn.genes.AddGene(def, false);
                    }
                    catch { Log.Warning($"Failed to set endogenes for human {humanData.name}"); }
                }
            }
        }

        private static void SetPawnFavoriteColor(Pawn pawn, HumanData humanData)
        {
            try
            {
                float r;
                float g;
                float b;
                float a;

                string favoriteColor = humanData.favoriteColor.Replace("RGBA(", "").Replace(")", "");
                string[] isolatedFavoriteColor = favoriteColor.Split(',');
                r = float.Parse(isolatedFavoriteColor[0]);
                g = float.Parse(isolatedFavoriteColor[1]);
                b = float.Parse(isolatedFavoriteColor[2]);
                a = float.Parse(isolatedFavoriteColor[3]);
                pawn.story.favoriteColor = new UnityEngine.Color(r, g, b, a);
            }
            catch { Log.Warning($"Failed to set colors in human {humanData.name}"); }
        }

        private static void SetPawnStory(Pawn pawn, HumanData humanData)
        {
            try
            {
                if (humanData.childhoodStory != "null")
                {
                    pawn.story.Childhood = DefDatabase<BackstoryDef>.AllDefs.ToList().Find(x => x.defName == humanData.childhoodStory);
                }

                if (humanData.adulthoodStory != "null")
                {
                    pawn.story.Adulthood = DefDatabase<BackstoryDef>.AllDefs.ToList().Find(x => x.defName == humanData.adulthoodStory);
                }
            }
            catch { Log.Warning($"Failed to set stories in human {humanData.name}"); }
        }

        private static void SetPawnSkills(Pawn pawn, HumanData humanData)
        {
            if (humanData.skillDefNames.Count() > 0)
            {
                for (int i = 0; i < humanData.skillDefNames.Count(); i++)
                {
                    try
                    {
                        pawn.skills.skills[i].levelInt = int.Parse(humanData.skillLevels[i]);

                        Enum.TryParse(humanData.passions[i], true, out Passion passion);
                        pawn.skills.skills[i].passion = passion;
                    }
                    catch { Log.Warning($"Failed to set skill {humanData.skillDefNames[i]} to human {humanData.name}"); }
                }
            }
        }

        private static void SetPawnTraits(Pawn pawn, HumanData humanData)
        {
            try { pawn.story.traits.allTraits.Clear(); }
            catch { Log.Warning($"Failed to remove traits of human {humanData.name}"); }

            if (humanData.traitDefNames.Count() > 0)
            {
                for (int i = 0; i < humanData.traitDefNames.Count(); i++)
                {
                    try
                    {
                        TraitDef traitDef = DefDatabase<TraitDef>.AllDefs.ToList().Find(x => x.defName == humanData.traitDefNames[i]);
                        Trait trait = new Trait(traitDef, int.Parse(humanData.traitDegrees[i]));
                        pawn.story.traits.GainTrait(trait);
                    }
                    catch { Log.Warning($"Failed to set trait {humanData.traitDefNames[i]} to human {humanData.name}"); }
                }
            }
        }

        private static void SetPawnApparel(Pawn pawn, HumanData humanData)
        {
            try
            {
                pawn.apparel.DestroyAll();
                pawn.apparel.DropAllOrMoveAllToInventory();
            }
            catch { Log.Warning($"Failed to destroy apparel in human {humanData.name}"); }

            if (humanData.equippedApparel.Count() > 0)
            {
                for (int i = 0; i < humanData.equippedApparel.Count(); i++)
                {
                    try
                    {
                        Apparel apparel = (Apparel)ThingScribeManager.StringToItem(humanData.equippedApparel[i]);
                        if (humanData.apparelWornByCorpse[i]) apparel.WornByCorpse.MustBeTrue();
                        else apparel.WornByCorpse.MustBeFalse();

                        pawn.apparel.Wear(apparel);
                    }
                    catch { Log.Warning($"Failed to set apparel in human {humanData.name}"); }
                }
            }
        }

        private static void SetPawnEquipment(Pawn pawn, HumanData humanData)
        {
            try { pawn.equipment.DestroyAllEquipment(); }
            catch { Log.Warning($"Failed to destroy equipment in human {humanData.name}"); }

            if (humanData.equippedWeapon != null)
            {
                try
                {
                    ThingWithComps thing = (ThingWithComps)ThingScribeManager.StringToItem(humanData.equippedWeapon);
                    pawn.equipment.AddEquipment(thing);
                }
                catch { Log.Warning($"Failed to set weapon in human {humanData.name}"); }
            }
        }

        private static void SetPawnInventory(Pawn pawn, HumanData humanData)
        {
            if (humanData.inventoryItems.Count() > 0)
            {
                foreach (ItemData item in humanData.inventoryItems)
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

        private static void SetPawnPosition(Pawn pawn, HumanData humanData)
        {
            if (humanData.position != null)
            {
                try
                {
                    pawn.Position = new IntVec3(int.Parse(humanData.position[0]), int.Parse(humanData.position[1]),
                        int.Parse(humanData.position[2]));
                }
                catch { Log.Message($"Failed to set position in human {pawn.Label}"); }
            }
        }

        private static void SetPawnRotation(Pawn pawn, HumanData humanData)
        {
            try { pawn.Rotation = new Rot4(humanData.rotation); }
            catch { Log.Message($"Failed to set rotation in human {pawn.Label}"); }
        }
    }

    //Class that handles transformation of animals

    public static class AnimalScribeManager
    {
        //Functions

        public static Pawn[] GetAnimalsFromString(TransferData transferData)
        {
            List<Pawn> animals = new List<Pawn>();

            for (int i = 0; i < transferData.animalDatas.Count(); i++)
            {
                AnimalData animalDetails = (AnimalData)Serializer.ConvertBytesToObject(transferData.animalDatas[i]);

                animals.Add(StringToAnimal(animalDetails));
            }

            return animals.ToArray();
        }

        public static AnimalData AnimalToString(Pawn animal)
        {
            AnimalData animalData = new AnimalData();

            GetAnimalBioDetails(animal, animalData);

            GetAnimalKind(animal, animalData);

            GetAnimalFaction(animal, animalData);

            GetAnimalHediffs(animal, animalData);

            GetAnimalSkills(animal, animalData);

            GetAnimalPosition(animal, animalData);

            GetAnimalRotation(animal, animalData);

            return animalData;
        }

        public static Pawn StringToAnimal(AnimalData animalData)
        {
            PawnKindDef kind = SetAnimalKind(animalData);

            Faction faction = SetAnimalFaction(animalData);

            Pawn animal = SetAnimal(kind, faction, animalData);

            SetAnimalBioDetails(animal, animalData);

            SetAnimalHediffs(animal, animalData);

            SetAnimalSkills(animal, animalData);

            SetAnimalPosition(animal, animalData);

            SetAnimalRotation(animal, animalData);

            return animal;
        }

        //Getters

        private static void GetAnimalBioDetails(Pawn animal, AnimalData animalData)
        {
            try
            {
                animalData.defName = animal.def.defName;
                animalData.name = animal.LabelShortCap.ToString();
                animalData.biologicalAge = animal.ageTracker.AgeBiologicalTicks.ToString();
                animalData.chronologicalAge = animal.ageTracker.AgeChronologicalTicks.ToString();
                animalData.gender = animal.gender.ToString();
            }
            catch { Log.Warning($"Failed to get biodetails of animal {animal.def.defName}"); }
        }

        private static void GetAnimalKind(Pawn animal, AnimalData animalData)
        {
            try { animalData.kindDef = animal.kindDef.defName; }
            catch { Log.Warning($"Failed to get kind from human {animal.Label}"); }
        }

        private static void GetAnimalFaction(Pawn animal, AnimalData animalData)
        {
            if (animal.Faction == null) return;

            try { animalData.factionDef = animal.Faction.def.defName; }
            catch { Log.Warning($"Failed to get faction from animal {animal.def.defName}"); }
        }

        private static void GetAnimalHediffs(Pawn animal, AnimalData animalData)
        {
            if (animal.health.hediffSet.hediffs.Count() > 0)
            {
                foreach (Hediff hd in animal.health.hediffSet.hediffs)
                {
                    try
                    {
                        animalData.hediffDefNames.Add(hd.def.defName);

                        if (hd.Part != null) animalData.hediffPartDefName.Add(hd.Part.def.defName.ToString());
                        else animalData.hediffPartDefName.Add("null");

                        animalData.hediffSeverity.Add(hd.Severity.ToString());
                        animalData.heddifPermanent.Add(hd.IsPermanent());
                    }
                    catch { Log.Warning($"Failed to get headdifs from animal {animal.def.defName}"); }
                }
            }
        }

        private static void GetAnimalSkills(Pawn animal, AnimalData animalData)
        {
            if (animal.training == null) return;

            foreach (TrainableDef trainable in DefDatabase<TrainableDef>.AllDefsListForReading)
            {
                try
                {
                    animalData.trainableDefNames.Add(trainable.defName);
                    animalData.canTrain.Add(animal.training.CanAssignToTrain(trainable).Accepted);
                    animalData.hasLearned.Add(animal.training.HasLearned(trainable));
                    animalData.isDisabled.Add(animal.training.GetWanted(trainable));
                }
                catch { Log.Warning($"Failed to get skills of animal {animal.def.defName}"); }
            }
        }

        private static void GetAnimalPosition(Pawn animal, AnimalData animalData)
        {
            try
            {
                animalData.position = new string[] { animal.Position.x.ToString(),
                        animal.Position.y.ToString(), animal.Position.z.ToString() };
            }
            catch { Log.Message($"Failed to get position of animal {animal.def.defName}"); }
        }

        private static void GetAnimalRotation(Pawn animal, AnimalData animalData)
        {
            try { animalData.rotation = animal.Rotation.AsInt; }
            catch { Log.Message($"Failed to get rotation of animal {animal.def.defName}"); }
        }

        //Setters

        private static PawnKindDef SetAnimalKind(AnimalData animalData)
        {
            try { return DefDatabase<PawnKindDef>.AllDefs.First(fetch => fetch.defName == animalData.defName); }
            catch { Log.Warning($"Failed to set kind in animal {animalData.name}"); }

            return null;
        }

        private static Faction SetAnimalFaction(AnimalData animalData)
        {
            if (animalData.factionDef == null) return null;

            try { return Find.FactionManager.AllFactions.First(fetch => fetch.def.defName == animalData.factionDef); }
            catch { Log.Warning($"Failed to set faction in animal {animalData.name}"); }

            return null;
        }

        private static Pawn SetAnimal(PawnKindDef kind, Faction faction, AnimalData animalData)
        {
            try { return PawnGenerator.GeneratePawn(kind, faction); }
            catch { Log.Warning($"Failed to set animal {animalData.name}"); }

            return null;
        }

        private static void SetAnimalBioDetails(Pawn animal, AnimalData animalData)
        {
            try
            {
                animal.Name = new NameSingle(animalData.name);
                animal.ageTracker.AgeBiologicalTicks = long.Parse(animalData.biologicalAge);
                animal.ageTracker.AgeChronologicalTicks = long.Parse(animalData.chronologicalAge);

                Enum.TryParse(animalData.gender, true, out Gender animalGender);
                animal.gender = animalGender;
            }
            catch { Log.Warning($"Failed to set biodetails of animal {animalData.name}"); }
        }

        private static void SetAnimalHediffs(Pawn animal, AnimalData animalData)
        {
            try
            {
                animal.health.RemoveAllHediffs();
                animal.health.Reset();
            }
            catch { Log.Warning($"Failed to remove heddifs of animal {animalData.name}"); }

            if (animalData.hediffDefNames.Count() > 0)
            {
                for (int i = 0; i < animalData.hediffDefNames.Count(); i++)
                {
                    try
                    {
                        HediffDef hediffDef = DefDatabase<HediffDef>.AllDefs.ToList().Find(x => x.defName == animalData.hediffDefNames[i]);
                        BodyPartRecord bodyPart = null;

                        if (animalData.hediffPartDefName[i] != "null")
                        {
                            bodyPart = animal.RaceProps.body.AllParts.ToList().Find(x =>
                                x.def.defName == animalData.hediffPartDefName[i]);
                        }

                        Hediff hediff = HediffMaker.MakeHediff(hediffDef, animal, bodyPart);
                        hediff.Severity = float.Parse(animalData.hediffSeverity[i]);

                        if (animalData.heddifPermanent[i])
                        {
                            HediffComp_GetsPermanent hediffComp = hediff.TryGetComp<HediffComp_GetsPermanent>();
                            hediffComp.IsPermanent = true;
                        }

                        animal.health.AddHediff(hediff);
                    }
                    catch { Log.Warning($"Failed to set headiffs in animal {animalData.defName}"); }
                }
            }
        }

        private static void SetAnimalSkills(Pawn animal, AnimalData animalData)
        {
            if (animalData.trainableDefNames.Count() > 0)
            {
                for (int i = 0; i < animalData.trainableDefNames.Count(); i++)
                {
                    try
                    {
                        TrainableDef trainable = DefDatabase<TrainableDef>.AllDefs.ToList().Find(x => x.defName == animalData.trainableDefNames[i]);
                        if (animalData.canTrain[i]) animal.training.Train(trainable, null, complete: animalData.hasLearned[i]);
                    }
                    catch { Log.Warning($"Failed to set skills of animal {animalData.name}"); }
                }
            }
        }

        private static void SetAnimalPosition(Pawn animal, AnimalData animalData)
        {
            if (animal.Position != null)
            {
                try
                {
                    animal.Position = new IntVec3(int.Parse(animalData.position[0]), int.Parse(animalData.position[1]),
                        int.Parse(animalData.position[2]));
                }
                catch { Log.Warning($"Failed to set position of animal {animalData.name}"); }
            }
        }

        private static void SetAnimalRotation(Pawn animal, AnimalData animalData)
        {
            try { animal.Rotation = new Rot4(animalData.rotation); }
            catch { Log.Message($"Failed to set rotation of animal {animalData.name}"); }
        }
    }

    //Class that handles transformation of things

    public static class ThingScribeManager
    {
        //Functions

        public static Thing[] GetItemsFromString(TransferData transferData)
        {
            List<Thing> things = new List<Thing>();

            for (int i = 0; i < transferData.itemDatas.Count(); i++)
            {
                ItemData itemData = (ItemData)Serializer.ConvertBytesToObject(transferData.itemDatas[i]);

                things.Add(StringToItem(itemData));
            }

            return things.ToArray();
        }

        public static ItemData ItemToString(Thing thing, int thingCount)
        {
            ItemData itemData = new ItemData();

            Thing toUse = null;
            if (GetItemMinified(thing, itemData)) toUse = thing.GetInnerIfMinified();
            else toUse = thing;

            GetItemName(toUse, itemData);

            GetItemMaterial(toUse, itemData);

            GetItemQuantity(toUse, itemData, thingCount);

            GetItemQuality(toUse, itemData);

            GetItemHitpoints(toUse, itemData);

            GetItemPosition(toUse, itemData);

            GetItemRotation(toUse, itemData);

            return itemData;
        }

        public static Thing StringToItem(ItemData itemData)
        {
            Thing thing = SetItem(itemData);

            SetItemQuantity(thing, itemData);

            SetItemQuality(thing, itemData);

            SetItemHitpoints(thing, itemData);

            SetItemPosition(thing, itemData);

            SetItemRotation(thing, itemData);

            SetItemMinified(thing, itemData);

            return thing;
        }

        //Getters

        private static void GetItemName(Thing thing, ItemData itemData)
        {
            try { itemData.defName = thing.def.defName; }
            catch { Log.Warning($"Failed to get name of thing {thing.def.defName}"); }
        }

        private static void GetItemMaterial(Thing thing, ItemData itemData)
        {
            try 
            {
                if (TransferManagerHelper.CheckIfThingHasMaterial(thing)) itemData.materialDefName = thing.Stuff.defName;
                else itemData.materialDefName = null;
            }
            catch { Log.Warning($"Failed to get material of thing {thing.def.defName}"); }
        }

        private static void GetItemQuantity(Thing thing, ItemData itemData, int thingCount)
        {
            try { itemData.quantity = thingCount; }
            catch { Log.Warning($"Failed to get quantity of thing {thing.def.defName}"); }
        }

        private static void GetItemQuality(Thing thing, ItemData itemData)
        {
            try { itemData.quality = TransferManagerHelper.GetThingQuality(thing); }
            catch { Log.Warning($"Failed to get quality of thing {thing.def.defName}"); }
        }

        private static void GetItemHitpoints(Thing thing, ItemData itemData)
        {
            try { itemData.hitpoints = thing.HitPoints; }
            catch { Log.Warning($"Failed to get hitpoints of thing {thing.def.defName}"); }
        }

        private static void GetItemPosition(Thing thing, ItemData itemData)
        {
            try
            {
                itemData.position = new string[] { thing.Position.x.ToString(),
                    thing.Position.y.ToString(), thing.Position.z.ToString() };
            }
            catch { Log.Warning($"Failed to get position of thing {thing.def.defName}"); }
        }

        private static void GetItemRotation(Thing thing, ItemData itemData)
        {
            try { itemData.rotation = thing.Rotation.AsInt; }
            catch { Log.Warning($"Failed to get rotation of thing {thing.def.defName}"); }
        }

        private static bool GetItemMinified(Thing thing, ItemData itemData)
        {
            try 
            {
                itemData.isMinified = TransferManagerHelper.CheckIfThingIsMinified(thing);
                return itemData.isMinified;
            }
            catch { Log.Warning($"Failed to get minified of thing {thing.def.defName}"); }

            return false;
        }

        //Setters

        private static Thing SetItem(ItemData itemData)
        {
            try
            {
                ThingDef thingDef = DefDatabase<ThingDef>.AllDefs.ToList().Find(x => x.defName == itemData.defName);
                ThingDef defMaterial = DefDatabase<ThingDef>.AllDefs.ToList().Find(x => x.defName == itemData.materialDefName);
                return ThingMaker.MakeThing(thingDef, defMaterial);
            }
            catch { Log.Warning($"Failed to set item for {itemData.defName}"); }

            return null;
        }

        private static void SetItemQuantity(Thing thing, ItemData itemData)
        {
            try { thing.stackCount = itemData.quantity; }
            catch { Log.Warning($"Failed to set item quantity for {itemData.defName}"); }
        }

        private static void SetItemQuality(Thing thing, ItemData itemData)
        {
            if (itemData.quality != "null")
            {
                try
                {
                    CompQuality compQuality = thing.TryGetComp<CompQuality>();
                    if (compQuality != null)
                    {
                        QualityCategory iCategory = (QualityCategory)int.Parse(itemData.quality);
                        compQuality.SetQuality(iCategory, ArtGenerationContext.Outsider);
                    }
                }
                catch { Log.Warning($"Failed to set item quality for {itemData.defName}"); }
            }
        }

        private static void SetItemHitpoints(Thing thing, ItemData itemData)
        {
            try { thing.HitPoints = itemData.hitpoints; }
            catch { Log.Warning($"Failed to set item hitpoints for {itemData.defName}"); }
        }

        private static void SetItemPosition(Thing thing, ItemData itemData)
        {
            if (itemData.position != null)
            {
                try
                {
                    thing.Position = new IntVec3(int.Parse(itemData.position[0]), int.Parse(itemData.position[1]),
                        int.Parse(itemData.position[2]));
                }
                catch { Log.Warning($"Failed to set position for item {itemData.defName}"); }
            }
        }

        private static void SetItemRotation(Thing thing, ItemData itemData)
        {
            try { thing.Rotation = new Rot4(itemData.rotation); }
            catch { Log.Warning($"Failed to set rotation for item {itemData.defName}"); }
        }

        private static void SetItemMinified(Thing thing, ItemData itemData)
        {
            if (itemData.isMinified)
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

        public static MapData MapToString(Map map, bool containsItems, bool containsHumans, bool containsAnimals)
        {
            MapData mapData = new MapData();

            GetMapTile(mapData, map);

            GetMapSize(mapData, map);

            GetMapThings(mapData, map, containsItems, containsHumans, containsAnimals);

            return mapData;
        }

        public static Map StringToMap(MapData mapData, bool containsItems, bool containsHumans, bool containsAnimals, bool lessLoot)
        {
            Map map = SetEmptyMap(mapData);

            SetMapThings(mapData, map, containsItems, lessLoot);

            if (containsHumans) SetMapHumans(mapData, map);

            if (containsAnimals) SetMapAnimals(mapData, map);

            SetMapTerrain(mapData, map);

            SetMapFog(map);

            SetMapRoof(map);

            return map;
        }

        //Getters

        private static void GetMapTile(MapData mapData, Map map)
        {
            mapData.mapTile = map.Tile.ToString();
        }

        private static void GetMapSize(MapData mapData, Map map)
        {
            mapData.mapSize = $"{map.Size.x}|{map.Size.y}|{map.Size.z}";
        }

        private static void GetMapThings(MapData mapData, Map map, bool containsItems, bool containsHumans, bool containsAnimals)
        {
            for (int z = 0; z < map.Size.z; ++z)
            {
                for (int x = 0; x < map.Size.x; ++x)
                {
                    IntVec3 vectorToCheck = new IntVec3(x, map.Size.y, z);

                    mapData.tileDefNames.Add(map.terrainGrid.TerrainAt(vectorToCheck).defName.ToString());

                    if (map.roofGrid.RoofAt(vectorToCheck) == null) mapData.roofDefNames.Add("null");
                    else mapData.roofDefNames.Add(map.roofGrid.RoofAt(vectorToCheck).defName.ToString());
                }
            }

            foreach (Thing thing in map.listerThings.AllThings)
            {
                if (TransferManagerHelper.CheckIfThingIsHuman(thing))
                {
                    if (containsHumans)
                    {
                        HumanData humanData = HumanScribeManager.HumanToString(thing as Pawn);
                        if (thing.Faction == Faction.OfPlayer) mapData.factionHumans.Add(humanData);
                        else mapData.nonFactionHumans.Add(humanData);
                    }
                }

                else if (TransferManagerHelper.CheckIfThingIsAnimal(thing))
                {
                    if (containsAnimals)
                    {
                        AnimalData animalData = AnimalScribeManager.AnimalToString(thing as Pawn);
                        if (thing.Faction == Faction.OfPlayer) mapData.factionAnimals.Add(animalData);
                        else mapData.nonFactionAnimals.Add(animalData);
                    }
                }

                else
                {
                    ItemData itemData = ThingScribeManager.ItemToString(thing, thing.stackCount);

                    if (thing.def.alwaysHaulable)
                    {
                        if (containsItems) mapData.factionThings.Add(itemData);
                        else continue;
                    }
                    else mapData.nonFactionThings.Add(itemData);
                }
            }
        }

        //Setters

        private static Map SetEmptyMap(MapData mapData)
        {
            string[] splitSize = mapData.mapSize.Split('|');

            IntVec3 mapSize = new IntVec3(int.Parse(splitSize[0]), int.Parse(splitSize[1]),
                int.Parse(splitSize[2]));

            PlanetManagerHelper.SetOverrideGenerators();
            Map toReturn = GetOrGenerateMapUtility.GetOrGenerateMap(ClientValues.chosenSettlement.Tile, mapSize, null);
            PlanetManagerHelper.SetDefaultGenerators();
            return toReturn;
        }

        private static void SetMapThings(MapData mapData, Map map, bool containsItems, bool lessLoot)
        {
            List<Thing> thingsToGetInThisTile = new List<Thing>();

            foreach (ItemData item in mapData.nonFactionThings)
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

                foreach (ItemData item in mapData.factionThings)
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

        private static void SetMapHumans(MapData mapData, Map map)
        {
            foreach (HumanData pawn in mapData.nonFactionHumans)
            {
                try
                {
                    Pawn human = HumanScribeManager.StringToHuman(pawn);
                    GenSpawn.Spawn(human, human.Position, map, human.Rotation);
                }
                catch { Log.Warning($"Failed to spawn human {pawn.name}"); }
            }

            foreach (HumanData pawn in mapData.factionHumans)
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

        private static void SetMapAnimals(MapData mapData, Map map)
        {
            foreach (AnimalData pawn in mapData.nonFactionAnimals)
            {
                try
                {
                    Pawn animal = AnimalScribeManager.StringToAnimal(pawn);
                    GenSpawn.Spawn(animal, animal.Position, map, animal.Rotation);
                }
                catch { Log.Warning($"Failed to spawn animal {pawn.name}"); }
            }

            foreach (AnimalData pawn in mapData.factionAnimals)
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

        private static void SetMapTerrain(MapData mapData, Map map)
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
                            mapData.tileDefNames[index]);

                        map.terrainGrid.SetTerrain(vectorToCheck, terrainToUse);

                    }
                    catch { Log.Warning($"Failed to set terrain at {vectorToCheck}"); }

                    try
                    {
                        RoofDef roofToUse = DefDatabase<RoofDef>.AllDefs.ToList().Find(fetch => fetch.defName ==
                                    mapData.roofDefNames[index]);

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
