using System;
using UnityEngine; 
// my namespace import does not appear to work w/r/t Syntax

public class Clock : MonoBehaviour { // Mono ( engine? ) Behaviour

    [SerializeField] // used to make these recognizable by Unity
    Transform hoursPivot, minutesPivot, secondsPivot; // same name as the component

    const float hoursToDegrees = -30f, minutesToDegrees = -6f, secondsToDegrees = -6f;

    void Update() { // keyword, like Awake
        //Debug.Log(DateTime.Now);
        TimeSpan  time = DateTime.Now.TimeOfDay;
        hoursPivot.localRotation = Quaternion.Euler(0f,0f,hoursToDegrees * (float)time.TotalHours); // x,y,z of one rotation
        minutesPivot.localRotation = Quaternion.Euler(0f,0f,minutesToDegrees * (float)time.TotalMinutes);
        secondsPivot.localRotation = Quaternion.Euler(0f,0f,secondsToDegrees * (float)time.TotalSeconds);
    }
}