using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public Camera camera1;
    public Camera camera2;
    public GameObject player;

    public Vector3 camera1PlayerPosition;
    public Quaternion camera1PlayerRotation;
    public Vector3 camera2PlayerPosition;
    public Quaternion camera2PlayerRotation;
    private bool isCamera1Active = true;

    private void Start()
    {
        camera1.enabled = true;
        camera2.enabled = false;

        player.transform.position = camera1PlayerPosition;
        player.transform.position = camera2PlayerPosition;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            if (isCamera1Active)
            {
                SwitchToCamera(camera2, camera2PlayerPosition, camera2PlayerRotation);
            }
            else
            {
                SwitchToCamera(camera1, camera2PlayerPosition, camera2PlayerRotation);
            }

            isCamera1Active = !isCamera1Active;
        }
    }

    private void SwitchToCamera(Camera targetCamera, Vector3 targetPosition, Quaternion targetRotation)
    {
        camera1.enabled = false;
        camera2.enabled = false;

        targetCamera.enabled = true;

        player.transform.position = targetPosition;
        player.transform.rotation = targetRotation;
    }
}