using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using Verse;

namespace GameClient.Scribers
{
    public static class AnimalScriber
    {
        public static AnimalFile AnimalToString(Pawn animal)
        {
            AnimalFile animalData = new AnimalFile();

            animalData.ID = animal.ThingID;

            animalData.ScribeData = RTScriber.ThingToScribe(animal);

            return animalData;
        }

        public static Pawn StringToAnimal(AnimalFile file, bool overrideID = false)
        {
            return (Pawn)RTScriber.ScribeToThing(file.ScribeData, overrideID);
        }
    }
}
