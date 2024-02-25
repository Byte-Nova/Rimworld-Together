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

        //Hediffs

        public List<string> hediffDefNames = new List<string>();
        public List<string> hediffPart = new List<string>();
        public List<string> hediffSeverity = new List<string>();
        public List<bool> heddifPermanent = new List<bool>();

        //Trainables

        public List<string> trainableDefNames = new List<string>();
        public List<bool> canTrain = new List<bool>();
        public List<bool> hasLearned = new List<bool>();
        public List<bool> isDisabled = new List<bool>();

        //Misc

        public string[] position;
        public string rotation;
    }
}