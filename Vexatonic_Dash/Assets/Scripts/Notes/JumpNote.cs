using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class JumpNote : Note
{
    public Vector3 GetInformationForPlayer(float inputWidth, float jumpHeight, Vector3 startPos, int gravity)
    {
        Debug.Log((int)direction);
        this.startPos = startPos;
        this.endPos = startPos + (Quaternion.AngleAxis(gravity, Vector3.forward) * new Vector3(inputWidth * (int)direction, jumpHeight));
        return endPos;
    }
}
