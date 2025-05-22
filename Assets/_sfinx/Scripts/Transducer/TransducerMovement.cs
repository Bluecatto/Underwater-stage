using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityStandardAssets.ImageEffects;
using UnityEngine.SceneManagement;
using System;

public class TransducerMovement : MonoBehaviour {
	/// <summary>
	/// *CHANGELOG*
	/// moverefprefab is now being assigned through code via resources.load
	/// You can still assign it manually
	/// 
	/// There are new level boundary values set in this script for alternate movement
	/// 
	/// Blur & grayscale functionality has been removed
	/// Rotate scanner has safety nets (If statement for existing etc.)
	/// Removed Moveref & rotref
	/// Added a new interaction with the new Control handler script
	/// On event trigger the control function delegate will change to the appropriate function
	/// </summary>

	public delegate void PositionEvent(Vector3 pos);
    /// <summary>
    /// Send the world position mouseposition in the scene
    /// </summary>
	public static event PositionEvent OnSendPositionInfo;

    public delegate void NormalizedPositionEvent(Vector2 normalizedMousePosition);
    /// <summary>
    /// Sends the normalized mouseposition in 0-1 range
    /// </summary>
    public static event NormalizedPositionEvent OnSendNormalizedMousePos;

    public delegate void DistanceEvent(float accumulatedDistance);
    public static event DistanceEvent OnSendAccumulatedDistance = delegate { };
    
    [SerializeField] private Transform objectToMove = null;

    delegate void movementFunction();
    movementFunction movementMethod;

    public bool movementFrozen = false;
    public bool clamp;

    [Range(0, 10)]
	[Tooltip("Additional height added to the rotation point of the transducer. Keep at or around 0.0")]
	public float hoogte = 0.0f;

	[Range(1, 100)]
	[Tooltip("Added delay when dragging the cursor. Low number is very soft movement, high number is very responsive")]
	public int positionDelay = 10;


	private int screenWidth;
	private int screenHeight;
    public float speed = 5f;
	private CanvasGroup canvasGroup;
	
	private Vector3[] bounderyPositions;
	private Vector3 leftBottomCorner, position;
	private float xBound;
	private float zBound;

    public float defaultLvlBoundX = 15f;
    public float defaultLvlBoundZ = 15f;

    private float accumulatedDistance = 0f;

    void Start()
    {
	    if (objectToMove == null)
		    objectToMove = transform;
	    
        screenWidth = Screen.width;
		screenHeight = Screen.height;

        if (LevelBoundaries.Instance != null)
        {
			bounderyPositions = LevelBoundaries.Instance.GetPositionsOfCorners();
            leftBottomCorner = bounderyPositions[0];
            xBound = LevelBoundaries.xBoundary;
            zBound = LevelBoundaries.zBoundary;
            clamp = true;
        }
        else
        {
            leftBottomCorner = Vector3.zero;
            xBound = defaultLvlBoundX;
            zBound = defaultLvlBoundZ;
        }

        OnSendAccumulatedDistance(accumulatedDistance);
        
        movementMethod = MouseMovement;
    }

    // Update is called once per frame
    void Update()
	{
		// sends the position of the pivot to everyone interested
		if (OnSendPositionInfo != null)
			OnSendPositionInfo(objectToMove.position);

        if (OnSendNormalizedMousePos != null)
            OnSendNormalizedMousePos(ConvertMouseposToNormalisedCoords(Input.mousePosition));

        if (movementMethod != null && !movementFrozen)
            movementMethod();
    }
 
    void MouseMovement()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 1;
        Vector3 oldPos = objectToMove.position; // used for tracking
        Vector3 newPos;

        newPos = new Vector3(
            ((mousePos.x / screenWidth) * xBound) + leftBottomCorner.x,
            hoogte,
            ((mousePos.y / screenHeight) * zBound) + leftBottomCorner.z
        );

// clamps the movemens between the boundaries. This is auto when playing fullscreen, 
// but when in editor you can move past that because the 'screen' is bigger than the window
#if UNITY_EDITOR
        if (clamp)
        {
            newPos = new Vector3(   
                Mathf.Clamp(newPos.x, leftBottomCorner.x, xBound + leftBottomCorner.x),
                hoogte,
                Mathf.Clamp(newPos.z, leftBottomCorner.z, zBound + leftBottomCorner.z)
            );
        }
#endif
            //currentCursorPosition.position = newPos; //*THIS EDITS THE MOVEREF POSITION!*
        objectToMove.position = Vector3.Lerp( objectToMove.position, newPos, Time.deltaTime * positionDelay );
        accumulatedDistance += Vector3.Distance(oldPos, objectToMove.position);

        OnSendAccumulatedDistance?.Invoke(accumulatedDistance);
    }

    public static Vector2 ConvertMouseposToNormalisedCoords(Vector2 mousePos, bool clamp = true)
    {
        Vector2 converted = new Vector2((1f / Screen.width) * mousePos.x, (1f / Screen.height) * mousePos.y);

        if (clamp)
        {
            converted.x = Mathf.Clamp01(converted.x);
            converted.y = Mathf.Clamp01(converted.y);
        }

        return converted;
    }
}