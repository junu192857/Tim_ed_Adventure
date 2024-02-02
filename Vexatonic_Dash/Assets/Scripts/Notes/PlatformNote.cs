using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformNote : Note
{
    public Vector3 GetInformationForPlayer(float platformScale, Vector3 startPos) {
        this.startPos = startPos;
        Vector2 directionVector = Quaternion.AngleAxis(angle, Vector2.right) * (platformScale * Vector2.right);
        endPos = startPos + new Vector3(directionVector.x, directionVector.y, 0);
        return endPos;
    }
}
