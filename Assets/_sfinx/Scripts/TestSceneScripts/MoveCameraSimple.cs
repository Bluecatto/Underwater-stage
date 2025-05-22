using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveCameraSimple : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float rotateSpeed;

    // Start is called before the first frame update
    void OnEnable()
    {

    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();

        HandleRotation();
    }


    private void HandleMovement()
    {
        Vector3 deltaMovement = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            deltaMovement.z += Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            deltaMovement.z -= Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            deltaMovement.x += Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            deltaMovement.x -= Time.deltaTime;
        }

        transform.position += deltaMovement * moveSpeed;
    }

    private void HandleRotation()
    {

    }
}
