using Shared.Files;
using System;
using System.IO;

namespace Shared.Files
{
    [Serializable]
    public class WorldValuesFile : BaseFile
    {
        public static string Path { get; set; } = string.Empty;

        public int PersistentRandomValue { get; set; } = -1;

        public string SeedString { get; set; } = string.Empty;

        public float PlanetCoverage { get; set; } = -1f;

        public int Rainfall { get; set; } = -1;

        public int Temperature { get; set; } = -1;

        public int Population { get; set; } = -1;

        public int LandmarkDensity { get; set; } = -1;

        public float Pollution { get; set; } = -1f;

        public PlanetFeatureDetails[] Features { get; set; } = null;

        public RoadDetails[] Roads { get; set; } = null;

        public PollutionDetails[] PollutedTiles { get; set; } = null;

        public PlanetNPCFactionDetails[] NPCFactions { get; set; } = null;

        public PlanetNPCSettlementDetails[] NPCSettlements { get; set; } = null;

        public override void Save()
        {
            try { Serializer.ObjectBytesToFile(Path, this); }
            catch (Exception e) { throw new Exception(e.ToString()); }
        }

        public static object Load<T>()
        {
            if (File.Exists(Path)) return Serializer.FileBytesToObject<T>(Path);
            else return null;
        }
    }
}
