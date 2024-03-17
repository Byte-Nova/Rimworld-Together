using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace GameClient
{
    public static class LongEventThread
    {
        static Queue<Action> EventQueue = new Queue<Action>();

        public static void QueueEvent(Action action)
        {
            EventQueue.Enqueue(action);
        }

        public static void RunLongEvenThread()
        {
            try {
                while (true) {

                    Thread.Sleep(250);

                    if (EventQueue.Count > 0) {
                        Thread.Sleep(10);
                        Logs.Message($"Queued Events: {EventQueue.Count}");
                        Logs.Message($"Running Long Event: {EventQueue.Peek().ToString()}");
                        Master.threadDispatcher.Enqueue(EventQueue.Dequeue());
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.Error($"[Error] > Rimworld Together Long Event Thread has failed: \n {ex}");
            }
        }

    }
}
