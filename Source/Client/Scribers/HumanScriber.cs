using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using Verse;

namespace GameClient.Scribers
{
    public static class HumanScriber
    {
        public static HumanFile HumanToString(Pawn human)
        {
            HumanFile humanFile = new HumanFile();

            humanFile.ID = human.ThingID;

            humanFile.ScribeData = RTScriber.ThingToScribe(human);

            return humanFile;
        }

        public static Pawn StringtoHuman(HumanFile file, bool overrideID = false)
        {
            return (Pawn)RTScriber.ScribeToThing(file.ScribeData, overrideID);
        }
    }
}
