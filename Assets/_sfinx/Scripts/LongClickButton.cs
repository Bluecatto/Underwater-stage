using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LongClickButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler //Interfaces.
{
    //Variables.
    public UnityEvent onLongClick;
    public float requiredHoldTime; 
    public string sceneToLoad; 
    public bool done;

    private bool pointerDown;
    private float pointerDownTimer;

    [SerializeField]
    private Image fillImage;

    public void OnPointerDown(PointerEventData eventData)
    {
       //Sets the pointerDown to true (data).
        pointerDown = true;
       Debug.Log("OnPointerDown");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //Goes to Reset void with OnPointerUp data.
        Reset();
        Debug.Log("OnPointerUp");
    }

    public void Start()
    {
        if (onLongClick == null)
            onLongClick = new UnityEvent();

        onLongClick.AddListener(() => Debug.Log("Long Click Detected"));
    }

    private void Update()
    {
        //Saves the pointerdown in a time. 
        if (pointerDown && done) 
        {
            pointerDownTimer += Time.deltaTime;
            Debug.Log("First checkpoint");

            if (pointerDownTimer > requiredHoldTime)
            {
                Debug.Log("Second checkpoint");
                if (onLongClick != null)
                {
                    //If the player has hold the button enough the game starts.
                    onLongClick.Invoke();
                    Debug.Log("Game Started");  
                }

                LoadScene(); 
                Reset();
            }
            fillImage.fillAmount = pointerDownTimer / requiredHoldTime;
            Debug.Log("Last checkpoint");
        }
    }

    private void Reset()
    {
        //Resets the click button.
        pointerDown = false;
        pointerDownTimer = 0;
        fillImage.fillAmount = 0;
        Debug.Log("Reset");
    }

    public void SetDone(bool status)
    {
        //Checks the status of the button.
        done = status;
    }

    private void LoadScene()
    {
        //Loads the game when the button is full.
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log($"Loading scene: {sceneToLoad}");
            SceneManager.LoadScene(sceneToLoad);
            done = false;
        }
        else
        {
            Debug.LogError("Scene name is not set yet!");
        }
    }
}