using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;
using Verse.AI;
using static Shared.CommonEnumerators;


namespace GameClient
{
    public static class OnlineVisitManager
    {
        public static List<Pawn> playerPawns = new List<Pawn>();

        public static List<Pawn> otherPlayerPawns = new List<Pawn>();

        public static List<Thing> mapThings = new List<Thing>();

        public static Map visitMap = null;

        public static void ParseVisitPacket(Packet packet)
        {
            VisitData visitData = (VisitData)Serializer.ConvertBytesToObject(packet.contents);

            switch (int.Parse(visitData.visitStepMode))
            {
                case (int)VisitStepMode.Request:
                    OnVisitRequest(visitData);
                    break;

                case (int)VisitStepMode.Accept:
                    OnVisitAccept(visitData);
                    break;

                case (int)VisitStepMode.Reject:
                    OnVisitReject();
                    break;

                case (int)VisitStepMode.Unavailable:
                    OnVisitUnavailable();
                    break;

                case (int)VisitStepMode.Action:
                    VisitActionGetter.ReceiveActions(visitData);
                    break;

                case (int)VisitStepMode.Stop:
                    OnVisitStop();
                    break;
            }
        }

        public static void RequestVisit()
        {
            if (ClientValues.isInVisit) DialogManager.PushNewDialog(new RT_Dialog_OK("ERROR", "You are already visiting someone!"));
            else
            {
                Action r1 = delegate
                {
                    DialogManager.clearStack();
                    DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for visit response"));

                    VisitData visitData = new VisitData();
                    visitData.visitStepMode = ((int)VisitStepMode.Request).ToString();
                    visitData.fromTile = Find.AnyPlayerHomeMap.Tile.ToString();
                    visitData.targetTile = ClientValues.chosenSettlement.Tile.ToString();
                    visitData = VisitThingHelper.GetPawnsForVisit(FetchMode.Player, visitData);

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitData);
                    Network.listener.EnqueuePacket(packet);
                };

                var d1 = new RT_Dialog_YesNo("This feature is still in beta, continue?", r1, null);
                DialogManager.PushNewDialog(d1);
            }
        }

