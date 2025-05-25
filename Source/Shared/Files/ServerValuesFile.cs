namespace Shared
{
    public class ServerValuesFile
    {
        public ServerValuesFile(string name)
        {
            ServerName = name;
        }

        public string ServerName = "RimWorld Together Server";

        public override string ToString()
        {
            return $"ServerValuesFile:|{ServerName}";
        }
    }
}