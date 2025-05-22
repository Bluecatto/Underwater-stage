using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;   // for the GUI
using System;
using DG.Tweening;

public class TransducerRotation : MonoBehaviour
{
	/// <summary>
	/// *CHANGELOG*
	/// Added several checks for the serialconnect script
	/// Removed rotref & moveref
	/// Added a new interaction with the new Control handler script
	/// On event trigger the control function delegate will change to the appropriate function
	/// Removed the functionality for changing the control scheme, that now has a serparate script
	/// </summary>
	/// 

	public delegate void RotateScannerEvent(Quaternion rotationData);
	public static event RotateScannerEvent OnSendQuaternionRotation;

    public delegate void CalibrationEvent(float newOffset);
    public static event CalibrationEvent OnCalibratedSuccesfully;
    public static event CalibrationEvent OnSavedCalibrationLoaded;

    [SerializeField] private Transform objectToRotate = null;
    public bool rotationFrozen = false;

	delegate void RotationFunction();
    RotationFunction rotationMethod;
    SerialConnect serialConnect;
    bool transducerControls = false;

    public KeyCode calibrationKey = KeyCode.Return;
    public KeyCode rotate90DegCW = KeyCode.RightArrow;
    public KeyCode rotate90DegCCW = KeyCode.LeftArrow;

    // what to send to the Arduino when you press the action Button
    private Quaternion newRot;
    // to receive values from the Arduino
    //private List<int> actValues;
    private float[] parsedData;
    [SerializeField] private string currentPayload;
    public TransducerPayload payload;

    private Quaternion origRot;

	public float offsetOnY = 0;
	
	public float aimForAngle = 180;
	private float threshHold = 1f;

    void OnEnable()
    {
        SerialConnect.OnSendSerialPayload += CacheParsedData;
    }

    private void OnDisable()
    {
        SerialConnect.OnSendSerialPayload -= CacheParsedData;
    }

    void Start ()
    {
	    if (objectToRotate == null) objectToRotate = transform;
	    
		offsetOnY = GetRotationOffset();
        origRot = transform.rotation;

        serialConnect = SerialConnect.Instance;

        rotationMethod = HandleTransducerControls;
    }

    // Update is called once per frame
    void Update() {
        // calibrate rotation with numpad-5, only usefull in transducer mode
        if (Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(calibrationKey) && transducerControls)
            StartCoroutine(ResetRot());

        if (Input.GetKeyDown(rotate90DegCW))
        {
	        RotateCamera(true);
        }
        else if (Input.GetKeyDown(rotate90DegCCW))
        {
	        RotateCamera(false);
        }

        if (rotationMethod != null)
            rotationMethod();

        if (payload != null)
	        currentPayload = payload.GetFormattedStringQuaternionAndVector(2, 2);
    }

    private void RotateCamera(bool cw)
    {
	    foreach (Transform child in transform)
	    {
		    if (DOTween.IsTweening(child))
			    return;
		    
		    child.DORotate(child.eulerAngles + new Vector3(0, 0,  cw ? -90 : 90), 1);
	    }
    }

    private void CacheParsedData(TransducerPayload payload)
    {
        this.payload = payload;
    }

    public bool flipSparrow = false;

    // responsible for the transducer controls and gathering of input 
    public void HandleTransducerControls()
    {
        if (payload == null)
            return;

        // X Y Z W ??
        newRot = payload.quaternion;

        Quaternion adjustedRot = Quaternion.AngleAxis(offsetOnY, Vector3.up) * newRot;
        
#if controller_v2
        adjustedRot *= Quaternion.AngleAxis(180f, Vector3.forward);
		adjustedRot *= Quaternion.AngleAxis(180f, Vector3.right);
#endif

        if (OnSendQuaternionRotation != null)
            OnSendQuaternionRotation(adjustedRot);

        if (rotationFrozen)
            return;

	    objectToRotate.rotation = adjustedRot;
    }

	public float GetRotationOffset()
	{
		float offset = 0;

		if (PlayerPrefs.HasKey("SavedRotationOffset"))
        {
			offset = PlayerPrefs.GetFloat("SavedRotationOffset");
        }

        if (OnSavedCalibrationLoaded != null)
            OnSavedCalibrationLoaded(offset);

        return offset;
	}

	public IEnumerator ResetRot()
	{
		int acceptCounter = 3;
		float precisionThreshold = threshHold;
		float adjustedOffset = (precisionThreshold * 2f) - 0.25f;

		do
		{
			if (Mathf.Abs(objectToRotate.eulerAngles.y) < (aimForAngle - threshHold) || Mathf.Abs(objectToRotate.eulerAngles.y) > (aimForAngle + threshHold))
			{
				if (objectToRotate.eulerAngles.y < (aimForAngle - threshHold))
				{
					offsetOnY += adjustedOffset;
					yield return null;
				}

				if (objectToRotate.eulerAngles.y > (aimForAngle + threshHold))
				{
					offsetOnY -= adjustedOffset;
					yield return null;
				}
			}
			else
			{
				// Every time this else statement triggers we do 3 things.
				// First, we count the accept counter down by one
				acceptCounter--;
				// Second, we half the precision threshold, making the next iteration look more carefully
				precisionThreshold /= 2f;
				// And third, we also half the adjustedOffset it is going to apply to make sure we don't 
				adjustedOffset = (precisionThreshold * 2f) - 0.25f;
			}
		}
		// If the value gets (acceptCounter) amount of times inbetween the acceptable region, we count one down. 
		// If counter reaches 0, we break out of the loop
		while (acceptCounter > 0);

		SaveOffsetToPc(offsetOnY);
		Debug.Log("Done adjusting, enjoy your game!");

        if (OnCalibratedSuccesfully != null)
            OnCalibratedSuccesfully(offsetOnY);

        yield return null;
	}

    void SaveOffsetToPc(float newOffset)
    {
        PlayerPrefs.SetFloat("SavedRotationOffset", newOffset);

        PlayerPrefs.Save();
    }

    private void LevelController_OnFreezeRotation(bool frozen)
    {
        rotationFrozen = frozen;
    }

    //Added Quaternion
    public Quaternion CurrentTransducerRotation
    {
        get
        {
            if (payload != null)
            {
                return Quaternion.AngleAxis(offsetOnY, Vector3.up) * payload.quaternion;
            }
            return Quaternion.identity;
        }
    }

    public Quaternion GetCurrentRotation()
    {
        return objectToRotate.rotation;
    }

    public Quaternion GetRawControllerRotation()
    {
       return payload != null ? payload.quaternion : Quaternion.identity;
    }
}