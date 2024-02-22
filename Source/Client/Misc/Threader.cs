using System.Threading;
using RimworldTogether.GameClient.Managers.Actions;

namespace RimworldTogether.GameClient.Misc
{
    public static class Threader
    {
        public enum Mode { Listener, Health, KASender, Visit }

        public static void GenerateThread(Mode mode)
        {
            if (mode == Mode.Listener)
            {
                Thread thread = new Thread(() => Network.Network.serverListener.ListenToServer());
                thread.IsBackground = true;
                thread.Name = "Listen";
                thread.Start();
            }

            else if (mode == Mode.Health)
            {
                Thread thread = new Thread(() => Network.Network.serverListener.CheckForConnectionHealth());
                thread.IsBackground = true;
                thread.Name = "Health";
                thread.Start();
            }

            else if (mode == Mode.KASender)
            {
                Thread thread = new Thread(() => Network.Network.serverListener.SendKAFlag());
                thread.IsBackground = true;
                thread.Name = "KASender";
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