using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliceObjectsWithPlane : MonoBehaviour
{
    public Material cutMaterial;

    private Plane plane;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        plane = new Plane(transform.forward, transform.position);

        Vector4 planeRepresentation = new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);

        cutMaterial.SetVector("_Plane", planeRepresentation);
    }
}
