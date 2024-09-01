using Shared;
using System;
using System.Reflection;
using static Shared.CommonEnumerators;

namespace GameServer.Updater
{
    public static class Updater
    {
        private static List<Shared.FactionFile> factions = new List<Shared.FactionFile>();
        public static void Update()
        {
            Backup();
            UpdateDifficulty();
            UpdateUserFile();
            UpdateSettlementFiles();
            UpdateSites();
            UpdateCaravan();
            UpdateWorld();
        }

        private static void Backup()
        {
            string newWorldFolderName = $"World-{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day} {DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}";
            string newWorldFolderPath = $"{Master.backupWorldPath + Path.DirectorySeparatorChar}{newWorldFolderName}";
            Logger.Warning($"The server will be saved as: {newWorldFolderPath}");
            Directory.CreateDirectory($"{newWorldFolderPath + Path.DirectorySeparatorChar}Core");
            Directory.CreateDirectory($"{newWorldFolderPath + Path.DirectorySeparatorChar}Factions");
            Directory.CreateDirectory($"{newWorldFolderPath + Path.DirectorySeparatorChar}Maps");
            Directory.CreateDirectory($"{newWorldFolderPath + Path.DirectorySeparatorChar}Saves");
            Directory.CreateDirectory($"{newWorldFolderPath + Path.DirectorySeparatorChar}Settlements");
            Directory.CreateDirectory($"{newWorldFolderPath + Path.DirectorySeparatorChar}Sites");
            Directory.CreateDirectory($"{newWorldFolderPath + Path.DirectorySeparatorChar}Users");
            Directory.CreateDirectory($"{newWorldFolderPath + Path.DirectorySeparatorChar}Caravans");
            foreach (string file in Directory.GetFiles(Master.corePath))
            {
                if (File.Exists(file)) File.Copy(file, $"{newWorldFolderPath + Path.DirectorySeparatorChar}Core{Path.DirectorySeparatorChar}{Path.GetFileName(file)}");
            }
            foreach (string file in Directory.GetFiles(Master.factionsPath))
            {
                if (File.Exists(file)) File.Copy(file, $"{newWorldFolderPath + Path.DirectorySeparatorChar}Factions{Path.DirectorySeparatorChar}{Path.GetFileName(file)}");
            }
            foreach (string file in Directory.GetFiles(Master.mapsPath))
            {
                if (File.Exists(file)) File.Copy(file, $"{newWorldFolderPath + Path.DirectorySeparatorChar}Maps{Path.DirectorySeparatorChar}{Path.GetFileName(file)}");
            }
            foreach (string file in Directory.GetFiles(Master.savesPath))
            {
                if (File.Exists(file)) File.Copy(file, $"{newWorldFolderPath + Path.DirectorySeparatorChar}Saves{Path.DirectorySeparatorChar}{Path.GetFileName(file)}");
            }
            foreach (string file in Directory.GetFiles(Master.settlementsPath))
            {
                if (File.Exists(file)) File.Copy(file, $"{newWorldFolderPath + Path.DirectorySeparatorChar}Settlements{Path.DirectorySeparatorChar}{Path.GetFileName(file)}");
            }
            foreach (string file in Directory.GetFiles(Master.sitesPath))
            {
                if (File.Exists(file)) File.Copy(file, $"{newWorldFolderPath + Path.DirectorySeparatorChar}Sites{Path.DirectorySeparatorChar}{Path.GetFileName(file)}");
            }
            foreach (string file in Directory.GetFiles(Master.usersPath))
            {
                if (File.Exists(file)) File.Copy(file, $"{newWorldFolderPath + Path.DirectorySeparatorChar}Users{Path.DirectorySeparatorChar}{Path.GetFileName(file)}");
            }
            foreach (string file in Directory.GetFiles(Master.caravansPath))
            {
                if (File.Exists(file)) File.Copy(file, $"{newWorldFolderPath + Path.DirectorySeparatorChar}Caravans{Path.DirectorySeparatorChar}{Path.GetFileName(file)}");
            }

            Main_.SetPaths();
        }
        private static void UpdateDifficulty() 
        {
            string pathToSave = Path.Combine(Master.corePath, "DifficultyValues.json");
            DifficultyData old = Serializer.SerializeFromFile<DifficultyData>(pathToSave);

            Shared.DifficultyData newDifficulty = new Shared.DifficultyData();
            DifficultyValuesFile newValues = new DifficultyValuesFile();

            newValues.UseCustomDifficulty = true;

            Type oldType = typeof(DifficultyData);
            Type newType = typeof(Shared.DifficultyValuesFile);
            FieldInfo[] fields = oldType.GetFields();
            foreach (FieldInfo field in fields)
            {
                try 
                {
                    FieldInfo newField = newType.GetField(field.Name, BindingFlags.Public | BindingFlags.Instance);
                    newField.SetValue(newDifficulty, field.GetValue(old));
                } catch { }
            }
            newDifficulty._values = newValues;
            Serializer.SerializeToFile(pathToSave, newDifficulty);
        }
        private static void UpdateUserFile()
        {

            string[] userFiles = Directory.GetFiles(Master.usersPath);
            foreach(string userFile in userFiles)
            {
                GameServer.Updater.UserFile old = Serializer.SerializeFromFile<UserFile>(userFile);
                GameServer.UserFile newFile = new GameServer.UserFile();
                string[] OldFaction = Directory.GetFiles(Master.factionsPath);
                FactionFile oldFactionSite = new FactionFile();
                foreach (string str in OldFaction) 
                {
                    FactionFile temp = Serializer.SerializeFromFile<FactionFile>(str);
                    if (temp.factionName == old.FactionName)
                    {
                        oldFactionSite = temp;
                        break;
                    }
                }
                newFile.Username = old.Username;
                newFile.Password = old.Password;
                newFile.Uid = old.Uid;
                newFile.IsAdmin = old.IsAdmin;
                newFile.IsBanned = old.IsBanned;
                newFile.SavedIP = old.SavedIP;
                newFile.ActivityProtectionTime = old.ActivityProtectionTime;
                newFile.EventProtectionTime = old.EventProtectionTime;
                newFile.AidProtectionTime = old.AidProtectionTime;
                newFile.RunningMods = old.RunningMods.ToArray();
                Shared.FactionFile factionFile = new Shared.FactionFile();
                factionFile.Name = old.FactionName;
                if (factionFile.Name == "null"){} else 
                {
                    factionFile.CurrentMembers = oldFactionSite.factionMembers;
                    foreach (string str in oldFactionSite.factionMemberRanks)
                    {
                        factionFile.CurrentRanks.Add(int.Parse(str));
                    }
                }
                UserRelationshipsFile relationshipsFile = new UserRelationshipsFile();
                relationshipsFile.AllyPlayers = old.AllyPlayers;
                relationshipsFile.EnemyPlayers = old.EnemyPlayers;
                newFile.FactionFile = factionFile;
                factions.Add(factionFile);
                newFile.Relationships = relationshipsFile;
                Serializer.SerializeToFile(userFile, newFile);
            }
        }

