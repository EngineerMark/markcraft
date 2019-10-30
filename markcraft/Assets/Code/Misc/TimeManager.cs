using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TimeState{
    DAY,
    NIGHT
}
public class TimeManager : MonoBehaviour
{
    public static TimeManager self;

    [SerializeField] private const int minTime = 0;
    [SerializeField] private const int maxTime = 24;
    [SerializeField] private float currentTime = 0;
    [SerializeField] private float lightRotation = 0;

    [SerializeField] private const float lightEndPointBeforeTransition = 0;

    [SerializeField] private float previousTick = 0.0f;
    [SerializeField] private float tick = 0.0f;
    [SerializeField] private Light sun;
    [SerializeField] private Light moon;
    [SerializeField] private TimeState timeState = TimeState.DAY;
    [SerializeField] private bool isPaused = false;

    [SerializeField] private Gradient dayNightGradient;

    void Start()
    {
        self = this;
    }

    void Transition(TimeState to){
        if (timeState == to)
            return;

        timeState = to;
    }

    void Update()
    {
        if(!isPaused)
            tick = Mathf.Min(1, tick + 0.0001f);
        if (tick >= 1)
            tick = 0;

        // Just update when tick is updated
        if (previousTick != tick)
        {
            lightRotation = Mathf.Lerp(1, 359, tick);
            //Vector3 rot = dirLight.transform.rotation.eulerAngles;
            //rot.x = lightRotation;
            moon.transform.rotation = Quaternion.AngleAxis(lightRotation, Vector3.right);
        }
        previousTick = tick;
    }
}
