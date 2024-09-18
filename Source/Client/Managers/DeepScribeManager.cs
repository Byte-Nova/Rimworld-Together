using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

            for (int i = 0; i < transferData._humans.Count(); i++) humans.Add(StringToHuman(transferData._humans[i]));

            return humans.ToArray();
        }

        public static HumanDataFile HumanToString(Pawn pawn, bool passInventory = true)
        {
            HumanDataFile humanData = new HumanDataFile();

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

        public static Pawn StringToHuman(HumanDataFile humanData)
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

        private static void GetPawnBioDetails(Pawn pawn, HumanDataFile humanData)
        {
            try
            {
                humanData.DefName = pawn.def.defName;
                humanData.Name = pawn.LabelShortCap.ToString();
                humanData.BiologicalAge = pawn.ageTracker.AgeBiologicalTicks.ToString();
                humanData.ChronologicalAge = pawn.ageTracker.AgeChronologicalTicks.ToString();
                humanData.Gender = pawn.gender.ToString();
                
                humanData.HairDefName = pawn.story.hairDef.defName.ToString();
                humanData.HairColor = pawn.story.HairColor.ToString();
                humanData.HeadTypeDefName = pawn.story.headType.defName.ToString();
                humanData.SkinColor = pawn.story.SkinColor.ToString();
                humanData.BeardDefName = pawn.style.beardDef.defName.ToString();
                humanData.BodyTypeDefName = pawn.story.bodyType.defName.ToString();
                humanData.FaceTattooDefName = pawn.style.FaceTattoo.defName.ToString();
                humanData.BodyTattooDefName = pawn.style.BodyTattoo.defName.ToString();
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetPawnKind(Pawn pawn, HumanDataFile humanData)
        {
            try { humanData.KindDef = pawn.kindDef.defName; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetPawnFaction(Pawn pawn, HumanDataFile humanData)
        {
            if (pawn.Faction == null) return;

            try { humanData.FactionDef = pawn.Faction.def.defName; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetPawnHediffs(Pawn pawn, HumanDataFile humanData)
        {
            if (pawn.health.hediffSet.hediffs.Count() > 0)
            {
                foreach (Hediff hd in pawn.health.hediffSet.hediffs)
                {
                    try
                    {
                        humanData.HediffDefNames.Add(hd.def.defName);

                        if (hd.Part != null) humanData.HediffPartDefName.Add(hd.Part.def.defName.ToString());
                        else humanData.HediffPartDefName.Add("null");

                        if (hd.def.CompProps<HediffCompProperties_Immunizable>() != null) humanData.HediffImmunity.Add(pawn.health.immunity.GetImmunity(hd.def));
                        else humanData.HediffImmunity.Add(-1f);

                        if (hd.def.tendable)
                        {
                            HediffComp_TendDuration comp = hd.TryGetComp<HediffComp_TendDuration>();
                            if (comp.IsTended)
                            {
                                humanData.HediffTendQuality.Add(comp.tendQuality);
                                humanData.HediffTendDuration.Add(comp.tendTicksLeft);
                            } 

                            else 
                            {
                                humanData.HediffTendDuration.Add(-1);
                                humanData.HediffTendQuality.Add(-1);
                            }

                            if (comp.TProps.disappearsAtTotalTendQuality >= 0)
                            {
                                Type type = comp.GetType();
                                FieldInfo fieldInfo = type.GetField("totalTendQuality", BindingFlags.NonPublic | BindingFlags.Instance);
                                humanData.HediffTotalTendQuality.Add((float)fieldInfo.GetValue(comp));
                            }
                            else humanData.HediffTotalTendQuality.Add(-1f);
                        } 

                        else 
                        {
                            humanData.HediffTendDuration.Add(-1);
                            humanData.HediffTendQuality.Add(-1);
                            humanData.HediffTotalTendQuality.Add(-1f);
                        }

                        humanData.HediffSeverity.Add(hd.Severity.ToString());
                        humanData.HeddifPermanent.Add(hd.IsPermanent());
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }
            }
        }

        private static void GetPawnChildState(Pawn pawn, HumanDataFile humanData)
        {
            try { humanData.GrowthPoints = pawn.ageTracker.growthPoints; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetPawnXenotype(Pawn pawn, HumanDataFile humanData)
        {
            try
            {
                if (pawn.genes.Xenotype != null) humanData.XenotypeDefName = pawn.genes.Xenotype.defName.ToString();
                else humanData.XenotypeDefName = "null";

                if (pawn.genes.CustomXenotype != null) humanData.CustomXenotypeName = pawn.genes.xenotypeName.ToString();
                else humanData.CustomXenotypeName = "null";
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetPawnXenogenes(Pawn pawn, HumanDataFile humanData)
        {
            if (pawn.genes.Xenogenes.Count() > 0)
            {
                foreach (Gene gene in pawn.genes.Xenogenes)
                {
                    try { humanData.XenogeneDefNames.Add(gene.def.defName); }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }
            }
        }

        private static void GetPawnEndogenes(Pawn pawn, HumanDataFile humanData)
        {
            if (pawn.genes.Endogenes.Count() > 0)
            {
                foreach (Gene gene in pawn.genes.Endogenes)
                {
                    try { humanData.EndogeneDefNames.Add(gene.def.defName.ToString()); }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }
            }
        }

        private static void GetPawnFavoriteColor(Pawn pawn, HumanDataFile humanData)
        {
            try { humanData.FavoriteColor = pawn.story.favoriteColor.ToString(); }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetPawnStory(Pawn pawn, HumanDataFile humanData)
        {
            try
            {
                if (pawn.story.Childhood != null) humanData.ChildhoodStory = pawn.story.Childhood.defName.ToString();
                else humanData.ChildhoodStory = "null";

                if (pawn.story.Adulthood != null) humanData.AdulthoodStory = pawn.story.Adulthood.defName.ToString();
                else humanData.AdulthoodStory = "null";
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetPawnSkills(Pawn pawn, HumanDataFile humanData)
        {
            if (pawn.skills.skills.Count() > 0)
            {
                foreach (SkillRecord skill in pawn.skills.skills)
                {
                    try
                    {
                        humanData.SkillDefNames.Add(skill.def.defName);
                        humanData.SkillLevels.Add(skill.levelInt.ToString());
                        humanData.Passions.Add(skill.passion.ToString());
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }
            }
        }

        private static void GetPawnTraits(Pawn pawn, HumanDataFile humanData)
        {
            if (pawn.story.traits.allTraits.Count() > 0)
            {
                foreach (Trait trait in pawn.story.traits.allTraits)
                {
                    try
                    {
                        humanData.TraitDefNames.Add(trait.def.defName);
                        humanData.TraitDegrees.Add(trait.Degree.ToString());
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }
            }
        }

        private static void GetPawnApparel(Pawn pawn, HumanDataFile humanData)
        {
            if (pawn.apparel.WornApparel.Count() > 0)
            {
                foreach (Apparel ap in pawn.apparel.WornApparel)
                {
                    try
                    {
                        ThingDataFile thingData = ThingScribeManager.ItemToString(ap, 1);
                        humanData.EquippedApparel.Add(thingData);
                        humanData.ApparelWornByCorpse.Add(ap.WornByCorpse);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }
            }
        }

        private static void GetPawnEquipment(Pawn pawn, HumanDataFile humanData)
        {
            if (pawn.equipment.Primary != null)
            {
                try
                {
                    ThingWithComps weapon = pawn.equipment.Primary;
                    ThingDataFile thingData = ThingScribeManager.ItemToString(weapon, weapon.stackCount);
                    humanData.EquippedWeapon = thingData;
                }
                catch (Exception e) { Logger.Warning(e.ToString()); }
            }
        }

        private static void GetPawnInventory(Pawn pawn, HumanDataFile humanData)
        {
            if (pawn.inventory.innerContainer.Count() != 0)
            {
                foreach (Thing thing in pawn.inventory.innerContainer)
                {
                    try
                    {
                        ThingDataFile thingData = ThingScribeManager.ItemToString(thing, thing.stackCount);
                        humanData.InventoryItems.Add(thingData);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }
            }
        }

        private static void GetPawnPosition(Pawn pawn, HumanDataFile humanData)
        {
            try
            {
                humanData.Position = new string[] { pawn.Position.x.ToString(),
                    pawn.Position.y.ToString(), pawn.Position.z.ToString() };
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetPawnRotation(Pawn pawn, HumanDataFile humanData)
        {
            try { humanData.Rotation = pawn.Rotation.AsInt; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        //Setters

        private static PawnKindDef SetPawnKind(HumanDataFile humanData)
        {
            try { return DefDatabase<PawnKindDef>.AllDefs.First(fetch => fetch.defName == humanData.KindDef); }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            return null;
        }

        private static Faction SetPawnFaction(HumanDataFile humanData)
        {
            if (humanData.FactionDef == null) return null;

            try { return Find.FactionManager.AllFactions.First(fetch => fetch.def.defName == humanData.FactionDef); }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            return null;
        }

        private static Pawn SetPawn(PawnKindDef kind, Faction faction, HumanDataFile humanData)
        {
            try { return PawnGenerator.GeneratePawn(kind, faction); }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            return null;
        }

        private static void SetPawnBioDetails(Pawn pawn, HumanDataFile humanData)
        {
            try
            {
                pawn.Name = new NameSingle(humanData.Name);
                pawn.ageTracker.AgeBiologicalTicks = long.Parse(humanData.BiologicalAge);
                pawn.ageTracker.AgeChronologicalTicks = long.Parse(humanData.ChronologicalAge);

                Enum.TryParse(humanData.Gender, true, out Gender humanGender);
                pawn.gender = humanGender;

                pawn.story.hairDef = DefDatabase<HairDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == humanData.HairDefName);
                pawn.story.headType = DefDatabase<HeadTypeDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == humanData.HeadTypeDefName);
                pawn.style.beardDef = DefDatabase<BeardDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == humanData.BeardDefName);
                pawn.story.bodyType = DefDatabase<BodyTypeDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == humanData.BodyTypeDefName);
                pawn.style.FaceTattoo = DefDatabase<TattooDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == humanData.FaceTattooDefName);
                pawn.style.BodyTattoo = DefDatabase<TattooDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == humanData.BodyTattooDefName);

                string hairColor = humanData.HairColor.Replace("RGBA(", "").Replace(")", "");
                string[] isolatedHair = hairColor.Split(',');
                float r = float.Parse(isolatedHair[0]);
                float g = float.Parse(isolatedHair[1]);
                float b = float.Parse(isolatedHair[2]);
                float a = float.Parse(isolatedHair[3]);
                pawn.story.HairColor = new UnityEngine.Color(r, g, b, a);

                string skinColor = humanData.SkinColor.Replace("RGBA(", "").Replace(")", "");
                string[] isolatedSkin = skinColor.Split(',');
                r = float.Parse(isolatedSkin[0]);
                g = float.Parse(isolatedSkin[1]);
                b = float.Parse(isolatedSkin[2]);
                a = float.Parse(isolatedSkin[3]);
                pawn.story.SkinColorBase = new UnityEngine.Color(r, g, b, a);
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void SetPawnHediffs(Pawn pawn, HumanDataFile humanData)
        {
            try
            {
                pawn.health.RemoveAllHediffs();
                pawn.health.Reset();
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            if (humanData.HediffDefNames.Count() > 0)
            {
                for (int i = 0; i < humanData.HediffDefNames.Count(); i++)
                {
                    try
                    {
                        HediffDef hediffDef = DefDatabase<HediffDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == humanData.HediffDefNames[i]);
                        BodyPartRecord bodyPart = null;

                        if (humanData.HediffPartDefName[i] != "null")
                        {
                            bodyPart = pawn.RaceProps.body.AllParts.ToList().Find(x =>
                                x.def.defName == humanData.HediffPartDefName[i]);
                        }

                        Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                        hediff.Severity = float.Parse(humanData.HediffSeverity[i]);

                        if (humanData.HeddifPermanent[i])
                        {
                            HediffComp_GetsPermanent hediffComp = hediff.TryGetComp<HediffComp_GetsPermanent>();
                            hediffComp.IsPermanent = true;
                        }

                        pawn.health.AddHediff(hediff, bodyPart);
                        if (humanData.HediffImmunity[i] != -1f)
                        {
                            pawn.health.immunity.TryAddImmunityRecord(hediffDef, hediffDef);
                            ImmunityRecord immunityRecord = pawn.health.immunity.GetImmunityRecord(hediffDef);
                            immunityRecord.immunity = humanData.HediffImmunity[i];
                        }

                        if (humanData.HediffTendDuration[i] != -1)
                        {
                            HediffComp_TendDuration comp = hediff.TryGetComp<HediffComp_TendDuration>();
                            comp.tendQuality = humanData.HediffTendQuality[i];
                            comp.tendTicksLeft = humanData.HediffTendDuration[i];
                        }
                        
                        if (humanData.HediffTotalTendQuality[i] != -1f) 
                        {
                            HediffComp_TendDuration comp = hediff.TryGetComp<HediffComp_TendDuration>();
                            Type type = comp.GetType();
                            FieldInfo fieldInfo = type.GetField("totalTendQuality", BindingFlags.NonPublic | BindingFlags.Instance);
                            fieldInfo.SetValue(comp,humanData.HediffTotalTendQuality[i]);
                        }
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }
            }
        }

        private static void SetPawnChildState(Pawn pawn, HumanDataFile humanData)
        {
            try { pawn.ageTracker.growthPoints = humanData.GrowthPoints; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void SetPawnXenotype(Pawn pawn, HumanDataFile humanData)
        {
            try
            {
                if (humanData.XenotypeDefName != "null")
                {
                    pawn.genes.SetXenotype(DefDatabase<XenotypeDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == humanData.XenotypeDefName));
                }

                if (humanData.CustomXenotypeName != "null")
                {
                    pawn.genes.xenotypeName = humanData.CustomXenotypeName;
                }
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void SetPawnXenogenes(Pawn pawn, HumanDataFile humanData)
        {
            try { pawn.genes.Xenogenes.Clear(); }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            if (humanData.XenogeneDefNames.Count() > 0)
            {
                foreach (string str in humanData.XenogeneDefNames)
                {
                    try
                    {
                        GeneDef def = DefDatabase<GeneDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == str);
                        if (def != null) pawn.genes.AddGene(def, true);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }
            }
        }

        private static void SetPawnEndogenes(Pawn pawn, HumanDataFile humanData)
        {
            try { pawn.genes.Endogenes.Clear(); }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            if (humanData.EndogeneDefNames.Count() > 0)
            {
                foreach (string str in humanData.EndogeneDefNames)
                {
                    try
                    {
                        GeneDef def = DefDatabase<GeneDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == str);
                        if (def != null) pawn.genes.AddGene(def, false);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }
            }
        }

        private static void SetPawnFavoriteColor(Pawn pawn, HumanDataFile humanData)
        {
            try
            {
                float r;
                float g;
                float b;
                float a;

                string favoriteColor = humanData.FavoriteColor.Replace("RGBA(", "").Replace(")", "");
                string[] isolatedFavoriteColor = favoriteColor.Split(',');
                r = float.Parse(isolatedFavoriteColor[0]);
                g = float.Parse(isolatedFavoriteColor[1]);
                b = float.Parse(isolatedFavoriteColor[2]);
                a = float.Parse(isolatedFavoriteColor[3]);
                pawn.story.favoriteColor = new UnityEngine.Color(r, g, b, a);
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void SetPawnStory(Pawn pawn, HumanDataFile humanData)
        {
            try
            {
                if (humanData.ChildhoodStory != "null")
                {
                    pawn.story.Childhood = DefDatabase<BackstoryDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == humanData.ChildhoodStory);
                }

                if (humanData.AdulthoodStory != "null")
                {
                    pawn.story.Adulthood = DefDatabase<BackstoryDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == humanData.AdulthoodStory);
                }
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void SetPawnSkills(Pawn pawn, HumanDataFile humanData)
        {
            if (humanData.SkillDefNames.Count() > 0)
            {
                for (int i = 0; i < humanData.SkillDefNames.Count(); i++)
                {
                    try
                    {
                        pawn.skills.skills[i].levelInt = int.Parse(humanData.SkillLevels[i]);

                        Enum.TryParse(humanData.Passions[i], true, out Passion passion);
                        pawn.skills.skills[i].passion = passion;
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }
            }
        }

        private static void SetPawnTraits(Pawn pawn, HumanDataFile humanData)
        {
            try { pawn.story.traits.allTraits.Clear(); }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            if (humanData.TraitDefNames.Count() > 0)
            {
                for (int i = 0; i < humanData.TraitDefNames.Count(); i++)
                {
                    try
                    {
                        TraitDef traitDef = DefDatabase<TraitDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == humanData.TraitDefNames[i]);
                        Trait trait = new Trait(traitDef, int.Parse(humanData.TraitDegrees[i]));
                        pawn.story.traits.GainTrait(trait);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }
            }
        }

        private static void SetPawnApparel(Pawn pawn, HumanDataFile humanData)
        {
            try
            {
                pawn.apparel.DestroyAll();
                pawn.apparel.DropAllOrMoveAllToInventory();
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            if (humanData.EquippedApparel.Count() > 0)
            {
                for (int i = 0; i < humanData.EquippedApparel.Count(); i++)
                {
                    try
                    {
                        Apparel apparel = (Apparel)ThingScribeManager.StringToItem(humanData.EquippedApparel[i]);
                        if (humanData.ApparelWornByCorpse[i]) apparel.WornByCorpse.MustBeTrue();
                        else apparel.WornByCorpse.MustBeFalse();

                        pawn.apparel.Wear(apparel);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }
            }
        }

        private static void SetPawnEquipment(Pawn pawn, HumanDataFile humanData)
        {
            try { pawn.equipment.DestroyAllEquipment(); }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            if (humanData.EquippedWeapon != null)
            {
                try
                {
                    ThingWithComps thing = (ThingWithComps)ThingScribeManager.StringToItem(humanData.EquippedWeapon);
                    pawn.equipment.AddEquipment(thing);
                }
                catch (Exception e) { Logger.Warning(e.ToString()); }
            }
        }

        private static void SetPawnInventory(Pawn pawn, HumanDataFile humanData)
        {
            if (humanData.InventoryItems.Count() > 0)
            {
                foreach (ThingDataFile item in humanData.InventoryItems)
                {
                    try
                    {
                        Thing thing = ThingScribeManager.StringToItem(item);
                        pawn.inventory.TryAddAndUnforbid(thing);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }
            }
        }

        private static void SetPawnPosition(Pawn pawn, HumanDataFile humanData)
        {
            if (humanData.Position != null)
            {
                try
                {
                    pawn.Position = new IntVec3(int.Parse(humanData.Position[0]), int.Parse(humanData.Position[1]),
                        int.Parse(humanData.Position[2]));
                }
                catch (Exception e) { Logger.Warning(e.ToString()); }
            }
        }

        private static void SetPawnRotation(Pawn pawn, HumanDataFile humanData)
        {
            try { pawn.Rotation = new Rot4(humanData.Rotation); }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }
    }

    //Class that handles transformation of animals

    public static class AnimalScribeManager
    {
        //Functions

        public static Pawn[] GetAnimalsFromString(TransferData transferData)
        {
            List<Pawn> animals = new List<Pawn>();

            for (int i = 0; i < transferData._animals.Count(); i++) animals.Add(StringToAnimal(transferData._animals[i]));

            return animals.ToArray();
        }

        public static AnimalDataFile AnimalToString(Pawn animal)
        {
            AnimalDataFile animalData = new AnimalDataFile();

            GetAnimalBioDetails(animal, animalData);

            GetAnimalKind(animal, animalData);

            GetAnimalFaction(animal, animalData);

            GetAnimalHediffs(animal, animalData);

            GetAnimalSkills(animal, animalData);

            GetAnimalPosition(animal, animalData);

            GetAnimalRotation(animal, animalData);

            return animalData;
        }

        public static Pawn StringToAnimal(AnimalDataFile animalData)
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

        private static void GetAnimalBioDetails(Pawn animal, AnimalDataFile animalData)
        {
            try
            {
                animalData.DefName = animal.def.defName;
                animalData.Name = animal.LabelShortCap.ToString();
                animalData.BiologicalAge = animal.ageTracker.AgeBiologicalTicks.ToString();
                animalData.ChronologicalAge = animal.ageTracker.AgeChronologicalTicks.ToString();
                animalData.Gender = animal.gender.ToString();
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetAnimalKind(Pawn animal, AnimalDataFile animalData)
        {
            try { animalData.KindDef = animal.kindDef.defName; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetAnimalFaction(Pawn animal, AnimalDataFile animalData)
        {
            if (animal.Faction == null) return;

            try { animalData.FactionDef = animal.Faction.def.defName; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetAnimalHediffs(Pawn animal, AnimalDataFile animalData)
        {
            if (animal.health.hediffSet.hediffs.Count() > 0)
            {
                foreach (Hediff hd in animal.health.hediffSet.hediffs)
                {
                    try
                    {
                        animalData.HediffDefNames.Add(hd.def.defName);

                        if (hd.Part != null) animalData.HediffPartDefName.Add(hd.Part.def.defName.ToString());
                        else animalData.HediffPartDefName.Add("null");

                        animalData.HediffSeverity.Add(hd.Severity.ToString());
                        animalData.HeddifPermanent.Add(hd.IsPermanent());
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }
            }
        }

        private static void GetAnimalSkills(Pawn animal, AnimalDataFile animalData)
        {
            if (animal.training == null) return;

            foreach (TrainableDef trainable in DefDatabase<TrainableDef>.AllDefsListForReading)
            {
                try
                {
                    animalData.TrainableDefNames.Add(trainable.defName);
                    animalData.CanTrain.Add(animal.training.CanAssignToTrain(trainable).Accepted);
                    animalData.HasLearned.Add(animal.training.HasLearned(trainable));
                    animalData.IsDisabled.Add(animal.training.GetWanted(trainable));
                }
                catch (Exception e) { Logger.Warning(e.ToString()); }
            }
        }

        private static void GetAnimalPosition(Pawn animal, AnimalDataFile animalData)
        {
            try
            {
                animalData.Position = new string[] { animal.Position.x.ToString(),
                        animal.Position.y.ToString(), animal.Position.z.ToString() };
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetAnimalRotation(Pawn animal, AnimalDataFile animalData)
        {
            try { animalData.Rotation = animal.Rotation.AsInt; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        //Setters

        private static PawnKindDef SetAnimalKind(AnimalDataFile animalData)
        {
            try { return DefDatabase<PawnKindDef>.AllDefs.First(fetch => fetch.defName == animalData.DefName); }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            return null;
        }

        private static Faction SetAnimalFaction(AnimalDataFile animalData)
        {
            if (animalData.FactionDef == null) return null;

            try { return Find.FactionManager.AllFactions.First(fetch => fetch.def.defName == animalData.FactionDef); }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            return null;
        }

        private static Pawn SetAnimal(PawnKindDef kind, Faction faction, AnimalDataFile animalData)
        {
            try { return PawnGenerator.GeneratePawn(kind, faction); }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            return null;
        }

        private static void SetAnimalBioDetails(Pawn animal, AnimalDataFile animalData)
        {
            try
            {
                animal.Name = new NameSingle(animalData.Name);
                animal.ageTracker.AgeBiologicalTicks = long.Parse(animalData.BiologicalAge);
                animal.ageTracker.AgeChronologicalTicks = long.Parse(animalData.ChronologicalAge);

                Enum.TryParse(animalData.Gender, true, out Gender animalGender);
                animal.gender = animalGender;
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void SetAnimalHediffs(Pawn animal, AnimalDataFile animalData)
        {
            try
            {
                animal.health.RemoveAllHediffs();
                animal.health.Reset();
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            if (animalData.HediffDefNames.Count() > 0)
            {
                for (int i = 0; i < animalData.HediffDefNames.Count(); i++)
                {
                    try
                    {
                        HediffDef hediffDef = DefDatabase<HediffDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == animalData.HediffDefNames[i]);
                        BodyPartRecord bodyPart = null;

                        if (animalData.HediffPartDefName[i] != "null")
                        {
                            bodyPart = animal.RaceProps.body.AllParts.ToList().Find(x =>
                                x.def.defName == animalData.HediffPartDefName[i]);
                        }

                        Hediff hediff = HediffMaker.MakeHediff(hediffDef, animal, bodyPart);
                        hediff.Severity = float.Parse(animalData.HediffSeverity[i]);

                        if (animalData.HeddifPermanent[i])
                        {
                            HediffComp_GetsPermanent hediffComp = hediff.TryGetComp<HediffComp_GetsPermanent>();
                            hediffComp.IsPermanent = true;
                        }

                        animal.health.AddHediff(hediff);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }
            }
        }

        private static void SetAnimalSkills(Pawn animal, AnimalDataFile animalData)
        {
            if (animalData.TrainableDefNames.Count() > 0)
            {
                for (int i = 0; i < animalData.TrainableDefNames.Count(); i++)
                {
                    try
                    {
                        TrainableDef trainable = DefDatabase<TrainableDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == animalData.TrainableDefNames[i]);
                        if (animalData.CanTrain[i]) animal.training.Train(trainable, null, complete: animalData.HasLearned[i]);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }
            }
        }

        private static void SetAnimalPosition(Pawn animal, AnimalDataFile animalData)
        {
            if (animal.Position != null)
            {
                try
                {
                    animal.Position = new IntVec3(int.Parse(animalData.Position[0]), int.Parse(animalData.Position[1]),
                        int.Parse(animalData.Position[2]));
                }
                catch (Exception e) { Logger.Warning(e.ToString()); }
            }
        }

        private static void SetAnimalRotation(Pawn animal, AnimalDataFile animalData)
        {
            try { animal.Rotation = new Rot4(animalData.Rotation); }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }
    }

    //Class that handles transformation of things

    public static class ThingScribeManager
    {
        //Functions

        public static Thing[] GetItemsFromString(TransferData transferData)
        {
            List<Thing> things = new List<Thing>();

            for (int i = 0; i < transferData._things.Count(); i++)
            {
                Thing thingToAdd = StringToItem(transferData._things[i]);
                if (thingToAdd != null) things.Add(thingToAdd);
            }

            return things.ToArray();
        }

        public static ThingDataFile ItemToString(Thing thing, int thingCount)
        {
            ThingDataFile thingData = new ThingDataFile();

            Thing toUse = null;
            if (GetItemMinified(thing, thingData)) toUse = thing.GetInnerIfMinified();
            else toUse = thing;

            GetItemName(toUse, thingData);

            GetItemMaterial(toUse, thingData);

            GetItemQuantity(toUse, thingData, thingCount);

            GetItemQuality(toUse, thingData);

            GetItemHitpoints(toUse, thingData);

            GetItemPosition(toUse, thingData);

            GetItemRotation(toUse, thingData);

            if (DeepScribeHelper.CheckIfThingIsGenepack(toUse)) GetGenepackDetails(toUse, thingData);
            else if (DeepScribeHelper.CheckIfThingIsBook(toUse)) GetBookDetails(toUse, thingData);
            return thingData;
        }

        public static Thing StringToItem(ThingDataFile thingData)
        {
            Thing thing = SetItem(thingData);

            SetItemQuantity(thing, thingData);

            SetItemQuality(thing, thingData);

            SetItemHitpoints(thing, thingData);

            SetItemPosition(thing, thingData);

            SetItemRotation(thing, thingData);

            SetItemMinified(thing, thingData);

            if (DeepScribeHelper.CheckIfThingIsGenepack(thing)) SetGenepackDetails(thing, thingData);
            else if (DeepScribeHelper.CheckIfThingIsBook(thing)) SetBookDetails(thing, thingData);
            return thing;
        }

        //Getters

        private static void GetItemName(Thing thing, ThingDataFile thingData)
        {
            try { thingData.DefName = thing.def.defName; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetItemMaterial(Thing thing, ThingDataFile thingData)
        {
            try
            {
                if (DeepScribeHelper.CheckIfThingHasMaterial(thing)) thingData.MaterialDefName = thing.Stuff.defName;
                else thingData.MaterialDefName = null;
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetItemQuantity(Thing thing, ThingDataFile thingData, int thingCount)
        {
            try { thingData.Quantity = thingCount; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetItemQuality(Thing thing, ThingDataFile thingData)
        {
            try { thingData.Quality = DeepScribeHelper.GetThingQuality(thing); }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetItemHitpoints(Thing thing, ThingDataFile thingData)
        {
            try { thingData.Hitpoints = thing.HitPoints; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetItemPosition(Thing thing, ThingDataFile thingData)
        {
            try { thingData.Position = new float[] { thing.Position.x, thing.Position.y, thing.Position.z }; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetItemRotation(Thing thing, ThingDataFile thingData)
        {
            try { thingData.Rotation = thing.Rotation.AsInt; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static bool GetItemMinified(Thing thing, ThingDataFile thingData)
        {
            try
            {
                thingData.IsMinified = DeepScribeHelper.CheckIfThingIsMinified(thing);
                return thingData.IsMinified;
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            return false;
        }

        private static void GetGenepackDetails(Thing thing, ThingDataFile thingData)
        {
            try
            {
                Genepack genepack = (Genepack)thing;

                Type type = genepack.GetType();
                FieldInfo fieldInfo = type.GetField("geneSet", BindingFlags.NonPublic | BindingFlags.Instance);
                GeneSet geneSet = (GeneSet)fieldInfo.GetValue(genepack);

                type = geneSet.GetType();
                fieldInfo = type.GetField("genes", BindingFlags.NonPublic | BindingFlags.Instance);
                List<GeneDef> geneList = (List<GeneDef>)fieldInfo.GetValue(geneSet);
                foreach (GeneDef gene in geneList) thingData.GenepackData.genepackDefs.Add(gene.defName);
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetBookDetails(Thing thing, ThingDataFile thingData)
        {
            try
            {
                BookData bookData = new BookData();
                Book book = (Book)thing;
                bookData.title = book.Title;
                bookData.description = book.DescriptionDetailed;
                bookData.descriptionFlavor = book.FlavorUI;

                Type type = book.GetType();
                FieldInfo fieldInfo = type.GetField("mentalBreakChancePerHour", BindingFlags.NonPublic | BindingFlags.Instance);
                bookData.mentalBreakChance = (float)fieldInfo.GetValue(book);

                type = book.GetType();
                fieldInfo = type.GetField("joyFactor", BindingFlags.NonPublic | BindingFlags.Instance);
                bookData.joyFactor = (float)fieldInfo.GetValue(book);

                book.BookComp.TryGetDoer<BookOutcomeDoerGainSkillExp>(out BookOutcomeDoerGainSkillExp xp);
                if (xp != null)
                {
                    foreach (KeyValuePair<SkillDef, float> pair in xp.Values)
                    {
                        bookData.skillData.Add(pair.Key.defName, pair.Value);
                    }
                }

                book.BookComp.TryGetDoer<ReadingOutcomeDoerGainResearch>(out ReadingOutcomeDoerGainResearch research);
                if (research != null)
                {
                    type = research.GetType();
                    fieldInfo = type.GetField("values", BindingFlags.NonPublic | BindingFlags.Instance);
                    Dictionary<ResearchProjectDef, float> researchDict = (Dictionary<ResearchProjectDef, float>)fieldInfo.GetValue(research);
                    foreach (ResearchProjectDef key in researchDict.Keys) bookData.researchData.Add(key.defName, researchDict[key]);
                }

                thingData.BookData = bookData;
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        //Setters

        private static Thing SetItem(ThingDataFile thingData)
        {
            try
            {
                ThingDef thingDef = DefDatabase<ThingDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == thingData.DefName);
                ThingDef defMaterial = DefDatabase<ThingDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == thingData.MaterialDefName);
                return ThingMaker.MakeThing(thingDef, defMaterial);
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            return null;
        }

        private static void SetItemQuantity(Thing thing, ThingDataFile thingData)
        {
            try { thing.stackCount = thingData.Quantity; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void SetItemQuality(Thing thing, ThingDataFile thingData)
        {
            if (thingData.Quality != -1)
            {
                try
                {
                    CompQuality compQuality = thing.TryGetComp<CompQuality>();
                    if (compQuality != null)
                    {
                        QualityCategory iCategory = (QualityCategory)thingData.Quality;
                        compQuality.SetQuality(iCategory, ArtGenerationContext.Outsider);
                    }
                }
                catch (Exception e) { Logger.Warning(e.ToString()); }
            }
        }

        private static void SetItemHitpoints(Thing thing, ThingDataFile thingData)
        {
            try { thing.HitPoints = thingData.Hitpoints; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void SetItemPosition(Thing thing, ThingDataFile thingData)
        {
            try { thing.Position = new IntVec3((int)thingData.Position[0], (int)thingData.Position[1], (int)thingData.Position[2]); }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void SetItemRotation(Thing thing, ThingDataFile thingData)
        {
            try { thing.Rotation = new Rot4(thingData.Rotation); }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void SetItemMinified(Thing thing, ThingDataFile thingData)
        {
            if (thingData.IsMinified)
            {
                //INFO
                //This function is where you should transform the item back into a minified.
                //However, this isn't needed and is likely to cause issues with caravans if used
            }
        }

        private static void SetGenepackDetails(Thing thing, ThingDataFile thingData)
        {
            try
            {
                Genepack genepack = (Genepack)thing;

                Type type = genepack.GetType();
                FieldInfo fieldInfo = type.GetField("geneSet", BindingFlags.NonPublic | BindingFlags.Instance);
                GeneSet geneSet = (GeneSet)fieldInfo.GetValue(genepack);

                type = geneSet.GetType();
                fieldInfo = type.GetField("genes", BindingFlags.NonPublic | BindingFlags.Instance);

                List<GeneDef> geneList = (List<GeneDef>)fieldInfo.GetValue(geneSet);
                geneList.Clear();
                foreach (string str in thingData.GenepackData.genepackDefs)
                {
                    GeneDef gene = DefDatabase<GeneDef>.AllDefs.First(fetch => fetch.defName == str);
                    geneList.Add(gene);
                }
                geneSet.GenerateName();
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void SetBookDetails(Thing thing, ThingDataFile thingData)
        {
            try
            {
                Book book = (Book)thing;
                Type type = book.GetType();

                FieldInfo fieldInfo = type.GetField("title", BindingFlags.NonPublic | BindingFlags.Instance);
                fieldInfo.SetValue(book, thingData.BookData.title);

                fieldInfo = type.GetField("description", BindingFlags.NonPublic | BindingFlags.Instance);
                fieldInfo.SetValue(book, thingData.BookData.description);

                fieldInfo = type.GetField("descriptionFlavor", BindingFlags.NonPublic | BindingFlags.Instance);
                fieldInfo.SetValue(book, thingData.BookData.descriptionFlavor);

                fieldInfo = type.GetField("mentalBreakChancePerHour", BindingFlags.NonPublic | BindingFlags.Instance);
                fieldInfo.SetValue(book, thingData.BookData.mentalBreakChance);

                fieldInfo = type.GetField("joyFactor", BindingFlags.NonPublic | BindingFlags.Instance);
                fieldInfo.SetValue(book, thingData.BookData.joyFactor);

                book.BookComp.TryGetDoer<BookOutcomeDoerGainSkillExp>(out BookOutcomeDoerGainSkillExp doerXP);
                if (doerXP != null)
                {
                    type = doerXP.GetType();
                    fieldInfo = type.GetField("values", BindingFlags.NonPublic | BindingFlags.Instance);
                    Dictionary<SkillDef, float> skilldict = new Dictionary<SkillDef, float>();

                    foreach (string str in thingData.BookData.skillData.Keys)
                    {
                        SkillDef skillDef = DefDatabase<SkillDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == str);
                        skilldict.Add(skillDef, thingData.BookData.skillData[str]);
                    }

                    fieldInfo.SetValue(doerXP, skilldict);
                }

                book.BookComp.TryGetDoer<ReadingOutcomeDoerGainResearch>(out ReadingOutcomeDoerGainResearch doerResearch);
                if (doerResearch != null)
                {
                    type = doerResearch.GetType();
                    fieldInfo = type.GetField("values", BindingFlags.NonPublic | BindingFlags.Instance);
                    Dictionary<ResearchProjectDef, float> researchDict = new Dictionary<ResearchProjectDef, float>();

                    foreach (string str in thingData.BookData.researchData.Keys)
                    {
                        ResearchProjectDef researchDef = DefDatabase<ResearchProjectDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == str);
                        researchDict.Add(researchDef, thingData.BookData.researchData[str]);
                    }

                    fieldInfo.SetValue(doerResearch, researchDict);
                }
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
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

            GetMapWeather(mapData, map);

            return mapData;
        }

        public static Map StringToMap(MapData mapData, bool factionThings, bool nonFactionThings, bool factionHumans, bool nonFactionHumans, bool factionAnimals, bool nonFactionAnimals, bool lessLoot = false)
        {
            Map map = SetEmptyMap(mapData);

            SetMapTerrain(mapData, map);

            if (factionThings || nonFactionThings) SetMapThings(mapData, map, factionThings, nonFactionThings, lessLoot);

            if (factionHumans || nonFactionHumans) SetMapHumans(mapData, map, factionHumans, nonFactionHumans);

            if (factionAnimals || nonFactionAnimals) SetMapAnimals(mapData, map, factionAnimals, nonFactionAnimals);

            SetWeatherData(mapData, map);

            SetMapFog(map);

            SetMapRoofs(map);

            return map;
        }

        //Getters

        private static void GetMapTile(MapData mapData, Map map)
        {
            try { mapData._mapTile = map.Tile; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetMapSize(MapData mapData, Map map)
        {
            try { mapData._mapSize = ValueParser.IntVec3ToArray(map.Size); }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetMapTerrain(MapData mapData, Map map)
        {
            try 
            {
                List<string> tempTileDefNames = new List<string>();
                List<string> tempTileRoofDefNames = new List<string>();
                List<bool> tempTilePollutions = new List<bool>();

                for (int z = 0; z < map.Size.z; ++z)
                {
                    for (int x = 0; x < map.Size.x; ++x)
                    {
                        IntVec3 vectorToCheck = new IntVec3(x, map.Size.y, z);

                        tempTileDefNames.Add(map.terrainGrid.TerrainAt(vectorToCheck).defName.ToString());
                        tempTilePollutions.Add(map.pollutionGrid.IsPolluted(vectorToCheck));

                        if (map.roofGrid.RoofAt(vectorToCheck) == null) tempTileRoofDefNames.Add("null");
                        else tempTileRoofDefNames.Add(map.roofGrid.RoofAt(vectorToCheck).defName.ToString());
                    }
                }

                mapData._tileDefNames = tempTileDefNames.ToArray();
                mapData._tileRoofDefNames = tempTileRoofDefNames.ToArray();
                mapData._tilePollutions = tempTilePollutions.ToArray();
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetMapThings(MapData mapData, Map map, bool factionThings, bool nonFactionThings)
        {
            try 
            {
                List<ThingDataFile> tempFactionThings = new List<ThingDataFile>();
                List<ThingDataFile> tempNonFactionThings = new List<ThingDataFile>();

                foreach (Thing thing in map.listerThings.AllThings)
                {
                    if (!DeepScribeHelper.CheckIfThingIsHuman(thing) && !DeepScribeHelper.CheckIfThingIsAnimal(thing))
                    {
                        ThingDataFile thingData = ThingScribeManager.ItemToString(thing, thing.stackCount);

                        if (thing.def.alwaysHaulable && factionThings) tempFactionThings.Add(thingData);
                        else if (!thing.def.alwaysHaulable && nonFactionThings) tempNonFactionThings.Add(thingData);

                        if (DeepScribeHelper.CheckIfThingCanGrow(thing))
                        {
                            try
                            {
                                Plant plant = thing as Plant;
                                thingData.PlantData.growthTicks = plant.Growth;
                            }
                            catch (Exception e) { Logger.Warning(e.ToString()); }
                        }
                    }
                }

                mapData._factionThings = tempFactionThings.ToArray();
                mapData._nonFactionThings = tempNonFactionThings.ToArray();
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetMapHumans(MapData mapData, Map map, bool factionHumans, bool nonFactionHumans)
        {
            try 
            {
                List<HumanDataFile> tempFactionHumans = new List<HumanDataFile>();
                List<HumanDataFile> tempNonFactionHumans = new List<HumanDataFile>();

                foreach (Thing thing in map.listerThings.AllThings)
                {
                    if (DeepScribeHelper.CheckIfThingIsHuman(thing))
                    {
                        HumanDataFile humanData = HumanScribeManager.HumanToString(thing as Pawn);

                        if (thing.Faction == Faction.OfPlayer && factionHumans) tempFactionHumans.Add(humanData);
                        else if (thing.Faction != Faction.OfPlayer && nonFactionHumans) tempNonFactionHumans.Add(humanData);
                    }
                }

                mapData._factionHumans = tempFactionHumans.ToArray();
                mapData._nonFactionHumans = tempNonFactionHumans.ToArray();
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetMapAnimals(MapData mapData, Map map, bool factionAnimals, bool nonFactionAnimals)
        {
            try 
            {
                List<AnimalDataFile> tempFactionAnimals = new List<AnimalDataFile>();
                List<AnimalDataFile> tempNonFactionAnimals = new List<AnimalDataFile>();

                foreach (Thing thing in map.listerThings.AllThings)
                {
                    if (DeepScribeHelper.CheckIfThingIsAnimal(thing))
                    {
                        AnimalDataFile animalData = AnimalScribeManager.AnimalToString(thing as Pawn);

                        if (thing.Faction == Faction.OfPlayer && factionAnimals) tempFactionAnimals.Add(animalData);
                        else if (thing.Faction != Faction.OfPlayer && nonFactionAnimals) tempNonFactionAnimals.Add(animalData);
                    }
                }

                mapData._factionAnimals = tempFactionAnimals.ToArray();
                mapData._nonFactionAnimals = tempNonFactionAnimals.ToArray();
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetMapWeather(MapData mapData, Map map)
        {
            try { mapData._curWeatherDefName = map.weatherManager.curWeather.defName; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        //Setters

        private static Map SetEmptyMap(MapData mapData)
        {
            IntVec3 mapSize = ValueParser.ArrayToIntVec3(mapData._mapSize);

            PlanetManagerHelper.SetOverrideGenerators();
            Map toReturn = GetOrGenerateMapUtility.GetOrGenerateMap(SessionValues.chosenSettlement.Tile, mapSize, null);
            PlanetManagerHelper.SetDefaultGenerators();

            return toReturn;
        }

        private static void SetMapTerrain(MapData mapData, Map map)
        {
            try
            {
                int index = 0;

                for (int z = 0; z < map.Size.z; ++z)
                {
                    for (int x = 0; x < map.Size.x; ++x)
                    {
                        IntVec3 vectorToCheck = new IntVec3(x, map.Size.y, z);

                        try
                        {
                            TerrainDef terrainToUse = DefDatabase<TerrainDef>.AllDefs.FirstOrDefault(fetch => fetch.defName ==
                                mapData._tileDefNames[index]);

                            map.terrainGrid.SetTerrain(vectorToCheck, terrainToUse);
                            map.pollutionGrid.SetPolluted(vectorToCheck, mapData._tilePollutions[index]);

                        }
                        catch (Exception e) { Logger.Warning(e.ToString()); }

                        try
                        {
                            RoofDef roofToUse = DefDatabase<RoofDef>.AllDefs.FirstOrDefault(fetch => fetch.defName ==
                                mapData._tileRoofDefNames[index]);

                            map.roofGrid.SetRoof(vectorToCheck, roofToUse);
                        }
                        catch (Exception e) { Logger.Warning(e.ToString()); }

                        index++;
                    }
                }
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void SetMapThings(MapData mapData, Map map, bool factionThings, bool nonFactionThings, bool lessLoot)
        {
            try
            {
                List<Thing> thingsToGetInThisTile = new List<Thing>();

                if (factionThings)
                {
                    Random rnd = new Random();

                    foreach (ThingDataFile item in mapData._factionThings)
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
                                plant.Growth = item.PlantData.growthTicks;
                            }
                        }
                        catch (Exception e) { Logger.Warning(e.ToString()); }
                    }
                }

                if (nonFactionThings)
                {
                    foreach (ThingDataFile item in mapData._nonFactionThings)
                    {
                        try
                        {
                            Thing toGet = ThingScribeManager.StringToItem(item);
                            thingsToGetInThisTile.Add(toGet);

                            if (DeepScribeHelper.CheckIfThingCanGrow(toGet))
                            {
                                Plant plant = toGet as Plant;
                                plant.Growth = item.PlantData.growthTicks;
                            }
                        }
                        catch (Exception e) { Logger.Warning(e.ToString()); }
                    }
                }

                foreach (Thing thing in thingsToGetInThisTile)
                {
                    try { GenPlace.TryPlaceThing(thing, thing.Position, map, ThingPlaceMode.Direct, rot: thing.Rotation); }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void SetMapHumans(MapData mapData, Map map, bool factionHumans, bool nonFactionHumans)
        {
            try
            {
                if (factionHumans)
                {
                    foreach (HumanDataFile pawn in mapData._factionHumans)
                    {
                        try
                        {
                            Pawn human = HumanScribeManager.StringToHuman(pawn);
                            human.SetFaction(FactionValues.neutralPlayer);

                            GenSpawn.Spawn(human, human.Position, map, human.Rotation);
                        }
                        catch (Exception e) { Logger.Warning(e.ToString()); }
                    }
                }

                if (nonFactionHumans)
                {
                    foreach (HumanDataFile pawn in mapData._nonFactionHumans)
                    {
                        try
                        {
                            Pawn human = HumanScribeManager.StringToHuman(pawn);
                            GenSpawn.Spawn(human, human.Position, map, human.Rotation);
                        }
                        catch (Exception e) { Logger.Warning(e.ToString()); }
                    }
                }
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void SetMapAnimals(MapData mapData, Map map, bool factionAnimals, bool nonFactionAnimals)
        {
            try
            {
                if (factionAnimals)
                {
                    foreach (AnimalDataFile pawn in mapData._factionAnimals)
                    {
                        try
                        {
                            Pawn animal = AnimalScribeManager.StringToAnimal(pawn);
                            animal.SetFaction(FactionValues.neutralPlayer);

                            GenSpawn.Spawn(animal, animal.Position, map, animal.Rotation);
                        }
                        catch (Exception e) { Logger.Warning(e.ToString()); }
                    }
                }

                if (nonFactionAnimals)
                {
                    foreach (AnimalDataFile pawn in mapData._nonFactionAnimals)
                    {
                        try
                        {
                            Pawn animal = AnimalScribeManager.StringToAnimal(pawn);
                            GenSpawn.Spawn(animal, animal.Position, map, animal.Rotation);
                        }
                        catch (Exception e) { Logger.Warning(e.ToString()); }
                    }
                }
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void SetWeatherData(MapData mapData, Map map)
        {
            try
            {
                WeatherDef weatherDef = DefDatabase<WeatherDef>.AllDefs.First(fetch => fetch.defName == mapData._curWeatherDefName);
                map.weatherManager.TransitionTo(weatherDef);
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void SetMapFog(Map map)
        {
            try { FloodFillerFog.FloodUnfog(MapGenerator.PlayerStartSpot, map); }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void SetMapRoofs(Map map)
        {
            try
            {
                map.roofCollapseBuffer.Clear();
                map.roofGrid.Drawer.SetDirty();
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }         
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
            PawnKindDef animal = DefDatabase<PawnKindDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == thing.def.defName);
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

            catch (Exception e) 
            { 
                //Don't log to avoid countless spam logs
                //Logger.Warning(e.ToString()); 
                
                return false;
            }
        }

        //Checks if transferable thing has a material

        public static bool CheckIfThingHasMaterial(Thing thing)
        {
            if (thing.Stuff != null) return true;
            else return false;
        }

        //Gets the quality of a transferable thing

        public static int GetThingQuality(Thing thing)
        {
            QualityCategory qc = QualityCategory.Normal;
            thing.TryGetQuality(out qc);

            return (int)qc;
        }

        //Checks if transferable thing is minified

        public static bool CheckIfThingIsMinified(Thing thing)
        {
            if (thing.def == ThingDefOf.MinifiedThing || thing.def == ThingDefOf.MinifiedTree) return true;
            else return false;
        }

        public static bool CheckIfThingIsBook(Thing thing)
        {
            if (!ModsConfig.AnomalyActive) return false;

            if (thing.def.defName == ThingDefOf.TextBook.defName) return true;
            else if (thing.def.defName == ThingDefOf.Schematic.defName) return true;
            else if (thing.def.defName == ThingDefOf.Tome.defName) return true;
            else if (thing.def.defName == ThingDefOf.Novel.defName) return true;
            else return false;
        }

        public static bool CheckIfThingIsGenepack(Thing thing)
        {
            if (!ModsConfig.BiotechActive) return false;

            if (thing.def.defName == ThingDefOf.Genepack.defName) return true;
            else return false;
        }
    }
}
