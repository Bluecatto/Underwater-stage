using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if (UNITY_EDITOR)
using UnityEditor;
#endif

[ExecuteInEditMode]
public class LevelBoundaries : MonoBehaviour {

    public static LevelBoundaries Instance { get; private set; }

    public enum Origin { corner, center };

	public Material boundaryMaterial;

	[Header("Dimensions in Unity meters")]
	public Origin origin;
	
    public float levelWidth = 15f;
    public float levelDepth = 15f;
    
	public float boundaryHeight = 5;
	
	[Space(30)]
	public static float xBoundary = 0;
    public static float zBoundary = 0;

	[HideInInspector]
	public List<Vector3> positions = new List<Vector3>();
	private GameObject bounderyMesh;

	private Vector3[] adjustedCornerPositions = new Vector3[0];

    // Use this for initialization
    void Awake () {
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;

        xBoundary = levelWidth;
		zBoundary = levelDepth;

#if (UNITY_EDITOR)
		if (xBoundary <= 0 || zBoundary <= 0)
		{
			Debug.LogError("You probably haven't setup your levelboundaries correctly!");

			if (EditorUtility.DisplayDialog("Set Level Boundaries", "You got an error because one or several of your levelboundaries are 0, this will result in not being able to move. You can manually change it on the 'LevelBoundaries object in the scene.", "Ok", "Continue anyway"))
			{
				Selection.activeGameObject = this.gameObject;
				UnityEditor.EditorApplication.isPlaying = false;
			}
		}
#endif
	}

    public Vector2 GetLevelSize()
    {
        return new Vector2(levelWidth, levelDepth);
    }

    [ContextMenu("Set Markers")]
	public void SetMarkers()
	{
		// if (boundaryMaterial == null)
		// 	boundaryMaterial = Resources.Load("BounderyMaterial") as Material;

		positions.Clear();

		if (origin == Origin.center)
		{
			positions.Add(Vector3.zero - new Vector3(levelWidth / 2f, 0, levelDepth / 2f) + transform.position);
			positions.Add(new Vector3(levelWidth - levelWidth / 2f, 0, levelDepth / 2f) + transform.position);
			positions.Add(new Vector3(-levelWidth / 2f, 0, levelDepth / 2f) + transform.position);
			positions.Add(new Vector3(levelWidth / 2f, 0, -levelDepth / 2f) + transform.position);
		}
		else 
		if (origin == Origin.corner)
		{
			positions.Add(Vector3.zero + transform.position);
			positions.Add(new Vector3(levelWidth, 0, 0) + transform.position);
			positions.Add(new Vector3(0, 0, levelDepth) + transform.position);
			positions.Add(new Vector3(levelWidth, 0, levelDepth) + transform.position);
		}
		
		xBoundary = levelWidth;
		zBoundary = levelDepth;

		Debug.Log("Boundaries are set to " + xBoundary + " by " + zBoundary + " meters");

		// remove old meshes
		if (bounderyMesh != null)
		{
			DestroyImmediate(bounderyMesh);
		}
		if (bounderyMesh == null)
		{
			foreach (Transform child in transform)
			{
				DestroyImmediate(child.gameObject);
			}
		}
	}

	/// <summary>
	/// Gets you all the four corners of the level in world-space
	/// </summary>
	/// <returns></returns>
	public Vector3[] GetPositionsOfCorners()
	{
		Vector3[] cornerPositions = new Vector3[4];

		for (int i = 0; i < 4; i++)
		{
			cornerPositions[i] = positions[i];
		}

		return cornerPositions;
	}

	/// <summary>
	/// Gets the 2 corners of the level with a specified adjustment towards the inside
	/// </summary>
	/// <param name="insideAdjustment">The amount to move inwards</param>
	/// <returns></returns>
	public Vector3[] GetAdjustedPositionsOfCorners(float insideAdjustment)
	{
		adjustedCornerPositions = new Vector3[2];

		adjustedCornerPositions[0] += new Vector3(positions[0].x + insideAdjustment, 0, positions[0].z + insideAdjustment);
		adjustedCornerPositions[1] += new Vector3(positions[3].x - insideAdjustment, 0, positions[3].z - insideAdjustment);

		return adjustedCornerPositions;
	}

    public bool IsWithinCurrentBounds(Vector3 position)
    {
        Vector3[] corners = GetPositionsOfCorners();

        if (position.x < corners[0].x ||
            position.x > corners[3].x ||
            position.z < corners[0].z ||
            position.z > corners[3].z)
            return false;

        return true;
    }

	void OnDrawGizmos()
    {
		if (positions.Count >= 4)
		{
			Gizmos.color = Color.yellow;

			Gizmos.DrawLine(positions[0], positions[0] + Vector3.up * boundaryHeight);

			Gizmos.DrawLine(positions[1], positions[1] + Vector3.up * boundaryHeight);

			Gizmos.DrawLine(positions[2], positions[2] + Vector3.up * boundaryHeight);

			Gizmos.DrawLine(positions[3], positions[3] + Vector3.up * boundaryHeight);
			
			
			Gizmos.DrawWireCube(origin == Origin.center ? transform.position : (transform.position + new Vector3(levelWidth / 2f, 0, levelDepth / 2f)),
				new Vector3(levelWidth, 0f, levelDepth));
		}

		if (adjustedCornerPositions.Length > 0)
		{
			Gizmos.color = Color.blue;

			Gizmos.DrawLine(adjustedCornerPositions[0], adjustedCornerPositions[0] + Vector3.up * boundaryHeight);
			Gizmos.DrawLine(adjustedCornerPositions[1], adjustedCornerPositions[1] + Vector3.up * boundaryHeight);
		}
    }
}
