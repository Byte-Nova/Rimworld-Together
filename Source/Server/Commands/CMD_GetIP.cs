using RTServer.Managers;
using RTShared.Commands;
using RTShared.Misc;

namespace RTServer.Commands
{
    public class CMD_GetIP : CMD_Base
    {
        public CMD_GetIP()
        {
            Prefix = "getip";
            Description = "Returns the public IPV4 of this server";
        }

        public override void Action() { Printer.Title($"Your server IPV4 is {ServerBrowserManager.GetPublicIP().Result}"); }
    }
}
