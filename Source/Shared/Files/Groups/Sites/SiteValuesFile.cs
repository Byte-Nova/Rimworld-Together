using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class SiteValuesFile
    {
        public float TimeIntervalMinute = 30f;
        public SiteConfigFile[] SiteIdendityFiles = new SiteConfigFile[]
        {
            new SiteConfigFile()
            {
                DefName = "RTFarmland",
                DefNameCost = new string[]{"Silver"},
                Cost = new int[]{500},
                Rewards = new RewardFile()
                {
                    RewardDefs = new string[]{"RawRice"},
                    RewardAmount = new int[] {50}
                }
            },
            new SiteConfigFile()
            {
                DefName = "RTQuarry",
                DefNameCost = new string[]{"Silver"},
                Cost = new int[]{500},
                Rewards = new RewardFile()
                {
                    RewardDefs = new string[]{"BlocksGranite", "BlocksMarble"},
                    RewardAmount = new int[] {50, 50}
                }
            },
            new SiteConfigFile()
            {
                DefName = "RTSawmill",
                DefNameCost = new string[]{"Silver"},
                Cost = new int[]{500},
                Rewards = new RewardFile()
                {
                    RewardDefs = new string[]{"WoodLog"},
                    RewardAmount = new int[] {50}
                }
            },
            new SiteConfigFile()
            {
                DefName = "RTBank",
                DefNameCost = new string[]{"Silver"},
                Cost = new int[]{500},
                Rewards = new RewardFile()
                {
                    RewardDefs = new string[]{"Silver", "Gold"},
                    RewardAmount = new int[] {25, 10}
                }
            },
            new SiteConfigFile()
            {
                DefName = "RTLaboratory",
                DefNameCost = new string[]{"Silver"},
                Cost = new int[]{750},
                Rewards = new RewardFile()
                {
                    RewardDefs = new string[]{"ComponentIndustrial"},
                    RewardAmount = new int[] {4}
                }
            },
            new SiteConfigFile()
            {
                DefName = "RTRefinery",
                DefNameCost = new string[]{"Silver"},
                Cost = new int[]{750},
                Rewards = new RewardFile()
                {
                    RewardDefs = new string[]{"Chemfuel"},
                    RewardAmount = new int[] {75}
                }
            },
            new SiteConfigFile()
            {
                DefName = "RTHerbalWorkshop",
                DefNameCost = new string[]{"Silver"},
                Cost = new int[]{750},
                Rewards = new RewardFile()
                {
                    RewardDefs = new string[]{"Cloth"},
                    RewardAmount = new int[] {50}
                }
            },
            new SiteConfigFile()
            {
                DefName = "RTTextileFactory",
                DefNameCost = new string[]{"Silver"},
                Cost = new int[]{750},
                Rewards = new RewardFile()
                {
                    RewardDefs = new string[]{"Cloth"},
                    RewardAmount = new int[] {50}
                }
            },
            new SiteConfigFile()
            {
                DefName = "RTFoodProcessor",
                DefNameCost = new string[]{"Silver"},
                Cost = new int[]{750},
                Rewards = new RewardFile()
                {
                    RewardDefs = new string[]{"MealSurvivalPack"},
                    RewardAmount = new int[] {10}
                }
            }
        };
    }
}
