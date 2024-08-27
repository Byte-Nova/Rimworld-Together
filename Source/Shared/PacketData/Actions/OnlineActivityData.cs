using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class OnlineActivityData
    {
        public OnlineActivityStepMode stepMode;

        public OnlineActivityType activityType;

        //Map

        public MapData mapData;

        public List<HumanData> mapHumans = new List<HumanData>();

        public List<AnimalData> mapAnimals = new List<AnimalData>();

        public List<HumanData> caravanHumans = new List<HumanData>();

        public List<AnimalData> caravanAnimals = new List<AnimalData>();

        //Misc

        public string engagerName;

        public int fromTile;

        public int toTile;

        //Orders

        public PawnOrder pawnOrder;

        public CreationOrder creationOrder;

        public DestructionOrder destructionOrder;

        public DamageOrder damageOrder;

        public HediffOrder hediffOrder;

        public TimeSpeedOrder timeSpeedOrder;

        public GameConditionOrder gameConditionOrder;

        public WeatherOrder weatherOrder;

        public KillOrder killOrder;
    }
}
