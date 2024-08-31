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

        public List<HumanData> _mapHumans = new List<HumanData>();

        public List<AnimalData> _mapAnimals = new List<AnimalData>();

        public List<HumanData> _caravanHumans = new List<HumanData>();

        public List<AnimalData> _caravanAnimals = new List<AnimalData>();

        //Misc

        public string _engagerName;

        public int _fromTile;

        public int _toTile;

        //Orders

        public PawnOrder _pawnOrder;

        public CreationOrder _creationOrder;

        public DestructionOrder _destructionOrder;

        public DamageOrder _damageOrder;

        public HediffOrder _hediffOrder;

        public TimeSpeedOrder _timeSpeedOrder;

        public GameConditionOrder _gameConditionOrder;

        public WeatherOrder _weatherOrder;

        public KillOrder _killOrder;
    }
}
