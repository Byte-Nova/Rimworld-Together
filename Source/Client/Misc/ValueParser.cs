using Verse;

namespace GameClient
{
    public static class ValueParser
    {
        public static IntVec3 StringToVector3(string data)
        {
            string[] dataSplit = data.Split('|');
            return new IntVec3(int.Parse(dataSplit[0]), int.Parse(dataSplit[1]), int.Parse(dataSplit[2]));
        }

        public static string Vector3ToString(IntVec3 data) { return $"{data.x}|{data.y}|{data.z}"; }

        public static int[] IntVec3ToArray(IntVec3 data) { return new int[] { data.x, data.y, data.z }; }

        public static IntVec3 ArrayToIntVec3(int[] data) { return new IntVec3(data[0], data[1], data[2]); }

        public static Rot4 IntToRot4(int data) { return new Rot4(data); } 

        public static int Rot4ToInt(Rot4 data) { return data.AsInt; }
    }
}
