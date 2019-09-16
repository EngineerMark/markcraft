using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static string saveName = "New Save";
    public static string dataPath;

    public void Start()
    {
        dataPath = Application.dataPath;
    }
}
