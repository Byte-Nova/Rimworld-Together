using RTShared.Commands;
using RTShared.Misc;

namespace RTServer.Commands
{
    public class CMD_GetVersion : CMD_Base
    {
        public CMD_GetVersion()
        {
            Prefix = "getversion";
            Description = "Returns the version of this server";
        }

        public override void Action() { Printer.Title($"Your server version is {CommonValues.ExecutableVersion}"); }
    }
}
