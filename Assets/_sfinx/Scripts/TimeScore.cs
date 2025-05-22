using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
//using UnityEditor.SearchService;
using UnityEngine.SceneManagement;
public class TimeScore : Timer //Inheritance from the Timer script.
{
    //Making a endscreen and a score text.
    [SerializeField] public GameObject endScreenUI;
    [SerializeField] public TextMeshProUGUI finalScoreText;

    void Start()
    {
        //Cant see the screen yet.
        endScreenUI.SetActive(false);
    }

   public void ShowEndScreen()
   {
        //Takes info from the Timer script and sets it into the UI screen.
        enabled = false;
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        finalScoreText.text = string.Format("Final Time: {0:00}:{1:00}", minutes, seconds);

        timerText.gameObject.SetActive(false);
        endScreenUI.SetActive(true);
   }
}