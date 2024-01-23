using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformNote : Note
{
    private int speedCoeff; // 해당 플랫폼 위에서 플레이어의 이동 속도 배수. 1이면 일반 노트, 1보다 크면 대쉬 노트. 저속 노트 만들까..?
    private int angle; // 플랫폼의 기울기. 0 <= angle < 90

    public Vector3 startPos;
    public Vector3 endPos;

    public override void GetInformationForPlayer(float platformScale, Vector3 startPos) {
        this.startPos = startPos;
        Vector2 directionVector = Quaternion.AngleAxis(angle, platformScale * Vector2.right) * (2 * Vector2.right);
        endPos = startPos + new Vector3(directionVector.x, directionVector.y, 0);
    }
}
