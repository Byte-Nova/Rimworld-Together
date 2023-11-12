using System.Threading;
using RimworldTogether.GameClient.Managers.Actions;

namespace RimworldTogether.GameClient.Misc
{
    public static class Threader
    {
        public enum Mode { Start, Heartbeat, Visit }

        public static void GenerateThread(Mode mode)
        {
            if (mode == Mode.Start)
            {
                Thread thread = new Thread(new ThreadStart(Network.Network.StartConnection));
                thread.IsBackground = true;
                thread.Name = "Networking";
                thread.Start();
            }

            else if (mode == Mode.Heartbeat)
            {
                Thread thread = new Thread(() => Network.Network.HeartbeatServer());
                thread.IsBackground = true;
                thread.Name = "Heartbeat";
                thread.Start();
            }

            else if (mode == Mode.Visit)
            {
                Thread thread = new Thread(() => VisitActionGetter.StartActionClock());
                thread.IsBackground = true;
                thread.Name = "Visit";
                thread.Start();
            }
        }
    }
}