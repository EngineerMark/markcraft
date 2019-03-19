using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetSunLight : MonoBehaviour
{

    public Renderer lightwall;

    Material sky;

    public Renderer water;

    public Transform stars;
    public Transform worldProbe;

    // Use this for initialization
    void Start()
    {

        sky = RenderSettings.skybox;

    }

    bool lighton = false;

    // Update is called once per frame
    void Update()
    {

        stars.transform.rotation = transform.rotation;

        if (Input.GetKeyDown(KeyCode.T))
        {

            lighton = !lighton;

        }

        Vector3 tvec = Camera.main.transform.position;
        worldProbe.transform.position = tvec;
    }
}