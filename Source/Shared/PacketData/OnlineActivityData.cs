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

        public CreationOrderData[] _creationOrders = new CreationOrderData[0];

        public DestructionOrderData[] _destructionOrders = new DestructionOrderData[0];

        public DamageOrderData[] _damageOrders = new DamageOrderData[0];

        public HediffOrderData[] _hediffOrders = new HediffOrderData[0];

        public TimeSpeedOrderData[] _timeSpeedOrders = new TimeSpeedOrderData[0];

        public GameConditionOrderData[] _gameConditionOrders = new GameConditionOrderData[0];

        public WeatherOrderData[] _weatherOrders = new WeatherOrderData[0];

        public PawnJobData[] _jobOrders = new PawnJobData[0];
    }
}
