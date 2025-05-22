using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BeginLevel : MonoBehaviour
{


    [Header("Hold settings")]
    public float holdDuration = 3f;
    public TextMeshProUGUI holdText;
    public RockingMovement controller;

    private float holdTimer = 0f;
    private bool isHolding = false;
    private bool gameStarted = false;

    private void Start()
    {
        if (holdText != null)
        {
            holdText.text = $"Hold for {holdDuration} seconds to start";
        }
    }

    public void OnPointer()
    {
        isHolding = false;
        holdTimer = 0f;

        if (holdText != null)
        {
            holdText.text = $"Hold for {holdDuration} seconds to start";
        }
    }

    private void Update()
    {
        if (gameStarted) return;

        if (isHolding)
        {
            holdTimer += Time.deltaTime;

            if (holdText != null)
            {
                holdText.text = $"Hold for {Mathf.Clamp(holdDuration - holdTimer, 0, holdDuration):0.0} seconds to start";
            }

            if (holdTimer >= holdDuration)
            {
                StartGame();
            }
        }
    }

    private void StartGame()
    {
        gameStarted = true;
        isHolding = false;

        if (controller != null)
        {
            controller.gameStarted = true;
        }

        if (holdText != null)
        {
            holdText.text = "Game started!";
        }

        Debug.Log("Game started!");
    }
}