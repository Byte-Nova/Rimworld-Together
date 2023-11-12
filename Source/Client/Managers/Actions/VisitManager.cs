using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RimWorld;
using RimWorld.Planet;
using RimworldTogether.GameClient.Dialogs;
using RimworldTogether.GameClient.Misc;
using RimworldTogether.GameClient.Values;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.JSON.Actions;
using RimworldTogether.Shared.JSON.Things;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using Shared.Misc;
using Verse;
using Verse.AI;


namespace RimworldTogether.GameClient.Managers.Actions
{
    public static class VisitManager
    {
        public static List<Pawn> playerPawns = new List<Pawn>();

        public static List<Pawn> otherPlayerPawns = new List<Pawn>();

        public static List<Thing> mapThings = new List<Thing>();

        public static Map visitMap = null;

        public static void ParseVisitPacket(Packet packet)
        {
            VisitDetailsJSON visitDetailsJSON = (VisitDetailsJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

            switch (int.Parse(visitDetailsJSON.visitStepMode))
            {
                case (int)CommonEnumerators.VisitStepMode.Request:
                    OnVisitRequest(visitDetailsJSON);
                    break;

                case (int)CommonEnumerators.VisitStepMode.Accept:
                    OnVisitAccept(visitDetailsJSON);
                    break;

                case (int)CommonEnumerators.VisitStepMode.Reject:
                    OnVisitReject();
                    break;

                case (int)CommonEnumerators.VisitStepMode.Unavailable:
                    OnVisitUnavailable();
                    break;

                case (int)CommonEnumerators.VisitStepMode.Action:
                    VisitActionGetter.ReceiveActions(visitDetailsJSON);
                    break;

                case (int)CommonEnumerators.VisitStepMode.Stop:
                    OnVisitStop();
                    break;
            }
        }

        public static void RequestVisit()
        {
            if (ClientValues.isInVisit) DialogManager.PushNewDialog(new RT_Dialog_Error("You are already visiting someone!"));
            else
            {
                Action r1 = delegate
                {
                    DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for visit response"));

                    VisitDetailsJSON visitDetailsJSON = new VisitDetailsJSON();
                    visitDetailsJSON.visitStepMode = ((int)CommonEnumerators.VisitStepMode.Request).ToString();
                    visitDetailsJSON.fromTile = Find.AnyPlayerHomeMap.Tile.ToString();
                    visitDetailsJSON.targetTile = ClientValues.chosenSettlement.Tile.ToString();
                    visitDetailsJSON = VisitThingHelper.GetPawnsForVisit(VisitThingHelper.FetchMode.Player, visitDetailsJSON);

                    Packet packet = Packet.CreatePacketFromJSON("VisitPacket", visitDetailsJSON);
                    Network.Network.serverListener.SendData(packet);
                };

                var d1 = new RT_Dialog_YesNo("This feature is still in beta, continue?", r1, null);
                DialogManager.PushNewDialog(d1);
            }
        }

        private static void SendRequestedMap(VisitDetailsJSON visitDetailsJSON)
        {
            visitDetailsJSON.visitStepMode = ((int)CommonEnumerators.VisitStepMode.Accept).ToString();

            MapDetailsJSON mapDetailsJSON = RimworldManager.GetMap(visitMap, true, false, false, true);
            visitDetailsJSON.mapDetails = ObjectConverter.ConvertObjectToBytes(mapDetailsJSON);
            Packet packet = Packet.CreatePacketFromJSON("VisitPacket", visitDetailsJSON);
            Network.Network.serverListener.SendData(packet);
        }

        private static void VisitMap(MapDetailsJSON mapDetailsJSON, VisitDetailsJSON visitDetailsJSON)
        {
            VisitThingHelper.SetMapForVisit(VisitThingHelper.FetchMode.Player, mapDetailsJSON: mapDetailsJSON);

            VisitThingHelper.GetCaravanPawns(VisitThingHelper.FetchMode.Player);

            VisitThingHelper.SpawnPawnsForVisit(VisitThingHelper.FetchMode.Player, visitDetailsJSON);

            CaravanEnterMapUtility.Enter(ClientValues.chosenCaravan, visitMap, CaravanEnterMode.Edge,
                CaravanDropInventoryMode.DoNotDrop, draftColonists: false);

            VisitThingHelper.GetMapItems();

            ClientValues.ToggleVisit(true);

            Threader.GenerateThread(Threader.Mode.Visit);
            RT_Dialog_OK_Loop d1 = new RT_Dialog_OK_Loop(new string[]
            {
                "You are now in online visit mode!",
                "Visit mode allows you to visit another player's base",
                "To stop the visit use /sv in the chat"
            });
            DialogManager.PushNewDialog(d1);
        }

        public static void StopVisit()
        {
            //TODO
            //Implement this

            VisitDetailsJSON visitDetailsJSON = new VisitDetailsJSON();
            visitDetailsJSON.visitStepMode = ((int)CommonEnumerators.VisitStepMode.Stop).ToString();

            Packet packet = Packet.CreatePacketFromJSON("VisitPacket", visitDetailsJSON);
            Network.Network.serverListener.SendData(packet);
        }

        private static void OnVisitRequest(VisitDetailsJSON visitDetailsJSON)
        {
            Action r1 = delegate
            {
                VisitThingHelper.SetMapForVisit(VisitThingHelper.FetchMode.Host, visitDetailsJSON: visitDetailsJSON);
                VisitThingHelper.GetMapPawns(VisitThingHelper.FetchMode.Host, visitDetailsJSON);
                visitDetailsJSON = VisitThingHelper.GetPawnsForVisit(VisitThingHelper.FetchMode.Host, visitDetailsJSON);
                VisitThingHelper.SpawnPawnsForVisit(VisitThingHelper.FetchMode.Host, visitDetailsJSON);
                SendRequestedMap(visitDetailsJSON);

                VisitThingHelper.GetMapItems();

                ClientValues.ToggleVisit(true);

                Threader.GenerateThread(Threader.Mode.Visit);
            };

            Action r2 = delegate
            {
                visitDetailsJSON.visitStepMode = ((int)CommonEnumerators.VisitStepMode.Reject).ToString();
                Packet packet = Packet.CreatePacketFromJSON("VisitPacket", visitDetailsJSON);
                Network.Network.serverListener.SendData(packet);
            };

            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo($"Visited by {visitDetailsJSON.visitorName}, accept?", r1, r2);
            DialogManager.PushNewDialog(d1);
        }

        private static void OnVisitAccept(VisitDetailsJSON visitDetailsJSON)
        {
            DialogManager.PopWaitDialog();

            MapDetailsJSON mapDetailsJSON = (MapDetailsJSON)ObjectConverter.ConvertBytesToObject(visitDetailsJSON.mapDetails);

            Action r1 = delegate { VisitMap(mapDetailsJSON, visitDetailsJSON); };
            if (ModManager.CheckIfMapHasConflictingMods(mapDetailsJSON))
            {
                DialogManager.PushNewDialog(new RT_Dialog_OK("Map received but contains unknown mod data", r1));
            }
            else DialogManager.PushNewDialog(new RT_Dialog_OK("Map received", r1));
        }

        private static void OnVisitReject()
        {
            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_Error("Player rejected the visit!"));
        }

