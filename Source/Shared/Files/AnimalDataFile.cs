using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]

    public class AnimalDataFile
    {
        //Bio

        public string DefName;

        public string Name;

        public string BiologicalAge;

        public string ChronologicalAge;

        public string Gender;

        public string FactionDef;

        public string KindDef;

        //Hediffs

        public List<string> HediffDefNames = new List<string>();

        public List<string> HediffPartDefName = new List<string>();

        public List<string> HediffSeverity = new List<string>();

        public List<bool> HeddifPermanent = new List<bool>();

        //Trainables

        public List<string> TrainableDefNames = new List<string>();

        public List<bool> CanTrain = new List<bool>();

        public List<bool> HasLearned = new List<bool>();

        public List<bool> IsDisabled = new List<bool>();

        //Misc

        public string[] Position;
        
        public int Rotation;
    }
}