        private static void UpdateSettlementFiles() 
        {
            string[] userFiles = Directory.GetFiles(Master.settlementsPath);
            foreach (string userFile in userFiles) 
            {
                GameServer.Updater.SettlementFile old = Serializer.SerializeFromFile<GameServer.Updater.SettlementFile>(userFile);
                Shared.SettlementFile newfile = new Shared.SettlementFile();
                newfile.Tile = old.tile;
                newfile.Owner = old.owner;
                newfile.Goodwill = Goodwill.Neutral;
                Serializer.SerializeToFile(userFile, newfile);
            }
        }

        private static void UpdateSites() 
        {
            string[] userFiles = Directory.GetFiles(Master.sitesPath);
            foreach (string userFile in userFiles)
            {
                GameServer.Updater.SiteFile old = Serializer.SerializeFromFile<SiteFile>(userFile);
                Shared.SiteFile newfile = new Shared.SiteFile();
                newfile.Tile = old.tile;
                newfile.Owner = old.owner;
                newfile.Type = old.type;
                newfile.WorkerData = old.workerData;
                Shared.FactionFile faction = null;
                foreach (Shared.FactionFile file in factions)
                {
                    if(file.Name == old.factionName) 
                    {
                        faction = file;
                        break;
                    }
                }
                newfile.FactionFile = faction;
                Serializer.SerializeToFile(userFile, newfile);
            }
        }

