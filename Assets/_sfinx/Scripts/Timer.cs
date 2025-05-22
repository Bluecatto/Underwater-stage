using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    //Making a UI timer.
    [SerializeField] public TextMeshProUGUI timerText;
    public float elapsedTime;

    void Update()
    {
        //Saves the elapsedTime into deltatime and calculates the time from ints.
        elapsedTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);

        //The time will work now on the UI screen.
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}