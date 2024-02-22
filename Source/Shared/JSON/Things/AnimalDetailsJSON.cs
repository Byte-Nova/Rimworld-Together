using System.Collections.Generic;

namespace RimworldTogether.Shared.JSON.Things
{
    public class AnimalDetailsJSON
    {
        public string defName;
        public string name;
        public string biologicalAge;
        public string chronologicalAge;
        public string gender;

        public List<string> hediffDefNames = new List<string>();
        public List<string> hediffPart = new List<string>();
        public List<string> hediffSeverity = new List<string>();
        public List<bool> heddifPermanent = new List<bool>();

        public List<string> trainableDefNames = new List<string>();
        public List<bool> canTrain = new List<bool>();
        public List<bool> hasLearned = new List<bool>();
        public List<bool> isDisabled = new List<bool>();

        public string position;
    }
}