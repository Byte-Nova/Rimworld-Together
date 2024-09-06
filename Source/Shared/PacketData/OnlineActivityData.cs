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

        public MapFile _mapData;

        public List<HumanFile> _mapHumans = new List<HumanFile>();

        public List<AnimalFile> _mapAnimals = new List<AnimalFile>();

        public List<HumanFile> _caravanHumans = new List<HumanFile>();

        public List<AnimalFile> _caravanAnimals = new List<AnimalFile>();

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
