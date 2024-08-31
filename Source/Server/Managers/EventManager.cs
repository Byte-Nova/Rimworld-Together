using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class EventManager
    {
        public static void ParseEventPacket(ServerClient client, Packet packet)
        {
            if (!Master.actionValues.EnableEvents)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

            EventData eventData = Serializer.ConvertBytesToObject<EventData>(packet.contents);

            switch (eventData._stepMode)
            {
                case EventStepMode.Send:
                    SendEvent(client, eventData);
                    break;

                case EventStepMode.Receive:
                    //Nothing goes here
                    break;

                case EventStepMode.Recover:
                    //Nothing goes here
                    break;
            }
        }

        public static void LoadEvents()
        {
            EventManagerHelper.CheckForEventFiles();
            EventManagerHelper.LoadAllEvents();

            Logger.Warning($"Loaded > {EventManagerHelper.loadedEvents.Length} events from '{Master.eventsPath}'");
        }

        public static void SendEvent(ServerClient client, EventData eventData)
        {
            if (!SettlementManager.CheckIfTileIsInUse(eventData._toTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Username} attempted to send an event to settlement at tile {eventData._toTile}, but it has no settlement");
            else
            {
                SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(eventData._toTile);
                if (!UserManagerHelper.CheckIfUserIsConnected(settlement.Owner))
                {
                    eventData._stepMode = EventStepMode.Recover;
                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.EventPacket), eventData);
                    client.listener.EnqueuePacket(packet);
                }

                else
                {
                    ServerClient target = UserManagerHelper.GetConnectedClientFromUsername(settlement.Owner);

                    if (Master.serverConfig.TemporalEventProtection && !TimeConverter.CheckForEpochTimer(target.userFile.EventProtectionTime, EventManagerHelper.baseMaxTimer))
                    {
                        eventData._stepMode = EventStepMode.Recover;
                        Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.EventPacket), eventData);
                        client.listener.EnqueuePacket(packet);
                    }

                    else
                    {
                        //Back to player

                        Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.EventPacket), eventData);
                        client.listener.EnqueuePacket(packet);

                        //To the person that should receive it

                        eventData._stepMode = EventStepMode.Receive;

                        target.userFile.UpdateEventTime();

                        packet = Packet.CreatePacketFromObject(nameof(PacketHandler.EventPacket), eventData);
                        target.listener.EnqueuePacket(packet);
                    }
                }
            }
        }
    }

    public static class EventManagerHelper
    {
        //Variables

        public static readonly double baseMaxTimer = 3600000;

        public static readonly string fileExtension = ".mpevent";

        public static EventFile[] loadedEvents;

        public static readonly Dictionary<string, string> baseEvents = new Dictionary<string, string>()
        {
            {"Ambush", "Ambush"},
            {"ManhunterAmbush", "Manhunter ambush"},
            {"CaravanMeeting", "Caravan meeting"},
            {"CaravanDemand", "Payment demand"},
            {"Disease_Flu", "Flu"},
            {"Disease_Plague", "Plague"},
            {"Disease_Malaria", "Malaria"},
            {"Disease_SleepingSickness", "Sleeping sickness"},
            {"Disease_FibrousMechanites", "Fibrous mechanites"},
            {"Disease_SensoryMechanites", "Sensory mechanites"},
            {"Disease_GutWorms", "Gut worms"},
            {"Disease_MuscleParasites", "Muscle parasites"},
            {"Disease_AnimalFlu", "Flu (animals)"},
            {"Disease_AnimalPlague", "Plague (animals)"},
            {"Disease_OrganDecay", "Organ decay"},
            {"ResourcePodCrash", "Resource pod crash"},
            {"PsychicSoothe", "Psychic soothe"},
            {"SelfTame", "Self-tame"},
            {"AmbrosiaSprout", "Ambrosia sprout"},
            {"FarmAnimalsWanderIn", "Farm animals wander in"},
            {"WandererJoin", "Wanderer join"},
            {"RefugeePodCrash", "Transport pod crash"},
            {"ThrumboPasses", "Thrumbos pass"},
            {"RansomDemand", "Ransom demand"},
            {"MeteoriteImpact", "Meteorite impact"},
            {"HerdMigration", "Herd migration"},
            {"WildManWandersIn", "Wild man wanders in"},
            {"PsychicDrone", "Psychic drone"},
            {"ToxicFallout", "Toxic fallout"},
            {"VolcanicWinter", "Volcanic winter"},
            {"HeatWave", "Heat wave"},
            {"ColdSnap", "Cold snap"},
            {"Flashstorm", "Flashstorm"},
            {"ShortCircuit", "Short circuit"},
            {"CropBlight", "Crop blight"},
            {"Alphabeavers", "Alphabeavers"},
            {"ShipChunkDrop", "Ship chunk drop"},
            {"OrbitalTraderArrival", "Orbital trader arrival"},
            {"TraderCaravanArrival", "Trader caravan arrival"},
            {"VisitorGroup", "Visitor group"},
            {"TravelerGroup", "Traveler group"},
            {"RaidFriendly", "Friendly raid"},
            {"StrangerInBlackJoin", "Man in black"},
            {"GameEndedWanderersJoin", "Wanderers join"},
            {"RaidEnemy", "Enemy raid"},
            {"Infestation", "Infestation"},
            {"DeepDrillInfestation", "Deep drill infestation"},
            {"ManhunterPack", "Manhunter pack"},
            {"DefoliatorShipPartCrash", "Ship part crash (defoliator)"},
            {"PsychicEmanatorShipPartCrash", "Ship part crash (psychic)"},
            {"AnimalInsanityMass", "Mass animal insanity"},
            {"AnimalInsanitySingle", "Single animal insanity"},
            {"Eclipse", "Eclipse"},
            {"SolarFlare", "Solar flare"},
            {"Aurora", "Aurora"},
            {"GiveQuest_Random", "Quest"},
            {"GiveQuest_EndGame_ShipEscape", "Journey offer"},
            {"WandererJoinAbasia", "Abasia join"},
            {"MechCluster", "Mech cluster"},
            {"CaravanArrivalTributeCollector", "Tribute collector caravan arrival"},
            {"AnimaTreeSpawn", "Anima tree"},
            {"GiveQuest_EndGame_RoyalAscent", "Royal ascent"},
            {"GiveQuest_Intro_Wimp", "Imperial wimp"},
            {"GiveQuest_Intro_Deserter", "Imperial deserter"},
            {"ProblemCauser", "Problem causer"},
            {"Disease_Abasia", "Paralytic abasia"},
            {"Disease_BloodRot", "Blood rot"},
            {"GiveQuest_Beggars", "Beggars arrive"},
            {"GiveQuest_ReliquaryPilgrims", "Pilgrims arrive"},
            {"GauranlenPodSpawn", "Gauranlen pod sprout"},
            {"Infestation_Jelly", "Insect jelly"},
            {"WanderersSkylantern", "Wanderers"},
            {"GiveQuest_WorkSite", "Work site"},
            {"GiveQuest_EndGame_ArchonexusVictory", "Archonexus victory"},
            {"GiveQuest_AncientComplex_Mechanitor", "Ancient mechanitor complex"},
            {"RefugeePodCrash_Baby", "Transport pod crash"},
            {"PoluxTreeSpawn", "Polux tree"},
            {"NoxiousHaze", "Acidic smog"},
            {"WastepackInfestation", "Wastepack infestation"},
            {"UnnaturalDarkness", "Unnatural darkness"},
            {"DeathPall", "Death pall"},
            {"MetalhorrorImplantation", "Metalhorror implantation"},
            {"WarpedObelisk_Mutator", "Twisted obelisk"},
            {"WarpedObelisk_Duplicator", "Corrupted obelisk"},
            {"WarpedObelisk_Abductor", "Warped obelisk"},
            {"Nociosphere", "Nociosphere"},
            {"PitGate", "Pit gate"},
            {"FleshmassHeart", "Fleshmass heart"},
            {"CreepJoinerJoin", "Creepjoiner join"},
            {"CreepJoinerJoin_Metalhorror", "Creepjoiner join"},
            {"BloodRain", "Blood rain"},
            {"MysteriousCargoUnnaturalCorpse", "Mysterious cargo corpse"},
            {"MysteriousCargoCube", "Mysterious cargo cube"},
            {"MysteriousCargoRevenantSpine", "Mysterious cargo revenant spine"},
            {"GiveQuest_DistressCall", "Distress signal"},
            {"RevenantEmergence", "Revenant emergence"},
            {"UnnaturalCorpseArrival", "Unnatural corpse arrival"},
            {"GoldenCubeArrival", "Golden cube arrival"},
            {"HarbingerTreeProvoked", "Harbinger tree"},
            {"HarbingerTreeSpawn", "Harbinger tree"},
            {"MonolithMigration", "Strange signal"},
            {"RefugeePodCrash_Ghoul", "Ghoul transport pod crash"},
            {"VoidCuriosity", "Void curiosity"},
            {"ShamblerSwarm", "Shambler swarm"},
            {"ShamblerSwarmAnimals", "Shambler swarm"},
            {"SmallShamblerSwarm", "Shambler swarm"},
            {"SightstealerSwarm", "Sightstealer swarm"},
            {"ShamblerAssault", "Shambler assault"},
            {"GhoulAttack", "Ghoul attacking"},
            {"Revenant", "Revenant"},
            {"SightstealerArrival", "Sightstealer arrival"},
            {"PsychicRitualSiege", "Psychic ritual siege"},
            {"HateChanters", "Hate chanters"},
            {"FrenziedAnimals", "Frenzied animals"},
            {"FleshbeastAttack", "Fleshbeast attack"},
            {"GorehulkAssault", "Gorehulk assault"},
            {"DevourerAssault", "Devourer assault"},
            {"DevourerWaterAssault", "Devourer assault"},
            {"ChimeraAssault", "Chimera assault"}
        };

        public static void CheckForEventFiles()
        {
            List<string> foundEvents = new List<string>();

            foreach(string str in Directory.GetFiles(Master.eventsPath))
            {
                foundEvents.Add(Path.GetFileNameWithoutExtension(str));
            }

            foreach(KeyValuePair<string, string> pair in baseEvents)
            {
                if (!foundEvents.Contains(pair.Key))
                {
                    GenerateDefaultEventFile(pair.Key, pair.Value);
                }
            }
        }

        public static void LoadAllEvents()
        {
            List<EventFile> toLoad = new List<EventFile>();
            foreach(string str in Directory.GetFiles(Master.eventsPath))
            {
                EventFile eventFile = Serializer.SerializeFromFile<EventFile>(str);
                if (eventFile.IsEnabled) toLoad.Add(eventFile);
            }

            loadedEvents = toLoad.OrderBy(fetch => fetch.Name).ToArray();
        }

        public static void GenerateDefaultEventFile(string defName, string name)
        {
            EventFile newEvent = new EventFile();
            newEvent.Name = name;
            newEvent.DefName = defName;
            newEvent.Cost = 500;
            newEvent.IsEnabled = true;

            Serializer.SerializeToFile(Path.Combine(Master.eventsPath, defName + fileExtension), newEvent);
        }
    }
}
