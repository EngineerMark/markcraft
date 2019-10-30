using Markcraft;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine;

public class SaveLoad
{
    private static string saveLocation = GameManager.dataPath+"/Saves/";

    private Queue<Thread> threadQueue;
    private int parallelThreadLimit = 2;
    private int currentThreadAmount = 0;

    public void Start()
    {
        threadQueue = new Queue<Thread>();
    }

    public void Update()
    {
        if (currentThreadAmount > parallelThreadLimit) return;
        if (threadQueue.Count == 0) return;

        Thread t = threadQueue.Dequeue();
        t.Start();
        currentThreadAmount++;
    }

    private void Add(Thread t){
        threadQueue.Enqueue(t);
    }

    private void Continue(Thread t){
        t.Join();
        currentThreadAmount--;
    }

    public void SaveChunk(Chunk c){
        Add(new Thread(()=>SaveThread(c)));
    }

    public void SaveThread(Chunk c){
        int x = Convert.ToInt32(c.ChunkPosition.x);
        int z = Convert.ToInt32(c.ChunkPosition.z);
        string filename = string.Format("{0};{1}.dat", x, z);
        string path = string.Format("{0}{1}{2}", saveLocation, GameManager.saveName, "/Chunks/");
        Directory.CreateDirectory(path);

        string fullpath = string.Format("{0}{1}", path, filename);
        if (File.Exists(fullpath))
            File.WriteAllText(fullpath, string.Empty);
        FileStream fs = File.Create(fullpath);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(fs, c.chunkData);

        fs.Close();
        Debug.Log(path);
        currentThreadAmount--;
    }

    public bool LoadChunk(Chunk c){
        int x = Convert.ToInt32(c.ChunkPosition.x);
        int z = Convert.ToInt32(c.ChunkPosition.z);
        string path = string.Format("{0}{1}{2}", saveLocation, GameManager.saveName, "/Chunks/");
        string filename = string.Format("{0};{1}.dat", x, z);
        if (File.Exists(path + filename)){
            FileStream fs = File.OpenRead(path + filename);
            BinaryFormatter bf = new BinaryFormatter();
            int[,,] data = (int[,,])bf.Deserialize(fs);
            c.chunkData = data;
            return true;
        }
        return false;
    }
}
