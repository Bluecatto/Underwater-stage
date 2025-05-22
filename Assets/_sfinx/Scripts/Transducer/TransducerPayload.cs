using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[System.Serializable]
public class TransducerPayload
{
    public ulong frameNumber;
    public Quaternion quaternion;
    public Vector2 mousePosition;

    public float offsetOnY;

    public TransducerPayload()
    {
        this.frameNumber = 0;
        this.mousePosition = Vector2.zero;
    }

    public TransducerPayload(ulong frameNumber, float[] quaternion, Vector2 mousePosition)
    {
        this.frameNumber = frameNumber;
        this.quaternion = new Quaternion(quaternion[0], quaternion[1], quaternion[2], quaternion[3]);

        this.mousePosition = mousePosition;
    }

    // currently used
    public TransducerPayload(ulong frameNumber, Quaternion quaternion, Vector2 mousePosition)
    {
        this.frameNumber = frameNumber;
        this.quaternion = quaternion;
        this.mousePosition = mousePosition;
    }

    // create a payload from an input string
    public TransducerPayload(string formattedString)
    {
        string[] spl = formattedString.Split(',');
        this.frameNumber = uint.Parse(spl[0]);
        this.quaternion = new Quaternion(float.Parse(spl[6]), float.Parse(spl[7]), float.Parse(spl[8]), float.Parse(spl[9]));
        this.mousePosition = new Vector2(float.Parse(spl[4]), float.Parse(spl[5]));
        this.offsetOnY = float.Parse(spl[10]);
    }

    // create a formatted string including the direction vector
    public string GetFormattedStringVector(int digitsVector = 3, int digitsMousePos = 3)
    {
        Vector3 dir = this.quaternion * Vector3.up;
        dir = dir.normalized;

        // Format(frameNumber, vX, vY, vZ, mouseposX, mouseposY, qW, qX, qY, qZ)
        string payloadString = string.Format("{0},{1},{2},{3},{4},{5}", 
            this.frameNumber, 
            dir.x.ToString("F" + digitsVector), 
            dir.y.ToString("F" + digitsVector), 
            dir.z.ToString("F" + digitsVector),
            this.mousePosition.x.ToString("F" + digitsMousePos), 
            this.mousePosition.y.ToString("F" + digitsMousePos));
        return payloadString;
    }

    // create a formatted string including a direction vector AND the quaternion
    public string GetFormattedStringQuaternionAndVector(int digitsVector = 3, int digitsMousePos = 3)
    {
        Vector3 dir = this.quaternion * Vector3.up;
        dir = dir.normalized;

        // Format(frameNumber, vX, vY, vZ, mouseposX, mouseposY, qW, qX, qY, qZ)
        string payloadString = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
            this.frameNumber,
            dir.x.ToString("F" + digitsVector),
            dir.y.ToString("F" + digitsVector),
            dir.z.ToString("F" + digitsVector),
            this.mousePosition.x.ToString("F" + digitsMousePos),
            this.mousePosition.y.ToString("F" + digitsMousePos),
            this.quaternion.x,
            this.quaternion.y,
            this.quaternion.z,
            this.quaternion.w
        );
        return payloadString;
    }
}
