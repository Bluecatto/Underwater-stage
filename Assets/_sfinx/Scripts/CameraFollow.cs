using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    //Making a new camera
    public CinemachineVirtualCamera virtualCamera;
    public Transform target;

    void Start()
    {
        //Makes the virtual camera follow the object
        if (virtualCamera != null && target != null) 
        {
            virtualCamera.Follow = target;
            virtualCamera.LookAt = target;
        }
    }
}