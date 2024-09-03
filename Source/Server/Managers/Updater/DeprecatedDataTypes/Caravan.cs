using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Updater
{
    public enum CaravanStepMode { Add, Remove, Move }
    [Serializable]
    public class CaravanDetails
    {
        public int ID;
        public int tile;
        public string owner;
        public double timeSinceRefresh;
    }
    [Serializable]
    public class CaravanData
    {
        public CaravanStepMode stepMode;
        public CaravanDetails details;
    }
}
