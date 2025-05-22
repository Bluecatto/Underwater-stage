using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class RockingMovement : MonoBehaviour
{
    //Variables for the movement settings.
    [Header("Movement settings")]
    public float forwardSpeed = 3f;
    public bool gameStarted = false;

    [Header("Tilting settings")]
    public float tiltIntensity = 1.0f;
    public float maxTiltAngle = 30.0f;
    public float maxTiltAllowed = 20.0f;

    [Header("Rocking settings")]
    public float tiltInterval = 3.0f;
    public float rockingSpeed = 1.0f;

    [Header("UI settings")]
    public TextMeshProUGUI text;
    public GameObject gameOverScreen;

    [SerializeField] private TransducerRotation transducerRotation;
    public float maxTiltTime = 3f;

    private Quaternion originalRotation;
    private float rockingTimer = 0f;
    private bool isTiltingRight = true;
    private float tiltDuration = 0f;
    private bool isTilting = false;

    private void Start()
    {
        //Saves the rotation into a variable and gets the TransducerRotation script component.
        originalRotation = transform.rotation;

        if (gameOverScreen != null)
        {
            //Sets the gameover screen false first.
            gameOverScreen.SetActive(false);
        }
    }

    private void Update()
    {
        if (gameStarted)
        {
            //Goes to void MoveBoat if the game started.
            MoveBoat();

            //Tilts the boat when the rocking timer is 0.
            if (!isTilting && rockingTimer == 0)
            {
                StartCoroutine(TiltBoatRandomly());
            }
        }
    }

    private void MoveBoat()
    {
        //Saves the transform into a Vector3, multiplies forwardspeed with the time counted.
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);
        rockingTimer += Time.deltaTime;

        if (!isTilting && rockingTimer >= tiltInterval)
        {
            //Tilts the boat on a few seconds.
            rockingTimer = 0f;
            StartCoroutine(TiltBoatRandomly());
        }
    }

    private IEnumerator TiltBoatRandomly()
    {
        //Makes the boat tilt at a random value.
        isTilting = true;
        isTiltingRight = Random.value > 0.5f;

        float targetTiltAngleZ = isTiltingRight ? maxTiltAngle : -maxTiltAngle;
        float elapsedTiltTime = 0f;

        while (elapsedTiltTime < tiltInterval)
        {
            //Won't show text whenever the game is over.
            if (WinningEnd.gameOver)
            {
                if (text != null)
                {
                    text.text = string.Empty;
                }
                yield break;
            }
            elapsedTiltTime += Time.deltaTime;

            if (transducerRotation != null)
            {
                //Inherites from the transducerRotation script and puts it in a quaternion.
                Quaternion transducerRot = transducerRotation.CurrentTransducerRotation;

                float playerTiltAdjustment = transducerRot.eulerAngles.z;
                if (playerTiltAdjustment > 180)
                    playerTiltAdjustment -= 360;

                //Calculates the tilt adjustments.
                playerTiltAdjustment = Mathf.Clamp(playerTiltAdjustment, -maxTiltAngle, maxTiltAngle) * tiltIntensity;

                float totalTilt = Mathf.Clamp(targetTiltAngleZ - playerTiltAdjustment, -maxTiltAngle, maxTiltAngle);

                if (Mathf.Abs(targetTiltAngleZ) > 0.01f || Mathf.Abs(playerTiltAdjustment) > 0.01f)
                {
                    Quaternion targetRotation = originalRotation * Quaternion.Euler(0, 0, totalTilt);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rockingSpeed * Time.deltaTime);
                }

                //If the tilt is bigger than the maxtiltallowed, ui text will appear saying that the ship is sinking.
                if (Mathf.Abs(totalTilt) > maxTiltAllowed)
                {
                    if (text != null)
                        text.text = "The ship is sinking";

                    tiltDuration += Time.deltaTime;

                    if (tiltDuration >= maxTiltTime)
                    {
                        GameOver();
                    }
                }
                else
                {
                    tiltDuration = 0f;
                    if (text != null)
                    {
                        text.text = string.Empty;
                    }
                }
            }
            yield return null;
        }
        isTilting = false;
    }

    private void GameOver()
    {
        //Sets the gameover UI screen to active and stops the game (also the timescale).
        WinningEnd.gameOver = true; 

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
            Time.timeScale = 0f;
        }
    }
}