        private static void SendRequestedMap(VisitData visitData)
        {
            visitData.visitStepMode = ((int)VisitStepMode.Accept).ToString();

            MapData mapData = MapManager.ParseMap(visitMap, true, false, false, true);
            visitData.mapData = Serializer.ConvertObjectToBytes(mapData);
            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitData);
            Network.listener.EnqueuePacket(packet);
        }

        private static void VisitMap(MapData mapData, VisitData visitData)
        {
            VisitThingHelper.SetMapForVisit(FetchMode.Player, mapData: mapData);

            //keep track of one pawn in the caravan to jump to later
            Pawn pawnToFocus = (ClientValues.chosenCaravan.pawns.Count > 0) ? ClientValues.chosenCaravan.pawns[0] : null;

            VisitThingHelper.GetCaravanPawns(FetchMode.Player);

            VisitThingHelper.SpawnPawnsForVisit(FetchMode.Player, visitData);

            CaravanEnterMapUtility.Enter(ClientValues.chosenCaravan, visitMap, CaravanEnterMode.Edge,
                CaravanDropInventoryMode.DoNotDrop, draftColonists: false);

            VisitThingHelper.GetMapItems();

            ClientValues.ToggleVisit(true);

            //Switch to the Map mode and focus on the caravan
            CameraJumper.TryJump(pawnToFocus);

            Threader.GenerateThread(Threader.Mode.Visit);
            RT_Dialog_OK_Loop d1 = new RT_Dialog_OK_Loop("MESSAGE", new string[]
            {
                "You are now in online visit mode!",
                "Visit mode allows you to visit another player's base",
                "To stop the visit use /sv in the chat"
            },
            DialogManager.clearStack);
            DialogManager.PushNewDialog(d1);
        }

        public static void StopVisit()
        {
            //TODO
            //Implement this

            VisitData visitData = new VisitData();
            visitData.visitStepMode = ((int)VisitStepMode.Stop).ToString();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitData);
            Network.listener.EnqueuePacket(packet);
        }

        private static void OnVisitRequest(VisitData visitData)
        {
            Action r1 = delegate
            {
                DialogManager.clearStack();
                VisitThingHelper.SetMapForVisit(FetchMode.Host, visitData: visitData);
                VisitThingHelper.GetMapPawns(FetchMode.Host, visitData);
                visitData = VisitThingHelper.GetPawnsForVisit(FetchMode.Host, visitData);
                VisitThingHelper.SpawnPawnsForVisit(FetchMode.Host, visitData);
                SendRequestedMap(visitData);

                VisitThingHelper.GetMapItems();

                ClientValues.ToggleVisit(true);

                Threader.GenerateThread(Threader.Mode.Visit);
            };

            Action r2 = delegate
            {
                DialogManager.clearStack();
                visitData.visitStepMode = ((int)VisitStepMode.Reject).ToString();
                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitData);
                Network.listener.EnqueuePacket(packet);
            };

            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo($"Visited by {visitData.visitorName}, accept?", r1, r2);
            DialogManager.PushNewDialog(d1);
        }

        private static void OnVisitAccept(VisitData visitData)
        {
            DialogManager.clearStack();

            MapData mapData = (MapData)Serializer.ConvertBytesToObject(visitData.mapData);

            Action r1 = delegate 
            {
                DialogManager.PushNewDialog(new RT_Dialog_Wait("Loading Map...",
                    delegate { VisitMap(mapData, visitData); })); 
            };

            if (ModManager.CheckIfMapHasConflictingMods(mapData))
            {
                DialogManager.PushNewDialog(new RT_Dialog_OK("MESSAGE", "Map received but contains unknown mod data", r1));
            }
            else DialogManager.PushNewDialog(new RT_Dialog_OK("MESSAGE", "Map received", r1));
        }

        private static void OnVisitReject()
        {
            DialogManager.clearStack();
            DialogManager.PushNewDialog(new RT_Dialog_OK("ERROR", "Player rejected the visit!"));
        }

        private static void OnVisitUnavailable()
        {
            DialogManager.clearStack();
            DialogManager.PushNewDialog(new RT_Dialog_OK("ERROR", "Player must be online!"));
        }

        private static void OnVisitStop()
        {
            if (!ClientValues.isInVisit) return;
            else
            {
                DialogManager.PushNewDialog(new RT_Dialog_OK("MESSAGE", "Visiting event ended"));

                foreach (Pawn pawn in otherPlayerPawns.ToArray()) pawn.Destroy();

                ClientValues.ToggleVisit(false);
            }
        }
    }

    public static class VisitThingHelper
    {
        public static void GetMapPawns(FetchMode mode, VisitData visitData)
        {
            if (mode == FetchMode.Host)
            {
                List<Pawn> mapHumans = OnlineVisitManager.visitMap.mapPawns.AllPawns
                    .FindAll(fetch => TransferManagerHelper.CheckIfThingIsHuman(fetch) && fetch.Faction == Faction.OfPlayer)
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<Pawn> mapAnimals = OnlineVisitManager.visitMap.mapPawns.AllPawns
                    .FindAll(fetch => TransferManagerHelper.CheckIfThingIsAnimal(fetch) && fetch.Faction == Faction.OfPlayer)
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<Pawn> allPawns = new List<Pawn>();
                foreach (Pawn pawn in mapHumans) allPawns.Add(pawn);
                foreach (Pawn pawn in mapAnimals) allPawns.Add(pawn);

                OnlineVisitManager.playerPawns = allPawns.ToList();
            }

            else if (mode == FetchMode.Player)
            {
                List<Pawn> pawnList = new List<Pawn>();

                foreach (string str in visitData.mapHumans)
                {
                    HumanData humanData = Serializer.SerializeFromString<HumanData>(str);
                    Pawn human = HumanScribeManager.StringToHuman(humanData);

                    pawnList.Add(human);
                }

                foreach (string str in visitData.mapAnimals)
                {
                    AnimalData animalData = Serializer.SerializeFromString<AnimalData>(str);
                    Pawn animal = AnimalScribeManager.StringToAnimal(animalData);

                    pawnList.Add(animal);
                }

                OnlineVisitManager.otherPlayerPawns = pawnList.ToList();
            }
        }

        public static void GetCaravanPawns(FetchMode mode, VisitData visitData = null)
        {
            if (mode == FetchMode.Host)
            {
                List<Pawn> pawnList = new List<Pawn>();

                foreach (string str in visitData.caravanHumans)
                {
                    HumanData humanData = Serializer.SerializeFromString<HumanData>(str);
                    Pawn human = HumanScribeManager.StringToHuman(humanData);
                    pawnList.Add(human);
                }

                foreach (string str in visitData.caravanAnimals)
                {
                    AnimalData animalData = Serializer.SerializeFromString<AnimalData>(str);
                    Pawn animal = AnimalScribeManager.StringToAnimal(animalData);
                    pawnList.Add(animal);
                }

                OnlineVisitManager.otherPlayerPawns = pawnList.ToList();
            }

            else if (mode == FetchMode.Player)
            {
                List<Pawn> caravanHumans = ClientValues.chosenCaravan.PawnsListForReading
                    .FindAll(fetch => TransferManagerHelper.CheckIfThingIsHuman(fetch))
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<Pawn> caravanAnimals = ClientValues.chosenCaravan.PawnsListForReading
                    .FindAll(fetch => TransferManagerHelper.CheckIfThingIsAnimal(fetch))
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<Pawn> allPawns = new List<Pawn>();
                foreach (Pawn pawn in caravanHumans) allPawns.Add(pawn);
                foreach (Pawn pawn in caravanAnimals) allPawns.Add(pawn);

                OnlineVisitManager.playerPawns = allPawns;
            }
        }

        public static void GetMapItems()
        {
            OnlineVisitManager.mapThings.Clear();

            for (int z = 0; z < OnlineVisitManager.visitMap.Size.z; ++z)
            {
                for (int x = 0; x < OnlineVisitManager.visitMap.Size.x; ++x)
                {
                    IntVec3 vectorToCheck = new IntVec3(x, OnlineVisitManager.visitMap.Size.y, z);

                    foreach (Thing thing in OnlineVisitManager.visitMap.thingGrid.ThingsListAt(vectorToCheck).ToList())
                    {
                        if (TransferManagerHelper.CheckIfThingIsHuman(thing)) continue;
                        else if (TransferManagerHelper.CheckIfThingIsAnimal(thing)) continue;
                        else OnlineVisitManager.mapThings.Add(thing);
                    }
                }
            }
        }

        public static void SetMapForVisit(FetchMode mode, VisitData visitData = null, MapData mapData = null)
        {
            if (mode == FetchMode.Host)
            {
                OnlineVisitManager.visitMap = Find.Maps.Find(fetch => fetch.Tile == int.Parse(visitData.targetTile));
            }

            else if (mode == FetchMode.Player)
            {
                OnlineVisitManager.visitMap = MapScribeManager.StringToMap(mapData, true, false, false, false);
            }
        }

        public static VisitData GetPawnsForVisit(FetchMode mode, VisitData visitData)
        {
            if (mode == FetchMode.Host)
            {
                List<Pawn> mapHumans = OnlineVisitManager.visitMap.mapPawns.AllPawns
                    .FindAll(fetch => TransferManagerHelper.CheckIfThingIsHuman(fetch) && fetch.Faction == Faction.OfPlayer)
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<string> humanStringList = new List<string>();
                foreach (Pawn human in mapHumans)
                {
                    string humanString = Serializer.SerializeToString(HumanScribeManager.HumanToString(human));
                    humanStringList.Add(humanString);
                }
                visitData.mapHumans = humanStringList;

                List<Pawn> mapAnimals = OnlineVisitManager.visitMap.mapPawns.AllPawns
                    .FindAll(fetch => TransferManagerHelper.CheckIfThingIsAnimal(fetch) && fetch.Faction == Faction.OfPlayer)
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<string> animalStringList = new List<string>();
                foreach (Pawn animal in mapAnimals)
                {
                    string animalString = Serializer.SerializeToString(AnimalScribeManager.AnimalToString(animal));
                    animalStringList.Add(animalString);
                }
                visitData.mapAnimals = animalStringList;
            }

            else if (mode == FetchMode.Player)
            {
                List<Pawn> caravanHumans = ClientValues.chosenCaravan.PawnsListForReading
                    .FindAll(fetch => TransferManagerHelper.CheckIfThingIsHuman(fetch))
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<string> humanStringList = new List<string>();
                foreach (Pawn human in caravanHumans)
                {
                    string humanString = Serializer.SerializeToString(HumanScribeManager.HumanToString(human));
                    humanStringList.Add(humanString);
                }
                visitData.caravanHumans = humanStringList;

                List<Pawn> caravanAnimals = ClientValues.chosenCaravan.PawnsListForReading
                    .FindAll(fetch => TransferManagerHelper.CheckIfThingIsAnimal(fetch))
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<string> animalStringList = new List<string>();
                foreach (Pawn animal in caravanAnimals)
                {
                    string animalString = Serializer.SerializeToString(AnimalScribeManager.AnimalToString(animal));
                    animalStringList.Add(animalString);
                }
                visitData.caravanAnimals = animalStringList;
            }

            return visitData;
        }

        public static void SpawnPawnsForVisit(FetchMode mode, VisitData visitData)
        {
            if (mode == FetchMode.Host)
            {
                GetCaravanPawns(FetchMode.Host, visitData);
                foreach (Pawn pawn in OnlineVisitManager.otherPlayerPawns)
                {
                    pawn.SetFaction(FactionValues.allyPlayer);
                    GenSpawn.Spawn(pawn, OnlineVisitManager.visitMap.Center, OnlineVisitManager.visitMap, Rot4.Random);
                }
            }

            else if (mode == FetchMode.Player)
            {
                GetMapPawns(FetchMode.Player, visitData);
                foreach (Pawn pawn in OnlineVisitManager.otherPlayerPawns)
                {
                    pawn.SetFaction(FactionValues.allyPlayer);
                    GenSpawn.Spawn(pawn, OnlineVisitManager.visitMap.Center, OnlineVisitManager.visitMap, Rot4.Random);
                }
            }
        }
    }

    public static class VisitActionGetter
    {
        public static void StartActionClock()
        {
            while (ClientValues.isInVisit)
            {
                Thread.Sleep(250);

                ActionClockTick();


                Find.TickManager.slower.SignalForceNormalSpeed();

                if (OnlineVisitManager.visitMap.Parent.Map == null)
                {
                    if (ClientValues.isInVisit)
                    {
                        ChatManager.SendMessage("/sv");
                        Logger.WriteToConsole("Visit has ended", LogMode.Message);
                    }
                }

            }
        }

        private static void ActionClockTick()
        {
            VisitData visitData = new VisitData();

            foreach (Pawn pawn in OnlineVisitManager.playerPawns.ToArray())
            {
                try
                {
                    if (pawn.jobs.curJob != null)
                    {
                        visitData.pawnActionDefNames.Add(pawn.jobs.curJob.def.defName);
                        visitData.actionTargetA.Add(VisitActionHelper.TransformActionTargetToString(pawn.jobs.curJob.targetA, visitData));
                    }

                    else
                    {
                        visitData.pawnActionDefNames.Add(JobDefOf.Goto.defName);

                        visitData.actionTargetA.Add(VisitActionHelper.TransformActionTargetToString(new LocalTargetInfo(pawn.Position),
                            visitData));
                    }

                    visitData.pawnPositions.Add($"{pawn.Position.x}|{pawn.Position.y}|{pawn.Position.z}");
                }
                catch { Logger.WriteToConsole($"Couldn't get job for {pawn}", LogMode.Error); }
            }

            visitData.visitStepMode = ((int)VisitStepMode.Action).ToString();
            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitData);
            Network.listener.EnqueuePacket(packet);
        }

        public static void ReceiveActions(VisitData visitData)
        {
            Pawn[] otherPawns = OnlineVisitManager.otherPlayerPawns.ToArray();

            for (int i = 0; i < otherPawns.Count(); i++)
            {
                try
                {
                    JobDef jobDef = VisitActionHelper.TryGetJobDefForJob(otherPawns[i], visitData.pawnActionDefNames[i]);

                    VisitActionHelper.TryChangePawnPosition(otherPawns[i], visitData, i);

                    LocalTargetInfo localTargetInfoA = VisitActionHelper.TryGetLocalTargetInfo(otherPawns[i],
                        visitData.actionTargetA[i], visitData.actionTargetType[i]);

                    Job newJob = VisitActionHelper.TryCreateNewJob(otherPawns[i], jobDef, localTargetInfoA);

                    VisitActionHelper.TryDraftPawnForJobIfNeeded(otherPawns[i], newJob);

                    VisitActionHelper.ChangeCurrentJobIfNeeded(otherPawns[i], newJob);

                }
                catch { Logger.WriteToConsole($"Couldn't set job for {otherPawns[i]}", LogMode.Error); }
            }
        }
    }

    public static class VisitActionHelper
    {
        public static string TransformActionTargetToString(LocalTargetInfo targetInfo, VisitData visitData)
        {
            string toReturn = "";

            try
            {
                if (targetInfo.Thing != null)
                {
                    if (TransferManagerHelper.CheckIfThingIsHuman(targetInfo.Thing))
                    {
                        visitData.actionTargetType.Add(((int)ActionTargetType.Human).ToString());
                        toReturn = Serializer.SerializeToString(HumanScribeManager.HumanToString(targetInfo.Pawn));
                    }

                    else if (TransferManagerHelper.CheckIfThingIsAnimal(targetInfo.Thing))
                    {
                        visitData.actionTargetType.Add(((int)ActionTargetType.Animal).ToString());
                        toReturn = Serializer.SerializeToString(AnimalScribeManager.AnimalToString(targetInfo.Pawn));
                    }

                    else
                    {
                        visitData.actionTargetType.Add(((int)ActionTargetType.Thing).ToString());
                        toReturn = Serializer.SerializeToString(ThingScribeManager.ItemToString(targetInfo.Thing, 1));
                    }
                }

                else if (targetInfo.Pawn != null)
                {
                    if (TransferManagerHelper.CheckIfThingIsHuman(targetInfo.Pawn))
                    {
                        visitData.actionTargetType.Add(((int)ActionTargetType.Human).ToString());
                        toReturn = Serializer.SerializeToString(HumanScribeManager.HumanToString(targetInfo.Pawn));
                    }

                    else
                    {
                        visitData.actionTargetType.Add(((int)ActionTargetType.Animal).ToString());
                        toReturn = Serializer.SerializeToString(AnimalScribeManager.AnimalToString(targetInfo.Pawn));
                    }
                }

                else if (targetInfo.Cell != null)
                {
                    visitData.actionTargetType.Add(((int)ActionTargetType.Cell).ToString());
                    toReturn = $"{targetInfo.Cell.x}|{targetInfo.Cell.y}|{targetInfo.Cell.z}";
                }
            }
            catch { Logger.WriteToConsole($"failed to parse {targetInfo}", LogMode.Error); }

            return toReturn;
        }

        public static LocalTargetInfo GetActionTargetFromString(string toReadFrom, string actionTargetType)
        {
            LocalTargetInfo target = LocalTargetInfo.Invalid;

            try
            {
                switch (int.Parse(actionTargetType))
                {
                    case (int)ActionTargetType.Thing:
                        ItemData itemData = Serializer.SerializeFromString<ItemData>(toReadFrom);
                        Thing thingToCompare = ThingScribeManager.StringToItem(itemData);
                        Thing realThing = OnlineVisitManager.mapThings.Find(fetch => fetch.Position == thingToCompare.Position && fetch.def.defName == thingToCompare.def.defName);
                        if (realThing != null) target = new LocalTargetInfo(realThing);
                        break;

                    case (int)ActionTargetType.Human:
                        HumanData humanData = Serializer.SerializeFromString<HumanData>(toReadFrom);
                        Pawn humanToCompare = HumanScribeManager.StringToHuman(humanData);
                        Pawn realHuman = OnlineVisitManager.visitMap.mapPawns.AllPawns.Find(fetch => fetch.Position == humanToCompare.Position);
                        if (realHuman != null) target = new LocalTargetInfo(realHuman);
                        break;

                    case (int)ActionTargetType.Animal:
                        AnimalData animalData = Serializer.SerializeFromString<AnimalData>(toReadFrom); ;
                        Pawn animalToCompare = AnimalScribeManager.StringToAnimal(animalData);
                        Pawn realAnimal = OnlineVisitManager.visitMap.mapPawns.AllPawns.Find(fetch => fetch.Position == animalToCompare.Position);
                        if (realAnimal != null) target = new LocalTargetInfo(realAnimal);
                        break;

                    case (int)ActionTargetType.Cell:
                        string[] cellCoords = toReadFrom.Split('|');
                        IntVec3 cell = new IntVec3(int.Parse(cellCoords[0]), int.Parse(cellCoords[1]), int.Parse(cellCoords[2]));
                        if (cell != null) target = new LocalTargetInfo(cell);
                        break;
                }
            }
            catch { Logger.WriteToConsole($"Failed to get target from {toReadFrom} as {actionTargetType}", LogMode.Error); }

            return target;
        }

        public static JobDef TryGetJobDefForJob(Pawn pawnForJob, string jobDefName)
        {
            try { return DefDatabase<JobDef>.AllDefs.ToList().Find(fetch => fetch.defName == jobDefName); }
            catch { Logger.WriteToConsole($"Couldn't get job def of {pawnForJob.Label}", LogMode.Error); }

            return null;
        }

        public static LocalTargetInfo TryGetLocalTargetInfo(Pawn pawnForJob, string actionTarget, string actionTargetType)
        {
            try { return GetActionTargetFromString(actionTarget, actionTargetType); }
            catch { Logger.WriteToConsole($"Couldn't get job target for {pawnForJob.Label}", LogMode.Error); }

            return null;
        }

        public static Job TryCreateNewJob(Pawn pawnForJob, JobDef jobDef, LocalTargetInfo localTargetA)
        {
            try { return JobMaker.MakeJob(jobDef, localTargetA); }
            catch { Logger.WriteToConsole($"Couldn't create job for {pawnForJob.Label}", LogMode.Error); }

            return null;
        }

        public static void TryDraftPawnForJobIfNeeded(Pawn pawn, Job job)
        {
            try
            {
                if (pawn.drafter == null) pawn.drafter = new Pawn_DraftController(pawn);
                if (job.def == JobDefOf.Wait_Combat) pawn.drafter.Drafted = true;
                else pawn.drafter.Drafted = false;
            }
            catch { Logger.WriteToConsole($"Couldn't draft {pawn}", LogMode.Error); }
        }

        public static void TryChangePawnPosition(Pawn pawn, VisitData visitData, int index)
        {
            try
            {
                string[] positionSplit = visitData.pawnPositions[index].Split('|');
                IntVec3 updatedPosition = new IntVec3(int.Parse(positionSplit[0]), int.Parse(positionSplit[1]), int.Parse(positionSplit[2]));
                pawn.pather.Notify_Teleported_Int();
                pawn.Position = updatedPosition;
            }
            catch { Logger.WriteToConsole($"Couldn't give position to {pawn}", LogMode.Error); }
        }

        public static void ChangeCurrentJobIfNeeded(Pawn pawn, Job newJob)
        {
            if (pawn.jobs.curJob == null)
            {
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                pawn.jobs.StartJob(newJob);
            }

            else
            {
                if (pawn.jobs.curJob.def == newJob.def) return;
                else
                {
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                    pawn.jobs.StartJob(newJob);
                }
            }
        }
    }
}