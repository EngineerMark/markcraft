using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassTest : MonoBehaviour
{
    public GameObject grass;
    void Start()
    {
        GameObject.Instantiate(grass, new Vector3(1,1,3), Quaternion.identity);
    }
}
