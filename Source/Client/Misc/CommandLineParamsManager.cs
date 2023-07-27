namespace RimworldTogether.GameClient.Misc
{
    public class CommandLineParamsManager
    {
        public static string GetArg(string name)
        {
            var args = System.Environment.GetCommandLineArgs();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Contains(name)) return args[i].Split('=')[1];
            }

            return null;
        }

        public static string name = GetArg("name");
        public static string password = GetArg("name");
        public static string ip = GetArg("ip") ?? "127.0.0.1";
        public static string port = GetArg("port") ?? "25555";
        public static string instantConnect = GetArg("instantConnect") ?? "false";
        public static string fastConnect = GetArg("fastConnect") ?? "false";
    }
}