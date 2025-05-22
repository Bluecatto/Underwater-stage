using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Winning : MonoBehaviour
{
    //Making an gameobject.
    public GameObject winUIPanel;

    private void OnCollisionEnter(Collision collision)
    {
        //If a gameobject has the tag wintrigger it will show up a screen (WinGame).
        if (collision.gameObject.CompareTag("WinTrigger"))
        {
            WinGame();
        }
    }

    private void WinGame()
    {
        //Sets the panel active and stops the game.
        if (winUIPanel != null)
        {
            winUIPanel.SetActive(true);
            //Debug.Log("You won!");
        }
        else
        {
            Debug.LogWarning("There is no panel yet");
        }

        Time.timeScale = 0f;
    }
}