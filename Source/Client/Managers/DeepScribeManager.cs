using System;
using System.Collections.Generic;
using System.Drawing;
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
                HumanData humanData = (HumanData)Serializer.ConvertBytesToObject(transferData.humanDatas[i]);

                humans.Add(StringToHuman(humanData));
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
                GetPawnChildState(pawn, humanData);

                GetPawnXenotype(pawn, humanData);

                GetPawnXenogenes(pawn, humanData);

                GetPawnEndogenes(pawn, humanData);
            }

            GetPawnStory(pawn, humanData);

            GetPawnSkills(pawn, humanData);

            GetPawnTraits(pawn, humanData);

            GetPawnApparel(pawn, humanData);

            GetPawnEquipment(pawn, humanData);

            if (passInventory) GetPawnInventory(pawn, humanData);

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
                SetPawnChildState(pawn, humanData);

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
            catch { Logger.Warning($"Failed to get biological details from human {pawn.Label}"); }
        }

        private static void GetPawnKind(Pawn pawn, HumanData humanData)
        {
            try { humanData.kindDef = pawn.kindDef.defName; }
            catch { Logger.Warning($"Failed to get kind from human {pawn.Label}"); }
        }

        private static void GetPawnFaction(Pawn pawn, HumanData humanData)
        {
            if (pawn.Faction == null) return;

            try { humanData.factionDef = pawn.Faction.def.defName; }
            catch { Logger.Warning($"Failed to get faction from human {pawn.Label}"); }
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
                    catch { Logger.Warning($"Failed to get heddif {hd} from human {pawn.Label}"); }
                }
            }
        }

        private static void GetPawnChildState(Pawn pawn, HumanData humanData)
        {
            try { humanData.growthPoints = pawn.ageTracker.growthPoints; }
            catch { Logger.Warning($"Failed to get child state from human {pawn.Label}"); }
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
            catch { Logger.Warning($"Failed to get xenotype from human {pawn.Label}"); }
        }

        private static void GetPawnXenogenes(Pawn pawn, HumanData humanData)
        {
            if (pawn.genes.Xenogenes.Count() > 0)
            {
                foreach (Gene gene in pawn.genes.Xenogenes)
                {
                    try { humanData.xenogeneDefNames.Add(gene.def.defName); }
                    catch { Logger.Warning($"Failed to get gene {gene} from human {pawn.Label}"); }
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
                    catch { Logger.Warning($"Failed to get endogene {gene} from human {pawn.Label}"); }
                }
            }
        }

        private static void GetPawnFavoriteColor(Pawn pawn, HumanData humanData)
        {
            try { humanData.favoriteColor = pawn.story.favoriteColor.ToString(); }
            catch { Logger.Warning($"Failed to get favorite color from human {pawn.Label}"); }
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
            catch { Logger.Warning($"Failed to get backstories from human {pawn.Label}"); }
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
                    catch { Logger.Warning($"Failed to get skill {skill} from human {pawn.Label}"); }
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
                    catch { Logger.Warning($"Failed to get trait {trait} from human {pawn.Label}"); }
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
                    catch { Logger.Warning($"Failed to get apparel {ap} from human {pawn.Label}"); }
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
                    ItemData itemData = ThingScribeManager.ItemToString(weapon, weapon.stackCount);
                    humanData.equippedWeapon = itemData;
                }
                catch { Logger.Warning($"Failed to get weapon from human {pawn.Label}"); }
            }
        }

        private static void GetPawnInventory(Pawn pawn, HumanData humanData)
        {
            if (pawn.inventory.innerContainer.Count() != 0)
            {
                foreach (Thing thing in pawn.inventory.innerContainer)
                {
                    try
                    {
                        ItemData itemData = ThingScribeManager.ItemToString(thing, thing.stackCount);
                        humanData.inventoryItems.Add(itemData);
                    }
                    catch { Logger.Warning($"Failed to get item from human {pawn.Label}"); }
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
            catch { Logger.Message("Failed to get human position"); }
        }

        private static void GetPawnRotation(Pawn pawn, HumanData humanData)
        {
            try { humanData.rotation = pawn.Rotation.AsInt; }
            catch { Logger.Message("Failed to get human rotation"); }
        }

        //Setters

        private static PawnKindDef SetPawnKind(HumanData humanData)
        {
            try { return DefDatabase<PawnKindDef>.AllDefs.First(fetch => fetch.defName == humanData.kindDef); }
            catch { Logger.Warning($"Failed to set kind in human {humanData.name}"); }

            return null;
        }

        private static Faction SetPawnFaction(HumanData humanData)
        {
            if (humanData.factionDef == null) return null;

            try { return Find.FactionManager.AllFactions.First(fetch => fetch.def.defName == humanData.factionDef); }
            catch { Logger.Warning($"Failed to set faction in human {humanData.name}"); }

            return null;
        }

        private static Pawn SetPawn(PawnKindDef kind, Faction faction, HumanData humanData)
        {
            try { return PawnGenerator.GeneratePawn(kind, faction); }
            catch { Logger.Warning($"Failed to set biological details in human {humanData.name}"); }

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
            catch { Logger.Warning($"Failed to set biological details in human {humanData.name}"); }
        }

        private static void SetPawnHediffs(Pawn pawn, HumanData humanData)
        {
            try
            {
                pawn.health.RemoveAllHediffs();
                pawn.health.Reset();
            }
            catch { Logger.Warning($"Failed to remove heddifs of human {humanData.name}"); }

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
                    catch { Logger.Warning($"Failed to set heddif in {humanData.hediffPartDefName[i]} to human {humanData.name}"); }
                }
            }
        }

        private static void SetPawnChildState(Pawn pawn, HumanData humanData)
        {
            try { pawn.ageTracker.growthPoints = humanData.growthPoints; }
            catch { Logger.Warning($"Failed to set child state in human {pawn.Label}"); }
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
            catch { Logger.Warning($"Failed to set xenotypes in human {humanData.name}"); }
        }

        private static void SetPawnXenogenes(Pawn pawn, HumanData humanData)
        {
            try { pawn.genes.Xenogenes.Clear(); }
            catch { Logger.Warning($"Failed to clear xenogenes for human {humanData.name}"); }

            if (humanData.xenogeneDefNames.Count() > 0)
            {
                foreach (string str in humanData.xenogeneDefNames)
                {
                    try
                    {
                        GeneDef def = DefDatabase<GeneDef>.AllDefs.First(fetch => fetch.defName == str);
                        pawn.genes.AddGene(def, true);
                    }
                    catch { Logger.Warning($"Failed to set xenogenes for human {humanData.name}"); }
                }
            }
        }

        private static void SetPawnEndogenes(Pawn pawn, HumanData humanData)
        {
            try { pawn.genes.Endogenes.Clear(); }
            catch { Logger.Warning($"Failed to clear endogenes for human {humanData.name}"); }

            if (humanData.endogeneDefNames.Count() > 0)
            {
                foreach (string str in humanData.endogeneDefNames)
                {
                    try
                    {
                        GeneDef def = DefDatabase<GeneDef>.AllDefs.First(fetch => fetch.defName == str);
                        pawn.genes.AddGene(def, false);
                    }
                    catch { Logger.Warning($"Failed to set endogenes for human {humanData.name}"); }
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
            catch { Logger.Warning($"Failed to set colors in human {humanData.name}"); }
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
            catch { Logger.Warning($"Failed to set stories in human {humanData.name}"); }
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
                    catch { Logger.Warning($"Failed to set skill {humanData.skillDefNames[i]} to human {humanData.name}"); }
                }
            }
        }

        private static void SetPawnTraits(Pawn pawn, HumanData humanData)
        {
            try { pawn.story.traits.allTraits.Clear(); }
            catch { Logger.Warning($"Failed to remove traits of human {humanData.name}"); }

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
                    catch { Logger.Warning($"Failed to set trait {humanData.traitDefNames[i]} to human {humanData.name}"); }
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
            catch { Logger.Warning($"Failed to destroy apparel in human {humanData.name}"); }

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
                    catch { Logger.Warning($"Failed to set apparel in human {humanData.name}"); }
                }
            }
        }

        private static void SetPawnEquipment(Pawn pawn, HumanData humanData)
        {
            try { pawn.equipment.DestroyAllEquipment(); }
            catch { Logger.Warning($"Failed to destroy equipment in human {humanData.name}"); }

            if (humanData.equippedWeapon != null)
            {
                try
                {
                    ThingWithComps thing = (ThingWithComps)ThingScribeManager.StringToItem(humanData.equippedWeapon);
                    pawn.equipment.AddEquipment(thing);
                }
                catch { Logger.Warning($"Failed to set weapon in human {humanData.name}"); }
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
                    catch { Logger.Warning($"Failed to add thing to pawn {pawn.Label}"); }
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
                catch { Logger.Message($"Failed to set position in human {pawn.Label}"); }
            }
        }

        private static void SetPawnRotation(Pawn pawn, HumanData humanData)
        {
            try { pawn.Rotation = new Rot4(humanData.rotation); }
            catch { Logger.Message($"Failed to set rotation in human {pawn.Label}"); }
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
                AnimalData animalData = (AnimalData)Serializer.ConvertBytesToObject(transferData.animalDatas[i]);

                animals.Add(StringToAnimal(animalData));
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
            catch { Logger.Warning($"Failed to get biodetails of animal {animal.def.defName}"); }
        }

        private static void GetAnimalKind(Pawn animal, AnimalData animalData)
        {
            try { animalData.kindDef = animal.kindDef.defName; }
            catch { Logger.Warning($"Failed to get kind from human {animal.Label}"); }
        }

        private static void GetAnimalFaction(Pawn animal, AnimalData animalData)
        {
            if (animal.Faction == null) return;

            try { animalData.factionDef = animal.Faction.def.defName; }
            catch { Logger.Warning($"Failed to get faction from animal {animal.def.defName}"); }
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
                    catch { Logger.Warning($"Failed to get headdifs from animal {animal.def.defName}"); }
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
                catch { Logger.Warning($"Failed to get skills of animal {animal.def.defName}"); }
            }
        }

        private static void GetAnimalPosition(Pawn animal, AnimalData animalData)
        {
            try
            {
                animalData.position = new string[] { animal.Position.x.ToString(),
                        animal.Position.y.ToString(), animal.Position.z.ToString() };
            }
            catch { Logger.Message($"Failed to get position of animal {animal.def.defName}"); }
        }

        private static void GetAnimalRotation(Pawn animal, AnimalData animalData)
        {
            try { animalData.rotation = animal.Rotation.AsInt; }
            catch { Logger.Message($"Failed to get rotation of animal {animal.def.defName}"); }
        }

        //Setters

        private static PawnKindDef SetAnimalKind(AnimalData animalData)
        {
            try { return DefDatabase<PawnKindDef>.AllDefs.First(fetch => fetch.defName == animalData.defName); }
            catch { Logger.Warning($"Failed to set kind in animal {animalData.name}"); }

            return null;
        }

        private static Faction SetAnimalFaction(AnimalData animalData)
        {
            if (animalData.factionDef == null) return null;

            try { return Find.FactionManager.AllFactions.First(fetch => fetch.def.defName == animalData.factionDef); }
            catch { Logger.Warning($"Failed to set faction in animal {animalData.name}"); }

            return null;
        }

        private static Pawn SetAnimal(PawnKindDef kind, Faction faction, AnimalData animalData)
        {
            try { return PawnGenerator.GeneratePawn(kind, faction); }
            catch { Logger.Warning($"Failed to set animal {animalData.name}"); }

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
            catch { Logger.Warning($"Failed to set biodetails of animal {animalData.name}"); }
        }

        private static void SetAnimalHediffs(Pawn animal, AnimalData animalData)
        {
            try
            {
                animal.health.RemoveAllHediffs();
                animal.health.Reset();
            }
            catch { Logger.Warning($"Failed to remove heddifs of animal {animalData.name}"); }

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
                    catch { Logger.Warning($"Failed to set headiffs in animal {animalData.defName}"); }
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
                    catch { Logger.Warning($"Failed to set skills of animal {animalData.name}"); }
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
                catch { Logger.Warning($"Failed to set position of animal {animalData.name}"); }
            }
        }

        private static void SetAnimalRotation(Pawn animal, AnimalData animalData)
        {
            try { animal.Rotation = new Rot4(animalData.rotation); }
            catch { Logger.Message($"Failed to set rotation of animal {animalData.name}"); }
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

                Thing thingToAdd = StringToItem(itemData);
                if (thingToAdd != null) things.Add(thingToAdd);
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
            catch { Logger.Warning($"Failed to get name of thing {thing.def.defName}"); }
        }

        private static void GetItemMaterial(Thing thing, ItemData itemData)
        {
            try 
            {
                if (DeepScribeHelper.CheckIfThingHasMaterial(thing)) itemData.materialDefName = thing.Stuff.defName;
                else itemData.materialDefName = null;
            }
            catch { Logger.Warning($"Failed to get material of thing {thing.def.defName}"); }
        }

        private static void GetItemQuantity(Thing thing, ItemData itemData, int thingCount)
        {
            try { itemData.quantity = thingCount; }
            catch { Logger.Warning($"Failed to get quantity of thing {thing.def.defName}"); }
        }

        private static void GetItemQuality(Thing thing, ItemData itemData)
        {
            try { itemData.quality = DeepScribeHelper.GetThingQuality(thing); }
            catch { Logger.Warning($"Failed to get quality of thing {thing.def.defName}"); }
        }

        private static void GetItemHitpoints(Thing thing, ItemData itemData)
        {
            try { itemData.hitpoints = thing.HitPoints; }
            catch { Logger.Warning($"Failed to get hitpoints of thing {thing.def.defName}"); }
        }

        private static void GetItemPosition(Thing thing, ItemData itemData)
        {
            try
            {
                itemData.position = new string[] { thing.Position.x.ToString(),
                    thing.Position.y.ToString(), thing.Position.z.ToString() };
            }
            catch { Logger.Warning($"Failed to get position of thing {thing.def.defName}"); }
        }

        private static void GetItemRotation(Thing thing, ItemData itemData)
        {
            try { itemData.rotation = thing.Rotation.AsInt; }
            catch { Logger.Warning($"Failed to get rotation of thing {thing.def.defName}"); }
        }

        private static bool GetItemMinified(Thing thing, ItemData itemData)
        {
            try 
            {
                itemData.isMinified = DeepScribeHelper.CheckIfThingIsMinified(thing);
                return itemData.isMinified;
            }
            catch { Logger.Warning($"Failed to get minified of thing {thing.def.defName}"); }

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
            catch { Logger.Warning($"Failed to set item for {itemData.defName}"); }

            return null;
        }

        private static void SetItemQuantity(Thing thing, ItemData itemData)
        {
            try { thing.stackCount = itemData.quantity; }
            catch { Logger.Warning($"Failed to set item quantity for {itemData.defName}"); }
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
                catch { Logger.Warning($"Failed to set item quality for {itemData.defName}"); }
            }
        }

        private static void SetItemHitpoints(Thing thing, ItemData itemData)
        {
            try { thing.HitPoints = itemData.hitpoints; }
            catch { Logger.Warning($"Failed to set item hitpoints for {itemData.defName}"); }
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
                catch { Logger.Warning($"Failed to set position for item {itemData.defName}"); }
            }
        }

        private static void SetItemRotation(Thing thing, ItemData itemData)
        {
            try { thing.Rotation = new Rot4(itemData.rotation); }
            catch { Logger.Warning($"Failed to set rotation for item {itemData.defName}"); }
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

        public static MapData MapToString(Map map, bool factionThings, bool nonFactionThings, bool factionHumans, bool nonFactionHumans, bool factionAnimals, bool nonFactionAnimals)
        {
            MapData mapData = new MapData();

            GetMapTile(mapData, map);

            GetMapSize(mapData, map);

            GetMapTerrain(mapData, map);

            GetMapThings(mapData, map, factionThings, nonFactionThings);

            GetMapHumans(mapData, map, factionHumans, nonFactionHumans);

            GetMapAnimals(mapData, map, factionAnimals, nonFactionAnimals);

            return mapData;
        }

        public static Map StringToMap(MapData mapData, bool factionThings, bool nonFactionThings, bool factionHumans, bool nonFactionHumans, bool factionAnimals, bool nonFactionAnimals, bool lessLoot = false)
        {
            Map map = SetEmptyMap(mapData);

            SetMapTerrain(mapData, map);

            if (factionThings || nonFactionThings) SetMapThings(mapData, map, factionThings, nonFactionThings, lessLoot);

            if (factionHumans || nonFactionHumans) SetMapHumans(mapData, map, factionHumans, nonFactionHumans);

            if (factionAnimals || nonFactionAnimals) SetMapAnimals(mapData, map, factionAnimals, nonFactionAnimals);

            UnfogMap(map);

            ResetMapRoofs(map);

            return map;
        }

        //Getters

        private static void GetMapTile(MapData mapData, Map map)
        {
            mapData.mapTile = map.Tile;
        }

        private static void GetMapSize(MapData mapData, Map map)
        {
            mapData.mapSize = ValueParser.Vector3ToString(map.Size);
        }

        private static void GetMapTerrain(MapData mapData, Map map)
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
        }

        private static void GetMapThings(MapData mapData, Map map, bool factionThings, bool nonFactionThings)
        {
            foreach (Thing thing in map.listerThings.AllThings)
            {
                if (!DeepScribeHelper.CheckIfThingIsHuman(thing) && !DeepScribeHelper.CheckIfThingIsAnimal(thing))
                {
                    ItemData itemData = ThingScribeManager.ItemToString(thing, thing.stackCount);

                    if (thing.def.alwaysHaulable && factionThings) mapData.factionThings.Add(itemData);
                    else if (!thing.def.alwaysHaulable && nonFactionThings) mapData.nonFactionThings.Add(itemData);

                    if (DeepScribeHelper.CheckIfThingCanGrow(thing))
                    {
                        try
                        {
                            Plant plant = thing as Plant;
                            itemData.growthTicks = plant.Growth; 
                        }
                        catch { Logger.Warning($"Failed to parse plant {thing.def.defName}"); }
                    }
                }
            }
        }

        private static void GetMapHumans(MapData mapData, Map map, bool factionHumans, bool nonFactionHumans)
        {
            foreach (Thing thing in map.listerThings.AllThings)
            {
                if (DeepScribeHelper.CheckIfThingIsHuman(thing))
                {
                    HumanData humanData = HumanScribeManager.HumanToString(thing as Pawn);

                    if (thing.Faction == Faction.OfPlayer && factionHumans) mapData.factionHumans.Add(humanData);
                    else if (thing.Faction != Faction.OfPlayer && nonFactionHumans) mapData.nonFactionHumans.Add(humanData);
                }
            }
        }

        private static void GetMapAnimals(MapData mapData, Map map, bool factionAnimals, bool nonFactionAnimals)
        {
            foreach (Thing thing in map.listerThings.AllThings)
            {
                if (DeepScribeHelper.CheckIfThingIsAnimal(thing))
                {
                    AnimalData animalData = AnimalScribeManager.AnimalToString(thing as Pawn);

                    if (thing.Faction == Faction.OfPlayer && factionAnimals) mapData.factionAnimals.Add(animalData);
                    else if (thing.Faction != Faction.OfPlayer && nonFactionAnimals) mapData.nonFactionAnimals.Add(animalData);
                }
            }
        }

        //Setters

        private static Map SetEmptyMap(MapData mapData)
        {
            IntVec3 mapSize = ValueParser.StringToVector3(mapData.mapSize);

            PlanetManagerHelper.SetOverrideGenerators();
            Map toReturn = GetOrGenerateMapUtility.GetOrGenerateMap(ClientValues.chosenSettlement.Tile, mapSize, null);
            PlanetManagerHelper.SetDefaultGenerators();

            return toReturn;
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
                    catch { Logger.Warning($"Failed to set terrain at {vectorToCheck}"); }

                    try
                    {
                        RoofDef roofToUse = DefDatabase<RoofDef>.AllDefs.ToList().Find(fetch => fetch.defName ==
                                    mapData.roofDefNames[index]);

                        map.roofGrid.SetRoof(vectorToCheck, roofToUse);
                    }
                    catch { Logger.Warning($"Failed to set roof at {vectorToCheck}"); }

                    index++;
                }
            }
        }

        private static void SetMapThings(MapData mapData, Map map, bool factionThings, bool nonFactionThings, bool lessLoot)
        {
            List<Thing> thingsToGetInThisTile = new List<Thing>();

            if (factionThings)
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

                        if (DeepScribeHelper.CheckIfThingCanGrow(toGet))
                        {
                            Plant plant = toGet as Plant;
                            plant.Growth = item.growthTicks;
                        }
                    }
                    catch { Logger.Warning($"Failed to parse thing {item.defName}"); }
                }
            }

            if (nonFactionThings)
            {
                foreach (ItemData item in mapData.nonFactionThings)
                {
                    try
                    {
                        Thing toGet = ThingScribeManager.StringToItem(item);
                        thingsToGetInThisTile.Add(toGet);

                        if (DeepScribeHelper.CheckIfThingCanGrow(toGet))
                        {
                            Plant plant = toGet as Plant;
                            plant.Growth = item.growthTicks;
                        }
                    }
                    catch { Logger.Warning($"Failed to parse thing {item.defName}"); }
                }
            }

            foreach (Thing thing in thingsToGetInThisTile)
            {
                try { GenPlace.TryPlaceThing(thing, thing.Position, map, ThingPlaceMode.Direct, rot: thing.Rotation); }
                catch { Logger.Warning($"Failed to place thing {thing.def.defName} at {thing.Position}"); }
            }
        }

        private static void SetMapHumans(MapData mapData, Map map, bool factionHumans, bool nonFactionHumans)
        {
            if (factionHumans)
            {
                foreach (HumanData pawn in mapData.factionHumans)
                {
                    try
                    {
                        Pawn human = HumanScribeManager.StringToHuman(pawn);
                        human.SetFaction(FactionValues.neutralPlayer);

                        GenSpawn.Spawn(human, human.Position, map, human.Rotation);
                    }
                    catch { Logger.Warning($"Failed to spawn human {pawn.name}"); }
                }
            }

            if (nonFactionHumans)
            {
                foreach (HumanData pawn in mapData.nonFactionHumans)
                {
                    try
                    {
                        Pawn human = HumanScribeManager.StringToHuman(pawn);
                        GenSpawn.Spawn(human, human.Position, map, human.Rotation);
                    }
                    catch { Logger.Warning($"Failed to spawn human {pawn.name}"); }
                }
            }
        }

        private static void SetMapAnimals(MapData mapData, Map map, bool factionAnimals, bool nonFactionAnimals)
        {
            if (factionAnimals)
            {
                foreach (AnimalData pawn in mapData.factionAnimals)
                {
                    try
                    {
                        Pawn animal = AnimalScribeManager.StringToAnimal(pawn);
                        animal.SetFaction(FactionValues.neutralPlayer);

                        GenSpawn.Spawn(animal, animal.Position, map, animal.Rotation);
                    }
                    catch { Logger.Warning($"Failed to spawn animal {pawn.name}"); }
                }
            }

            if (nonFactionAnimals)
            {
                foreach (AnimalData pawn in mapData.nonFactionAnimals)
                {
                    try
                    {
                        Pawn animal = AnimalScribeManager.StringToAnimal(pawn);
                        GenSpawn.Spawn(animal, animal.Position, map, animal.Rotation);
                    }
                    catch { Logger.Warning($"Failed to spawn animal {pawn.name}"); }
                }
            }
        }

        //Misc

        private static void UnfogMap(Map map)
        {
            FloodFillerFog.FloodUnfog(MapGenerator.PlayerStartSpot, map);
        }

        private static void ResetMapRoofs(Map map)
        {
            map.roofCollapseBuffer.Clear();
            map.roofGrid.Drawer.SetDirty();
        }
    }

    //Class that contains helping functions for the deep scriber

    public static class DeepScribeHelper
    {
        //Checks if transferable thing is a human

        public static bool CheckIfThingIsHuman(Thing thing)
        {
            if (thing.def.defName == "Human") return true;
            else return false;
        }

        //Checks if transferable thing is an animal

        public static bool CheckIfThingIsAnimal(Thing thing)
        {
            PawnKindDef animal = DefDatabase<PawnKindDef>.AllDefs.ToList().Find(fetch => fetch.defName == thing.def.defName);
            if (animal != null) return true;
            else return false;
        }

        //Checks if transferable thing is an item that can have a growth state

        public static bool CheckIfThingCanGrow(Thing thing)
        {
            try
            {
                Plant plant = thing as Plant;
                _ = plant.Growth;
                return true;
            }
            catch { return false; }
        }

        //Checks if transferable thing has a material

        public static bool CheckIfThingHasMaterial(Thing thing)
        {
            if (thing.Stuff != null) return true;
            else return false;
        }

        //Gets the quality of a transferable thing

        public static string GetThingQuality(Thing thing)
        {
            QualityCategory qc = QualityCategory.Normal;
            thing.TryGetQuality(out qc);

            return ((int)qc).ToString();
        }

        //Checks if transferable thing is minified

        public static bool CheckIfThingIsMinified(Thing thing)
        {
            if (thing.def == ThingDefOf.MinifiedThing || thing.def == ThingDefOf.MinifiedTree) return true;
            else return false;
        }
    }


    //class that handles transformation of factions
    public static class FactionScribeManager
    {
        public static FactionData factionToFactionDetails(FactionDef faction)
        {
            FactionData factionData = new FactionData();
            factionData.defName = faction.defName;
            factionData.fixedName = faction.fixedName;
            factionData.autoFlee = faction.autoFlee;
            factionData.canSiege = faction.canSiege;
            factionData.canStageAttacks = faction.canStageAttacks;
            factionData.canUseAvoidGrid = faction.canUseAvoidGrid;
            factionData.earliestRaidDays = faction.earliestRaidDays;
            factionData.rescueesCanJoin = faction.rescueesCanJoin;
            factionData.naturalEnemy = faction.naturalEnemy;
            factionData.permanentEnemy = faction.permanentEnemy;
            factionData.permanentEnemyToEveryoneExceptPlayer = faction.permanentEnemyToEveryoneExceptPlayer;
            factionData.techLevel = (byte)faction.techLevel;
            factionData.factionIconPath = faction.factionIconPath;
            factionData.settlementTexturePath = faction.settlementTexturePath;
            factionData.hidden = faction.hidden;

            return factionData;
        }

    }
}
