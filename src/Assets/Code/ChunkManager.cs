using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    // This manager handles all chunk threads
    private Queue<Thread> threadQueue;
    private const int maxParallelThreads = 4;
    private int currentThreadAmount = 0;
    public static ChunkManager self;

    private void Start(){
        threadQueue = new Queue<Thread>();
        self = this;
    }

    public void Add(Thread t){
        threadQueue.Enqueue(t);
    }

    public void Continue(Thread t){
        t.Join();
        currentThreadAmount--;
    }

    private void Update()
    {
        if (currentThreadAmount >= maxParallelThreads) return;
        if (threadQueue.Count == 0) return;

        Thread t = threadQueue.Dequeue();
        t.Start();
        currentThreadAmount++;
    }
}
