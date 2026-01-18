using GameClient.Misc;
using Synchronous.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Synchronous.Misc
{
    public static class Converter
    {
        public static string IntVector3ToString(IntVec3 value)
        {
            return $"{value.x}|{value.y}|{value.z}";
        }

        public static IntVec3 StringToIntVec3(string value)
        {
            string[] split = value.Split('|');

            return new IntVec3(int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2]));
        }

        public static string LocalTargetInfoToString(LocalTargetInfo value)
        {
            if (value.Pawn != null) return value.Pawn.ThingID.ToString();
            else if (value.Thing != null) return value.Thing.ThingID.ToString();
            else return IntVector3ToString(value.Cell);
        }

        public static LocalTargetInfo StringToLocalTargetInfo(string value)
        {
            Pawn pawn = SessionHandler.SynchronousMap.mapPawns.AllPawns.FirstOrDefault(fetch => fetch.ThingID == value);
            if (pawn != null) return new LocalTargetInfo(pawn);

            Thing thing = SessionHandler.SynchronousMap.listerThings.AllThings.FirstOrDefault(fetch => fetch.ThingID == value);
            if (thing != null) return new LocalTargetInfo(thing);

            return new LocalTargetInfo(StringToIntVec3(value));
        }

        public static List<string> LocalTargetQueueToString(List<LocalTargetInfo> value)
        {
            List<string> results = new List<string>();
            foreach (LocalTargetInfo info in value) results.Add(LocalTargetInfoToString(info));
            return results;
        }

        public static Job PlayerJobToJob(Job job, PlayerJob playerJob)
        {
            if (playerJob.TargetA != null) job.SetTarget(TargetIndex.A, StringToLocalTargetInfo(playerJob.TargetA));
            if (playerJob.TargetB != null) job.SetTarget(TargetIndex.B, StringToLocalTargetInfo(playerJob.TargetB));
            if (playerJob.TargetC != null) job.SetTarget(TargetIndex.C, StringToLocalTargetInfo(playerJob.TargetC));
            if (playerJob.QueueA != null) foreach (string str in playerJob.QueueA) job.AddQueuedTarget(TargetIndex.A, StringToLocalTargetInfo(str));
            if (playerJob.QueueB != null) foreach (string str in playerJob.QueueB) job.AddQueuedTarget(TargetIndex.B, StringToLocalTargetInfo(str));

            return job;
        }
    }
}