        private static void OnVisitUnavailable()
        {
            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_Error("Player must be online!"));
        }

        private static void OnVisitStop()
        {
            if (!ClientValues.isInVisit) return;
            else
            {
                DialogManager.PushNewDialog(new RT_Dialog_OK("Visiting event ended"));

                foreach (Pawn pawn in otherPlayerPawns.ToArray()) pawn.Destroy();

                ClientValues.ToggleVisit(false);
            }
        }
    }

    public static class VisitThingHelper
    {
        public enum FetchMode { Host, Player }

        public static void GetMapPawns(FetchMode mode, VisitDetailsJSON visitDetailsJSON)
        {
            if (mode == FetchMode.Host)
            {
                List<Pawn> mapHumans = VisitManager.visitMap.mapPawns.AllPawns
                    .FindAll(fetch => TransferManagerHelper.CheckIfThingIsHuman(fetch) && fetch.Faction == Faction.OfPlayer)
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<Pawn> mapAnimals = VisitManager.visitMap.mapPawns.AllPawns
                    .FindAll(fetch => TransferManagerHelper.CheckIfThingIsAnimal(fetch) && fetch.Faction == Faction.OfPlayer)
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<Pawn> allPawns = new List<Pawn>();
                foreach (Pawn pawn in mapHumans) allPawns.Add(pawn);
                foreach (Pawn pawn in mapAnimals) allPawns.Add(pawn);

                VisitManager.playerPawns = allPawns.ToList();
            }

            else if (mode == FetchMode.Player)
            {
                List<Pawn> pawnList = new List<Pawn>();

                foreach (string str in visitDetailsJSON.mapHumans)
                {
                    HumanDetailsJSON humanDetailsJSON = Serializer.SerializeFromString<HumanDetailsJSON>(str);
                    Pawn human = DeepScribeManager.GetHumanSimple(humanDetailsJSON);

                    pawnList.Add(human);
                }

                foreach (string str in visitDetailsJSON.mapAnimals)
                {
                    AnimalDetailsJSON animalDetailsJSON = Serializer.SerializeFromString<AnimalDetailsJSON>(str);
                    Pawn animal = DeepScribeManager.GetAnimalSimple(animalDetailsJSON);

                    pawnList.Add(animal);
                }

                VisitManager.otherPlayerPawns = pawnList.ToList();
            }
        }

        public static void GetCaravanPawns(FetchMode mode, VisitDetailsJSON visitDetailsJSON = null)
        {
            if (mode == FetchMode.Host)
            {
                List<Pawn> pawnList = new List<Pawn>();

                foreach (string str in visitDetailsJSON.caravanHumans)
                {
                    HumanDetailsJSON humanDetailsJSON = Serializer.SerializeFromString<HumanDetailsJSON>(str);
                    Pawn human = DeepScribeManager.GetHumanSimple(humanDetailsJSON);
                    pawnList.Add(human);
                }

                foreach (string str in visitDetailsJSON.caravanAnimals)
                {
                    AnimalDetailsJSON animalDetailsJSON = Serializer.SerializeFromString<AnimalDetailsJSON>(str);
                    Pawn animal = DeepScribeManager.GetAnimalSimple(animalDetailsJSON);
                    pawnList.Add(animal);
                }

                VisitManager.otherPlayerPawns = pawnList.ToList();
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

                VisitManager.playerPawns = allPawns;
            }
        }

        public static void GetMapItems()
        {
            VisitManager.mapThings.Clear();

            for (int z = 0; z < VisitManager.visitMap.Size.z; ++z)
            {
                for (int x = 0; x < VisitManager.visitMap.Size.x; ++x)
                {
                    IntVec3 vectorToCheck = new IntVec3(x, VisitManager.visitMap.Size.y, z);

                    foreach (Thing thing in VisitManager.visitMap.thingGrid.ThingsListAt(vectorToCheck).ToList())
                    {
                        if (TransferManagerHelper.CheckIfThingIsHuman(thing)) continue;
                        else if (TransferManagerHelper.CheckIfThingIsAnimal(thing)) continue;
                        else VisitManager.mapThings.Add(thing);
                    }
                }
            }
        }

        public static void SetMapForVisit(FetchMode mode, VisitDetailsJSON visitDetailsJSON = null, MapDetailsJSON mapDetailsJSON = null)
        {
            if (mode == FetchMode.Host)
            {
                VisitManager.visitMap = Find.Maps.Find(fetch => fetch.Tile == int.Parse(visitDetailsJSON.targetTile));
            }

            else if (mode == FetchMode.Player)
            {
                VisitManager.visitMap = DeepScribeManager.GetMapSimple(mapDetailsJSON, true, false, false, false);
            }
        }

        public static VisitDetailsJSON GetPawnsForVisit(FetchMode mode, VisitDetailsJSON visitDetailsJSON)
        {
            if (mode == FetchMode.Host)
            {
                List<Pawn> mapHumans = VisitManager.visitMap.mapPawns.AllPawns
                    .FindAll(fetch => TransferManagerHelper.CheckIfThingIsHuman(fetch) && fetch.Faction == Faction.OfPlayer)
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<string> humanStringList = new List<string>();
                foreach (Pawn human in mapHumans)
                {
                    string humanString = Serializer.SerializeToString(DeepScribeManager.TransformHumanToString(human));
                    humanStringList.Add(humanString);
                }
                visitDetailsJSON.mapHumans = humanStringList;

                List<Pawn> mapAnimals = VisitManager.visitMap.mapPawns.AllPawns
                    .FindAll(fetch => TransferManagerHelper.CheckIfThingIsAnimal(fetch) && fetch.Faction == Faction.OfPlayer)
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<string> animalStringList = new List<string>();
                foreach (Pawn animal in mapAnimals)
                {
                    string animalString = Serializer.SerializeToString(DeepScribeManager.TransformAnimalToString(animal));
                    animalStringList.Add(animalString);
                }
                visitDetailsJSON.mapAnimals = animalStringList;
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
                    string humanString = Serializer.SerializeToString(DeepScribeManager.TransformHumanToString(human));
                    humanStringList.Add(humanString);
                }
                visitDetailsJSON.caravanHumans = humanStringList;

                List<Pawn> caravanAnimals = ClientValues.chosenCaravan.PawnsListForReading
                    .FindAll(fetch => TransferManagerHelper.CheckIfThingIsAnimal(fetch))
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<string> animalStringList = new List<string>();
                foreach (Pawn animal in caravanAnimals)
                {
                    string animalString = Serializer.SerializeToString(DeepScribeManager.TransformAnimalToString(animal));
                    animalStringList.Add(animalString);
                }
                visitDetailsJSON.caravanAnimals = animalStringList;
            }

            return visitDetailsJSON;
        }

        public static void SpawnPawnsForVisit(FetchMode mode, VisitDetailsJSON visitDetailsJSON)
        {
            if (mode == FetchMode.Host)
            {
                GetCaravanPawns(FetchMode.Host, visitDetailsJSON);
                foreach (Pawn pawn in VisitManager.otherPlayerPawns)
                {
                    pawn.SetFaction(FactionValues.allyPlayer);
                    GenSpawn.Spawn(pawn, VisitManager.visitMap.Center, VisitManager.visitMap, Rot4.Random);
                }
            }

            else if (mode == FetchMode.Player)
            {
                GetMapPawns(FetchMode.Player, visitDetailsJSON);
                foreach (Pawn pawn in VisitManager.otherPlayerPawns)
                {
                    pawn.SetFaction(FactionValues.allyPlayer);
                    GenSpawn.Spawn(pawn, VisitManager.visitMap.Center, VisitManager.visitMap, Rot4.Random);
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
            }
        }

        private static void ActionClockTick()
        {
            VisitDetailsJSON visitDetailsJSON = new VisitDetailsJSON();

            foreach (Pawn pawn in VisitManager.playerPawns.ToArray())
            {
                try
                {
                    if (pawn.jobs.curJob != null)
                    {
                        visitDetailsJSON.pawnActionDefNames.Add(pawn.jobs.curJob.def.defName);
                        visitDetailsJSON.actionTargetA.Add(VisitActionHelper.TransformActionTargetToString(pawn.jobs.curJob.targetA, visitDetailsJSON));
                    }

                    else
                    {
                        visitDetailsJSON.pawnActionDefNames.Add(JobDefOf.Goto.defName);

                        visitDetailsJSON.actionTargetA.Add(VisitActionHelper.TransformActionTargetToString(new LocalTargetInfo(pawn.Position),
                            visitDetailsJSON));
                    }

                    visitDetailsJSON.pawnPositions.Add($"{pawn.Position.x}|{pawn.Position.y}|{pawn.Position.z}");
                }
                catch { Log.Warning($"Couldn't get job for {pawn}"); }
            }

            visitDetailsJSON.visitStepMode = ((int)CommonEnumerators.VisitStepMode.Action).ToString();
            Packet packet = Packet.CreatePacketFromJSON("VisitPacket", visitDetailsJSON);
            Network.Network.serverListener.SendData(packet);
        }

        public static void ReceiveActions(VisitDetailsJSON visitDetailsJSON)
        {
            Pawn[] otherPawns = VisitManager.otherPlayerPawns.ToArray();

            for (int i = 0; i < otherPawns.Count(); i++)
            {
                try
                {
                    JobDef jobDef = VisitActionHelper.TryGetJobDefForJob(otherPawns[i], visitDetailsJSON.pawnActionDefNames[i]);

                    VisitActionHelper.TryChangePawnPosition(otherPawns[i], visitDetailsJSON, i);

                    LocalTargetInfo localTargetInfoA = VisitActionHelper.TryGetLocalTargetInfo(otherPawns[i],
                        visitDetailsJSON.actionTargetA[i], visitDetailsJSON.actionTargetType[i]);

                    Job newJob = VisitActionHelper.TryCreateNewJob(otherPawns[i], jobDef, localTargetInfoA);

                    VisitActionHelper.TryDraftPawnForJobIfNeeded(otherPawns[i], newJob);

                    VisitActionHelper.ChangeCurrentJobIfNeeded(otherPawns[i], newJob);

                }
                catch { Log.Warning($"Couldn't set job for {otherPawns[i]}"); }
            }
        }
    }

    public static class VisitActionHelper
    {
        public static string TransformActionTargetToString(LocalTargetInfo targetInfo, VisitDetailsJSON visitDetailsJSON)
        {
            string toReturn = "";

            try
            {
                if (targetInfo.Thing != null)
                {
                    if (TransferManagerHelper.CheckIfThingIsHuman(targetInfo.Thing))
                    {
                        visitDetailsJSON.actionTargetType.Add(((int)CommonEnumerators.ActionTargetType.Human).ToString());
                        toReturn = Serializer.SerializeToString(DeepScribeManager.TransformHumanToString(targetInfo.Pawn));
                    }

                    else if (TransferManagerHelper.CheckIfThingIsAnimal(targetInfo.Thing))
                    {
                        visitDetailsJSON.actionTargetType.Add(((int)CommonEnumerators.ActionTargetType.Animal).ToString());
                        toReturn = Serializer.SerializeToString(DeepScribeManager.TransformAnimalToString(targetInfo.Pawn));
                    }

                    else
                    {
                        visitDetailsJSON.actionTargetType.Add(((int)CommonEnumerators.ActionTargetType.Thing).ToString());
                        toReturn = Serializer.SerializeToString(DeepScribeManager.TransformItemToString(targetInfo.Thing, 1));
                    }
                }

                else if (targetInfo.Pawn != null)
                {
                    if (TransferManagerHelper.CheckIfThingIsHuman(targetInfo.Pawn))
                    {
                        visitDetailsJSON.actionTargetType.Add(((int)CommonEnumerators.ActionTargetType.Human).ToString());
                        toReturn = Serializer.SerializeToString(DeepScribeManager.TransformHumanToString(targetInfo.Pawn));
                    }

                    else
                    {
                        visitDetailsJSON.actionTargetType.Add(((int)CommonEnumerators.ActionTargetType.Animal).ToString());
                        toReturn = Serializer.SerializeToString(DeepScribeManager.TransformAnimalToString(targetInfo.Pawn));
                    }
                }

                else if (targetInfo.Cell != null)
                {
                    visitDetailsJSON.actionTargetType.Add(((int)CommonEnumerators.ActionTargetType.Cell).ToString());
                    toReturn = $"{targetInfo.Cell.x}|{targetInfo.Cell.y}|{targetInfo.Cell.z}";
                }
            }
            catch { Log.Error($"failed to parse {targetInfo}"); }

            return toReturn;
        }

        public static LocalTargetInfo GetActionTargetFromString(string toReadFrom, string actionTargetType)
        {
            LocalTargetInfo target = LocalTargetInfo.Invalid;

            try
            {
                switch (int.Parse(actionTargetType))
                {
                    case (int)CommonEnumerators.ActionTargetType.Thing:
                        ItemDetailsJSON itemDetailsJSON = Serializer.SerializeFromString<ItemDetailsJSON>(toReadFrom);
                        Thing thingToCompare = DeepScribeManager.GetItemSimple(itemDetailsJSON);
                        Thing realThing = VisitManager.mapThings.Find(fetch => fetch.Position == thingToCompare.Position && fetch.def.defName == thingToCompare.def.defName);
                        if (realThing != null) target = new LocalTargetInfo(realThing);
                        break;

                    case (int)CommonEnumerators.ActionTargetType.Human:
                        HumanDetailsJSON humanDetailsJSON = Serializer.SerializeFromString<HumanDetailsJSON>(toReadFrom);
                        Pawn humanToCompare = DeepScribeManager.GetHumanSimple(humanDetailsJSON);
                        Pawn realHuman = VisitManager.visitMap.mapPawns.AllPawns.Find(fetch => fetch.Position == humanToCompare.Position);
                        if (realHuman != null) target = new LocalTargetInfo(realHuman);
                        break;

                    case (int)CommonEnumerators.ActionTargetType.Animal:
                        AnimalDetailsJSON animalDetailsJSON = Serializer.SerializeFromString<AnimalDetailsJSON>(toReadFrom); ;
                        Pawn animalToCompare = DeepScribeManager.GetAnimalSimple(animalDetailsJSON);
                        Pawn realAnimal = VisitManager.visitMap.mapPawns.AllPawns.Find(fetch => fetch.Position == animalToCompare.Position);
                        if (realAnimal != null) target = new LocalTargetInfo(realAnimal);
                        break;

                    case (int)CommonEnumerators.ActionTargetType.Cell:
                        string[] cellCoords = toReadFrom.Split('|');
                        IntVec3 cell = new IntVec3(int.Parse(cellCoords[0]), int.Parse(cellCoords[1]), int.Parse(cellCoords[2]));
                        if (cell != null) target = new LocalTargetInfo(cell);
                        break;
                }
            }
            catch { Log.Error($"Failed to get target from {toReadFrom} as {actionTargetType}"); }

            return target;
        }

        public static JobDef TryGetJobDefForJob(Pawn pawnForJob, string jobDefName)
        {
            try { return DefDatabase<JobDef>.AllDefs.ToList().Find(fetch => fetch.defName == jobDefName); }
            catch { Log.Warning($"Couldn't get job def of {pawnForJob.Label}"); }

            return null;
        }

        public static LocalTargetInfo TryGetLocalTargetInfo(Pawn pawnForJob, string actionTarget, string actionTargetType)
        {
            try { return GetActionTargetFromString(actionTarget, actionTargetType); }
            catch { Log.Warning($"Couldn't get job target for {pawnForJob.Label}"); }

            return null;
        }

        public static Job TryCreateNewJob(Pawn pawnForJob, JobDef jobDef, LocalTargetInfo localTargetA)
        {
            try { return JobMaker.MakeJob(jobDef, localTargetA); }
            catch { Log.Warning($"Couldn't create job for {pawnForJob.Label}"); }

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
            catch { Log.Warning($"Couldn't draft {pawn}"); }
        }

        public static void TryChangePawnPosition(Pawn pawn, VisitDetailsJSON visitDetailsJSON, int index)
        {
            try
            {
                string[] positionSplit = visitDetailsJSON.pawnPositions[index].Split('|');
                IntVec3 updatedPosition = new IntVec3(int.Parse(positionSplit[0]), int.Parse(positionSplit[1]), int.Parse(positionSplit[2]));
                pawn.pather.Notify_Teleported_Int();
                pawn.Position = updatedPosition;
            }
            catch { Log.Warning($"Couldn't give position to {pawn}"); }
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