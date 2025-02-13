using System;
using System.Collections.Concurrent;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    public static readonly ConcurrentQueue<Action> Actions = new ConcurrentQueue<Action>();

    void Update()
    {
        while (Actions.TryDequeue(out Action action))
        {
            action?.Invoke();
        }
    }
}
