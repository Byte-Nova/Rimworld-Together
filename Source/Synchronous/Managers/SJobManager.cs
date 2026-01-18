using GameClient;
using GameClient.Core.Configs;
using GameClient.Hooks.TCPNetwork;
using GameClient.Managers;
using GameClient.Misc;
using RimWorld;
using Shared;
using Shared.Misc;
using Synchronous.Misc;
using Synchronous.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;
using Verse;
using Verse.AI;

namespace Synchronous.Managers
{
    public static class SJobManager
    {
        private static List<PlayerJob> PlayerJobs { get; set; } = new List<PlayerJob>();

        private static double UpdateTimer { get; set; } = -1;

        [OnSessionStart]
        private static void Initialize() 
        { 
            PlayerJobs = new List<PlayerJob>();
            UpdateTimer = 0;
        }

        [OnUpdate]
        private static void Check()
        {
            if (UpdateTimer < 1f) UpdateTimer += Time.deltaTime;
            else
            {
                if (PlayerJobs.Count > 0)
                {
                    ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.SPlayerJob, Serializer.ConvertObjectToBytes(PlayerJobs));
                    PlayerJobs.Clear();
                }

                UpdateTimer = 0;
            }
        }

        public static void Ask(Job job, Pawn pawn)
        {
            if (PlayerJobs.FirstOrDefault(fetch => fetch.PawnID == pawn.ThingID) != null) return;

            try
            {
                PlayerJob newJob = new PlayerJob();
                newJob.PawnID = pawn.ThingID;
                newJob.PawnPosition = Converter.IntVector3ToString(pawn.Position);
                newJob.TargetA = Converter.LocalTargetInfoToString(job.GetTarget(TargetIndex.A));
                newJob.TargetB = Converter.LocalTargetInfoToString(job.GetTarget(TargetIndex.B));
                newJob.TargetC = Converter.LocalTargetInfoToString(job.GetTarget(TargetIndex.C));
                newJob.QueueA = Converter.LocalTargetQueueToString(job.GetTargetQueue(TargetIndex.A));
                newJob.QueueB = Converter.LocalTargetQueueToString(job.GetTargetQueue(TargetIndex.B));
                newJob.Job = ScribeManager.SerializeToString(job, ScribeManager.SerializableType.Other);

                PlayerJobs.Add(newJob);

                PatchHandler.ExecuteInBypass(delegate
                {
                    pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.Wait, 1000),
                    JobCondition.InterruptForced);
                });
            }
            catch { }
        }

        [HandlesPacket(PacketHeader.SPlayerJob)]
        private static void Receive(byte[] bytes)
        {
            PlayerJob[] jobs = Serializer.ConvertBytesToObject<PlayerJob[]>(bytes);

            PatchHandler.ExecuteInBypass(delegate
            {
                foreach (PlayerJob playerJob in jobs)
                {
                    try
                    {
                        Job newJob = ScribeManager.SerializeFromString<Job>(playerJob.Job);
                        newJob = Converter.PlayerJobToJob(newJob, playerJob);

                        Pawn pawn = Finder.GetPawnFromID(SessionHandler.SynchronousMap, playerJob.PawnID);
                        pawn.SetPositionDirect(Converter.StringToIntVec3(playerJob.PawnPosition));
                        pawn.jobs.StartJob(newJob, JobCondition.InterruptForced);
                    }
                    catch { };
                }
            });
        }
    }
}
