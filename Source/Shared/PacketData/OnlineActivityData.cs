using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class OnlineActivityData
    {
        public OnlineActivityStepMode _stepMode;

        public OnlineActivityType _activityType;

        //Map

        public MapData _mapData;

        public List<HumanDataFile> _mapHumans = new List<HumanDataFile>();

        public List<AnimalDataFile> _mapAnimals = new List<AnimalDataFile>();

        public List<HumanDataFile> _caravanHumans = new List<HumanDataFile>();

        public List<AnimalDataFile> _caravanAnimals = new List<AnimalDataFile>();

        //Misc

        public string _engagerName;

        public int _fromTile;

        public int _toTile;

        //Orders

        public PawnOrderData _pawnOrder;

        public CreationOrderData _creationOrder;

        public DestructionOrderData _destructionOrder;

        public DamageOrderData _damageOrder;

        public HediffOrderData _hediffOrder;

        public TimeSpeedOrderData _timeSpeedOrder;

        public GameConditionOrderData _gameConditionOrder;

        public WeatherOrderData _weatherOrder;

        public KillOrderData _killOrder;
    }
}
