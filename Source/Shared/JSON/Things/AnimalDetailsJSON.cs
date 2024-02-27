using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]

    public class AnimalDetailsJSON
    {
        //Bio

        public string defName;
        public string name;
        public string biologicalAge;
        public string chronologicalAge;
        public string gender;
        public string factionDef;
        public string kindDef;

        //Hediffs

        public List<string> hediffDefNames = new List<string>();
        public List<string> hediffPartDefName = new List<string>();
        public List<string> hediffSeverity = new List<string>();
        public List<bool> heddifPermanent = new List<bool>();

        //Trainables

        public List<string> trainableDefNames = new List<string>();
        public List<bool> canTrain = new List<bool>();
        public List<bool> hasLearned = new List<bool>();
        public List<bool> isDisabled = new List<bool>();

        //Misc

        public string[] position;
        public int rotation;
    }
}