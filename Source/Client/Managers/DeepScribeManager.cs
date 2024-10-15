using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
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

        public static HumanFile HumanToString(Pawn pawn, bool passInventory = true)
        {
            HumanFile humanData = new HumanFile();

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

            GetPawnTransform(pawn, humanData);

            return humanData;
        }

        public static Pawn StringToHuman(HumanFile humanData)
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

            SetPawnTransform(pawn, humanData);

            return pawn;
        }

        //Getters

        private static void GetPawnBioDetails(Pawn pawn, HumanFile humanData)
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

        private static void GetPawnKind(Pawn pawn, HumanFile humanData)
        {
            try { humanData.KindDef = pawn.kindDef.defName; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetPawnFaction(Pawn pawn, HumanFile humanData)
        {
            if (pawn.Faction == null) return;

            try { humanData.FactionDef = pawn.Faction.def.defName; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetPawnHediffs(Pawn pawn, HumanFile humanData)
        {
            if (pawn.health.hediffSet.hediffs.Count() > 0)
            {
                List<HediffComponent> toGet = new List<HediffComponent>();

                foreach (Hediff hd in pawn.health.hediffSet.hediffs)
                {
                    try
                    {
                        HediffComponent component = new HediffComponent();
                        component.DefName = hd.def.defName;

                        if (hd.Part != null) component.PartDefName = hd.Part.def.defName;
                        else component.PartDefName = "null";

                        if (hd.def.CompProps<HediffCompProperties_Immunizable>() != null) component.Immunity = pawn.health.immunity.GetImmunity(hd.def);
                        else component.Immunity = -1f;

                        if (hd.def.tendable)
                        {
                            HediffComp_TendDuration comp = hd.TryGetComp<HediffComp_TendDuration>();
                            if (comp.IsTended)
                            {
                                component.TendQuality = comp.tendQuality;
                                component.TendDuration = comp.tendTicksLeft;
                            } 

                            else 
                            {
                                component.TendDuration = -1;
                                component.TendQuality = -1;
                            }

                            if (comp.TProps.disappearsAtTotalTendQuality >= 0)
                            {
                                Type type = comp.GetType();
                                FieldInfo fieldInfo = type.GetField("totalTendQuality", BindingFlags.NonPublic | BindingFlags.Instance);
                                component.TotalTendQuality = (float)fieldInfo.GetValue(comp);
                            }
                            else component.TotalTendQuality = -1f;
                        } 

                        else 
                        {
                            component.TendDuration = -1;
                            component.TendQuality = -1;
                            component.TotalTendQuality = -1f;
                        }

                        component.Severity = hd.Severity;
                        component.IsPermanent = hd.IsPermanent();

                        toGet.Add(component);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }

                humanData.Hediffs = toGet.ToArray();
            }
        }

        private static void GetPawnChildState(Pawn pawn, HumanFile humanData)
        {
            try { humanData.GrowthPoints = pawn.ageTracker.growthPoints; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetPawnXenotype(Pawn pawn, HumanFile humanData)
        {
            try
            {
                if (pawn.genes.Xenotype != null) humanData.Xenotype.DefName = pawn.genes.Xenotype.defName.ToString();
                else humanData.Xenotype.DefName = "null";

                if (pawn.genes.CustomXenotype != null) humanData.Xenotype.CustomXenotypeName = pawn.genes.xenotypeName.ToString();
                else humanData.Xenotype.CustomXenotypeName = "null";
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetPawnXenogenes(Pawn pawn, HumanFile humanData)
        {
            if (pawn.genes.Xenogenes.Count() > 0)
            {
                List<XenogeneComponent> toGet = new List<XenogeneComponent>();

                foreach (Gene gene in pawn.genes.Xenogenes)
                {
                    try                 
                    { 
                        XenogeneComponent component = new XenogeneComponent();
                        component.DefName = gene.def.defName;

                        toGet.Add(component);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }

                humanData.Xenogenes = toGet.ToArray();
            }
        }

        private static void GetPawnEndogenes(Pawn pawn, HumanFile humanData)
        {
            if (pawn.genes.Endogenes.Count() > 0)
            {
                List<EndogeneComponent> toGet = new List<EndogeneComponent>();

                foreach (Gene gene in pawn.genes.Endogenes)
                {
                    try 
                    {  
                        EndogeneComponent component = new EndogeneComponent();
                        component.DefName = gene.def.defName;

                        toGet.Add(component);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }

                humanData.Endogenes = toGet.ToArray();
            }
        }

        private static void GetPawnFavoriteColor(Pawn pawn, HumanFile humanData)
        {
            try { humanData.FavoriteColor = pawn.story.favoriteColor.ToString(); }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetPawnStory(Pawn pawn, HumanFile humanData)
        {
            try
            {
                if (pawn.story.Childhood != null) humanData.Stories.ChildhoodStoryDefName = pawn.story.Childhood.defName.ToString();
                else humanData.Stories.ChildhoodStoryDefName = "null";

                if (pawn.story.Adulthood != null) humanData.Stories.AdulthoodStoryDefName = pawn.story.Adulthood.defName.ToString();
                else humanData.Stories.AdulthoodStoryDefName = "null";
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetPawnSkills(Pawn pawn, HumanFile humanData)
        {
            if (pawn.skills.skills.Count() > 0)
            {
                List<SkillComponent> toGet = new List<SkillComponent>();

                foreach (SkillRecord skill in pawn.skills.skills)
                {
                    try
                    {
                        SkillComponent component = new SkillComponent();
                        component.DefName = skill.def.defName;
                        component.Level = skill.levelInt;
                        component.Passion = skill.passion.ToString();

                        toGet.Add(component);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }

                humanData.Skills = toGet.ToArray();
            }
        }

        private static void GetPawnTraits(Pawn pawn, HumanFile humanData)
        {
            if (pawn.story.traits.allTraits.Count() > 0)
            {
                List<TraitComponent> toGet = new List<TraitComponent>();

                foreach (Trait trait in pawn.story.traits.allTraits)
                {
                    try
                    {
                        TraitComponent component = new TraitComponent();
                        component.DefName = trait.def.defName;
                        component.Degree = trait.Degree;

                        toGet.Add(component);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }

                humanData.Traits = toGet.ToArray();
            }
        }

        private static void GetPawnApparel(Pawn pawn, HumanFile humanData)
        {
            if (pawn.apparel.WornApparel.Count() > 0)
            {
                List<ApparelComponent> toGet = new List<ApparelComponent>();

                foreach (Apparel ap in pawn.apparel.WornApparel)
                {
                    try
                    {
                        ThingFile thingData = ThingScribeManager.ItemToString(ap, 1);
                        ApparelComponent component = new ApparelComponent();
                        component.EquippedApparel = thingData;
                        component.WornByCorpse = ap.WornByCorpse;

                        toGet.Add(component);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }

                humanData.Apparel = toGet.ToArray();
            }
        }

        private static void GetPawnEquipment(Pawn pawn, HumanFile humanData)
        {
            if (pawn.equipment.Primary != null)
            {
                try
                {
                    ThingWithComps weapon = pawn.equipment.Primary;
                    ThingFile thingData = ThingScribeManager.ItemToString(weapon, weapon.stackCount);
                    humanData.Weapon = thingData;
                }
                catch (Exception e) { Logger.Warning(e.ToString()); }
            }
        }

        private static void GetPawnInventory(Pawn pawn, HumanFile humanData)
        {
            if (pawn.inventory.innerContainer.Count() != 0)
            {
                List<ItemComponent> toGet = new List<ItemComponent>();

                foreach (Thing thing in pawn.inventory.innerContainer)
                {
                    try
                    {
                        ThingFile thingData = ThingScribeManager.ItemToString(thing, thing.stackCount);
                        ItemComponent component = new ItemComponent();
                        component.Item = thingData;

                        toGet.Add(component);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }

                humanData.Items = toGet.ToArray();
            }
        }

        private static void GetPawnTransform(Pawn pawn, HumanFile humanData)
        {
            try
            {
                humanData.Transform.Position = new int[] 
                { 
                    pawn.Position.x,
                    pawn.Position.y, 
                    pawn.Position.z 
                };

                humanData.Transform.Rotation = pawn.Rotation.AsInt;
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        //Setters

        private static PawnKindDef SetPawnKind(HumanFile humanData)
        {
            try { return DefDatabase<PawnKindDef>.AllDefs.First(fetch => fetch.defName == humanData.KindDef); }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            return null;
        }

        private static Faction SetPawnFaction(HumanFile humanData)
        {
            if (humanData.FactionDef == null) return null;

            try { return Find.FactionManager.AllFactions.First(fetch => fetch.def.defName == humanData.FactionDef); }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            return null;
        }

        private static Pawn SetPawn(PawnKindDef kind, Faction faction, HumanFile humanData)
        {
            try { return PawnGenerator.GeneratePawn(kind, faction); }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            return null;
        }

        private static void SetPawnBioDetails(Pawn pawn, HumanFile humanData)
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

        private static void SetPawnHediffs(Pawn pawn, HumanFile humanData)
        {
            try
            {
                pawn.health.RemoveAllHediffs();
                pawn.health.Reset();
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            if (humanData.Hediffs.Length > 0)
            {
                for (int i = 0; i < humanData.Hediffs.Length; i++)
                {
                    try
                    {
                        HediffComponent component = humanData.Hediffs[i];
                        HediffDef hediffDef = DefDatabase<HediffDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == component.DefName);
                        BodyPartRecord bodyPart = null;

                        if (component.PartDefName != "null")
                        {
                            bodyPart = pawn.RaceProps.body.AllParts.ToList().Find(x =>
                                x.def.defName == component.PartDefName);
                        }

                        Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                        hediff.Severity = component.Severity;

                        if (component.IsPermanent)
                        {
                            HediffComp_GetsPermanent hediffComp = hediff.TryGetComp<HediffComp_GetsPermanent>();
                            hediffComp.IsPermanent = true;
                        }

                        pawn.health.AddHediff(hediff, bodyPart);
                        if (component.Immunity != -1f)
                        {
                            pawn.health.immunity.TryAddImmunityRecord(hediffDef, hediffDef);
                            ImmunityRecord immunityRecord = pawn.health.immunity.GetImmunityRecord(hediffDef);
                            immunityRecord.immunity = component.Immunity;
                        }

                        if (component.TendDuration != -1)
                        {
                            HediffComp_TendDuration comp = hediff.TryGetComp<HediffComp_TendDuration>();
                            comp.tendQuality = component.TendQuality;
                            comp.tendTicksLeft = component.TendDuration;
                        }
                        
                        if (component.TotalTendQuality != -1f) 
                        {
                            HediffComp_TendDuration comp = hediff.TryGetComp<HediffComp_TendDuration>();
                            Type type = comp.GetType();
                            FieldInfo fieldInfo = type.GetField("totalTendQuality", BindingFlags.NonPublic | BindingFlags.Instance);
                            fieldInfo.SetValue(comp, component.TotalTendQuality);
                        }
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }
            }
        }

        private static void SetPawnChildState(Pawn pawn, HumanFile humanData)
        {
            try { pawn.ageTracker.growthPoints = humanData.GrowthPoints; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void SetPawnXenotype(Pawn pawn, HumanFile humanData)
        {
            try
            {
                if (humanData.Xenotype.DefName != "null")
                {
                    pawn.genes.SetXenotype(DefDatabase<XenotypeDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == humanData.Xenotype.DefName));
                }

                if (humanData.Xenotype.CustomXenotypeName != "null")
                {
                    pawn.genes.xenotypeName = humanData.Xenotype.CustomXenotypeName;
                }
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void SetPawnXenogenes(Pawn pawn, HumanFile humanData)
        {
            try { pawn.genes.Xenogenes.Clear(); }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            if (humanData.Xenogenes.Length > 0)
            {
                for (int i = 0; i < humanData.Xenogenes.Length; i++)
                {
                    try
                    {
                        XenogeneComponent component = humanData.Xenogenes[i];
                        GeneDef def = DefDatabase<GeneDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == component.DefName);
                        if (def != null) pawn.genes.AddGene(def, true);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }
            }
        }

        private static void SetPawnEndogenes(Pawn pawn, HumanFile humanData)
        {
            try { pawn.genes.Endogenes.Clear(); }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            if (humanData.Endogenes.Length > 0)
            {
                for (int i = 0; i < humanData.Endogenes.Length; i++)
                {
                    try
                    {
                        EndogeneComponent component = humanData.Endogenes[i];
                        GeneDef def = DefDatabase<GeneDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == component.DefName);
                        if (def != null) pawn.genes.AddGene(def, true);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }
            }
        }

        private static void SetPawnFavoriteColor(Pawn pawn, HumanFile humanData)
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

        private static void SetPawnStory(Pawn pawn, HumanFile humanData)
        {
            try
            {
                if (humanData.Stories.ChildhoodStoryDefName != "null")
                {
                    pawn.story.Childhood = DefDatabase<BackstoryDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == humanData.Stories.ChildhoodStoryDefName);
                }

                if (humanData.Stories.AdulthoodStoryDefName != "null")
                {
                    pawn.story.Adulthood = DefDatabase<BackstoryDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == humanData.Stories.AdulthoodStoryDefName);
                }
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void SetPawnSkills(Pawn pawn, HumanFile humanData)
        {
            if (humanData.Skills.Length > 0)
            {
                for (int i = 0; i < humanData.Skills.Length; i++)
                {
                    try
                    {
                        SkillComponent component = humanData.Skills[i];
                        pawn.skills.skills[i].levelInt = component.Level;

                        Enum.TryParse(component.Passion, true, out Passion passion);
                        pawn.skills.skills[i].passion = passion;
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }
            }
        }

        private static void SetPawnTraits(Pawn pawn, HumanFile humanData)
        {
            try { pawn.story.traits.allTraits.Clear(); }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            if (humanData.Traits.Length > 0)
            {
                for (int i = 0; i < humanData.Traits.Length; i++)
                {
                    try
                    {
                        TraitComponent component = humanData.Traits[i];
                        TraitDef traitDef = DefDatabase<TraitDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == component.DefName);
                        Trait trait = new Trait(traitDef, component.Degree);
                        pawn.story.traits.GainTrait(trait);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }
            }
        }

        private static void SetPawnApparel(Pawn pawn, HumanFile humanData)
        {
            try
            {
                pawn.apparel.DestroyAll();
                pawn.apparel.DropAllOrMoveAllToInventory();
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            if (humanData.Apparel.Length > 0)
            {
                for (int i = 0; i < humanData.Apparel.Length; i++)
                {
                    try
                    {
                        ApparelComponent component = humanData.Apparel[i];
                        Apparel apparel = (Apparel)ThingScribeManager.StringToItem(component.EquippedApparel);
                        if (component.WornByCorpse) apparel.WornByCorpse.MustBeTrue();
                        else apparel.WornByCorpse.MustBeFalse();

                        pawn.apparel.Wear(apparel);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }
            }
        }

        private static void SetPawnEquipment(Pawn pawn, HumanFile humanData)
        {
            try { pawn.equipment.DestroyAllEquipment(); }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            if (humanData.Weapon != null)
            {
                try
                {
                    ThingWithComps thing = (ThingWithComps)ThingScribeManager.StringToItem(humanData.Weapon);
                    pawn.equipment.AddEquipment(thing);
                }
                catch (Exception e) { Logger.Warning(e.ToString()); }
            }
        }

        private static void SetPawnInventory(Pawn pawn, HumanFile humanData)
        {
            if (humanData.Items.Length > 0)
            {
                for (int i = 0; i < humanData.Items.Length; i++)
                {
                    try
                    {
                        ItemComponent component = humanData.Items[i];
                        Thing thing = ThingScribeManager.StringToItem(component.Item);
                        pawn.inventory.TryAddAndUnforbid(thing);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }
            }
        }

        private static void SetPawnTransform(Pawn pawn, HumanFile humanData)
        {
            try
            {
                pawn.Position = new IntVec3(humanData.Transform.Position[0], humanData.Transform.Position[1], humanData.Transform.Position[2]);
                pawn.Rotation = new Rot4(humanData.Transform.Rotation);
            }
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

        public static AnimalFile AnimalToString(Pawn animal)
        {
            AnimalFile animalData = new AnimalFile();

            GetAnimalBioDetails(animal, animalData);

            GetAnimalKind(animal, animalData);

            GetAnimalFaction(animal, animalData);

            GetAnimalHediffs(animal, animalData);

            GetAnimalSkills(animal, animalData);

            GetAnimalTransform(animal, animalData);

            return animalData;
        }

        public static Pawn StringToAnimal(AnimalFile animalData)
        {
            PawnKindDef kind = SetAnimalKind(animalData);

            Faction faction = SetAnimalFaction(animalData);

            Pawn animal = SetAnimal(kind, faction, animalData);

            SetAnimalBioDetails(animal, animalData);

            SetAnimalHediffs(animal, animalData);

            SetAnimalSkills(animal, animalData);

            SetAnimalTransform(animal, animalData);

            return animal;
        }

        //Getters

        private static void GetAnimalBioDetails(Pawn animal, AnimalFile animalData)
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

        private static void GetAnimalKind(Pawn animal, AnimalFile animalData)
        {
            try { animalData.KindDef = animal.kindDef.defName; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetAnimalFaction(Pawn animal, AnimalFile animalData)
        {
            if (animal.Faction == null) return;

            try { animalData.FactionDef = animal.Faction.def.defName; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetAnimalHediffs(Pawn animal, AnimalFile animalData)
        {
            if (animal.health.hediffSet.hediffs.Count() > 0)
            {
                List<HediffComponent> toGet = new List<HediffComponent>();

                foreach (Hediff hd in animal.health.hediffSet.hediffs)
                {
                    try
                    {
                        HediffComponent component = new HediffComponent();
                        component.DefName = hd.def.defName;

                        if (hd.Part != null) component.PartDefName = hd.Part.def.defName;
                        else component.PartDefName = "null";

                        component.Severity = hd.Severity;
                        component.IsPermanent = hd.IsPermanent();

                        toGet.Add(component);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }

                animalData.Hediffs = toGet.ToArray();
            }
        }

        private static void GetAnimalSkills(Pawn animal, AnimalFile animalData)
        {
            if (animal.training == null) return;

            List<TrainableComponent> toGet = new List<TrainableComponent>();

            foreach (TrainableDef trainable in DefDatabase<TrainableDef>.AllDefsListForReading)
            {
                try
                {
                    TrainableComponent component = new TrainableComponent();
                    component.DefName = trainable.defName;
                    component.CanTrain = animal.training.CanAssignToTrain(trainable).Accepted;
                    component.HasLearned = animal.training.HasLearned(trainable);
                    component.IsDisabled = animal.training.GetWanted(trainable);

                    toGet.Add(component);
                }
                catch (Exception e) { Logger.Warning(e.ToString()); }
            }

            animalData.Trainables = toGet.ToArray();
        }

        private static void GetAnimalTransform(Pawn animal, AnimalFile animalData)
        {
            try
            {
                animalData.Transform.Position = new int[] { animal.Position.x, animal.Position.y, animal.Position.z};
                animalData.Transform.Rotation = animal.Rotation.AsInt;
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        //Setters

        private static PawnKindDef SetAnimalKind(AnimalFile animalData)
        {
            try { return DefDatabase<PawnKindDef>.AllDefs.First(fetch => fetch.defName == animalData.DefName); }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            return null;
        }

        private static Faction SetAnimalFaction(AnimalFile animalData)
        {
            if (animalData.FactionDef == null) return null;

            try { return Find.FactionManager.AllFactions.First(fetch => fetch.def.defName == animalData.FactionDef); }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            return null;
        }

        private static Pawn SetAnimal(PawnKindDef kind, Faction faction, AnimalFile animalData)
        {
            try { return PawnGenerator.GeneratePawn(kind, faction); }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            return null;
        }

        private static void SetAnimalBioDetails(Pawn animal, AnimalFile animalData)
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

        private static void SetAnimalHediffs(Pawn animal, AnimalFile animalData)
        {
            try
            {
                animal.health.RemoveAllHediffs();
                animal.health.Reset();
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            if (animalData.Hediffs.Length > 0)
            {
                for (int i = 0; i < animalData.Hediffs.Length; i++)
                {
                    try
                    {
                        HediffComponent component = animalData.Hediffs[i];
                        HediffDef hediffDef = DefDatabase<HediffDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == component.DefName);
                        BodyPartRecord bodyPart = null;

                        if (component.PartDefName != "null")
                        {
                            bodyPart = animal.RaceProps.body.AllParts.ToList().Find(x =>
                                x.def.defName == component.PartDefName);
                        }

                        Hediff hediff = HediffMaker.MakeHediff(hediffDef, animal, bodyPart);
                        hediff.Severity = component.Severity;

                        if (component.IsPermanent)
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

        private static void SetAnimalSkills(Pawn animal, AnimalFile animalData)
        {
            if (animalData.Trainables.Length > 0)
            {
                for (int i = 0; i < animalData.Trainables.Length; i++)
                {
                    try
                    {
                        TrainableComponent component = animalData.Trainables[i];
                        TrainableDef trainable = DefDatabase<TrainableDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == component.DefName);
                        if (component.CanTrain) animal.training.Train(trainable, null, complete: component.HasLearned);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString()); }
                }
            }
        }

        private static void SetAnimalTransform(Pawn animal, AnimalFile animalData)
        {
            try
            {
                animal.Position = new IntVec3(animalData.Transform.Position[0], animalData.Transform.Position[1], animalData.Transform.Position[2]);
                animal.Rotation = new Rot4(animalData.Transform.Rotation);
            }
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

        public static ThingFile ItemToString(Thing thing, int thingCount)
        {
            ThingDataFile thingData = new ThingDataFile();
            Thing toUse = null;
            
            if (GetItemMinified(thing, thingData)) toUse = thing.GetInnerIfMinified();
            else toUse = thing;

            GetItemID(toUse, thingData);

            GetItemName(toUse, thingData);

            GetItemMaterial(toUse, thingData);

            GetItemQuantity(toUse, thingData, thingCount);

            GetItemQuality(toUse, thingData);

            GetItemHitpoints(toUse, thingData);

            GetItemTransform(toUse, thingData);

            if (DeepScribeHelper.CheckIfThingIsGenepack(toUse)) GetGenepackDetails(toUse, thingData);
            else if (DeepScribeHelper.CheckIfThingIsBook(toUse)) GetBookDetails(toUse, thingData);
            else if (DeepScribeHelper.CheckIfThingIsXenoGerm(toUse)) GetXenoGermDetails(toUse, thingData);
            return thingData;
        }

        public static Thing StringToItem(ThingFile thingData)
        {

            Thing thing = SetItem(thingData);

            //SetItemID(thing, thingData);

            SetItemQuantity(thing, thingData);

            SetItemQuality(thing, thingData);

            SetItemHitpoints(thing, thingData);

            SetItemTransform(thing, thingData);

            if (DeepScribeHelper.CheckIfThingIsGenepack(thing)) SetGenepackDetails(thing, thingData);
            else if (DeepScribeHelper.CheckIfThingIsBook(thing)) SetBookDetails(thing, thingData);
            else if (DeepScribeHelper.CheckIfThingIsXenoGerm(thing)) SetXenoGermDetails(thing, thingData);
            return thing;
        }

        //Getters

        private static void GetItemID(Thing thing, ThingFile thingData)
        {
            try
            {
                thingData.ThingID = thing.ThingID;
                thingData.ThingIDNumber = thing.thingIDNumber;
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetItemName(Thing thing, ThingFile thingData)
        {
            try { thingData.DefName = thing.def.defName; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetItemMaterial(Thing thing, ThingFile thingData)
        {
            try
            {
                if (DeepScribeHelper.CheckIfThingHasMaterial(thing)) thingData.MaterialDefName = thing.Stuff.defName;
                else thingData.MaterialDefName = null;
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetItemQuantity(Thing thing, ThingFile thingData, int thingCount)
        {
            try { thingData.Quantity = thingCount; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetItemQuality(Thing thing, ThingFile thingData)
        {
            try { thingData.Quality = DeepScribeHelper.GetThingQuality(thing); }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetItemHitpoints(Thing thing, ThingFile thingData)
        {
            try { thingData.Hitpoints = thing.HitPoints; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetItemTransform(Thing thing, ThingFile thingData)
        {
            try
            {
                thingData.Transform.Position = new int[] { thing.Position.x, thing.Position.y, thing.Position.z };
                thingData.Transform.Rotation = thing.Rotation.AsInt;
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static bool GetItemMinified(Thing thing, ThingFile thingData)
        {
            try
            {
                thingData.IsMinified = DeepScribeHelper.CheckIfThingIsMinified(thing);
                return thingData.IsMinified;
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }

            return false;
        }

        private static void GetGenepackDetails(Thing thing, ThingFile thingData)
        {
            try
            {
                Genepack genepack = (Genepack)thing;
                foreach (GeneDef gene in genepack.GeneSet.GenesListForReading) thingData.GenepackData.genepackDefs.Add(gene.defName);
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetBookDetails(Thing thing, ThingFile thingData)
        {
            try
            {
                BookComponent bookData = new BookComponent();
                Book book = (Book)thing;
                bookData.Title = book.Title;
                bookData.Description = book.DescriptionDetailed;
                bookData.DescriptionFlavor = book.FlavorUI;

                Type type = book.GetType();
                FieldInfo fieldInfo = type.GetField("mentalBreakChancePerHour", BindingFlags.NonPublic | BindingFlags.Instance);
                bookData.MentalBreakChance = (float)fieldInfo.GetValue(book);

                type = book.GetType();
                fieldInfo = type.GetField("joyFactor", BindingFlags.NonPublic | BindingFlags.Instance);
                bookData.JoyFactor = (float)fieldInfo.GetValue(book);

                book.BookComp.TryGetDoer<BookOutcomeDoerGainSkillExp>(out BookOutcomeDoerGainSkillExp xp);
                if (xp != null)
                {
                    foreach (KeyValuePair<SkillDef, float> pair in xp.Values)
                    {
                        bookData.SkillData.Add(pair.Key.defName, pair.Value);
                    }
                }

                book.BookComp.TryGetDoer<ReadingOutcomeDoerGainResearch>(out ReadingOutcomeDoerGainResearch research);
                if (research != null)
                {
                    type = research.GetType();
                    fieldInfo = type.GetField("values", BindingFlags.NonPublic | BindingFlags.Instance);
                    Dictionary<ResearchProjectDef, float> researchDict = (Dictionary<ResearchProjectDef, float>)fieldInfo.GetValue(research);
                    foreach (ResearchProjectDef key in researchDict.Keys) bookData.ResearchData.Add(key.defName, researchDict[key]);
                }

                thingData.BookData = bookData;
                Logger.Warning(bookData.title);
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetXenoGermDetails(Thing thing, ThingDataFile thingDataFile) 
        {
            try 
            {
                Xenogerm germData = (Xenogerm)thing;
                foreach (GeneDef gene in germData.GeneSet.GenesListForReading) thingDataFile.XenoGermData.geneDefs.Add(gene.defName);
                thingDataFile.XenoGermData.xenoTypeName = germData.xenotypeName;
                thingDataFile.XenoGermData.iconDef = germData.iconDef.defName;
            } catch (Exception e ) { Logger.Warning(e.ToString()); } 
        }

        //Setters

        private static Thing SetItem(ThingFile thingData)
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

        private static void SetItemID(Thing thing, ThingFile thingData)
        {
            try
            {
                thing.ThingID = thingData.ThingID;
                thing.thingIDNumber = thing.thingIDNumber;
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void SetItemQuantity(Thing thing, ThingFile thingData)
        {
            try { thing.stackCount = thingData.Quantity; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void SetItemQuality(Thing thing, ThingFile thingData)
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

        private static void SetItemHitpoints(Thing thing, ThingFile thingData)
        {
            try { thing.HitPoints = thingData.Hitpoints; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void SetItemTransform(Thing thing, ThingFile thingData)
        {
            try
            { 
                thing.Position = new IntVec3(thingData.Transform.Position[0], thingData.Transform.Position[1], thingData.Transform.Position[2]);
                thing.Rotation = new Rot4(thingData.Transform.Rotation);
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void SetGenepackDetails(Thing thing, ThingFile thingData)
        {
            try
            {
                Genepack genepack = (Genepack)thing;
                List<GeneDef> geneDefs = new List<GeneDef>();
                foreach (string str in thingData.GenepackData.genepackDefs)
                {
                    GeneDef gene = DefDatabase<GeneDef>.AllDefs.First(fetch => fetch.defName == str);
                    geneDefs.Add(gene);
                }
                genepack.Initialize(geneDefs);
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void SetBookDetails(Thing thing, ThingFile thingData)
        {
            try
            {
                Book book = (Book)thing;
                Type type = book.GetType();

                FieldInfo fieldInfo = type.GetField("title", BindingFlags.NonPublic | BindingFlags.Instance);
                fieldInfo.SetValue(book, thingData.BookComponent.Title);

                fieldInfo = type.GetField("description", BindingFlags.NonPublic | BindingFlags.Instance);
                fieldInfo.SetValue(book, thingData.BookComponent.Description);

                fieldInfo = type.GetField("descriptionFlavor", BindingFlags.NonPublic | BindingFlags.Instance);
                fieldInfo.SetValue(book, thingData.BookComponent.DescriptionFlavor);

                fieldInfo = type.GetField("mentalBreakChancePerHour", BindingFlags.NonPublic | BindingFlags.Instance);
                fieldInfo.SetValue(book, thingData.BookComponent.MentalBreakChance);

                fieldInfo = type.GetField("joyFactor", BindingFlags.NonPublic | BindingFlags.Instance);
                fieldInfo.SetValue(book, thingData.BookComponent.JoyFactor);

                book.BookComp.TryGetDoer<BookOutcomeDoerGainSkillExp>(out BookOutcomeDoerGainSkillExp doerXP);
                if (doerXP != null)
                {
                    type = doerXP.GetType();
                    fieldInfo = type.GetField("values", BindingFlags.NonPublic | BindingFlags.Instance);
                    Dictionary<SkillDef, float> skilldict = new Dictionary<SkillDef, float>();

                    foreach (string str in thingData.BookComponent.SkillData.Keys)
                    {
                        SkillDef skillDef = DefDatabase<SkillDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == str);
                        skilldict.Add(skillDef, thingData.BookComponent.SkillData[str]);
                    }

                    fieldInfo.SetValue(doerXP, skilldict);
                }

                book.BookComp.TryGetDoer<ReadingOutcomeDoerGainResearch>(out ReadingOutcomeDoerGainResearch doerResearch);
                if (doerResearch != null)
                {
                    type = doerResearch.GetType();
                    fieldInfo = type.GetField("values", BindingFlags.NonPublic | BindingFlags.Instance);
                    Dictionary<ResearchProjectDef, float> researchDict = new Dictionary<ResearchProjectDef, float>();

                    foreach (string str in thingData.BookComponent.ResearchData.Keys)
                    {
                        ResearchProjectDef researchDef = DefDatabase<ResearchProjectDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == str);
                        researchDict.Add(researchDef, thingData.BookComponent.ResearchData[str]);
                    }

                    fieldInfo.SetValue(doerResearch, researchDict);
                }
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void SetXenoGermDetails(Thing thing, ThingDataFile thingDataFile)
        {
            try
            {
                Xenogerm germData = (Xenogerm)thing;
                List<Genepack> genePacks = new List<Genepack>();
                foreach (string genepacks in thingDataFile.XenoGermData.geneDefs) 
                {
                    Genepack genepack = new Genepack();
                    List<GeneDef> geneDefs = new List<GeneDef>();
                    geneDefs.Add(DefDatabase<GeneDef>.GetNamed(genepacks));
                    genepack.Initialize(geneDefs);
                    genePacks.Add(genepack);
                }
                germData.Initialize(genePacks, thingDataFile.XenoGermData.xenoTypeName, DefDatabase<XenotypeIconDef>.GetNamed(thingDataFile.XenoGermData.iconDef));
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }
    }

    //Class that handles transformation of maps

    public static class MapScribeManager
    {
        //Functions

        public static MapFile MapToString(Map map, bool factionThings, bool nonFactionThings, bool factionHumans, bool nonFactionHumans, bool factionAnimals, bool nonFactionAnimals)
        {
            MapFile mapFile = new MapFile();

            GetMapTile(mapFile, map);

            GetMapSize(mapFile, map);

            GetMapTerrain(mapFile, map);

            GetMapThings(mapFile, map, factionThings, nonFactionThings);

            GetMapHumans(mapFile, map, factionHumans, nonFactionHumans);

            GetMapAnimals(mapFile, map, factionAnimals, nonFactionAnimals);

            GetMapWeather(mapFile, map);

            return mapFile;
        }

        public static Map StringToMap(MapFile mapFile, bool factionThings, bool nonFactionThings, bool factionHumans, bool nonFactionHumans, bool factionAnimals, bool nonFactionAnimals, bool lessLoot = false)
        {
            Map map = SetEmptyMap(mapFile);

            SetMapTerrain(mapFile, map);

            if (factionThings || nonFactionThings) SetMapThings(mapFile, map, factionThings, nonFactionThings, lessLoot);

            if (factionHumans || nonFactionHumans) SetMapHumans(mapFile, map, factionHumans, nonFactionHumans);

            if (factionAnimals || nonFactionAnimals) SetMapAnimals(mapFile, map, factionAnimals, nonFactionAnimals);

            SetWeatherData(mapFile, map);

            SetMapFog(map);

            SetMapRoofs(map);

            return map;
        }

        //Getters

        private static void GetMapTile(MapFile mapFile, Map map)
        {
            try { mapFile.Tile = map.Tile; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetMapSize(MapFile mapFile, Map map)
        {
            try { mapFile.Size = ValueParser.IntVec3ToArray(map.Size); }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetMapTerrain(MapFile mapFile, Map map)
        {
            try 
            {
                List<TileComponent> toGet = new List<TileComponent>();

                for (int z = 0; z < map.Size.z; ++z)
                {
                    for (int x = 0; x < map.Size.x; ++x)
                    {
                        TileComponent component = new TileComponent();
                        IntVec3 vectorToCheck = new IntVec3(x, map.Size.y, z);
                        component.DefName = map.terrainGrid.TerrainAt(vectorToCheck).defName;
                        component.IsPolluted = map.pollutionGrid.IsPolluted(vectorToCheck);

                        if (map.roofGrid.RoofAt(vectorToCheck) == null) component.RoofDefName = "null";
                        else component.RoofDefName = map.roofGrid.RoofAt(vectorToCheck).defName;

                        toGet.Add(component);
                    }
                }

                mapFile.Tiles = toGet.ToArray();
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetMapThings(MapFile mapFile, Map map, bool factionThings, bool nonFactionThings)
        {
            try 
            {
                List<ThingFile> tempFactionThings = new List<ThingFile>();
                List<ThingFile> tempNonFactionThings = new List<ThingFile>();

                foreach (Thing thing in map.listerThings.AllThings)
                {
                    if (!DeepScribeHelper.CheckIfThingIsHuman(thing) && !DeepScribeHelper.CheckIfThingIsAnimal(thing))
                    {
                        ThingFile thingData = ThingScribeManager.ItemToString(thing, thing.stackCount);

                        if (thing.def.alwaysHaulable && factionThings) tempFactionThings.Add(thingData);
                        else if (!thing.def.alwaysHaulable && nonFactionThings) tempNonFactionThings.Add(thingData);

                        if (DeepScribeHelper.CheckIfThingCanGrow(thing))
                        {
                            try
                            {
                                Plant plant = thing as Plant;
                                thingData.PlantComponent.GrowthTicks = plant.Growth;
                            }
                            catch (Exception e) { Logger.Warning(e.ToString()); }
                        }
                    }
                }

                mapFile.FactionThings = tempFactionThings.ToArray();
                mapFile.NonFactionThings = tempNonFactionThings.ToArray();
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetMapHumans(MapFile mapFile, Map map, bool factionHumans, bool nonFactionHumans)
        {
            try 
            {
                List<HumanFile> tempFactionHumans = new List<HumanFile>();
                List<HumanFile> tempNonFactionHumans = new List<HumanFile>();

                foreach (Thing thing in map.listerThings.AllThings)
                {
                    if (DeepScribeHelper.CheckIfThingIsHuman(thing))
                    {
                        HumanFile humanData = HumanScribeManager.HumanToString(thing as Pawn);

                        if (thing.Faction == Faction.OfPlayer && factionHumans) tempFactionHumans.Add(humanData);
                        else if (thing.Faction != Faction.OfPlayer && nonFactionHumans) tempNonFactionHumans.Add(humanData);
                    }
                }

                mapFile.FactionHumans = tempFactionHumans.ToArray();
                mapFile.NonFactionHumans = tempNonFactionHumans.ToArray();
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetMapAnimals(MapFile mapFile, Map map, bool factionAnimals, bool nonFactionAnimals)
        {
            try 
            {
                List<AnimalFile> tempFactionAnimals = new List<AnimalFile>();
                List<AnimalFile> tempNonFactionAnimals = new List<AnimalFile>();

                foreach (Thing thing in map.listerThings.AllThings)
                {
                    if (DeepScribeHelper.CheckIfThingIsAnimal(thing))
                    {
                        AnimalFile animalData = AnimalScribeManager.AnimalToString(thing as Pawn);

                        if (thing.Faction == Faction.OfPlayer && factionAnimals) tempFactionAnimals.Add(animalData);
                        else if (thing.Faction != Faction.OfPlayer && nonFactionAnimals) tempNonFactionAnimals.Add(animalData);
                    }
                }

                mapFile.FactionAnimals = tempFactionAnimals.ToArray();
                mapFile.NonFactionAnimals = tempNonFactionAnimals.ToArray();
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void GetMapWeather(MapFile mapFile, Map map)
        {
            try { mapFile.CurWeatherDefName = map.weatherManager.curWeather.defName; }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        //Setters

        private static Map SetEmptyMap(MapFile mapFile)
        {
            IntVec3 mapSize = ValueParser.ArrayToIntVec3(mapFile.Size);

            PlanetManagerHelper.SetOverrideGenerators();
            Map toReturn = GetOrGenerateMapUtility.GetOrGenerateMap(SessionValues.chosenSettlement.Tile, mapSize, null);
            PlanetManagerHelper.SetDefaultGenerators();

            return toReturn;
        }

        private static void SetMapTerrain(MapFile mapFile, Map map)
        {
            try
            {
                int index = 0;

                for (int z = 0; z < map.Size.z; ++z)
                {
                    for (int x = 0; x < map.Size.x; ++x)
                    {
                        TileComponent component = mapFile.Tiles[index];
                        IntVec3 vectorToCheck = new IntVec3(x, map.Size.y, z);

                        try
                        {
                            TerrainDef terrainToUse = DefDatabase<TerrainDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == component.DefName);
                            map.terrainGrid.SetTerrain(vectorToCheck, terrainToUse);
                            map.pollutionGrid.SetPolluted(vectorToCheck, component.IsPolluted);
                        }
                        catch (Exception e) { Logger.Warning(e.ToString()); }

                        try
                        {
                            RoofDef roofToUse = DefDatabase<RoofDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == component.RoofDefName);
                            map.roofGrid.SetRoof(vectorToCheck, roofToUse);
                        }
                        catch (Exception e) { Logger.Warning(e.ToString()); }

                        index++;
                    }
                }
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        private static void SetMapThings(MapFile mapFile, Map map, bool factionThings, bool nonFactionThings, bool lessLoot)
        {
            try
            {
                List<Thing> thingsToGetInThisTile = new List<Thing>();

                if (factionThings)
                {
                    Random rnd = new Random();

                    foreach (ThingFile item in mapFile.FactionThings)
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
                                plant.Growth = item.PlantComponent.GrowthTicks;
                            }
                        }
                        catch (Exception e) { Logger.Warning(e.ToString()); }
                    }
                }

                if (nonFactionThings)
                {
                    foreach (ThingFile item in mapFile.NonFactionThings)
                    {
                        try
                        {
                            Thing toGet = ThingScribeManager.StringToItem(item);
                            thingsToGetInThisTile.Add(toGet);

                            if (DeepScribeHelper.CheckIfThingCanGrow(toGet))
                            {
                                Plant plant = toGet as Plant;
                                plant.Growth = item.PlantComponent.GrowthTicks;
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

        private static void SetMapHumans(MapFile mapFile, Map map, bool factionHumans, bool nonFactionHumans)
        {
            try
            {
                if (factionHumans)
                {
                    foreach (HumanFile pawn in mapFile.FactionHumans)
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
                    foreach (HumanFile pawn in mapFile.NonFactionHumans)
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

        private static void SetMapAnimals(MapFile mapFile, Map map, bool factionAnimals, bool nonFactionAnimals)
        {
            try
            {
                if (factionAnimals)
                {
                    foreach (AnimalFile pawn in mapFile.FactionAnimals)
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
                    foreach (AnimalFile pawn in mapFile.NonFactionAnimals)
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

        private static void SetWeatherData(MapFile mapFile, Map map)
        {
            try
            {
                WeatherDef weatherDef = DefDatabase<WeatherDef>.AllDefs.First(fetch => fetch.defName == mapFile.CurWeatherDefName);
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

            if (thing.def.defName == ThingDefOf.TextBook.defName) return true;
            else if (thing.def.defName == ThingDefOf.Schematic.defName) return true;
            else if (thing.def.defName == ThingDefOf.Novel.defName) return true;
            if (!ModsConfig.AnomalyActive) return false;
            if (thing.def.defName == ThingDefOf.Tome.defName) return true;
            return false;
        }

        public static bool CheckIfThingIsGenepack(Thing thing)
        {
            if (!ModsConfig.BiotechActive) return false;

            if (thing.def.defName == ThingDefOf.Genepack.defName) return true;
            return false;
        }

        public static bool CheckIfThingIsXenoGerm(Thing thing) 
        {
            if(!ModsConfig.BiotechActive) return false;

            if (thing.def.defName == ThingDefOf.Xenogerm.defName) return true;
            else return false;
        }
    }
}
