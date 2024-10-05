using RimWorld;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GameClient
{
    public static class RewardManager
    {
        public static void ParsePacket(Packet packet)
        {
            RewardData siteData = Serializer.ConvertBytesToObject<RewardData>(packet.contents);
            ReceiveRewards(siteData);
        }

        private static void ReceiveRewards(RewardData siteData)
        {
            List<Thing> rewards = new List<Thing>();
            foreach (RewardFile reward in siteData._rewardData)
            {
                if (DefDatabase<ThingDef>.GetNamedSilentFail(reward.RewardDef) != null)
                {
                    ThingDataFile thingData = new ThingDataFile();
                    thingData.DefName = reward.RewardDef;
                    thingData.Quantity = reward.RewardAmount;
                    thingData.Quality = 0;
                    thingData.Hitpoints = DefDatabase<ThingDef>.GetNamed(thingData.DefName).BaseMaxHitPoints;
                    rewards.Add(ThingScribeManager.StringToItem(thingData));
                    Logger.Message($"Received {reward.RewardAmount} of {reward.RewardDef}", CommonEnumerators.LogImportanceMode.Verbose);
                } else 
                {
                    Logger.Warning($"Rewards couldn't be delivered with def {reward.RewardDef}. Double check if the def exist.");
                }
            }
            if (rewards.Count > 0)
            {
                TransferManager.GetTransferedItemsToSettlement(rewards.ToArray(),false,false,false);
                RimworldManager.GenerateLetter("Site rewards", $"You've received your site rewards", LetterDefOf.PositiveEvent);
                Logger.Message("Rewards delivered");
            }
        }
    }
}
