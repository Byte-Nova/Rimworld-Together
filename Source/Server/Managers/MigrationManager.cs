using Newtonsoft.Json.Linq;
using RTServer.Core;
using RTShared.Misc;
using static RTShared.Misc.Printer;

namespace RTServer.Managers
{
    public static class MigrationManager
    {
        public static void RunMigrations()
        {
            MigrateWorldObjectFactionDef();
        }

        // Servers before 26.8.16.1 stored world objects with a 'FactionDef' key instead of 'FactionDefName',
        // making the faction silently deserialize as empty and clients display NPC settlements as hostile
        private static void MigrateWorldObjectFactionDef()
        {
            int migratedCount = 0;

            foreach (string file in Directory.GetFiles(Master.WorldObjectsPath, "*.json"))
            {
                try
                {
                    JObject worldObjectJSON = JObject.Parse(File.ReadAllText(file));

                    JProperty legacyProperty = worldObjectJSON.Property("FactionDef");
                    if (legacyProperty == null || worldObjectJSON.Property("FactionDefName") != null) continue;

                    legacyProperty.Replace(new JProperty("FactionDefName", legacyProperty.Value));
                    File.WriteAllText(file, worldObjectJSON.ToString());

                    migratedCount++;
                    Printer.Warning($"Migrated legacy 'FactionDef' key at '{Path.GetFileName(file)}'", Verbosity.Verbose);
                }
                catch (Exception e) { Printer.Error($"Failed to migrate world object at '{file}'. Exception > {e}"); }
            }

            if (migratedCount > 0) Printer.Warning($"Migrated '{migratedCount}' world objects from a previous version");
        }
    }
}
