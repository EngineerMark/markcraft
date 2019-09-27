using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static string saveName = "New Save";
    public static string dataPath;
    public static int saveSeed = 5;

    public static SaveLoad saveLoadSystem;

    public void Start()
    {
        dataPath = Application.dataPath;
        saveLoadSystem = new SaveLoad();
        saveLoadSystem.Start();
    }

    public void Update()
    {
        saveLoadSystem.Update();

    }
}
