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

        public static string Vector3ToString(IntVec3 data)
        {
            return $"{data.x}|{data.y}|{data.z}";
        }

        public static Rot4 StringToRot4(int data)
        {
            return new Rot4(data);
        }

        public static int Rot4ToInt(Rot4 data)
        {
            return data.AsInt;
        }
    }
}
