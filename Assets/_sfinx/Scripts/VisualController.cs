using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Shapes;

public class VisualController : MonoBehaviour
{
    [Header("Visual settings")]
    public GameObject controller;
    public GameObject greenArea;
    public GameObject circleConnect;
    public Transform dot;
    public float uiRange = 100f;

    [Header("UI Settings")]
    public TextMeshProUGUI warningText;
    public GameObject showGameOverScreen;

    private RockingMovement rockingMovement;
    private TransducerRotation transducerRotation;

    private float warningTimer = 3f;
    private bool isGameOver = false;

    void OnEnable()
    {
        TransducerRotation.OnSendQuaternionRotation += UpdateDotPosition;
    }

    void OnDisable()
    {
        TransducerRotation.OnSendQuaternionRotation -= UpdateDotPosition;
    }

    void Start()
    {
        if (controller != null)
        {
            rockingMovement = controller.GetComponent<RockingMovement>();
            transducerRotation = controller.GetComponent<TransducerRotation>();
        }

        if (warningText != null)
        {
            warningText.text = string.Empty;
        }
        if (dot != null)
        {
            dot.position = Vector3.zero;
        }
    }

    void UpdateDotPosition(Quaternion rotationData)
    {
        if (dot == null) return;

        Vector3 eulerRotation = rotationData.eulerAngles;
        if (eulerRotation == Vector3.zero)
        {
            dot.localPosition = Vector3.zero;
            return;
        }

        float tiltX = eulerRotation.x;
        float tiltZ = eulerRotation.z;

        if (tiltX > 180) tiltX -= 360;
        if (tiltZ > 180) tiltZ -= 360;

        float xPos = Mathf.Clamp(tiltZ / 45f, -1f, 1f) * uiRange;
        float yPos = Mathf.Clamp(tiltX / 45f, -1f, 1f) * uiRange;

        Vector2 clampedPosition = new Vector2(xPos, yPos);
        if (clampedPosition.magnitude > uiRange)
        {
            clampedPosition = clampedPosition.normalized * uiRange;
        }
        dot.localPosition = clampedPosition;
    }

    void Update()
    {
        if (greenArea == null || dot == null || warningText == null) return;

        Collider2D greenAreaCollider = greenArea.GetComponent<Collider2D>();
        if (greenAreaCollider == null) return;

        Vector2 dotWorldPosition = dot.position;
        bool isInGreenArea = greenAreaCollider.OverlapPoint(dotWorldPosition);

        if (isInGreenArea)
        {
            warningText.text = string.Empty;
            dot.gameObject.SetActive(true);
            warningTimer = 0f;
        }
        else
        {
            //Needs fixing
            warningText.text = "Hold the controller correctly!";
            dot.gameObject.SetActive(false);
            warningTimer += Time.deltaTime;
            DotConnect();
        }
        dot.gameObject.SetActive(true);

        if (warningTimer >= 3f)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        isGameOver = true;
        WinningEnd.gameOver = true;

        if (warningText != null)
        {
            warningText.text = string.Empty;
        }

        if (showGameOverScreen != null)
        {
            showGameOverScreen.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    private void DotConnect()
    {
        if (Mouse.current.rightButton.isPressed)
        {
            circleConnect.gameObject.SetActive(true);
            Debug.Log("Object is true");
        }
        else
        {
            circleConnect.gameObject.SetActive(false);
            Debug.Log("Object is false");
        }
        //fixing
        circleConnect.gameObject.SetActive(true);
    }
}