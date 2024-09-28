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
                Rewards = new RewardFile[]
                {new RewardFile()
                    {
                        RewardDef = "RawRice",
                        RewardAmount = 50
                    },new RewardFile()
                    {
                        RewardDef = "RawCorn",
                        RewardAmount = 50
                    }
                    ,new RewardFile()
                    {
                        RewardDef = "SmokeleafLeaves",
                        RewardAmount = 25
                    }
                    ,new RewardFile()
                    {
                        RewardDef = "PsychoidLeaves",
                        RewardAmount = 25
                    }
                }
            },
            new SiteInfoFile()
            {
                DefName = "RTHunterCamp",
                DefNameCost = new string[]{"Silver"},
                Cost = new int[]{500},
                Rewards = new RewardFile[]
                {new RewardFile()
                    {
                        RewardDef = "Meat_Muffalo",
                        RewardAmount = 125
                    },new RewardFile()
                    {
                        RewardDef = "Meat_Human",
                        RewardAmount = 125
                    },new RewardFile()
                    {
                        RewardDef = "Leather_Chinchilla",
                        RewardAmount = 60
                    }
                    ,new RewardFile()
                    {
                        RewardDef = "Leather_Bear",
                        RewardAmount = 60
                    },
                }
            },
            new SiteInfoFile()
            {
                DefName = "RTQuarry",
                DefNameCost = new string[]{"Silver"},
                Cost = new int[]{500},
                Rewards = new RewardFile[]
                {
                    new RewardFile()
                    {
                        RewardDef = "BlocksGranite",
                        RewardAmount = 50
                    },
                    new RewardFile()
                    {
                        RewardDef = "BlocksMarble",
                        RewardAmount = 50
                    },
                    new RewardFile()
                    {
                        RewardDef = "Steel",
                        RewardAmount = 30
                    },
                    new RewardFile()
                    {
                        RewardDef = "Plasteel",
                        RewardAmount = 10
                    }
                }
            },
            new SiteInfoFile()
            {
                DefName = "RTSawmill",
                DefNameCost = new string[]{"Silver"},
                Cost = new int[]{300},
                Rewards = new RewardFile[]
                {
                    new RewardFile()
                    {
                        RewardDef = "WoodLog",
                        RewardAmount = 100
                    }
                }
            },
            new SiteInfoFile()
            {
                DefName = "RTBank",
                DefNameCost = new string[]{"Silver"},
                Cost = new int[]{750},
                Rewards = new RewardFile[]
                {
                    new RewardFile()
                    {
                        RewardDef = "Silver",
                        RewardAmount = 50
                    },
                    new RewardFile()
                    {
                        RewardDef = "Gold",
                        RewardAmount = 15
                    }
                }
            },
            new SiteInfoFile()
            {
                DefName = "RTLaboratory",
                DefNameCost = new string[]{"Silver"},
                Cost = new int[]{750},
                Rewards = new RewardFile[]
                {
                    new RewardFile()
                    {
                        RewardDef = "ComponentIndustrial",
                        RewardAmount = 10
                    },
                    new RewardFile()
                    {
                        RewardDef = "ComponentSpacer",
                        RewardAmount = 2
                    },
                }
            },
            new SiteInfoFile()
            {
                DefName = "RTRefinery",
                DefNameCost = new string[]{"Silver"},
                Cost = new int[]{750},
                Rewards = new RewardFile[]
                {
                    new RewardFile()
                    {
                        RewardDef = "Chemfuel",
                        RewardAmount = 50
                    }
                }
            },
            new SiteInfoFile()
            {
                DefName = "RTHerbalWorkshop",
                DefNameCost = new string[]{"Silver"},
                Cost = new int[]{750},
                Rewards = new RewardFile[]
                {
                    new RewardFile()
                    {
                        RewardDef = "MedicineHerbal",
                        RewardAmount = 10
                    },
                    new RewardFile()
                    {
                        RewardDef = "MedicineIndustrial",
                        RewardAmount = 2
                    }
                }
            },
            new SiteInfoFile()
            {
                DefName = "RTTextileFactory",
                DefNameCost = new string[]{"Silver"},
                Cost = new int[]{750},
                Rewards = new RewardFile[]
                {
                    new RewardFile()
                    {
                        RewardDef = "Cloth",
                        RewardAmount = 50
                    },
                    new RewardFile()
                    {
                        RewardDef = "DevilstrandCloth",
                        RewardAmount = 30
                    }
                }
            },
            new SiteInfoFile()
            {
                DefName = "RTFoodProcessor",
                DefNameCost = new string[]{"Silver"},
                Cost = new int[]{750},
                Rewards = new RewardFile[]
                {
                    new RewardFile()
                    {
                        RewardDef = "MealSurvivalPack",
                        RewardAmount = 10
                    },
                    new RewardFile()
                    {
                        RewardDef = "MealNutrientPaste",
                        RewardAmount = 30
                    }
                }
            }
        };
    }
}
