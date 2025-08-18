using GameServer.Core;
using GameServer.Files;
using Shared;

namespace GameServer.Misc
{
    public static class Validator
    {
        private static readonly string PathToVersionFile = Path.Combine(Master.MainPath, "Prev-Version.json");

        public static void CheckIfFirstBoot()
        {
            if (!File.Exists(PathToVersionFile))
            {
                Serializer.SerializeToFile(PathToVersionFile, new VersionFile());

                Printer.Error("If this is your first time installing Rimworld Together, please take a look around the configuration files and our wiki > https://github.com/RimWorld-Together/Rimworld-Together/wiki");
            }
        }
    }
}