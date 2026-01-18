using Shared;
using Shared.Misc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GameClient.Misc
{
    public class MainThreadHandler : MonoBehaviour
    {
        public static MainThreadHandler Instance { get; private set; }

        private static Queue<Action> ActionQueue { get; set; } = new Queue<Action>();

        public MainThreadHandler() { Instance = this; }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            Printer.Warning($"Created dispatcher for version > {CommonValues.ExecutableVersion}");
        }

        private void Update() 
        { 
            ExecuteAllQueue();

            if (SessionHandler.CurrentNetworkState == CommonEnumerators.ClientNetworkState.Connected)
            {
                DoPerFrameMethods();
            }
        }

        public void DoOnStartMethods()
        {
            foreach (MethodInfo method in MethodGatherer.OnStartMethods)
            {
                method.Invoke(null, null);
            }
        }

        public void DoOnEndMethods()
        {
            foreach (MethodInfo method in MethodGatherer.OnEndMethods)
            {
                method.Invoke(null, null);
            }
        }

        private void DoPerFrameMethods()
        {
            foreach (MethodInfo method in MethodGatherer.PerFrameMethods)
            {
                method.Invoke(null, null);
            }
        }

        public void DoOnSynchronousStartMethods()
        {
            foreach (MethodInfo method in MethodGatherer.OnSynchronousStartMethods)
            {
                method.Invoke(null, null);
            }
        }

        public void DoOnSynchronousEndMethods()
        {
            foreach (MethodInfo method in MethodGatherer.OnSynchronousEndMethods)
            {
                method.Invoke(null, null);
            }
        }

        public void Enqueue(Action action) { Enqueue(ActionWrapper(action)); }

        private void Enqueue(IEnumerator action)
        {
            lock (ActionQueue)
            {
                ActionQueue.Enqueue(() =>
                {
                    StartCoroutine(action);
                });
            }
        }

        private void ExecuteAllQueue()
        {
            lock (ActionQueue)
            {
                while (ActionQueue.Count > 0)
                {
                    ActionQueue.Dequeue().Invoke();
                }
            }
        }

        IEnumerator ActionWrapper(Action action)
        {
            action();
            yield return null;
        }
    }
}
