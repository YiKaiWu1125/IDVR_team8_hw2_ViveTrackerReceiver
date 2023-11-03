using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher instance = null;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
    }

    public static UnityMainThreadDispatcher Instance()
    {
        if (instance == null)
        {
            GameObject obj = new GameObject("UnityMainThreadDispatcher");
            instance = obj.AddComponent<UnityMainThreadDispatcher>();
        }
        return instance;
    }

    private Queue<Action> actionQueue = new Queue<Action>();

    public void Enqueue(Action action)
    {
        actionQueue.Enqueue(action);
    }

    void Update()
    {
        while (actionQueue.Count > 0)
        {
            actionQueue.Dequeue().Invoke();
        }
    }
}
