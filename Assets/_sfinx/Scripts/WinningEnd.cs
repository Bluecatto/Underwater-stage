using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class WinningEnd : MonoBehaviour
{
    //Making a gamobject.
    public GameObject winningScreen;
    public static bool gameOver = false;

    private void Start()
    {
        //Sets the UI screen, if there is a UI screen set up.
        if (winningScreen != null)
        {
            winningScreen.SetActive(false);
        }
        else
        {
            Debug.Log("There is no UI screen yet.");
        }

        gameOver = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Looks for a tag and a collision on a object.
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (winningScreen != null)
            {
                winningScreen.SetActive(true);
            }
        }
        //Stops the game.
        gameOver = true;
        Time.timeScale = 0f;

    }
}