        private static void UpdateCaravan()
        {
            string[] userFiles = Directory.GetFiles(Master.caravansPath);
            foreach (string userFile in userFiles)
            {
                GameServer.Updater.CaravanDetails old = Serializer.SerializeFromFile<CaravanDetails>(userFile);
                Shared.CaravanData newfile = new Shared.CaravanData();
                Shared.CaravanFile caravan = new Shared.CaravanFile();
                Shared.CaravanData data = new Shared.CaravanData();
                caravan.Tile = old.tile;
                caravan.TimeSinceRefresh = old.timeSinceRefresh;
                caravan.ID = old.ID;
                caravan.Owner = old.owner;
                newfile._caravanFile = caravan;
                Serializer.SerializeToFile(userFile, newfile);
            }
        }

        private static void UpdateWorld() 
        {
            string pathToSave = Path.Combine(Master.corePath, "WorldValues.json");
            WorldValuesFile old = Serializer.SerializeFromFile<WorldValuesFile>(pathToSave);

            Shared.WorldValuesFile newValues = new Shared.WorldValuesFile();

            newValues.Pollution = old.Pollution;
            newValues.Population = old.Population;
            newValues.SeedString = old.SeedString;
            newValues.Rainfall = old.Rainfall;
            newValues.Temperature = old.Temperature;
            newValues.PersistentRandomValue = old.PersistentRandomValue;

            List<Shared.PlanetFeature> newFeatures = new List<Shared.PlanetFeature>();
            foreach (PlanetFeature feature in old.Features)
            {
                Shared.PlanetFeature newfeature = new Shared.PlanetFeature();
                newfeature.defName = feature.defName;
                newfeature.name = feature.featureName;
                newfeature.drawCenter = feature.drawCenter;
                newfeature.maxDrawSizeInTiles = feature.maxDrawSizeInTiles;
                newFeatures.Add(newfeature);
            }
            newValues.Features = newFeatures.ToArray();

            List<Shared.RoadDetails> newRoads = new List<Shared.RoadDetails>();
            foreach(RoadDetails road in old.Roads) 
            {
                Shared.RoadDetails newRoad = new Shared.RoadDetails();
                newRoad.roadDefName = road.roadDefName;
                newRoad.toTile = road.tileB;
                newRoad.fromTile = road.tileA;
                newRoads.Add(newRoad);
            }
            newValues.Roads = newRoads.ToArray();

            List<Shared.RiverDetails> newRivers = new List<Shared.RiverDetails>();
            foreach (RiverDetails river in old.Rivers)
            {
                Shared.RiverDetails newRiver = new Shared.RiverDetails();
                newRiver.riverDefName = river.riverDefName;
                newRiver.fromTile = river.tileA;
                newRiver.toTile = river.tileB;
                newRivers.Add(newRiver);
            }
            newValues.Rivers = newRivers.ToArray();

            List<Shared.PlanetNPCFaction> newNpcFactions = new List<Shared.PlanetNPCFaction>();
            foreach (PlanetNPCFaction faction in old.NPCFactions)
            {
                Shared.PlanetNPCFaction newFaction = new Shared.PlanetNPCFaction();
                newFaction.defName = faction.factionDefName;
                newFaction.name = faction.factionName;
                newFaction.color = faction.factionColor;
                newNpcFactions.Add(newFaction);
            }
            newValues.NPCFactions = newNpcFactions.ToArray();

            List<Shared.PlanetNPCSettlement> newNpcSettlement = new List<Shared.PlanetNPCSettlement>();
            foreach (PlanetNPCSettlement settlement in old.NPCSettlements)
            {
                Shared.PlanetNPCSettlement newSettlement = new Shared.PlanetNPCSettlement();
                newSettlement.defName = settlement.factionDefName;
                newSettlement.name = settlement.name;
                newSettlement.tile = settlement.tile;
                newNpcSettlement.Add(newSettlement);
            }
            newValues.NPCSettlements = newNpcSettlement.ToArray();
            Serializer.SerializeToFile(pathToSave, newValues);
        }
    }
}
