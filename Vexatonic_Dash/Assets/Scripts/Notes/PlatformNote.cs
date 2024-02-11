using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformNote : Note
{
    public Vector3 GetInformationForPlayer(float platformScale, Vector3 startPos, int gravity) {
        this.startPos = startPos;

        Vector2 directionVector = new Vector2(platformScale * (int)direction, Mathf.Tan(Mathf.Deg2Rad * angle));
        directionVector = Quaternion.AngleAxis(gravity, Vector3.forward) * directionVector;
        endPos = startPos + new Vector3(directionVector.x, directionVector.y, 0);
        Debug.Log(endPos);
        return endPos;
    }
}
