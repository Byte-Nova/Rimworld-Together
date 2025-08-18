using Shared.Files;
using System;
using System.IO;

namespace Shared.Files
{
    [Serializable]
    public class RoadValuesFile : BaseFile
    {
        public static string Path { get; set; } = string.Empty;

        //Allowance of the roads

        public bool AllowDirtPath { get; set; } = true;

        public bool AllowDirtRoad { get; set; } = true;

        public bool AllowStoneRoad { get; set; } = true;

        public bool AllowAsphaltPath { get; set; } = true;

        public bool AllowAsphaltHighway { get; set; } = true;

        //Cost of the roads

        public int DirtPathCost { get; set; } = 10;

        public int DirtRoadCost { get; set; } = 20;

        public int StoneRoadCost { get; set; } = 25;

        public int AsphaltPathCost { get; set; } = 30;

        public int AsphaltHighwayCost { get; set; } = 50;

        public override void Save()
        {
            try { Serializer.SerializeToFile(Path, this); }
            catch (Exception e) { throw new Exception(e.ToString()); }
        }

        public static object Load<T>()
        {
            if (File.Exists(Path)) return Serializer.SerializeFromFile<T>(Path);
            else
            {
                RoadValuesFile file = new RoadValuesFile();
                Serializer.SerializeToFile(Path, file);
                return file;
            }
        }
    }
}
