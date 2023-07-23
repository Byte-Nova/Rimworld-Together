namespace RimworldTogether.GameServer.Files
{
    [Serializable]
    public class WorldValuesFile
    {
        public string SeedString;
        public float PlanetCoverage;
        public float Rainfall;
        public float Temperature;
        public float Population;
        public float Pollution;

        public List<string> Factions = new List<string>();
    }
}
