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
        public SiteInfoFile[] SiteInfoFiles = new SiteInfoFile[]
        {
            new SiteInfoFile()
            {
                DefName = "RTFarmland",
                DefNameCost = new string[]{"Silver"},
                Cost = new int[]{500},
                Rewards = new RewardFile()
                {
                    RewardDefs = new string[]{"RawRice", "RawCorn"},
                    RewardAmount = new int[] {50, 50}
                }
            },
            new SiteInfoFile()
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
            new SiteInfoFile()
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
            new SiteInfoFile()
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
            new SiteInfoFile()
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
            new SiteInfoFile()
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
            new SiteInfoFile()
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
            new SiteInfoFile()
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
            new SiteInfoFile()
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
