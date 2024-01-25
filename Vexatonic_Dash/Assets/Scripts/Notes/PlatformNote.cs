using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformNote : Note
{
    private int angle; // 플랫폼의 기울기. 0 <= angle < 90

    public Vector3 startPos;
    public Vector3 endPos;

    public override void GetInformationForPlayer(float platformScale, Vector3 startPos) {
        this.startPos = startPos;
        Debug.Log("startPos: " + startPos);
        Vector2 directionVector = Quaternion.AngleAxis(angle, Vector2.right) * (platformScale * Vector2.right);
        endPos = startPos + new Vector3(directionVector.x, directionVector.y, 0);
        Debug.Log("endPos: " + endPos);
    }
}
