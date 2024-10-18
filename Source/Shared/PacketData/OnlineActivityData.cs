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

        public MapFile _mapFile;

        public HumanFile[] _guestHumans = new HumanFile[0];

        public AnimalFile[] _guestAnimals = new AnimalFile[0];

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
